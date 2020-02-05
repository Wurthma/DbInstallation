using DbInstallation.Interfaces;
using NLog;
using System;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public class SqlServerInstallationFunctions : BaseInstallationDbFunctions, IDatabaseFunctions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SqlServerInstallationFunctions(DatabaseProperties databaseProperties)
            : base(databaseProperties)
        {
            base.SetConnectionString(ProductConnectionString.GetConnectionString(databaseProperties, ProductDbType.SqlServer));
        }

        public bool CheckDatabaseInstall()
        {
            throw new NotImplementedException(); //TODO:
        }

        public bool TestConnection()
        {
            throw new NotImplementedException(); //TODO:
        }
    }
}
