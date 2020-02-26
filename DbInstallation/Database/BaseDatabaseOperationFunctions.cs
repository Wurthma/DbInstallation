namespace DbInstallation.Database
{
    public abstract class BaseDatabaseOperationFunctions
    {
        protected BaseDatabaseOperationFunctions(DatabaseProperties databaseProperties)
        {
            DatabaseProperties = databaseProperties;
        }

        public string ConnectionString { get; private set; }

        public DatabaseProperties DatabaseProperties { get; private set; }

        protected void SetConnectionString(string connectionString) => ConnectionString = connectionString;

        protected static bool IsPlatypusSqlCommand(string fileName) =>
            fileName.ToUpper().Contains(@"0-PLATYPUS") ||
            (fileName.ToUpper().Contains(@"2-BFW") && fileName.ToUpper().Contains(@"ESTRUTURA"));
    }
}
