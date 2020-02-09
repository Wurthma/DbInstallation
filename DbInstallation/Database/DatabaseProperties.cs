using DbInstallation.Interfaces;
using NLog;
using System;

namespace DbInstallation.Database
{
    public class DatabaseProperties : IDatabaseProperties
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DatabaseProperties(string dataBaseUser, string databasePassword, string serverOrTns, string tablespaceData, string tablespaceIndex)
        {
            DatabaseUser = dataBaseUser;
            DatabasePassword = databasePassword;
            ServerOrTns = serverOrTns;
            TablespaceData = tablespaceData;
            TablespaceIndex = tablespaceIndex;
            DatabaseName = null;
            IsTrustedConnection = false;
            ValidatePropertiesOracle();
        }

        public DatabaseProperties(string dataBaseUser, string databasePassword, string serverOrTns, string databaseName, bool isTrustedConnection)
        {
            DatabaseUser = dataBaseUser;
            DatabasePassword = databasePassword;
            ServerOrTns = serverOrTns;
            TablespaceData = null;
            TablespaceIndex = null;
            DatabaseName = databaseName;
            IsTrustedConnection = isTrustedConnection;
            ValidatePropertiesSqlServer(isTrustedConnection);
        }

        public string DatabaseUser { get; private set; }

        public string DatabasePassword { get; private set; }

        public string ServerOrTns { get; private set; }

        public string TablespaceData { get; private set; }

        public string TablespaceIndex { get; private set; }

        public string DatabaseName { get; private set; }

        public bool IsTrustedConnection { get; private set; }

        private bool ValidatePropertiesOracle()
        {
            bool isValid = ValidateProperties();

            if (string.IsNullOrEmpty(TablespaceData))
            {
                isValid = false;
                LoggerNullException(nameof(TablespaceData));
            }
            if (string.IsNullOrEmpty(TablespaceIndex))
            {
                isValid = false;
                LoggerNullException(nameof(TablespaceIndex));
            }

            return isValid;
        }

        private bool ValidatePropertiesSqlServer(bool isTrustedConnection)
        {
            bool isValid = true;
            if (!isTrustedConnection)
            {
                isValid = ValidateProperties();
            }
            if (string.IsNullOrEmpty(ServerOrTns))
            {
                isValid = false;
                LoggerNullException(nameof(ServerOrTns));
            }
            if (string.IsNullOrEmpty(DatabaseName))
            {
                isValid = false;
                LoggerNullException(nameof(DatabaseName));
            }

            return isValid;
        }

        private bool ValidateProperties()
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(DatabaseUser))
            {
                isValid = false;
                LoggerNullException(nameof(DatabaseUser));
            }
            if (string.IsNullOrEmpty(DatabasePassword))
            {
                isValid = false;
                LoggerNullException(nameof(DatabasePassword));
            }

            return isValid;
        }

        private void LoggerNullException(string propertie)
        {
            var ex = new ArgumentNullException($@"The propertie {propertie} from {nameof(DatabaseProperties)} class canot be null.");
            Logger.Error(ex, ex.Message);
            throw ex;
        }
    }
}
