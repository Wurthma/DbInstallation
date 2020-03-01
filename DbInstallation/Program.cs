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
            Logger.Info(Messages.Message015);
            Console.WriteLine();
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
                string argMaster = args[0].ToLower();
                if (argMaster == "-newupdt")
                {
                    if (!FileHelper.CreateNextUpdateFolders())
                    {
                        othersLogger.Error(Messages.ErrorMessage020);
                    }
                }
                else if (argMaster == "-oracle")
                {// -oracle owner password tnsname td ti [i|u]
                    if(args.Length == 7)
                    {
                        var databaseProperties = new DatabaseProperties(args[1], args[2], args[3], args[4], args[5]);
                        string operation = args[6];
                        SetConnectionStartDatabaseOperation(ProductDbType.Oracle, databaseProperties, operation);
                    }
                    else
                    {
                        othersLogger.Error(Messages.ErrorMessage023);
                        return;
                    }
                }
                else if (argMaster == "-sqlserver")
                {// -sqlserver user password server databaseName install ||OR|| -sqlserver trusted server databaseName [i|u]
                    if (args.Length >= 4)
                    {
                        DatabaseProperties databaseProperties;
                        string operation;

                        if (args.Length == 6)
                        {
                            operation = args[5];
                            databaseProperties = new DatabaseProperties(args[1], args[2], args[3], args[4], false);
                        }
                        else if (args.Length == 4)
                        {
                            operation = args[3];
                            databaseProperties = new DatabaseProperties(null, null, args[1], args[2], true);
                        }
                        else
                        {
                            othersLogger.Error(Messages.ErrorMessage023);
                            return;
                        }

                        SetConnectionStartDatabaseOperation(ProductDbType.SqlServer, databaseProperties, operation);
                    }
                }
            }
        }

        private static void SetConnectionStartDatabaseOperation(ProductDbType dbType, DatabaseProperties databaseProperties, string operation)
        {
            var productDatabase = new ProductDatabase();
            if (productDatabase.SetConnection(dbType, databaseProperties))
            {
                OperationType operationType = productDatabase.GetOperationType(operation);
                productDatabase.StartDatabaseOperation(operationType);
            }
            else
            {
                throw new Exception(Messages.ErrorMessage001);
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
