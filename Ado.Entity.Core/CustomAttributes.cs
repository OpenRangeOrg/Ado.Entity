using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ado.Entity.Core
{
    public class Primary : Attribute
    {
    }
    public class Unique : Attribute
    {
    }
    public class Table : Attribute
    {
        public string TableName { get; private set; }
        public Table(string tableName)
        {
            TableName = tableName;
        }
    }
    public class Column : Attribute
    {
        public string Name { get; private set; }
        public Column(string name)
        {
            Name = name;
        }
    }
    public class Ignore : Attribute
    {
        public Ignore()
        {
        }
    }
    public class DataType : Attribute
    {
        public NpgsqlDbType Value { get; private set; }
        public DataType(NpgsqlDbType value)
        {
            Value = value;

        }
    }
}
