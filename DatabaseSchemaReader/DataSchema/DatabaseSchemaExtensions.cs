﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Extensions to enable schema to be created with a simple fluent interface
    /// </summary>
    public static class DatabaseSchemaExtensions
    {
        /// <summary>
        /// Adds a table.
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static DatabaseTable AddTable(this DatabaseSchema databaseSchema, string tableName)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema", "databaseSchema must not be null");
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "tableName must not be null");

            var table = new DatabaseTable { Name = tableName };
            databaseSchema.Tables.Add(table);
            table.DatabaseSchema = databaseSchema;
            table.SchemaOwner = databaseSchema.Owner;
            return table;
        }

        /// <summary>
        /// Adds a table.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static DatabaseTable AddTable(this DatabaseTable databaseTable, string tableName)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            var schema = databaseTable.DatabaseSchema;
            return schema.AddTable(tableName);
        }

        /// <summary>
        /// Adds the table.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static DatabaseTable AddTable(this DatabaseColumn databaseColumn, string tableName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddTable(tableName);
        }

        /// <summary>
        /// Adds the index.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public static DatabaseTable AddIndex(this DatabaseTable databaseTable, string indexName, IEnumerable<DatabaseColumn> columns)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (!columns.Any()) throw new ArgumentException("columns is empty", "columns");
            var index = new DatabaseIndex
                            {
                                Name = indexName,
                                TableName = databaseTable.Name,
                                SchemaOwner = databaseTable.SchemaOwner,
                                IndexType = "NONCLUSTERED"
                            };
            index.Columns.AddRange(columns);
            databaseTable.AddIndex(index);
            return databaseTable;
        }

        /// <summary>
        /// Determines whether is a many to many table (association or junction table joining two or more other tables in a many to many relationship)
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <returns>
        /// 	<c>true</c> if this is a many to many table; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsManyToManyTable(this DatabaseTable databaseTable)
        {
            return (databaseTable.Columns.All(c => c.IsPrimaryKey && c.IsForeignKey));
        }

        /// <summary>
        /// Via a many to many table, find the opposite many relationship
        /// </summary>
        /// <param name="manyToManyTable">The many to many table.</param>
        /// <param name="fromTable">From table.</param>
        /// <returns></returns>
        public static DatabaseTable ManyToManyTraversal(this DatabaseTable manyToManyTable, DatabaseTable fromTable)
        {
            var foreignKey = manyToManyTable.ForeignKeys.FirstOrDefault(x => x.RefersToTable != fromTable.Name);
            //a self many to many
            if (foreignKey == null) return fromTable;
            return foreignKey.ReferencedTable(fromTable.DatabaseSchema);
        }
    }
}
