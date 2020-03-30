using System;

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

        private static string _message011 { get { return @"Current Version of database {0}/{1} is: {2}."; } }
        /// <summary>
        /// Current Version of database {0}/{1} is: {2}.
        /// </summary>
        /// <param name="databaseUser"></param>
        /// <param name="tnsOrServerConnection"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string Message011(string databaseUser, string tnsOrServerConnection, int version) => string.Format(_message011, databaseUser, tnsOrServerConnection, version);

        private static string _message012 { get { return @"Update folder structure 'Update\{0}' created for Oracle and Sql Server."; } }
        /// <summary>
        /// Update folder structure 'Update\{0}' created for Oracle and Sql Server.
        /// </summary>
        /// <param name="versionFolderCreated"></param>
        /// <returns></returns>
        public static string Message012(int versionFolderCreated) => string.Format(_message012, versionFolderCreated);

        private static string _message013 { get { return @"Directory {0} sussecefully created!"; } }
        /// <summary>
        /// Directory {0} sussecefully created!
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string Message013(string folder) => string.Format(_message013, folder);

        private static string _message014 { get { return "The database will be updated to version {0}."; } }
        /// <summary>
        /// "The database will be updated to version {0}."
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string Message014(int version) => string.Format(_message014, version);

        /// <summary>
        /// ### Application started! ###
        /// </summary>
        public static string Message015 { get { return "### Application started! ###"; } }

        /// <summary>
        /// Oracle integrity validation is disabled.
        /// </summary>
        public static string Message016 { get { return "Oracle integrity validation is disabled."; } }

        private static string _message017 { get { return "File '{0}' successfully created!"; } }
        /// <summary>
        /// File '{0}' successfully created!
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string Message017(string fileName) => string.Format(_message017, fileName);

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

        private static string _errorMessage008 { get { return "There are no files for directory {0}"; } }
        /// <summary>
        /// There are no files for directory {0}
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ErrorMessage008(string path) => string.Format(_errorMessage008, path);

        private static string _errorMessage009 { get { return "Database {0} is not empty."; } }
        /// <summary>
        /// Database {0} is not empty.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="numObjects"></param>
        /// <returns></returns>
        public static string ErrorMessage009(string databaseName) => string.Format(_errorMessage009, databaseName);

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

        /// <summary>
        /// An update operation was selected but the parameters to update the database was not set.
        /// </summary>
        public static string ErrorMessage014 { get { return @"An update operation was selected but the parameters to update the database was not set."; } }

        /// <summary>
        /// No version was found to database. Please, check if database connection is right and if the application was installed.
        /// </summary>
        public static string ErrorMessage015 { get { return @"No version was found to database. Please, check if database connection is right and if the application was installed."; } }

        private static string _errorMessage016 { get { return @"The database can't be updated to version {0}. The current version is already {1}."; } }
        /// <summary>
        /// The database can't be updated to version {0}. The current version is already {1}.
        /// </summary>
        /// <param name="versionToUpdate"></param>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public static string ErrorMessage016(int versionToUpdate, int currentVersion) => string.Format(_errorMessage016, versionToUpdate, currentVersion);

        private static string _errorMessage017 { get { return @"Format name {0} is not valid for directory 'Update'. Folders in directory 'Update' must be sequencial and numeric."; } }
        /// <summary>
        /// Format name {0} is not valid for directory 'Update'. Folders in directory 'Update' must be sequencial and numeric.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string ErrorMessage017(string folder) => string.Format(_errorMessage017, folder);

        private static string _errorMessage018 { get { return @"Action aborterd: Update folder is different for Oracle (current {0}) and Sql Server (current {1})."; } }
        /// <summary>
        /// Action aborterd: Update folder is different for Oracle (current {0}) and Sql Server (current {1}).
        /// </summary>
        /// <param name="oracleVersion"></param>
        /// <param name="sqlServerVersion"></param>
        /// <returns></returns>
        public static string ErrorMessage018(int oracleVersion, int sqlServerVersion) => string.Format(_errorMessage018, oracleVersion, sqlServerVersion);

        private static string _errorMessage019 { get { return @"An error occurred while trying to create folder {0}."; } }
        /// <summary>
        /// An error occurred while trying to create folder {0}.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string ErrorMessage019(string folder) => string.Format(_errorMessage019, folder);

        /// <summary>
        /// An error occurred while trying to execute this action.
        /// </summary>
        public static string ErrorMessage020 { get { return @"An error occurred while trying to execute this action."; } }

        /// <summary>
        /// An error occurred during the process.
        /// </summary>
        public static string ErrorMessage021 { get { return @"An error occurred during the process."; } }

        /// <summary>
        /// Check the log to perform the necessary corrections and perform the update again.
        /// </summary>
        public static string ErrorMessage022 { get { return @"Check the log to perform the necessary corrections and perform the update again."; } }

        /// <summary>
        /// Invalid arguments.
        /// </summary>
        public static string ErrorMessage023 { get { return @"Invalid arguments."; } }

        private static string _errorMessage024 { get { return @"Database {0} is empty. Update to version {1} was aborted."; } }
        /// <summary>
        /// Database {0} is empty. Update to version {1} was aborted.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string ErrorMessage024(string databaseName, int version) => string.Format(_errorMessage024, databaseName, version);

        private static string _errorMessage025 { get { return "File {0} already exists. Delete it before generate another file."; } }
        /// <summary>
        /// Database {0} is empty. Update to version {1} was aborted.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string ErrorMessage025(string fileName) => string.Format(_errorMessage025, fileName);

        /// <summary>
        /// An error occurred while trying to execute SQL script. Press any key to continue or end de process by closing it.
        /// </summary>
        public static string ErrorMessage026 { get { return "An error occurred while trying to execute SQL script. Press any key to continue or end de process by closing it."; } }

        /// <summary>
        /// Applcation configuration 'MinVersion' invalid.
        /// </summary>
        public static string ErrorMessage027 { get { return @"Applcation configuration 'MinVersion' invalid."; } }

        private static string _errorMessage028 { get { return "The minimum application version to use the update operation is {0} but the current version of application database is {1}."; } }
        /// <summary>
        /// The minimum application version to use the update operation is {0} but the current version of application database is {1}.
        /// </summary>
        /// <param name="minimumVersion"></param>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        public static string ErrorMessage028(int minimumVersion, int currentVersion) => string.Format(_errorMessage028, minimumVersion, currentVersion);

        public static string ErrorMessage029 { get { return @"Application version invalid."; } }

        #endregion Error messages
    }
}
