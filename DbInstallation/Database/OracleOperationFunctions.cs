using DbInstallation.Enums;
using DbInstallation.Interfaces;
using DbInstallation.Util;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbInstallation.Database
{
    public class OracleOperationFunctions : BaseDatabaseOperationFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger IntegrityLogger = LogManager.GetLogger("integrity");
        private static readonly Logger integrityNlsParametersLogger = LogManager.GetLogger("integrityNlsParameters");
        
        private static int RegexTimeout { get => 40000; }

        private class DatabaseObjectIntegrity
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string NonconformityType { get; set; }
        }

        private class IntegrityHash
        {
            public decimal ObjectSystemId { get; set; }
            public string ObjectName { private get; set; }
            public string ObjectType { private get; set; }
            public string VersionAlteredObject { private get; set; }
            public DateTime CreationDate { private get; set; }
            public string ObjectHash { private get; set; }
            public string Flag { private get; set; }

            public string GetObjectName() => GetStringProperty(ObjectName);

            public string GetObjectType() => GetStringProperty(ObjectType);

            public string GetVersionAlteredObject() => GetStringProperty(VersionAlteredObject);

            public string GetCreationDate() => 
                $@"to_date('{CreationDate.Day.ToString("00")}-{CreationDate.Month.ToString("00")}-{CreationDate.Year} {CreationDate.Hour.ToString("00")}:{CreationDate.Minute.ToString("00")}:{CreationDate.Second.ToString("00")}', 'dd-mm-yyyy hh24:mi:ss')";

            public string GetObjectHash() => GetStringProperty(ObjectHash);

            public string GetFlag() => GetStringProperty(Flag);

            private string GetStringProperty(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return "null";
                else
                    return $@"'{value}'";
            }
        }

        public OracleOperationFunctions(DatabaseProperties databaseProperties)
            : base(databaseProperties)
        {
            base.SetConnectionString(ProductConnectionString.GetConnectionString(databaseProperties, ProductDbType.Oracle));
            Environment.SetEnvironmentVariable("nls_lang", "AMERICAN_AMERICA.WE8MSWIN1252");
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
            if (!CheckEmptyDatabase())
            {
                Logger.Error(Messages.ErrorMessage009($@"{DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}"));
                Environment.ExitCode = -1;
                return false;
            }

            List<string> folderList = FileHelper.ListFolders(ProductDbType.Oracle, OperationType.Install);

            return ExecuteDatabaseCommands(folderList);
        }

        public bool Update(int version)
        {
            if (CheckEmptyDatabase())
            {
                Logger.Error(Messages.ErrorMessage024(DatabaseProperties.DatabaseUser, version));
                Environment.ExitCode = -1;
                return false;
            }

            int currentVersion = GetDatabaseCurrentVersion(Common.GetAppSetting("ProjectDescription"));
            List<string> folderList = FileHelper.ListFolders(ProductDbType.Oracle, OperationType.Update, currentVersion, version);
            
            Console.WriteLine();
            Logger.Info(Messages.Message011(DatabaseProperties.DatabaseUser, DatabaseProperties.ServerOrTns, currentVersion));
            Console.WriteLine();

            if(currentVersion >= version)
            {
                Logger.Error(Messages.ErrorMessage016(version, currentVersion));
                Environment.ExitCode = -1;
                return false;
            }

            bool executedWithSuccess = ExecuteDatabaseCommands(folderList);

            if (executedWithSuccess)
            {
                return true;
            }
            else
            {
                Console.WriteLine();
                Logger.Error(Messages.ErrorMessage021);
                Logger.Error(Messages.ErrorMessage022);
                Console.WriteLine();
                Environment.ExitCode = -1;
                return false;
            }
        }

        private bool ExecuteDatabaseCommands(List<string> folderList)
        {
            using (var oracleConnection = new OracleConnection(ConnectionString))
            {
                oracleConnection.Open();
                using (var command = new OracleCommand() { Connection = oracleConnection })
                {
                    SetOracleNlsLengthSemantics(command);
                    string sqlCmdAux = string.Empty;
                    try
                    {
                        foreach (string folder in folderList)
                        {
                            foreach (string file in FileHelper.ListFiles(folder))
                            {
                                Logger.Info(Messages.Message007(Path.GetFileName(folder), Path.GetFileName(file)));
                                foreach (string sqlCmd in ListSqlCommandsFromFile(file))
                                {
                                    sqlCmdAux = command.CommandText = ReplaceDatabaseProperties(sqlCmd);
                                    command.CommandType = CommandType.Text;
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex, Messages.ErrorMessage010(sqlCmdAux));
                                        if (Common.ContinueExecutionOnScriptError())
                                        {
                                            Console.WriteLine(Messages.ErrorMessage026);
                                            Console.ReadLine();
                                            continue;
                                        }
                                        else
                                        {
                                            Environment.ExitCode = -1;
                                            return false;
                                        }
                                    }
                                }
                            }

                            if(folderList.Last() == folder)
                            {
                                //Se for a execução de uma atualização, a cada versão concluída, inserir no BD
                                if (FileHelper.GetVersionFromDirectoryPath(folder, out int finishedVersion))
                                {
                                    InsertDatabaseVersion(finishedVersion, Common.GetAppSetting("ProjectDescription"));
                                }
                            }
                        }
                        Console.WriteLine();
                        Logger.Info(Messages.Message008);
                        Console.WriteLine();
                        ValidateDatabaseInstallation();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return true;
        }

        private void SetOracleNlsLengthSemantics(OracleCommand command)
        {
            string sqlCmd = $@"declare
                                v_s_CharacterSet VARCHAR2(160);
                            begin
                                select VALUE
                                into v_s_CharacterSet
                                from NLS_DATABASE_PARAMETERS
                                where PARAMETER = 'NLS_CHARACTERSET';
                                if v_s_CharacterSet in ('UTF8', 'AL32UTF8') then
                                    execute immediate 'alter session set nls_length_semantics=char';
                                end if;
                            end;";

            command.CommandText = ReplaceDatabaseProperties(sqlCmd);
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
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

        private bool LoadDatabaseIntegrityHash(bool generate = false)
        {
            if (IsOracleIntegrityValidationEnable())
            {
                using (var oracleConnection = new OracleConnection(ConnectionString))
                {
                    Environment.SetEnvironmentVariable("nls_lang", "AMERICAN_AMERICA.WE8MSWIN1252");
                    oracleConnection.Open();
                    using (var command = new OracleCommand() { Connection = oracleConnection })
                    {
                        string sqlCmdAux = string.Empty;
                        try
                        {
                            command.CommandText = "PRC_CMP_CARREGA_INTEGRID_HASH";
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("pi_s_schema_owner", OracleDbType.Varchar2, ParameterDirection.Input).Value = DatabaseProperties.DatabaseUser;
                            if (generate)
                            {
                                command.Parameters.Add("pi_s_operacao", OracleDbType.Varchar2, ParameterDirection.Input).Value = "GENERATE";
                            }
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, Messages.ErrorMessage010(sqlCmdAux));
                        }
                    }
                }
                if (!generate)
                {
                    return GenerateIntegrityLog();
                }
                return true;
            }
            else
            {
                Console.WriteLine();
                IntegrityLogger.Warn(Messages.Message016);
                Console.WriteLine();
            }
            return false;
        }

        public bool GenerateIntegrityLog()
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
                            Environment.ExitCode = -1;
                        }
                    }
                }
                return listDatabaseObjectIntegrity;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                Environment.ExitCode = -1;
                return listDatabaseObjectIntegrity;
            }
        }

        private void LogNlsCharacterSetParameters()
        {
            Dictionary<string, string> nlsDatabaseParameters = GetNlsCharacterSetParameters();

            integrityNlsParametersLogger.Info($@"ENVIROMENT VARIABLE NLS_LANG: {Environment.GetEnvironmentVariable("nls_lang", EnvironmentVariableTarget.Process)}");
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
                            Environment.ExitCode = -1;
                        }
                    }
                }
                return nlsDatabaseParameters;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                Environment.ExitCode = -1;
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
                
                return emptyDatabase;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                Environment.ExitCode = -1;
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
                Environment.ExitCode = -1;
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
                        {
                            Logger.Error($@"Invalid tablespace {tablespace}.");
                            Environment.ExitCode = -1;
                        }
                    }
                    else
                    {
                        Logger.Error($@"Failed to check tablespace to database {DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}.");
                        validTablespace = false;
                        Environment.ExitCode = -1;
                    }
                }
                return validTablespace;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                Environment.ExitCode = -1;
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
                Environment.ExitCode = -1;
                return false;
            }
        }

        private static Regex GetRegexPattern(string fileName, string fileContent)
        {
            Regex regex = new Regex(@"(?<cmd>[\s\S.]+?);\s*[\n\r]", RegexOptions.None, TimeSpan.FromMilliseconds(RegexTimeout));
            if (IsPlSqlFunctionProceduresPackageTriggerView(fileName))
            {
                regex = new Regex(@"(?<cmd>[\s\S.]+?;)\s*\/[\n\r]", RegexOptions.None, TimeSpan.FromMilliseconds(RegexTimeout));
            }
            else if (IsPlatypusSqlCommand(fileName) || ContainExplicitDefinitionForPlSqlCommand(fileContent))
            {
                regex = new Regex(@"(?<cmd>[^\s\/][\s\S.]+?;)\s*\/", RegexOptions.None, TimeSpan.FromMilliseconds(RegexTimeout));
            }
            return regex;
        }

        private static bool ContainExplicitDefinitionForPlSqlCommand(string fileContent) =>
            fileContent.Contains(Common.GetAppSetting("ExplicitSetPlSqlCommand"));

        private static bool IsPlSqlFunctionProceduresPackageTriggerView(string file) =>
            file.ToUpper().Contains(@"\FUNCTIONS\") ||
            file.ToUpper().Contains(@"\PROCEDURE\") ||
            file.ToUpper().Contains(@"\PACKAGE\") ||
            file.ToUpper().Contains(@"\TRIGGER\") ||
            file.ToUpper().Contains(@"\VIEW\");

        private static List<string> ListSqlCommandsFromFile(string filePath)
        {
            List<string> sqlCommandList = new List<string>();
            var content = File.ReadAllText(filePath) + Environment.NewLine; //Adiciona nova linha ao final do conteúdo para o funcionamento correto do regex

            var sw = Stopwatch.StartNew();
            try
            {
                if (content.Contains(Common.GetAppSetting("ExplicitSetScriptLoadData")))
                {
                    sqlCommandList.Add($@"BEGIN{Environment.NewLine}{content}{Environment.NewLine}END;");
                    return sqlCommandList;
                }

                Regex regex = GetRegexPattern(filePath, content);
                MatchCollection matchCollection = regex.Matches(content);

                foreach (Match match in matchCollection)
                {
                    string sqlCommand = match.Groups["cmd"].Value.Trim();
                    if (!IsComment(sqlCommand))
                    {
                        sqlCommandList.Add(sqlCommand);
                    }
                }
                sw.Stop();
                return sqlCommandList;
            }
            catch (RegexMatchTimeoutException ex)
            {
                sw.Stop();
                Logger.Fatal(ex, ex.Message);
                throw;
            }
        }

        private static bool IsComment(string sqlCommand)
        {
            string fullCommand = string.Empty;
            if (sqlCommand.StartsWith("--"))
            {
                Regex regex = new Regex(@"--.+(?<cmd>[\s\S.]*)", RegexOptions.None, TimeSpan.FromMilliseconds(RegexTimeout));
                MatchCollection matchCollection = regex.Matches(sqlCommand);
                foreach (Match match in matchCollection)
                {
                    fullCommand += match.Groups["cmd"].Value.Trim();
                }
                return string.IsNullOrEmpty(fullCommand);
            }
            return false;
        }

        private static bool IsOracleIntegrityValidationEnable() =>
            Common.GetAppSetting("OracleIntegrityValidation").ToLower() == "true";

        public void GenerateIntegrityValidation()
        {
            if (IsOracleIntegrityValidationEnable())
            {
                int version = GetDatabaseCurrentVersion(Common.GetAppSetting("ProjectDescription"));
                LoadDatabaseIntegrityHash(true);
                IEnumerable<IntegrityHash> listIntegrityHash = GetDataIntegrityHash();
                string fileContent = $@"--Script data load for Oracle integrity Validation {Environment.NewLine}";
                fileContent += $@"--SCRIPT GENERATED FOR VERSION: {version} {Environment.NewLine}";
                fileContent += $@"--Generation date: {DateTime.Now} {Environment.NewLine}";
                fileContent += $@"--System Variable nls_lang: {Environment.GetEnvironmentVariable("nls_lang", EnvironmentVariableTarget.Process)} {Environment.NewLine}{Environment.NewLine}";
                fileContent += "--Oracle NLS_DATABASE_PARAMETERS:";

                Dictionary<string, string> nlsDatabaseParameters = GetNlsCharacterSetParameters();
                foreach (var item in nlsDatabaseParameters)
                {
                    fileContent += $@"--{item.Key}: {item.Value} {Environment.NewLine}";
                }

                fileContent += $@"{Environment.NewLine}DELETE FROM CMP_VERIFIC_INTEGRIDADE_HASH; {Environment.NewLine}{Environment.NewLine}";

                foreach (IntegrityHash integrityHash in listIntegrityHash)
                {
                    fileContent += "insert into CMP_VERIFIC_INTEGRIDADE_HASH(N_COD_SISTEMA_OBJETO, S_NOME_OBJETO, S_TIPO_OBJETO, S_VERSAO_ALTEROU_OBJETO, D_DATA_CRIACAO, S_HASH_OBJETO, S_FLAG) ";
                    fileContent += $@"values({integrityHash.ObjectSystemId}, {integrityHash.GetObjectName()}, {integrityHash.GetObjectType()}, {integrityHash.GetVersionAlteredObject()}, ";
                    fileContent += $@"{integrityHash.GetCreationDate()}, {integrityHash.GetObjectHash()}, {integrityHash.GetFlag()}); {Environment.NewLine}";
                }

                var date = DateTime.Now;
                FileHelper.CreateSqlScriptFile(ProductDbType.Oracle, $@"COMPARE_V{version}_{date.Year}{date.Month}{date.Day}_{date.Hour}{date.Minute}{date.Second}", fileContent);
            }
            else
            {
                Console.WriteLine();
                IntegrityLogger.Warn(Messages.Message016);
                Console.WriteLine();
            }
        }

        private IEnumerable<IntegrityHash> GetDataIntegrityHash()
        {
            List<IntegrityHash> listIntegrityHash = new List<IntegrityHash>();
            string sqlQuery = "SELECT * FROM CMP_VERIFIC_INTEGRIDADE_HASH";
            
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(ConnectionString))
                {
                    using (var command = new OracleCommand(sqlQuery, oracleConnection))
                    {
                        oracleConnection.Open();
                        command.CommandType = CommandType.Text;
                        OracleDataReader dataReader = command.ExecuteReader();

                        while (dataReader.Read())
                        {
                            listIntegrityHash.Add(new IntegrityHash()
                            {
                                ObjectSystemId = dataReader.GetDecimal("N_COD_SISTEMA_OBJETO"),
                                ObjectName = dataReader.GetString("S_NOME_OBJETO"),
                                ObjectType = dataReader.GetString("S_TIPO_OBJETO"),
                                VersionAlteredObject = dataReader.IsDBNull("S_VERSAO_ALTEROU_OBJETO") ? null : dataReader.GetString("S_VERSAO_ALTEROU_OBJETO"),
                                CreationDate = dataReader.GetDateTime("D_DATA_CRIACAO"),
                                ObjectHash = dataReader.GetString("S_HASH_OBJETO"),
                                Flag = dataReader.IsDBNull("S_FLAG") ? null : dataReader.GetChar("S_FLAG").ToString()
                            });
                        }
                        return listIntegrityHash;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
