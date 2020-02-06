using DbInstallation.Database;
using NLog;
using System;
using static DbInstallation.Enums.EnumDbType;
using static DbInstallation.Enums.EnumOperation;

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

            if (productDatabase.SetConnection(dbType))
            {
                SelectOperation(dbType, productDatabase);
            }
            else
            {
                throw new Exception("Failure with the connection or with the validations of the database. Check the log for more details.");
            }
        }

        private static void SelectOperation(ProductDbType dbType, ProductDatabase productDatabase)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Type to select operation:");
            Console.WriteLine("(I) - Install");
            Console.WriteLine("(U) - Update");

            OperationType operationType = productDatabase.GetOperationType(Console.ReadLine());
            
            productDatabase.StartDatabaseOperation(dbType, operationType);
        }
    }
}
