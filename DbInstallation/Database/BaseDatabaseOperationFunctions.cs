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
    }
}
