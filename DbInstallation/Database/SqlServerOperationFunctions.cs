using DbInstallation.Enums;
using DbInstallation.Interfaces;
using NLog;
using System;
using System.Data.SqlClient;

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
                            Logger.Info(Messages.Message001($@"{DatabaseProperties.ServerOrTns}/{DatabaseProperties.DatabaseName}"));
                        else
                            return false;
                    }
                    else
                    {
                        Logger.Error(Messages.ErrorMessage002($@"{DatabaseProperties.ServerOrTns}/{DatabaseProperties.DatabaseName}"));
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
            Console.WriteLine(Messages.Message002);
            Console.WriteLine(Environment.NewLine);
            bool isOk = CheckEmptyDatabase();

            if (isOk)
            {
                Logger.Info(Messages.Message003);
                Logger.Info($@"OK: User/Connection: {DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}.");
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
                        throw new Exception(Messages.ErrorMessage003("SYS.OBJECTS", $@"{DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}"));
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
