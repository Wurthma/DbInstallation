using DbInstallation.Database;
using DbInstallation.Enums;
using DbInstallation.Util;
using NLog;
using System;

namespace DbInstallation
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger othersLogger = LogManager.GetLogger("othersLog");

        static void Main(string[] args)
        {
            try
            {
                if(args.Length > 0)
                {
                    ExecuteArguments(args);
                    return;
                }
                SelectDatabase(new ProductDatabase());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
        }

        private static void ExecuteArguments(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "-newupdt")
                {
                    if (!FileHelper.CreateNextUpdateFolders())
                    {
                        othersLogger.Error(Messages.ErrorMessage020);
                    }
                }
                return;
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
                SelectOperation(productDatabase);
            }
            else
            {
                throw new Exception(Messages.ErrorMessage001);
            }
        }

        private static void SelectOperation(ProductDatabase productDatabase)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Type to select operation:");
            Console.WriteLine("(I) - Install");
            Console.WriteLine("(U) - Update");

            OperationType operationType = productDatabase.GetOperationType(Console.ReadLine());
            
            productDatabase.StartDatabaseOperation(operationType);
        }
    }
}
