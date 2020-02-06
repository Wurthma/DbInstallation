using DbInstallation.Interfaces;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public class OracleOperationFunctions : BaseDatabaseOperationFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string TablespaceData { get; private set; }

        public string TablespaceIndex { get; private set; }

        public OracleOperationFunctions(DatabaseProperties databaseProperties, string tablespaceData, string tablespaceIndex)
            : base(databaseProperties)
        {
            TablespaceData = tablespaceData.ToUpper();
            TablespaceIndex = tablespaceIndex.ToUpper();
            base.SetConnectionString(ProductConnectionString.GetConnectionString(databaseProperties, ProductDbType.Oracle));
        }

        public bool TestConnection()
        {
            try
            {
                using (OracleConnection oc = new OracleConnection(ConnectionString))
                {
                    oc.Open();

                    string sql = "SELECT 1 FROM DUAL";

                    OracleDataAdapter oda = new OracleDataAdapter(sql, oc);
                    DataTable dt = new DataTable();
                    oda.Fill(dt);
                    string dbNameAndStatus = $@"{DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns} : Database State: {oc.State}";
                    if (dt.Rows.Count == 0)
                    {
                        Logger.Info($@"Failed to execute basic instruction after connecting to the database {dbNameAndStatus}");
                        return false;
                    }
                    Logger.Info($@"Connection successfully made to the database {dbNameAndStatus}");
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
            bool isOk = CheckEmptyDatabase() && 
                        ValidateDatabaseUser() && 
                        ValidateTableSpace(TablespaceData) && 
                        ValidateTableSpace(TablespaceIndex) && 
                        ValidateDbmsCryptoAccess();

            if (isOk)
            {
                Logger.Info("OK: Validations carried out with SUCCESS!");
                Logger.Info($@"OK: User/Connection: {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}.");
                Logger.Info($@"OK: Data tablespace {TablespaceData}.");
                Logger.Info($@"OK: Index tablespace {TablespaceIndex}.");
            }

            return isOk;
        }

        private bool CheckEmptyDatabase()
        {
            try
            {
                bool emptyDatabase = false;
                using (OracleConnection oc = new OracleConnection(ConnectionString))
                {
                    oc.Open();

                    string sql = "select count(1) from USER_OBJECTS WHERE OBJECT_TYPE <>'LOB'";

                    OracleDataAdapter oda = new OracleDataAdapter(sql, oc);
                    DataTable dt = new DataTable();
                    oda.Fill(dt);

                    if (dt.Rows.Count == 1)
                    {
                        emptyDatabase = dt.Rows[0].Field<decimal>(0) == 0;
                    }
                    else
                    {
                        throw new Exception($@"Failed to check USER_OBJECTS in database {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}.");
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

        private bool ValidateDatabaseUser()
        {
            string userDb = DatabaseProperties.DataBaseUser.ToUpper();
            if (userDb != "SYS" && userDb != "SYSTEM")
            {
                return true;
            }
            else
            {
                Logger.Error($"Owner user cannot be {userDb}");
                return false;
            }
        }

        private bool ValidateTableSpace(string tablespace)
        {
            try
            {
                bool validTablespace = false;
                using (OracleConnection oc = new OracleConnection(ConnectionString))
                {
                    oc.Open();

                    OracleCommand command = oc.CreateCommand();
                    command.BindByName = true;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT COUNT(1) FROM USER_TABLESPACES WHERE TABLESPACE_NAME = :td";
                    command.Parameters.Add("td", tablespace);

                    OracleDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        validTablespace = reader.GetInt32(0) == 1;
                        if (!validTablespace)
                            Logger.Error($@"Invalid tablespace {tablespace}.");
                    }
                    else
                    {
                        Logger.Error($@"Failed to check tablespace to database {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}.");
                        validTablespace = false;
                    }
                }
                return validTablespace;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return false;
            }
        }

        private bool ValidateDbmsCryptoAccess()
        {
            try
            {
                bool hasAccess = false;
                using (OracleConnection oc = new OracleConnection(ConnectionString))
                {
                    oc.Open();

                    OracleCommand command = oc.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT COUNT(1) FROM USER_TAB_PRIVS WHERE TABLE_NAME='DBMS_CRYPTO' and PRIVILEGE='EXECUTE'";

                    OracleDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        hasAccess = reader.GetInt32(0) == 1;
                        if (!hasAccess)
                        {
                            Logger.Error($@"User '{DatabaseProperties.DataBaseUser}' does not have a grant for DBMS_CRYPTO.");
                            Logger.Error($@"Please run the following command as a SYS user:");
                            Logger.Error($@"grant execute on DBMS_CRYPTO to {DatabaseProperties.DataBaseUser};");
                        }
                    }
                    else
                    {
                        Logger.Error($@"Failed to check access to DBMS_CRYPTO | {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}.");
                        hasAccess = false;
                    }
                }
                return hasAccess;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return false;
            }
        }
    }
}
