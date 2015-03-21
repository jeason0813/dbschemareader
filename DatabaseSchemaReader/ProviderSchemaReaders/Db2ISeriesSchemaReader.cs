﻿using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class Db2ISeriesSchemaReader : SchemaExtendedReader
    {
        //http://www-01.ibm.com/support/knowledgecenter/ssw_ibm_i_71/db2/rbafzcatalogtbls.htm

        //GetSchema Collections:
        //MetaDataCollections
        //DataSourceInformation
        //DataTypes
        //Restrictions
        //ReservedWords
        //Schemas
        //Tables
        //Columns
        //Databases
        //Procedures
        //ProcedureParameters
        //Indexes
        //IndexColumns
        //Views
        //ViewColumns


        public Db2ISeriesSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        protected override DataTable Sequences(DbConnection connection)
        {
            DataTable dt = CreateDataTable(SequencesCollectionName);

            const string sqlCommand = @"SELECT 
    SEQUENCE_SCHEMA AS SCHEMA, 
    SEQUENCE_NAME, 
    INCREMENT AS INCREMENTBY, 
    MINIMUM_VALUE AS minvalue, 
    MAXIMUM_VALUE AS maxvalue 
FROM QSYS2.SYSSEQUENCES 
WHERE SEQUENCE_SCHEMA <> 'SYSIBM'";

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection connection)
        {
            DataTable dt = CreateDataTable(IdentityColumnsCollectionName);
            const string sqlCommand = @"SELECT 
    TABLE_SCHEMA AS tabschema, 
    TABLE_NAME As TableName, 
    COLUMN_NAME As ColumnName
FROM QSYS2.SYSCOLUMNS
WHERE TABLE_NAME = @tableName or @tableName Is NULL
AND TABLE_SCHEMA = @schemaOwner or @schemaOwner Is NULL
AND HAS_DEFAULT = 'I' OR HAS_DEFAULT = 'J'";
            //I: The column is defined with the AS IDENTITY and GENERATED ALWAYS attributes.
            //J: The column is defined with the AS IDENTITY and GENERATED BY DEFAULT attributes.

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;
                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
    cons.CONSTRAINT_SCHEMA, 
    cons.CONSTRAINT_NAME, 
    cons.TABLE_SCHEMA AS schema_name, 
    cons.TABLE_NAME, 
    cols.COLUMN_NAME, 
    cols.ORDINAL_POSITION 
FROM QSYS2.SYSCST cons
INNER JOIN QSYS2.SYSKEYCST cols
    ON cons.CONSTRAINT_NAME = cols.CONSTRAINT_NAME
    AND cons.TABLE_SCHEMA = cols.TABLE_SCHEMA 
    AND cons.TABLE_NAME = cols.TABLE_NAME
WHERE 
    cons.CONSTRAINT_TYPE = 'PRIMARY KEY' AND
    cons.TABLE_NAME = @tableName or @tableName Is NULL
    AND cons.TABLE_SCHEMA = @schemaOwner or @schemaOwner Is NULL
";

            return CommandForTable(tableName, connection, PrimaryKeysCollectionName, sql);
        }

        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
    cons.CONSTRAINT_SCHEMA, 
    cons.CONSTRAINT_NAME, 
    cons.TABLE_SCHEMA AS schema_name, 
    cons.TABLE_NAME, 
    child.COLUMN_NAME, 
    child.ORDINAL_POSITION,
    refs.UNIQUE_CONSTRAINT_NAME,
    parent.TABLE_NAME AS fk_table,
    refs.delete_rule,
    refs.update_rule
FROM QSYS2.SYSCST cons
INNER JOIN QSYS2.SYSKEYCST child
    ON cons.CONSTRAINT_NAME = child.CONSTRAINT_NAME
    AND cons.TABLE_SCHEMA = child.TABLE_SCHEMA 
    AND cons.TABLE_NAME = child.TABLE_NAME

INNER JOIN QSYS2.SYSREFCST refs
   ON child.CONSTRAINT_SCHEMA = refs.CONSTRAINT_SCHEMA
   AND child.CONSTRAINT_NAME = refs.CONSTRAINT_NAME

INNER JOIN QSYS2.SYSKEYCST parent 
    ON refs.UNIQUE_CONSTRAINT_SCHEMA = parent.CONSTRAINT_SCHEMA
   AND refs.UNIQUE_CONSTRAINT_NAME = parent.CONSTRAINT_NAME

WHERE 
    cons.CONSTRAINT_TYPE = 'FOREIGN KEY' AND
    cons.TABLE_NAME = @tableName or @tableName Is NULL
    AND cons.TABLE_SCHEMA = @schemaOwner or @schemaOwner Is NULL
";

            return CommandForTable(tableName, connection, ForeignKeyColumnsCollectionName, sql);
        }

        /*
        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"select tabschema as Owner, 
trigname as Trigger_Name, 
tabname as table_name, 
CASE trigevent 
WHEN 'I' THEN 'INSERT'
WHEN 'D' THEN 'DELETE'
WHEN 'U' THEN 'UPDATE'
END as TRIGGERING_EVENT,
CASE trigtime
WHEN 'A' THEN 'AFTER'
WHEN 'B' THEN 'BEFORE'
WHEN 'I' THEN 'INSTEAD OF'
END as TRIGGER_TYPE,
text as TRIGGER_BODY
from syscat.triggers
where tabschema <> 'SYSTOOLS'
AND valid= 'Y'
AND (tabname = @tableName OR @tableName IS NULL) 
AND (tabschema = @schemaOwner OR @schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, TriggersCollectionName, sqlCommand);
        }

        public override DataTable TableDescription(string tableName)
        {
            const string sqlCommand = @"SELECT 
    TABSCHEMA AS 'SchemaOwner', 
    TABNAME AS 'TableName', 
    REMARKS AS 'TableDescription'
FROM SYSCAT.TABLES
WHERE 
    REMARKS IS NOT NULL AND
    (TABNAME = @tableName OR @tableName IS NULL) AND 
    (TABSCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return CommandForTable(tableName, connection, TableDescriptionCollectionName, sqlCommand);
            }
        }

        public override DataTable ColumnDescription(string tableName)
        {
            const string sqlCommand = @"SELECT 
    TABSCHEMA AS 'SchemaOwner', 
    TABNAME AS 'TableName', 
    COLNAME AS 'ColumnName',
    REMARKS AS 'ColumnDescription'
FROM SYSCAT.COLUMNS
WHERE 
    REMARKS IS NOT NULL AND
    (TABNAME = @tableName OR @tableName IS NULL) AND 
    (TABSCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return CommandForTable(tableName, connection, ColumnDescriptionCollectionName, sqlCommand);
            }
        }
         * */
    }
}