using DbInstallation.Interfaces;
using NLog;
using System;
using System.Data.SqlClient;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public class SqlServerOperationFunctions : BaseDatabaseOperationFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SqlServerOperationFunctions(DatabaseProperties databaseProperties)
            : base(databaseProperties)
        {
            SetConnectionString(ProductConnectionString.GetConnectionString(databaseProperties, ProductDbType.SqlServer));
        }

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    string queryString = "SELECT 1";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        if (reader.GetInt32(0) == 1)
                            Logger.Info($@"Connection successfully made to the database {DatabaseProperties.ServerOrTns}/{DatabaseProperties.DatabaseName}.");
                        else
                            return false;
                    }
                    else
                    {
                        Logger.Error($@"Failed to execute basic instruction after connecting to the database {DatabaseProperties.ServerOrTns}/{DatabaseProperties.DatabaseName}.");
                        return false;
                    }
                }
                return ValidateDatabase();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return false;
            }
        }

        public bool Install()
        {
            throw new NotImplementedException(); //TODO;
        }

        public bool Update()
        {
            throw new NotImplementedException(); //TODO;
        }

        private bool ValidateDatabase()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Checking database settings...");
            Console.WriteLine(Environment.NewLine);
            bool isOk = CheckEmptyDatabase();

            if (isOk)
            {
                Logger.Info("OK: Validations carried out with SUCCESS!");
                Logger.Info($@"OK: User/Connection: {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}.");
            }

            return isOk;
        }

        private bool CheckEmptyDatabase()
        {
            try
            {
                bool emptyDatabase = false;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    string queryString = "SELECT COUNT(1) FROM sys.objects WHERE is_ms_shipped = 0";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        emptyDatabase = reader.GetInt32(0) == 0;
                    }
                    else
                    {
                        throw new Exception($@"Failed to check SYS.OBJECTS in database {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}.");
                    }
                }
                return emptyDatabase;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return false;
            }
        }
    }
}
