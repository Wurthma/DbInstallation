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
                var dict = new Dictionary<int, string>();
                dict.Add(0, @"\Platypus");
                dict.Add(1, @"\Table");
                dict.Add(2, @"\Constraints");
                dict.Add(3, @"\Sequence");
                dict.Add(4, @"\Functions");
                dict.Add(5, @"\Procedure");
                dict.Add(6, @"\Package");
                dict.Add(7, @"\Trigger");
                dict.Add(8, @"\View");
                dict.Add(9, @"\Carga");
                return dict;
            } 
        }

        private static Dictionary<int, string> SqlServerListFolder { 
            get 
            {
                var dict = new Dictionary<int, string>();
                dict.Add(0, @"\Platypus");
                dict.Add(1, @"\Table");
                dict.Add(2, @"\Constraints");
                dict.Add(3, @"\Functions");
                dict.Add(4, @"\Procedure");
                dict.Add(5, @"\Trigger");
                dict.Add(6, @"\View");
                dict.Add(7, @"\Carga");
                return dict;
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
            if (dbType == ProductDbType.Oracle)
            {
                if(operationType == OperationType.Install)
                {
                    return OracleListFolder
                    .OrderBy(o => o.Key)
                    .Select(s => GetScriptsPath(dbType, operationType) + s.Value)
                    .ToList();
                }
                else if (operationType == OperationType.Update)
                {
                    if(currentVersion != null)
                    {
                        List<string> updateFolders = new List<string>();

                        for (int i = currentVersion.Value+1; i <= updateVersion.Value; i++)
                        {
                            updateFolders.AddRange(
                                OracleListFolder
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
            }
            else if (dbType == ProductDbType.SqlServer)
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

        public static List<string> ListSqlCommandsFromFile(string filePath)
        {
            List<string> sqlCommandList = new List<string>();
            Regex regex = GetRegexPattern(filePath);
            var content = File.ReadAllText(filePath) + Environment.NewLine; //Adiciona nova linha ao final do conteúdo para o funcionamento correto do regex
            MatchCollection matchCollection = regex.Matches(content);

            foreach (Match match in matchCollection)
            {
                string sqlCommand = match.Groups["cmd"].Value.Trim();
                if (!IsComment(sqlCommand))
                {
                    sqlCommandList.Add(sqlCommand);
                }
            }
            return sqlCommandList;
        }

        private static Regex GetRegexPattern(string file)
        {
            Regex regex = new Regex(@"(?<cmd>[\s\S.]+?);\s*[\n\r]");
            if (IsPlSqlCommand(file))
            {
                regex = new Regex(@"(?<cmd>[^\s\/][\s\S.]+?;)\s*\/");
            }
            else if (IsPlSqlFunctionProceduresPackageTriggerView(file))
            {
                regex = new Regex(@"(?<cmd>[\s\S.]+?;)\s*\/[\n\r]");
            }
            return regex;
        }

        private static bool IsPlSqlCommand(string file) => 
            file.ToUpper().Contains(@"0-PLATYPUS") || 
            (file.ToUpper().Contains(@"2-BFW") && file.ToUpper().Contains(@"ESTRUTURA"));

        private static bool IsPlSqlFunctionProceduresPackageTriggerView(string file) => 
            file.ToUpper().Contains(@"\FUNCTIONS\") ||
            file.ToUpper().Contains(@"\PROCEDURE\") ||
            file.ToUpper().Contains(@"\PACKAGE\") ||
            file.ToUpper().Contains(@"\TRIGGER\") ||
            file.ToUpper().Contains(@"\VIEW\");

        private static bool IsComment(string sqlCommand)
        {
            string fullCommand = string.Empty;
            if (sqlCommand.StartsWith("--"))
            {
                Regex regex = new Regex(@"--.+(?<cmd>[\s\S.]*)");
                MatchCollection matchCollection = regex.Matches(sqlCommand);
                foreach (Match match in matchCollection)
                {
                    fullCommand += match.Groups["cmd"].Value.Trim();
                }
                return string.IsNullOrEmpty(fullCommand);
            }
            return false;
        }
    }
}
