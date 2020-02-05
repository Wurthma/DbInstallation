using DbInstallation.Interfaces;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public class OracleInstallationFunctions : BaseInstallationDbFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string TablespaceData { get; private set; }

        public string TablespaceIndex { get; private set; }

        public OracleInstallationFunctions(DatabaseProperties databaseProperties, string tablespaceData, string tablespaceIndex)
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
                using (OracleConnection oc = new OracleConnection())
                {
                    oc.ConnectionString = ConnectionString;
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
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return false;
            }
        }

        public bool CheckDatabaseInstall() =>
            CheckEmptyDatabase() && ValidateDatabaseUser() && ValidateTableSpace(TablespaceData) && ValidateTableSpace(TablespaceIndex);

        private bool CheckEmptyDatabase()
        {
            try
            {
                bool emptyDatabase = false;
                using (OracleConnection oc = new OracleConnection())
                {
                    oc.ConnectionString = ConnectionString;
                    oc.Open();

                    string sql = "select count(1) from USER_OBJECTS WHERE OBJECT_TYPE <>'LOB'";

                    OracleDataAdapter oda = new OracleDataAdapter(sql, oc);
                    DataTable dt = new DataTable();
                    oda.Fill(dt);

                    if (dt.Rows.Count == 1)
                    {
                        emptyDatabase = dt.Rows[0].Field<int>(0) == 0;
                    }
                    else
                    {
                        Logger.Error($@"Failed to check USER_OBJECTS in database {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}");
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
                using (OracleConnection oc = new OracleConnection())
                {
                    oc.ConnectionString = ConnectionString;
                    oc.Open();

                    OracleCommand command = oc.CreateCommand();
                    command.BindByName = true;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT COUNT(1) FROM USER_TABLESPACES WHERE TABLESPACE_NAME = :td";
                    command.Parameters.Add("td", tablespace);

                    OracleDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        validTablespace = reader.GetInt32(1) == 1;
                        if(!validTablespace)
                            Logger.Error($@"Invalid tablespace {tablespace}");
                    }
                    else
                    {
                        Logger.Error($@"Failed to check tablespace to database {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns}");
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
    }
}
