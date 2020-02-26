using DbInstallation.Enums;
using DbInstallation.Interfaces;
using DbInstallation.Util;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            List<string> folderList = FileHelper.ListFolders(ProductDbType.SqlServer, OperationType.Install);

            if (!CheckEmptyDatabase())
            {
                return false;
            }

            return ExecuteDatabaseCommands(folderList);
        }

        public bool Update(int version)
        {
            int currentVersion = GetDatabaseCurrentVersion(Common.GetAppSetting("ProjectDescription"));
            List<string> folderList = FileHelper.ListFolders(ProductDbType.SqlServer, OperationType.Update, currentVersion, version);

            Console.WriteLine();
            Logger.Info(Messages.Message011(DatabaseProperties.DatabaseUser, DatabaseProperties.ServerOrTns, currentVersion));
            Console.WriteLine();

            if (currentVersion >= version)
            {
                Logger.Error(Messages.ErrorMessage016(version, currentVersion));
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
                return false;
            }
        }

        private bool ExecuteDatabaseCommands(List<string> folderList)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand() { Connection = connection })
                {
                    string sqlCmdAux = string.Empty;
                    try
                    {
                        foreach (string folder in folderList)
                        {
                            foreach (string file in FileHelper.ListFiles(folder))
                            {
                                string content = File.ReadAllText(file).Trim();
                                Logger.Info(Messages.Message007(Path.GetFileName(folder), Path.GetFileName(file)));

                                sqlCmdAux = command.CommandText = content;
                                command.CommandType = CommandType.Text;
                                command.ExecuteNonQuery();
                            }

                            if (folderList.Last() == folder)
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
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, Messages.ErrorMessage010(sqlCmdAux));
                        return false;
                    }
                }
            }
            return true;
        }

        private void InsertDatabaseVersion(int databaseVersion, string descricaoProjeto)
        {
            string sqlQuery = @"INSERT INTO TBFR_VERSAO_BANCO (COD_VERSAO_BANCO, DAT_VERSAO, QTD_BLOCO, DES_PROJETO) VALUES (@version, @dateVersion, 0, @projectDescription) ";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (var command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("version", SqlDbType.Int).Value = databaseVersion;
                    command.Parameters.Add("dateVersion", SqlDbType.Date).Value = DateTime.Now;
                    command.Parameters.Add("projectDescription", SqlDbType.VarChar).Value = descricaoProjeto;
                    command.ExecuteNonQuery();
                }
            }
        }

        private int GetDatabaseCurrentVersion(string projectDescription)
        {
            string sqlQuery = @"SELECT MAX(COD_VERSAO_BANCO) FROM TBFR_VERSAO_BANCO WHERE DES_PROJETO = @projectDescription ";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (var command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open();
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add("projectDescription", SqlDbType.VarChar).Value = projectDescription;
                        SqlDataReader dataReader = command.ExecuteReader();

                        if (dataReader.Read())
                        {
                            return Convert.ToInt32(dataReader.GetDecimal(0));
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

        private bool ValidateDatabase()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Messages.Message002);
            Console.WriteLine(Environment.NewLine);

            bool isOk = true; //Se for necessário validaões adicionar nessa etapa

            if (isOk)
            {
                Logger.Info(Messages.Message003);
                Logger.Info($@"OK: User/Connection: {DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}.");
            }

            return isOk;
        }

        public bool CheckEmptyDatabase()
        {
            try
            {
                bool emptyDatabase = false;
                int qtyObjects = 0;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                { 
                    string queryString = "SELECT COUNT(1) FROM sys.objects WHERE is_ms_shipped = 0";
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        qtyObjects = reader.GetInt32(0);
                        emptyDatabase = qtyObjects == 0;
                    }
                    else
                    {
                        throw new Exception(Messages.ErrorMessage003("SYS.OBJECTS", $@"{DatabaseProperties.DatabaseUser}/{DatabaseProperties.ServerOrTns}"));
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
    }
}
