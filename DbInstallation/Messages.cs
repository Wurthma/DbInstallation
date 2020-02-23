using System;
using System.Collections.Generic;
using System.Text;

namespace DbInstallation
{
    public static class Messages
    {
        #region Info messages
        private static string _message001 { get { return "Connection successfully made to the database {0}."; } }
        /// <summary>
        /// Connection successfully made to the database {0}.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static string Message001(string databaseName) => string.Format(_message001, databaseName);

        /// <summary>
        /// Checking database settings...
        /// </summary>
        public static string Message002 { get { return "Checking database settings..."; } }
        /// <summary>
        /// OK: Validations carried out with SUCCESS!
        /// </summary>
        public static string Message003 { get { return "OK: Validations carried out with SUCCESS!"; } }

        private static string _message004 { get { return @"OK: User/Connection: {0}/{1}."; } }
        /// <summary>
        /// OK: User/Connection: {0}/{1}.
        /// </summary>
        /// <param name="databaseUser"></param>
        /// <param name="tnsOrServerConnection"></param>
        /// <returns></returns>
        public static string Message004(string databaseUser, string tnsOrServerConnection) => string.Format(_message004, databaseUser, tnsOrServerConnection);

        private static string _message005 { get { return "OK: Data tablespace {0}."; } }
        /// <summary>
        /// OK: Data tablespace {0}.
        /// </summary>
        /// <param name="dataTablespaceName"></param>
        /// <returns></returns>
        public static string Message005(string dataTablespaceName) => string.Format(_message005, dataTablespaceName);

        private static string _message006 { get { return "OK: Index tablespace {0}."; } }
        /// <summary>
        /// OK: Index tablespace {0}.
        /// </summary>
        /// <param name="indexTablespaceName"></param>
        /// <returns></returns>
        public static string Message006(string indexTablespaceName) => string.Format(_message006, indexTablespaceName);

        private static string _message007 { get { return @"Executing file >> {0}\{1} <<"; } }
        /// <summary>
        /// Executing file >> {0} <<
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string Message007(string folderName, string fileName) => string.Format(_message007, folderName, fileName);

        /// <summary>
        /// Installation completed successfully!
        /// </summary>
        public static string Message008 { get { return @"Installation completed successfully!"; } }

        /// <summary>
        /// "Validating installation..."
        /// </summary>
        public static string Message009 { get { return @"Validating installation..."; } }

        /// <summary>
        /// SUCCESS: NO INTEGRITY FAILURE FOUND!
        /// </summary>
        public static string Message010 { get { return @"SUCCESS: NO INTEGRITY FAILURE FOUND!"; } }

        #endregion Info messages

        #region Error messages
        /// <summary>
        /// Failure with the connection or with the validations of the database. Check the log for more details.
        /// </summary>
        public static string ErrorMessage001 { get { return "Failure with the connection or with the validations of the database. Check the log for more details."; } }

        private static string _errorMessage002 { get { return "Failed to execute basic instruction after connecting to the database {0}."; } }
        /// <summary>
        /// Failed to execute basic instruction after connecting to the database {0}.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static string ErrorMessage002(string databaseName) => string.Format(_errorMessage002, databaseName);

        private static string _errorMessage003 { get { return "Failed to validate {0} in database {1}."; } }
        /// <summary>
        /// Failed to validate {0} in database {1}.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static string ErrorMessage003(string validateItem, string databaseName) => string.Format(_errorMessage003, validateItem, databaseName);

        /// <summary>
        /// Database connection was not defined.
        /// </summary>
        public static string ErrorMessage004 { get { return "Database connection was not defined."; } }

        /// <summary>
        /// Invalid database operation.
        /// </summary>
        public static string ErrorMessage005 { get { return "Invalid database operation."; } }

        /// <summary>
        /// Database type is not defined.
        /// </summary>
        public static string ErrorMessage006 { get { return "Database type is not defined."; } }

        /// <summary>
        /// Invalid Database type option.
        /// </summary>
        public static string ErrorMessage007 { get { return "Invalid Database type option."; } }

        private static string _errorMessage008 { get { return "{0} is not a valid directory."; } }
        /// <summary>
        /// {0} is not a valid directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ErrorMessage008(string path) => string.Format(_errorMessage008, path);

        private static string _errorMessage009 { get { return "Database {0} is not empty. Found {1} objects."; } }
        /// <summary>
        /// Database {0} is not empty. Found {1} objects.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="numObjects"></param>
        /// <returns></returns>
        public static string ErrorMessage009(string databaseName, decimal numObjects) => string.Format(_errorMessage009, databaseName, numObjects);

        private static string _errorMessage010 { get { return @"Failed to execute sql command: " + Environment.NewLine + "{0}"; } }
        /// <summary>
        /// Failed to execute sql command: " + Environment.NewLine + "{0}
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <returns></returns>
        public static string ErrorMessage010(string sqlCommand) => string.Format(_errorMessage010, sqlCommand);

        /// <summary>
        /// Object Name: {0}" + Environment.NewLine + "Object Type: {1}" + Environment.NewLine + "Nonconformity Type: {2}
        /// </summary>
        public static string ErrorMessage011 { get { return @"Failed to load database integrity result."; } }

        private static string _errorMessage012 { get { return @"Object Name: {0}" + Environment.NewLine + "Object Type: {1}" + Environment.NewLine + "Nonconformity Type: {2}"; } }
        /// <summary>
        /// Object Name: {0}" + Environment.NewLine + "Object Type: {1}" + Environment.NewLine + "Nonconformity Type: {2}"
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="objectType"></param>
        /// <param name="nonconformityType"></param>
        /// <returns></returns>
        public static string ErrorMessage012(string objectName, string objectType, string nonconformityType) => string.Format(_errorMessage012, objectName, objectType, nonconformityType);

        private static string _errorMessage013 { get { return @"{0} INTEGRITY FAULTS WERE FOUND!"; } }
        /// <summary>
        /// {0} INTEGRITY FAULTS WERE FOUND!
        /// </summary>
        /// <param name="qtyFailures"></param>
        /// <returns></returns>
        public static string ErrorMessage013(int qtyFailures) => string.Format(_errorMessage013, qtyFailures);

        #endregion Error messages
    }
}
