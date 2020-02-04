using System;
using System.Collections.Generic;
using System.Text;

namespace DbInstallation.Database
{
    public abstract class BaseInstallationDbFunctions
    {
        

        protected BaseInstallationDbFunctions(DatabaseProperties databaseProperties)
        {
            DatabaseProperties = databaseProperties;
        }

        public string ConnectionString { get; private set; }

        public DatabaseProperties DatabaseProperties { get; private set; }

        protected void SetConnectionString(string connectionString) => ConnectionString = connectionString;

    }
}
