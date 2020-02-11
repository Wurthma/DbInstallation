using DbInstallation.Database;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static DbInstallation.Enums.EnumDbType;
using static DbInstallation.Enums.EnumOperation;

namespace DbInstallation.Util
{
    public static class FileHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                dict.Add(10, @"\Custom");
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
                dict.Add(8, @"\Custom");
                return dict;
            } 
        }

        private static string GetScriptsPath(ProductDbType dbType, OperationType operationType) => 
            Path.GetDirectoryName(new Uri(Assembly.GetAssembly(typeof(ProductDatabase)).CodeBase).LocalPath) + SqlScriptsFolder + GetDbFolder(dbType) + GetOperationFolder(operationType);

        private static string GetDbFolder(ProductDbType dbType)
        {
            if (dbType == ProductDbType.Oracle)
                return OracleFolder;
            else if (dbType == ProductDbType.SqlServer)
                return SqlServerFolder;
            else
                throw new Exception(Messages.ErrorMessage006);
        }

        private static string GetOperationFolder(OperationType operationType)
        {
            if (operationType == OperationType.Install)
                return InstallFolder;
            else if (operationType == OperationType.Update)
                return UpdateFolder;
            else
                throw new Exception(Messages.ErrorMessage005);
        }

        public static List<string> ListFolders(ProductDbType dbType, OperationType operationType)
        {
            if (dbType == ProductDbType.Oracle)
            {
                return OracleListFolder
                    .OrderBy(o => o.Key)
                    .Select(s => GetScriptsPath(dbType, operationType) + s.Value)
                    .ToList();
            }
            else if (dbType == ProductDbType.SqlServer)
            {
                return SqlServerListFolder
                    .OrderBy(o => o.Key)
                    .Select(s => GetScriptsPath(dbType, operationType) + s.Value)
                    .ToList();
            }
            else 
            {
                throw new Exception(Messages.ErrorMessage006);
            }
        }


        public static List<string> ListFiles(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path).ToList();
            }
            else
            {
                Logger.Error(Messages.ErrorMessage008(path));
                return new List<string>();
            }
        }
    }
}
