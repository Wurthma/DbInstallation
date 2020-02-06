using NLog;
using System;
using static DbInstallation.Enums.EnumDbType;
using static DbInstallation.Enums.EnumOperation;

namespace DbInstallation.Database
{
    public class ProductDatabase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isConnectionDefined;

        public ProductDatabase()
        {
            _isConnectionDefined = false;
        }

        private OracleOperationFunctions _oracleOperationFunctions { get; set; }

        private SqlServerOperationFunctions _sqlServerOperationFunctions { get; set; }

        public bool SetConnection(ProductDbType dbType)
        {
            if(dbType == ProductDbType.Oracle)
            {
                return SetOracleConnection();
            }
            else if(dbType == ProductDbType.SqlServer)
            {
                return SetSqlServerConnection();
            }
            else
            {
                throw new InvalidOperationException("Invalid Database type option.");
            }
        }

        public ProductDbType GetDatabaseType(string dbType)
        {
            Console.Write(Environment.NewLine);
            if (dbType.ToUpper() == "O")
            {
                Logger.Info("Selected Oracle Database");
                Console.Write(Environment.NewLine);
                return ProductDbType.Oracle;
            }
            else if (dbType.ToUpper() == "S")
            {
                Logger.Info("Selected SQL Server Database");
                Console.Write(Environment.NewLine);
                return ProductDbType.SqlServer;
            }
            return ProductDbType.None;
        }

        public OperationType GetOperationType(string operation)
        {
            if (operation.ToUpper() == "I")
            {
                Logger.Info("Selected Install operation.");
                return OperationType.Install;
            }
            else if (operation.ToUpper() == "U")
            {
                Logger.Info("Selected Update operation.");
                return OperationType.Update;
            }
            return OperationType.None;
        }

        public void StartDatabaseOperation(ProductDbType dbType, OperationType operationType)
        {
            if (_isConnectionDefined)
            {
                if (operationType == OperationType.Install)
                {
                    InstallDatabase(dbType);
                }
                else if (operationType == OperationType.Update)
                {
                    UpdateDatabase(dbType);
                }
                else
                {
                    throw new InvalidOperationException("Invalid Database operation.");
                }
            }
            else
            {
                throw new Exception("Database connection was not defined.");
            }
        }

        private void InstallDatabase(ProductDbType dbType)
        {
            if(dbType == ProductDbType.Oracle)
            {
                _oracleOperationFunctions.Install();
            }
            else if (dbType == ProductDbType.SqlServer)
            {
                //_sqlServerOperationFunctions.CheckDatabaseInstall(); //TODO:
            }
        }

        private void UpdateDatabase(ProductDbType dbType)
        {
            throw new NotImplementedException(); //TODO:
        }

        private bool SetOracleConnection()
        {
            DatabaseProperties databaseProperties = RequestDbInputsProperties();
            Console.WriteLine("Enter data TABLESPACE:");
            string tablespaceData = Console.ReadLine();
            Console.WriteLine("Enter index TABLESPACE:");
            string tablespaceIndex = Console.ReadLine();

            OracleOperationFunctions oracleOperationFunctions = new OracleOperationFunctions(
                databaseProperties,
                tablespaceData,
                tablespaceIndex);

            try
            {
                _isConnectionDefined = oracleOperationFunctions.TestConnection();
                _oracleOperationFunctions = oracleOperationFunctions;
                return _isConnectionDefined;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }

        private bool SetSqlServerConnection()
        {
            //TODO: criar opção para escolher Trusted connection (windows)
            DatabaseProperties databaseProperties = RequestDbInputsProperties();
            Console.WriteLine("Enter Sql Server Database Name:");
            string databaseName = Console.ReadLine();
            
            SqlServerOperationFunctions sqlServerOperationFunctions = new SqlServerOperationFunctions(databaseProperties, databaseName);

            try
            {
                _isConnectionDefined = sqlServerOperationFunctions.TestConnection();
                _sqlServerOperationFunctions = sqlServerOperationFunctions;
                return _isConnectionDefined;
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
            Console.WriteLine("Enter the Server or TNS:");
            string tnsConnection = Console.ReadLine();

            return new DatabaseProperties(dbUser, dbPassword, tnsConnection);
        }
    }
}
