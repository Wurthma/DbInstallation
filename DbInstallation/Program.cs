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
                SelectDatabase(new ProductDatabase());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
        }

        private static void SelectDatabase(ProductDatabase productDatabase)
        {
            Console.WriteLine("Type to select database type:");
            Console.WriteLine("(O) - Oracle");
            Console.WriteLine("(S) - SQL Server");
            
            ProductDbType dbType = productDatabase.GetDatabaseType(Console.ReadLine());

            productDatabase.SetConnection(dbType);
            SelectOperation(dbType, productDatabase);
        }

        private static void SelectOperation(ProductDbType dbType, ProductDatabase productDatabase)
        {
            Console.WriteLine("Type to select operation:");
            Console.WriteLine("(I) - Install");
            Console.WriteLine("(U) - Update");

            throw new NotImplementedException(); //TODO:
        }
    }
}
