using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public class ProductDatabase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public void SetConnection(ProductDbType dbType)
        {
            if(dbType == ProductDbType.Oracle)
            {
                SetOracleConnection();
            }
            else if(dbType == ProductDbType.SqlServer)
            {
                SetSqlServerConnection();
            }
            else
            {
                throw new Exception("Invalid Database type option.");
            }
        }

        public ProductDbType GetDatabaseType(string dbType)
        {
            if (dbType.ToUpper() == "O")
            {
                Logger.Info("Selected Oracle Database");
                return ProductDbType.Oracle;
            }
            else if (dbType.ToUpper() == "S")
            {
                Logger.Info("Selected SQL Server Database");
                return ProductDbType.SqlServer;
            }
            return ProductDbType.None;
        }

        public void InstallDatabase(ProductDbType dbType)
        {
            throw new NotImplementedException();
        }

        public void UpdateDatabase(ProductDbType dbType)
        {
            throw new NotImplementedException();
        }

        private void SetOracleConnection()
        {
            DatabaseProperties databaseProperties = RequestDbInputsProperties();
            Console.WriteLine("Enter data TABLESPACE:");
            string tablespaceData = Console.ReadLine();
            Console.WriteLine("Enter index TABLESPACE:");
            string tablespaceIndex = Console.ReadLine();

            OracleInstallationFunctions oracleInstallationFunctions = new OracleInstallationFunctions(
                databaseProperties,
                tablespaceData,
                tablespaceIndex);

            try
            {
                oracleInstallationFunctions.TestConnection();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }

        private void SetSqlServerConnection()
        {
            DatabaseProperties databaseProperties = RequestDbInputsProperties();

            SqlServerInstallationFunctions oracleInstallationFunctions = new SqlServerInstallationFunctions(databaseProperties);

            try
            {
                oracleInstallationFunctions.TestConnection();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }

        private DatabaseProperties RequestDbInputsProperties()
        {
            Console.WriteLine("Enter database user:");
            string dbUser = Console.ReadLine();
            Console.WriteLine("Enter the password:");
            string dbPassword = Console.ReadLine();
            Console.WriteLine("Enter the string connection or TNS:");
            string tnsConnection = Console.ReadLine();

            return new DatabaseProperties(dbUser, dbPassword, tnsConnection);
        }
    }
}
