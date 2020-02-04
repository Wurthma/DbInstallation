using NLog;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation.Database
{
    public class IrpjDatabase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SetConnection(IrpjDbType dbType)
        {
            if(dbType == IrpjDbType.Oracle)
            {
                SetOracleConnection();
            }
            else if(dbType == IrpjDbType.SqlServer)
            {

            }
        }

        private void SetOracleConnection()
        {
            Console.WriteLine("Enter IRPJ database user:");
            string dbUser = Console.ReadLine();
            Console.WriteLine("Enter the password:");
            string dbPassword = Console.ReadLine();
            Console.WriteLine("Enter the string connection or TNS:");
            string tnsConnection = Console.ReadLine();
            Console.WriteLine("Enter data TABLESPACE:");
            string tablespaceData = Console.ReadLine();
            Console.WriteLine("Enter index TABLESPACE:");
            string tablespaceIndex = Console.ReadLine();

            OracleInstallationFunctions oracleInstallationFunctions = new OracleInstallationFunctions(
                new DatabaseProperties(dbUser, dbPassword, tnsConnection),
                tablespaceData,
                tablespaceIndex);

            try
            {
                oracleInstallationFunctions.TestConnection();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }
    }
}
