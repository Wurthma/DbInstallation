BEGIN
    -- Generated by Oracle SQL Developer Data Modeler Version: 2.0.0 Build: 570
    --   at:        2009-06-29 12:05:54
    --   site:      Oracle Database 11g
    --   type:      Oracle Database 11g



    CREATE TABLE COUNTRIES 
        ( 
         COUNTRY_ID CHAR (2 BYTE)  NOT NULL , 
         COUNTRY_NAME VARCHAR2 (40 BYTE) , 
         REGION_ID NUMBER 
        ) LOGGING 
    /



    COMMENT ON COLUMN COUNTRIES.COUNTRY_ID IS 'Primary key of countries table.' 
    /

    COMMENT ON COLUMN COUNTRIES.COUNTRY_NAME IS 'Country name' 
    /

    COMMENT ON COLUMN COUNTRIES.REGION_ID IS 'Region ID for the country. Foreign key to region_id column in the departments table.' 
    /


    CREATE TABLE DEPARTMENTS 
        ( 
         DEPARTMENT_ID NUMBER (4)  NOT NULL , 
         DEPARTMENT_NAME VARCHAR2 (30 BYTE)  NOT NULL , 
         MANAGER_ID NUMBER (6) , 
         LOCATION_ID NUMBER (4) 
        ) LOGGING 
    /



    COMMENT ON COLUMN DEPARTMENTS.DEPARTMENT_ID IS 'Primary key column of departments table.' 
    /

    COMMENT ON COLUMN DEPARTMENTS.DEPARTMENT_NAME IS 'A not null column that shows name of a department. Administration, 
    Marketing, Purchasing, Human Resources, Shipping, IT, Executive, Public 
    Relations, Sales, Finance, and Accounting. ' 
    /

    COMMENT ON COLUMN DEPARTMENTS.MANAGER_ID IS 'Manager_id of a department. Foreign key to employee_id column of employees table. The manager_id column of the employee table references this column.' 
    /

    COMMENT ON COLUMN DEPARTMENTS.LOCATION_ID IS 'Location id where a department is located. Foreign key to location_id column of locations table.' 
    /
    CREATE INDEX DEPT_LOCATION_IX ON DEPARTMENTS 
        ( 
         LOCATION_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /


    CREATE TABLE EMPLOYEES 
        ( 
         EMPLOYEE_ID NUMBER (6)  NOT NULL , 
         FIRST_NAME VARCHAR2 (20 BYTE) , 
         LAST_NAME VARCHAR2 (25 BYTE)  NOT NULL , 
         EMAIL VARCHAR2 (25 BYTE)  NOT NULL , 
         PHONE_NUMBER VARCHAR2 (20 BYTE) , 
         HIRE_DATE DATE  NOT NULL , 
         JOB_ID VARCHAR2 (10 BYTE)  NOT NULL , 
         SALARY NUMBER (8,2) , 
         COMMISSION_PCT NUMBER (2,2) , 
         MANAGER_ID NUMBER (6) , 
         DEPARTMENT_ID NUMBER (4) 
        ) LOGGING 
    /



    COMMENT ON COLUMN EMPLOYEES.EMPLOYEE_ID IS 'Primary key of employees table.' 
    /

    COMMENT ON COLUMN EMPLOYEES.FIRST_NAME IS 'First name of the employee. A not null column.' 
    /

    COMMENT ON COLUMN EMPLOYEES.LAST_NAME IS 'Last name of the employee. A not null column.' 
    /

    COMMENT ON COLUMN EMPLOYEES.EMAIL IS 'Email id of the employee' 
    /

    COMMENT ON COLUMN EMPLOYEES.PHONE_NUMBER IS 'Phone number of the employee/ includes country code and area code' 
    /

    COMMENT ON COLUMN EMPLOYEES.HIRE_DATE IS 'Date when the employee started on this job. A not null column.' 
    /

    COMMENT ON COLUMN EMPLOYEES.JOB_ID IS 'Current job of the employee/ foreign key to job_id column of the 
    jobs table. A not null column.' 
    /

    COMMENT ON COLUMN EMPLOYEES.SALARY IS 'Monthly salary of the employee. Must be greater 
    than zero (enforced by constraint emp_salary_min)' 
    /

    COMMENT ON COLUMN EMPLOYEES.COMMISSION_PCT IS 'Commission percentage of the employee/ Only employees in sales 
    department elgible for commission percentage' 
    /

    COMMENT ON COLUMN EMPLOYEES.MANAGER_ID IS 'Manager id of the employee/ has same domain as manager_id in 
    departments table. Foreign key to employee_id column of employees table.
    (useful for reflexive joins and CONNECT BY query)' 
    /

    COMMENT ON COLUMN EMPLOYEES.DEPARTMENT_ID IS 'Department id where employee works/ foreign key to department_id 
    column of the departments table' 
    /
    CREATE INDEX EMP_DEPARTMENT_IX ON EMPLOYEES 
        ( 
         DEPARTMENT_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX EMP_NAME_IX ON EMPLOYEES 
        ( 
         LAST_NAME ASC , 
         FIRST_NAME ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX EMP_JOB_IX ON EMPLOYEES 
        ( 
         JOB_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX EMP_MANAGER_IX ON EMPLOYEES 
        ( 
         MANAGER_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /

    CREATE TABLE JOBS 
        ( 
         JOB_ID VARCHAR2 (10 BYTE)  NOT NULL , 
         JOB_TITLE VARCHAR2 (35 BYTE)  NOT NULL , 
         MIN_SALARY NUMBER (6) , 
         MAX_SALARY NUMBER (6) 
        ) LOGGING 
    /



    COMMENT ON COLUMN JOBS.JOB_ID IS 'Primary key of jobs table.' 
    /

    COMMENT ON COLUMN JOBS.JOB_TITLE IS 'A not null column that shows job title, e.g. AD_VP, FI_ACCOUNTANT' 
    /

    COMMENT ON COLUMN JOBS.MIN_SALARY IS 'Minimum salary for a job title.' 
    /

    COMMENT ON COLUMN JOBS.MAX_SALARY IS 'Maximum salary for a job title' 
    /


    CREATE TABLE JOB_HISTORY 
        ( 
         EMPLOYEE_ID NUMBER (6)  NOT NULL , 
         START_DATE DATE  NOT NULL , 
         END_DATE DATE  NOT NULL , 
         JOB_ID VARCHAR2 (10 BYTE)  NOT NULL , 
         DEPARTMENT_ID NUMBER (4) 
        ) LOGGING 
    /



    ALTER TABLE JOB_HISTORY 
        ADD CONSTRAINT JHIST_DATE_INTERVAL 
        CHECK (end_date > start_date)
            INITIALLY IMMEDIATE 
            ENABLE 
            VALIDATE 
    /


    COMMENT ON COLUMN JOB_HISTORY.EMPLOYEE_ID IS 'A not null column in the complex primary key employee_id+start_date.
    Foreign key to employee_id column of the employee table' 
    /

    COMMENT ON COLUMN JOB_HISTORY.START_DATE IS 'A not null column in the complex primary key employee_id+start_date. 
    Must be less than the end_date of the job_history table. (enforced by 
    constraint jhist_date_interval)' 
    /

    COMMENT ON COLUMN JOB_HISTORY.END_DATE IS 'Last day of the employee in this job role. A not null column. Must be 
    greater than the start_date of the job_history table. 
    (enforced by constraint jhist_date_interval)' 
    /

    COMMENT ON COLUMN JOB_HISTORY.JOB_ID IS 'Job role in which the employee worked in the past/ foreign key to 
    job_id column in the jobs table. A not null column.' 
    /

    COMMENT ON COLUMN JOB_HISTORY.DEPARTMENT_ID IS 'Department id in which the employee worked in the past/ foreign key to deparment_id column in the departments table' 
    /
    CREATE INDEX JHIST_JOB_IX ON JOB_HISTORY 
        ( 
         JOB_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX JHIST_EMPLOYEE_IX ON JOB_HISTORY 
        ( 
         EMPLOYEE_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX JHIST_DEPARTMENT_IX ON JOB_HISTORY 
        ( 
         DEPARTMENT_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /


    CREATE TABLE LOCATIONS 
        ( 
         LOCATION_ID NUMBER (4)  NOT NULL , 
         STREET_ADDRESS VARCHAR2 (40 BYTE) , 
         POSTAL_CODE VARCHAR2 (12 BYTE) , 
         CITY VARCHAR2 (30 BYTE)  NOT NULL , 
         STATE_PROVINCE VARCHAR2 (25 BYTE) , 
         COUNTRY_ID CHAR (2 BYTE) 
        ) LOGGING 
    /



    COMMENT ON COLUMN LOCATIONS.LOCATION_ID IS 'Primary key of locations table' 
    /

    COMMENT ON COLUMN LOCATIONS.STREET_ADDRESS IS 'Street address of an office, warehouse, or production site of a company.
    Contains building number and street name' 
    /

    COMMENT ON COLUMN LOCATIONS.POSTAL_CODE IS 'Postal code of the location of an office, warehouse, or production site 
    of a company. ' 
    /

    COMMENT ON COLUMN LOCATIONS.CITY IS 'A not null column that shows city where an office, warehouse, or 
    production site of a company is located. ' 
    /

    COMMENT ON COLUMN LOCATIONS.STATE_PROVINCE IS 'State or Province where an office, warehouse, or production site of a 
    company is located.' 
    /

    COMMENT ON COLUMN LOCATIONS.COUNTRY_ID IS 'Country where an office, warehouse, or production site of a company is
    located. Foreign key to country_id column of the countries table.' 
    /
    CREATE INDEX LOC_CITY_IX ON LOCATIONS 
        ( 
         CITY ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX LOC_COUNTRY_IX ON LOCATIONS 
        ( 
         COUNTRY_ID ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /
    CREATE INDEX LOC_STATE_PROVINCE_IX ON LOCATIONS 
        ( 
         STATE_PROVINCE ASC 
        ) 
        NOLOGGING 
        NOCOMPRESS 
        NOPARALLEL 
    /


    CREATE TABLE REGIONS 
        ( 
         REGION_ID NUMBER  NOT NULL , 
         REGION_NAME VARCHAR2 (25 BYTE) 
        ) LOGGING 
    /



    COMMENT ON COLUMN REGIONS.REGION_ID IS 'Primary key of regions table.' 
    /

    COMMENT ON COLUMN REGIONS.REGION_NAME IS 'Names of regions. Locations are in the countries of these regions.' 
    /
END/
