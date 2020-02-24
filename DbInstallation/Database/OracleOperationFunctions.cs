using DbInstallation.Enums;
using DbInstallation.Interfaces;
using DbInstallation.Util;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DbInstallation.Database
{
    public class OracleOperationFunctions : BaseDatabaseOperationFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger IntegrityLogger = LogManager.GetLogger("integrity");
        private static readonly Logger integrityNlsParametersLogger = LogManager.GetLogger("integrityNlsParameters");

        private class DatabaseObjectIntegrity
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string NonconformityType { get; set; }
        }

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
            List<string> folderList = FileHelper.ListFolders(ProductDbType.Oracle, OperationType.Install);

            if (!CheckEmptyDatabase())
            {
                return false;
            }

            return ExecuteDatabaseCommands(folderList);
        }

        public bool Update(int version)
        {
            int currentVersion = GetDatabaseCurrentVersion(Common.GetAppSetting("ProjectDescription"));
            List<string> folderList = FileHelper.ListFolders(ProductDbType.Oracle, OperationType.Update, currentVersion, version);
            
            Console.WriteLine(Environment.NewLine);
            Logger.Info(Messages.Message011(DatabaseProperties.DatabaseUser, DatabaseProperties.ServerOrTns, currentVersion));
            Console.WriteLine(Environment.NewLine);

            if(currentVersion <= version)
            {
                Logger.Error(Messages.ErrorMessage016(version, currentVersion));
                return false;
            }

            return ExecuteDatabaseCommands(folderList);
        }

        private bool ExecuteDatabaseCommands(List<string> folderList)
        {
            using (var oracleConnection = new OracleConnection(ConnectionString))
            {
                oracleConnection.Open();
                using (var command = new OracleCommand() { Connection = oracleConnection })
                {
                    string sqlCmdAux = string.Empty;
                    try
                    {
                        foreach (string folder in folderList)
                        {
                            foreach (string file in FileHelper.ListFiles(folder))
                            {
                                Logger.Info(Messages.Message007(Path.GetFileName(folder), Path.GetFileName(file)));
                                foreach (string sqlCmd in FileHelper.ListSqlCommandsFromFile(file))
                                {
                                    sqlCmdAux = command.CommandText = ReplaceDatabaseProperties(sqlCmd);
                                    command.CommandType = CommandType.Text;
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        Console.WriteLine(Environment.NewLine);
                        Logger.Info(Messages.Message008);
                        ValidateDatabaseInstallation();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, Messages.ErrorMessage010(sqlCmdAux));
                    }
                }
            }
            return true;
        }

        private void ValidateDatabaseInstallation()
        {
            Logger.Info(Messages.Message009);
            LogNlsCharacterSetParameters();
            CompileObjects();
            CompileObjects();
            LoadDatabaseIntegrityHash();
        }

        private void CompileObjects()
        {
            using (var oracleConnection = new OracleConnection(ConnectionString))
            {
                oracleConnection.Open();
                using (var command = new OracleCommand() { Connection = oracleConnection })
                {
                    string sqlCmdAux = string.Empty;
                    try
                    {
                        command.CommandText = "PRC_COMPILA_OBJETO";
                        command.CommandType = CommandType.StoredProcedure;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, Messages.ErrorMessage010(sqlCmdAux));
                    }
                }
            }
        }

        private bool LoadDatabaseIntegrityHash()
        {
            using (var oracleConnection = new OracleConnection(ConnectionString))
            {
                oracleConnection.Open();
                using (var command = new OracleCommand() { Connection = oracleConnection })
                {
                    string sqlCmdAux = string.Empty;
                    try
                    {
                        command.CommandText = "PRC_CMP_CARREGA_INTEGRID_HASH";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("pi_s_schema_owner", OracleDbType.Varchar2, ParameterDirection.Input).Value = DatabaseProperties.DatabaseUser;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, Messages.ErrorMessage010(sqlCmdAux));
                    }
                }
            }
            return GenerateIntegrityLog();
        }

        private bool GenerateIntegrityLog()
        {
            List<DatabaseObjectIntegrity> listDatabaseObjectIntegrity = GetDatabaseIntegrityResult();

            foreach(var objectIntegrity in listDatabaseObjectIntegrity)
            {
                IntegrityLogger.Warn(Messages.ErrorMessage013(listDatabaseObjectIntegrity.Count));
                IntegrityLogger.Warn(Messages.ErrorMessage012(
                    objectIntegrity.Name,
                    objectIntegrity.Type,
                    objectIntegrity.NonconformityType)
                );
            }

            if(listDatabaseObjectIntegrity.Count == 0)
            {
                IntegrityLogger.Info(Messages.Message010);
                return true;
            }

            return false;
        }

        private List<DatabaseObjectIntegrity> GetDatabaseIntegrityResult()
        {
            List<DatabaseObjectIntegrity> listDatabaseObjectIntegrity = new List<DatabaseObjectIntegrity>();

            string sqlQuery = @"SELECT s_tipo_objeto, s_nome_objeto, s_tipo_inconformidade
                            FROM cmp_verific_integridade_obj
                            WHERE id_verificacao =
                                (SELECT MAX(id_verificacao) FROM cmp_verific_integridade vi)
                            ORDER BY s_tipo_objeto, s_tipo_inconformidade ";

            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(ConnectionString))
                {
                    using (var command = new OracleCommand() { Connection = oracleConnection })
                    {
                        try
                        {
                            oracleConnection.Open();
                            command.CommandText = sqlQuery;
                            command.CommandType = CommandType.Text;
                            OracleDataReader dataReader = command.ExecuteReader();

                            while (dataReader.Read())
                            {
                                DatabaseObjectIntegrity databaseObjectIntegrity = new DatabaseObjectIntegrity()
                                {
                                    Type = dataReader.GetString("s_tipo_objeto"),
                                    Name = dataReader.GetString("s_nome_objeto"),
                                    NonconformityType = dataReader.GetString("s_tipo_inconformidade")
                                };
                                listDatabaseObjectIntegrity.Add(databaseObjectIntegrity);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, Messages.ErrorMessage011);
                        }
                    }
                }
                return listDatabaseObjectIntegrity;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return listDatabaseObjectIntegrity;
            }
        }

        private void LogNlsCharacterSetParameters()
        {
            Dictionary<string, string> nlsDatabaseParameters = GetNlsCharacterSetParameters();

            foreach(var item in nlsDatabaseParameters)
            {
                integrityNlsParametersLogger.Info($@"PARAMETER: {item.Key} | VALUE: {item.Value}");
            }
        }

        private Dictionary<string, string> GetNlsCharacterSetParameters()
        {
            Dictionary<string, string> nlsDatabaseParameters = new Dictionary<string, string>();
            string sqlQuery = @"SELECT * FROM NLS_DATABASE_PARAMETERS ";

            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(ConnectionString))
                {
                    using (var command = new OracleCommand(sqlQuery, oracleConnection))
                    {
                        try
                        {
                            oracleConnection.Open();
                            command.CommandType = CommandType.Text;
                            OracleDataReader dataReader = command.ExecuteReader();

                            while (dataReader.Read())
                            {
                                nlsDatabaseParameters.Add(dataReader.GetString("PARAMETER"), dataReader.GetString("VALUE"));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, Messages.ErrorMessage011);
                        }
                    }
                }
                return nlsDatabaseParameters;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return nlsDatabaseParameters;
            }
        }

        private int GetDatabaseCurrentVersion(string projectDescription)
        {
            string sqlQuery = @"SELECT MAX(COD_VERSAO_BANCO) FROM TBFR_VERSAO_BANCO WHERE DES_PROJETO = :projectDescription ";

            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(ConnectionString))
                {
                    using (var command = new OracleCommand(sqlQuery, oracleConnection))
                    {
                        oracleConnection.Open();
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add("projectDescription", OracleDbType.Varchar2).Value = projectDescription;
                        OracleDataReader dataReader = command.ExecuteReader();

                        if (dataReader.Read())
                        {
                            return dataReader.GetInt32(0);
                        }
                        throw new Exception(Messages.ErrorMessage015);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InsertDatabaseVersion(int databaseVersion, string descricaoProjeto)
        {
            string sqlQuery = @"INSERT INTO TBFR_VERSAO_BANCO (COD_VERSAO_BANCO, DAT_VERSAO, QTD_BLOCO, DES_PROJETO) VALUES (:version, :dateVersion, 0, :projectDescription) ";

            using (OracleConnection oracleConnection = new OracleConnection(ConnectionString))
            {
                using (var command = new OracleCommand(sqlQuery, oracleConnection))
                {
                    oracleConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("version", OracleDbType.Int32).Value = databaseVersion;
                    command.Parameters.Add("dateVersion", OracleDbType.Date).Value = DateTime.Now;
                    command.Parameters.Add("projectDescription", OracleDbType.Varchar2).Value = descricaoProjeto;
                    command.ExecuteNonQuery();
                }
            }
        }

        private string ReplaceDatabaseProperties(string sqlCommand) => 
            sqlCommand.Replace("'&OWNBFW'", "'&OWN'")
            .Replace("&OWNBFW..", "&OWN.")
            .Replace("&OWN..", "&OWN.")
            .Replace("&OWN", DatabaseProperties.DatabaseUser)
            .Replace("&&TD", "&TD")
            .Replace("&TD", DatabaseProperties.TablespaceData)
            .Replace("&&TI", "&TI")
            .Replace("&TI", DatabaseProperties.TablespaceIndex);

        private bool ValidateDatabase() 
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Messages.Message002);
            Console.WriteLine(Environment.NewLine);

            bool isOk = ValidateDatabaseUser() && 
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

        public bool CheckEmptyDatabase()
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

        public bool ValidateUpdateVersion()
        {
            throw new NotImplementedException();
        }
    }
}
