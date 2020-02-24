using DbInstallation.Database;
using DbInstallation.Enums;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.IO;

namespace DbInstallation
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

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
                throw new Exception(Messages.ErrorMessage001);
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
