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
            DataBaseUser = dataBaseUser;
            DatabasePassword = databasePassword;
            ServerOrTns = serverOrTns;
            TablespaceData = tablespaceData;
            TablespaceIndex = tablespaceIndex;
            DatabaseName = null;
            IsTrustedConnection = false;
        }

        public DatabaseProperties(string dataBaseUser, string databasePassword, string serverOrTns, string databaseName, bool isTrustedConnection)
        {
            DataBaseUser = dataBaseUser;
            DatabasePassword = databasePassword;
            ServerOrTns = serverOrTns;
            TablespaceData = null;
            TablespaceIndex = null;
            DatabaseName = databaseName;
            IsTrustedConnection = isTrustedConnection;
        }

        public string DataBaseUser { get; private set; }

        public string DatabasePassword { get; private set; }

        public string ServerOrTns { get; private set; }

        public string TablespaceData { get; private set; }

        public string TablespaceIndex { get; private set; }

        public string DatabaseName { get; private set; }

        public bool IsTrustedConnection { get; private set; }

        private bool ValidateProperties()
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(DataBaseUser))
            {
                isValid = false;
                LoggerNullException(nameof(DataBaseUser));
            }
            if (string.IsNullOrEmpty(DataBaseUser))
            {
                isValid = false;
                LoggerNullException(nameof(DatabasePassword));
            }
            if (string.IsNullOrEmpty(ServerOrTns))
            {
                isValid = false;
                LoggerNullException(nameof(ServerOrTns));
            }

            return isValid;
        }

        //TODO: realizar restante das validações da classe

        private void LoggerNullException(string propertie)
        {
            var ex = new ArgumentNullException($@"The propertie {propertie} from {nameof(DatabaseProperties)} class canot be null.");
            Logger.Error(ex, ex.Message);
            throw ex;
        }
    }
}
