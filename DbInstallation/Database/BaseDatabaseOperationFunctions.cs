using DbInstallation.Util;
using NLog;
using System;

namespace DbInstallation.Database
{
    public abstract class BaseDatabaseOperationFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected BaseDatabaseOperationFunctions(DatabaseProperties databaseProperties)
        {
            DatabaseProperties = databaseProperties;
        }

        public string ConnectionString { get; private set; }

        public DatabaseProperties DatabaseProperties { get; private set; }

        protected void SetConnectionString(string connectionString) => ConnectionString = connectionString;

        protected static bool IsPlatypusSqlCommand(string fileName) =>
            fileName.ToUpper().Contains(@"0-PLATYPUS") ||
            (fileName.ToUpper().Contains(@"2-BFW") && fileName.ToUpper().Contains(@"ESTRUTURA"));

        protected bool ValidateMinimumVersion(int currentVersion)
        {
            if (int.TryParse(Common.GetAppSetting("MinVersion"), out int minimumVersion))
            {
                bool isValid = currentVersion >= minimumVersion;
                if(!isValid)
                {
                    Logger.Error(Messages.ErrorMessage028(minimumVersion, currentVersion));
                    Environment.ExitCode = -1;
                }
                return isValid;
            }
            else
            {
                Environment.ExitCode = -1;
                throw new Exception(Messages.ErrorMessage027);
            }
        }
    }
}
