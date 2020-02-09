using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public static class ProductConnectionString
    {
        private static string _oracleConnectionString
        {
            get
            {
                return "User ID={0}; Password={1}; Data Source={2};";
            }
        }

        private static string _sqlServerConnectionString
        {
            get
            {
                return "Server={0};Database={1};User Id={2};Password={3};";
            }
        }

        private static string _sqlServerTrustedConnectionString
        {
            get
            {
                return "Server={0};Database={1};Trusted_Connection=True;";
            }
        }

        public static string GetConnectionString(DatabaseProperties databaseProperties, ProductDbType dbType)
        {
            if(dbType == ProductDbType.Oracle)
            {
                return string.Format(_oracleConnectionString, databaseProperties.DatabaseUser, databaseProperties.DatabasePassword, databaseProperties.ServerOrTns);
            }
            else if (dbType == ProductDbType.SqlServer)
            {
                if (databaseProperties.IsTrustedConnection)
                {
                    return string.Format(_sqlServerTrustedConnectionString, databaseProperties.ServerOrTns, databaseProperties.DatabaseName);
                }
                else
                {
                    return string.Format(_sqlServerConnectionString, databaseProperties.ServerOrTns, databaseProperties.DatabaseName, databaseProperties.DatabaseUser, databaseProperties.DatabasePassword);
                }
            }
            return null;
        }
    }
}
