using DbInstallation.Interfaces;
using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

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
            TablespaceData = tablespaceData;
            TablespaceIndex = tablespaceIndex;
            base.SetConnectionString($@"User ID={databaseProperties.DataBaseUser}; Password={databaseProperties.DatabasePassword}; Data Source={databaseProperties.ServerOrTns};");
        }

        public bool TestConnection()
        {
            using (OracleConnection oc = new OracleConnection())
            {
                oc.ConnectionString = ConnectionString;
                oc.Open();

                Logger.Info($@"Connection successfully made to the database {DatabaseProperties.DataBaseUser}/{DatabaseProperties.ServerOrTns} : Database State: {oc.State}");

                //TODO: realizar testes básicos com os dados inseridos pelo usuário
                string sql = "SELECT 1 FROM DUAL";

                OracleDataAdapter oda = new OracleDataAdapter(sql, oc);
                DataTable dt = new DataTable();
                oda.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    Console.WriteLine("Conectado...");
                }
            }
            return true;
        }
    }
}
