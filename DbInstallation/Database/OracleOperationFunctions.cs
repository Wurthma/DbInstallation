using DbInstallation.Interfaces;
using DbInstallation.Util;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.IO;
using static DbInstallation.Enums.EnumDbType;
using static DbInstallation.Enums.EnumOperation;

namespace DbInstallation.Database
{
    public class OracleOperationFunctions : BaseDatabaseOperationFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OracleOperationFunctions(DatabaseProperties databaseProperties)
            : base(databaseProperties)
        {
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
                    string dbNameAndStatus = $@"{DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns} : Database State: {oc.State}";
                    if (dt.Rows.Count == 0)
                    {
                        Logger.Error(Messages.ErrorMessage002(dbNameAndStatus));
                        return false;
                    }
                    Logger.Info(Messages.Message001(dbNameAndStatus));
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
            foreach (string sqlCmd in FileHelper.ListSqlCommands(ProductDbType.Oracle, OperationType.Install))
            {
                using (var oracleConnection = new OracleConnection(ConnectionString))
                {
                    oracleConnection.Open();
                    using (var command = new OracleCommand(sqlCmd) { Connection = oracleConnection })
                    {
                        try
                        {
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, Messages.ErrorMessage010(sqlCmd));
                        }
                    }
                }
            }
            return true;
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
            bool isOk = CheckEmptyDatabase() && 
                        ValidateDatabaseUser() && 
                        ValidateTableSpace(DatabaseProperties.TablespaceData) && 
                        ValidateTableSpace(DatabaseProperties.TablespaceIndex) && 
                        ValidateDbmsCryptoAccess();

            if (isOk)
            {
                Logger.Info(Messages.Message003);
                Logger.Info(Messages.Message004(DatabaseProperties.DatabaseUser, DatabaseProperties.ServerOrTns));
                Logger.Info(Messages.Message005(DatabaseProperties.TablespaceData));
                Logger.Info(Messages.Message006(DatabaseProperties.TablespaceIndex));
            }

            return isOk;
        }

        private bool CheckEmptyDatabase()
        {
            try
            {
                bool emptyDatabase = false;
                decimal qtyObjects = 0;
                using (OracleConnection oc = new OracleConnection(ConnectionString))
                {
                    oc.Open();

                    string sql = "select count(1) from USER_OBJECTS WHERE OBJECT_TYPE <>'LOB'";

                    OracleDataAdapter oda = new OracleDataAdapter(sql, oc);
                    DataTable dt = new DataTable();
                    oda.Fill(dt);

                    if (dt.Rows.Count == 1)
                    {
                        qtyObjects = dt.Rows[0].Field<decimal>(0);
                        emptyDatabase = qtyObjects == 0;
                    }
                    else
                    {
                        throw new Exception(Messages.ErrorMessage003("USER_OBJECTS", $@"{DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}"));
                    }
                }
                if (!emptyDatabase) 
                { 
                    Logger.Error(Messages.ErrorMessage009($@"{DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}", qtyObjects)); 
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
            string userDb = DatabaseProperties.DatabaseUser.ToUpper();
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
                        Logger.Error($@"Failed to check tablespace to database {DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}.");
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
                            Logger.Error($@"User '{DatabaseProperties.DatabaseUser}' does not have a grant for DBMS_CRYPTO.");
                            Logger.Error($@"Please run the following command as a SYS user:");
                            Logger.Error($@"grant execute on DBMS_CRYPTO to {DatabaseProperties.DatabaseUser};");
                        }
                    }
                    else
                    {
                        Logger.Error($@"Failed to check access to DBMS_CRYPTO | {DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}.");
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
