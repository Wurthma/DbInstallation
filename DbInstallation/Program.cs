using DbInstallation.Database;
using DbInstallation.Enums;
using NLog;
using System;
using static DbInstallation.Enums.EnumDbType;

namespace DbInstallation
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                SelectDatabase(new IrpjDatabase());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
        }

        public static void SelectDatabase(IrpjDatabase irpjDatabase)
        {
            Console.WriteLine("Type to select database type:");
            Console.WriteLine("(O) - Oracle");
            Console.WriteLine("(S) - SQL Server");
            string dbType = Console.ReadLine();

            switch (dbType.ToUpper())
            {
                case "O":
                    Logger.Info("Selected Oracle Database");
                    irpjDatabase.SetConnection(IrpjDbType.Oracle);
                    break;
                case "S":
                    Logger.Info("Selected SQL Server Database");
                    irpjDatabase.SetConnection(IrpjDbType.SqlServer);
                    break;
                default:
                    break;
            }
        }
    }
}
