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

        public static string GetConnectionString(DatabaseProperties databaseProperties, ProductDbType dbType)
        {
            if(dbType == ProductDbType.Oracle)
            {
                return string.Format(_oracleConnectionString, databaseProperties.DataBaseUser, databaseProperties.DatabasePassword, databaseProperties.ServerOrTns);
            }
            else if (dbType == ProductDbType.SqlServer)
            {
                return string.Format(_sqlServerConnectionString, databaseProperties.DataBaseUser, databaseProperties.DatabasePassword, databaseProperties.ServerOrTns);
            }
            return null;
        }
    }
}
