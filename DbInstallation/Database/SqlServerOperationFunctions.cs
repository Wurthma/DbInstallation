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

        public string DatabaseName { get; private set; }

        public SqlServerOperationFunctions(DatabaseProperties databaseProperties, string databaseName)
            : base(databaseProperties)
        {
            DatabaseName = databaseName;
            SetConnectionString(ProductConnectionString.GetConnectionString(databaseProperties, ProductDbType.SqlServer));
        }

        public bool TestConnection()
        {
            bool isOk = false;
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
                        isOk = reader.GetInt32(0) == 1;
                        if(isOk)
                            Logger.Info($@"Connection successfully made to the database {DatabaseProperties.ServerOrTns}/{DatabaseName}.");
                    }
                    else
                    {
                        Logger.Error($@"Failed to execute basic instruction after connecting to the database {DatabaseProperties.ServerOrTns}/{DatabaseName}.");
                        isOk = false;
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
            throw new NotImplementedException();
        }

        private bool CheckEmptyDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
