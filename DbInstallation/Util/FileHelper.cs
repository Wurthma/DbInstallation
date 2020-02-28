using DbInstallation.Database;
using DbInstallation.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DbInstallation.Util
{
    public static class FileHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger othersLogger = LogManager.GetLogger("othersLog");

        private static string SqlScriptsFolder { get => @"\Database\SqlScript"; }
        
        private static string OracleFolder { get => @"\Oracle"; }

        private static string SqlServerFolder { get => @"\SqlServer"; }

        private static string InstallFolder { get => @"\Install"; }

        private static string UpdateFolder { get => @"\Update"; }

        private static Dictionary<int, string> OracleListFolder { 
            get 
            {
                return new Dictionary<int, string>
                {
                    { 0, @"\Platypus" },
                    { 1, @"\Table" },
                    { 2, @"\Constraints" },
                    { 3, @"\Sequence" },
                    { 4, @"\Functions" },
                    { 5, @"\Procedure" },
                    { 6, @"\Package" },
                    { 7, @"\Trigger" },
                    { 8, @"\View" },
                    { 9, @"\Carga" }
                };
            } 
        }

        private static Dictionary<int, string> SqlServerListFolder { 
            get 
            {
                return new Dictionary<int, string>
                {
                    { 0, @"\Platypus" },
                    { 1, @"\Table" },
                    { 2, @"\Constraints" },
                    { 3, @"\Functions" },
                    { 4, @"\Procedure" },
                    { 5, @"\Trigger" },
                    { 6, @"\View" },
                    { 7, @"\Carga" }
                };
            } 
        }

        private static string GetBaseFolder(ProductDbType dbType) =>
            Path.GetDirectoryName(new Uri(Assembly.GetAssembly(typeof(ProductDatabase)).CodeBase).LocalPath) + SqlScriptsFolder + GetDbFolder(dbType);

        private static string GetScriptsPath(ProductDbType dbType, OperationType operationType, int? version = null) =>
            GetBaseFolder(dbType) + GetOperationFolder(operationType, version);

        private static string GetDbFolder(ProductDbType dbType)
        {
            if (dbType == ProductDbType.Oracle)
                return OracleFolder;
            else if (dbType == ProductDbType.SqlServer)
                return SqlServerFolder;
            else
                throw new Exception(Messages.ErrorMessage006);
        }

        private static string GetOperationFolder(OperationType operationType, int? version = null)
        {
            if (operationType == OperationType.Install)
            {
                return InstallFolder;
            }
            else if (operationType == OperationType.Update)
            {
                if(version != null)
                {
                    return $@"{UpdateFolder}\{version}";
                }
                throw new Exception(Messages.ErrorMessage014);
            }
            else
                throw new Exception(Messages.ErrorMessage005);
        }

        public static bool CreateNextUpdateFolders()
        {
            int nextUpdateFolder = GetNextUpdateFolder();
            try
            {
                string oracleNextUpdateFolder = GetBaseFolder(ProductDbType.Oracle) + UpdateFolder + "\\" + nextUpdateFolder;
                string sqlServerNextUpdateFolder = GetBaseFolder(ProductDbType.SqlServer) + UpdateFolder + "\\" + nextUpdateFolder;

                DirectoryInfo createdFolderOracle = Directory.CreateDirectory(oracleNextUpdateFolder);
                DirectoryInfo createdFolderSqlServer = Directory.CreateDirectory(sqlServerNextUpdateFolder);

                if (Directory.Exists(oracleNextUpdateFolder) && Directory.Exists(sqlServerNextUpdateFolder))
                {
                    othersLogger.Info(Messages.Message012(nextUpdateFolder));

                    bool createListFolders =
                        CreateFolders(createdFolderOracle.FullName, OracleListFolder.Values.ToList()) &&
                        CreateFolders(createdFolderSqlServer.FullName, SqlServerListFolder.Values.ToList());

                    return createListFolders;
                }
                othersLogger.Error(Messages.ErrorMessage019($@"Update\{nextUpdateFolder}"));
                return false;
            }
            catch (Exception ex)
            {
                othersLogger.Error(ex, ex.Message);
                throw;
            }
        }

        private static bool CreateFolders(string createInPath, List<string> listFolderToCreate)
        {
            bool foldersCreated = true;
            foreach (var folder in listFolderToCreate)
            {
                DirectoryInfo createdFolder = Directory.CreateDirectory(createInPath + folder);
                if (createdFolder.Exists)
                {
                    othersLogger.Info(Messages.Message013(createdFolder.FullName));
                }
                else
                {
                    othersLogger.Error(Messages.ErrorMessage019(createdFolder.FullName));
                    foldersCreated = false;
                }
            }
            return foldersCreated;
        }

        private static int GetNextUpdateFolder()
        {
            var oracleDirectoryList = Directory.GetDirectories(
                GetBaseFolder(ProductDbType.Oracle) + UpdateFolder, "*", SearchOption.TopDirectoryOnly)
                .ToList();

            var sqlServerDirectoryList = Directory.GetDirectories(
                GetBaseFolder(ProductDbType.SqlServer) + UpdateFolder, "*", SearchOption.TopDirectoryOnly)
                .ToList();

            int oracleMaxVersion = GetCurrentUpdateFolder(oracleDirectoryList);
            int sqlServerMaxVersion = GetCurrentUpdateFolder(sqlServerDirectoryList);

            if(oracleMaxVersion == sqlServerMaxVersion)
            {
                return oracleMaxVersion+1;
            }
            else
            {
                throw new DirectoryNotFoundException(Messages.ErrorMessage018(oracleMaxVersion, sqlServerMaxVersion));
            }
        }

        private static int GetCurrentUpdateFolder(List<string> directoryList)
        {           
            int maxVersion = 0;

            foreach (var folderVersion in directoryList)
            {
                if(int.TryParse(Path.GetFileName(folderVersion), out int version))
                {
                    if (maxVersion < version)
                    {
                        maxVersion = version;
                    }
                }
                else
                {
                    Logger.Warn(Messages.ErrorMessage017(folderVersion));
                }
                
            }
            return maxVersion;
        }

        public static List<string> ListFolders(ProductDbType dbType, OperationType operationType, int? currentVersion = null, int? updateVersion = null)
        {
            Dictionary<int, string> folderList = new Dictionary<int, string>();

            if (dbType == ProductDbType.Oracle)
            {
                folderList = OracleListFolder;
            }
            else if (dbType == ProductDbType.SqlServer)
            {
                folderList = SqlServerListFolder;
            }

            if (operationType == OperationType.Install)
            {
                return folderList
                .OrderBy(o => o.Key)
                .Select(s => GetScriptsPath(dbType, operationType) + s.Value)
                .ToList();
            }
            else if (operationType == OperationType.Update)
            {
                if (currentVersion != null)
                {
                    List<string> updateFolders = new List<string>();

                    for (int i = currentVersion.Value + 1; i <= updateVersion.Value; i++)
                    {
                        updateFolders.AddRange(
                            folderList
                                .OrderBy(o => o.Key)
                                .Select(s => GetScriptsPath(dbType, operationType, i) + s.Value)
                                .ToList()
                        );
                    }
                    return updateFolders;
                }
                else
                    throw new ArgumentException(Messages.ErrorMessage014);
            }

            if (dbType == ProductDbType.SqlServer)
            {
                //TODO: Rever ao passar por esse trecho no Sql Server (reaproveitar códigos acima...)
                if (operationType == OperationType.Install)
                {
                    return SqlServerListFolder
                    .OrderBy(o => o.Key)
                    .Select(s => GetScriptsPath(dbType, operationType) + s.Value)
                    .ToList();
                }
                else if (operationType == OperationType.Update)
                {
                    //TODO
                }
            }
            else 
            {
                throw new Exception(Messages.ErrorMessage006);
            }

            throw new ArgumentException(Messages.ErrorMessage007);
        }


        public static List<string> ListFiles(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path).ToList();
            }
            else
            {
                Logger.Warn(Messages.ErrorMessage008(path));
                return new List<string>();
            }
        }

        /// <summary>
        /// Return the version that the path represents (if its not an installation)
        /// </summary>
        /// <param name="path">Directory path to check.</param>
        /// <param name="version">The version that path represents.</param>
        /// <returns>True if an update and version is found, otherwise it returns false.</returns>
        public static bool GetVersionFromDirectoryPath(string path, out int version)
        {
            if (int.TryParse(Directory.GetParent(path).Name, out version))
            {
                return true;
            }
            return false;
        }
    }
}
