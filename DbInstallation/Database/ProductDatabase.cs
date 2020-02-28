using DbInstallation.Enums;
using DbInstallation.Interfaces;
using DbInstallation.Util;
using NLog;
using System;

namespace DbInstallation.Database
{
    public class ProductDatabase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isConnectionDefined;

        private IDatabaseFunctions DatabaseFunctions { get; set; }

        public ProductDatabase()
        {
            _isConnectionDefined = false;
        }

        public bool SetConnection(ProductDbType dbType)
        {
            
            if(dbType == ProductDbType.None)
            {
                throw new InvalidOperationException(Messages.ErrorMessage007);
            }
            else
            {
                return Connect(dbType);
            }
        }

        public bool SetConnection(ProductDbType dbType, DatabaseProperties databaseProperties) => 
            Connect(dbType, databaseProperties);

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

        public void StartDatabaseOperation(OperationType operationType)
        {
            if (_isConnectionDefined)
            {
                if (operationType == OperationType.Install)
                {
                    DatabaseFunctions.Install();
                }
                else if (operationType == OperationType.Update)
                {
                    int versionToInstall = Convert.ToInt32(Common.GetAppSetting("VersionToInstall"));
                    Console.WriteLine();
                    Logger.Info(Messages.Message014(versionToInstall));
                    Console.WriteLine();
                    DatabaseFunctions.Update(versionToInstall);
                }
                else
                {
                    throw new InvalidOperationException(Messages.ErrorMessage005);
                }
            }
            else
            {
                throw new Exception(Messages.ErrorMessage004);
            }
        }

        private bool Connect(ProductDbType dbType)
        {
            DatabaseProperties databaseProperties = RequestDbInputsProperties(dbType);
            return Connect(dbType, databaseProperties);
        }

        private bool Connect(ProductDbType dbType, DatabaseProperties databaseProperties)
        {
            IDatabaseFunctions databaseFunctions;
            if (dbType == ProductDbType.Oracle)
            {
                databaseFunctions = new OracleOperationFunctions(databaseProperties);
            }
            else
            {
                databaseFunctions = new SqlServerOperationFunctions(databaseProperties);
            }

            try
            {
                _isConnectionDefined = databaseFunctions.TestConnection();
                DatabaseFunctions = databaseFunctions;
                return _isConnectionDefined;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }

        private DatabaseProperties RequestDbInputsProperties(ProductDbType dbType)
        {
            string dbUser = null;
            string dbPassword = null;

            if (dbType == ProductDbType.SqlServer)
            {
                bool isTrustedConnection = RequestSqlServerTrustedConnection();
                Console.WriteLine("Enter Sql Server Database Name:");
                string databaseName = Console.ReadLine();
                
                if (!isTrustedConnection)
                {
                    dbUser = RequestDatabaseUser();
                    dbPassword = RequestDatabasePassword();
                }

                Console.WriteLine("Enter the Server name:");
                string serverName = Console.ReadLine();

                return new DatabaseProperties(dbUser, dbPassword, serverName, databaseName, isTrustedConnection);
            }
            else if(dbType == ProductDbType.Oracle)
            {
                dbUser = RequestDatabaseUser();
                dbPassword = RequestDatabasePassword();

                Console.WriteLine("Enter the TNS connection string:");
                string tnsOrServerConnection = Console.ReadLine();
                Console.WriteLine("Enter data TABLESPACE:");
                string tablespaceData = Console.ReadLine().ToUpper();
                Console.WriteLine("Enter index TABLESPACE:");
                string tablespaceIndex = Console.ReadLine().ToUpper();

                return new DatabaseProperties(dbUser, dbPassword, tnsOrServerConnection, tablespaceData, tablespaceIndex);
            }
            else
            {
                throw new Exception(Messages.ErrorMessage006);
            }
        }

        private string RequestDatabaseUser()
        {
            Console.WriteLine("Enter database user:");
            return Console.ReadLine();
        }

        private string RequestDatabasePassword()
        {
            Console.WriteLine("Enter the password:");
            return Console.ReadLine();
        }

        private bool RequestSqlServerTrustedConnection()
        {
            Console.WriteLine("Choose authenticationi type:");
            Console.WriteLine("(W) Windows");
            Console.WriteLine("(U) Sql Server User");
            string authType = Console.ReadLine();

            if (authType.ToUpper() != "W" && authType.ToUpper() != "U")
            {
                throw new Exception($@"Invalid Sql Server type connection '{authType}'.");
            }
            return authType.ToUpper() == "W";
        }
    }
}
