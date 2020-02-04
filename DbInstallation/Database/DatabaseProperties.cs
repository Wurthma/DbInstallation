using DbInstallation.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbInstallation.Database
{
    public class DatabaseProperties : IDatabaseProperties
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DatabaseProperties(string dataBaseUser, string databasePassword, string serverOrTns)
        {
            DataBaseUser = dataBaseUser;
            DatabasePassword = databasePassword;
            ServerOrTns = serverOrTns;
        }

        public string DataBaseUser { get; private set; }

        public string DatabasePassword { get; private set; }

        public string ServerOrTns { get; private set; }

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

        private void LoggerNullException(string propertie)
        {
            var ex = new ArgumentNullException($@"The propertie {propertie} from {nameof(DatabaseProperties)} class canot be null.");
            Logger.Error(ex, ex.Message);
            throw ex;
        }
    }
}
