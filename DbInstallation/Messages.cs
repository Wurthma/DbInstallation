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
        #endregion Error messages
    }
}
