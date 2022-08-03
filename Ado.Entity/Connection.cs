﻿using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity
{
    class Connection:IConnection
    {
        private string ConnectionString;
        public Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public List<T> GetDataByQuery<T>(string queryFormat)
        {
            return GetDataByQuery<T>(queryFormat, null);
        }
        public List<T> GetDataByQuery<T>(string queryFormat, string[] param = null)
        {
            List<T> dtoList = new List<T>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    param = param ?? new string[] { };
                    var queryString = string.Format(queryFormat, param);
                    SqlCommand objSqlCommand = new SqlCommand(queryString, con);
                    objSqlCommand.CommandType = CommandType.Text;
                    using (SqlDataReader reader = objSqlCommand.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        dtoList.AddRange(Map<T>(dataTable));
                    }
                }
                catch (Exception ex)
                {
                    return dtoList;
                }
                finally
                {
                    con.Close();
                }
            }
            return dtoList;
        }
        public bool AddEntry<T>(T obj)
        {
            string queryString = BuildInsertQueryString<T>(obj);
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                try
                {
                    SqlCommand objSqlCommand = new SqlCommand(queryString, con);
                    objSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        private string BuildInsertQueryString<T>(T obj)
        {

            var queryString = $"SET IDENTITY_INSERT {obj.GetType().Name} ON ;insert into {obj.GetType().Name} (";
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                queryString += $"[{property.Name}],";
            }
            queryString = queryString.Remove(queryString.Length - 1, 1) + ") VALUES (";
            foreach (var property in properties)
            {
                if (property.PropertyType.Name == "String" || property.PropertyType.Name == "Type")
                {
                    queryString += $"'{property.GetValue(obj)}',";
                }
                else if (property.PropertyType.Name == "Boolean")
                {
                    queryString += $"{Convert.ToByte(property.GetValue(obj))},";
                }
                else
                {
                    queryString += $"{property.GetValue(obj)},";
                }
            }
            queryString = queryString.Remove(queryString.Length - 1, 1) + $");SET IDENTITY_INSERT {obj.GetType().Name} OFF ;";
            return queryString;
        }
        private T GetObject<T>(SqlDataReader reader)
        {
            return (T)Activator.CreateInstance(typeof(T), reader);
        }
        private List<T> Map<T>(DataTable dataTable)
        {

            List<T> _objList = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T _obj = Execute<T>(dataTable.Columns, row);
                if (_obj != null)
                {
                    _objList.Add(_obj);
                }
            }
            return _objList;
        }
        private T Execute<T>(DataColumnCollection column, DataRow row)
        {

            var _object = (T)Activator.CreateInstance(typeof(T));
            var properties = _object.GetType().GetProperties();
            foreach (var property in properties)
            {
                var x = property.GetCustomAttributes(true).Count() > 0 ? property.GetCustomAttributes(true)[0].GetType().Name : null;
                var index = column.IndexOf(property.Name);
                if (index != -1)
                {
                    if (property.PropertyType.IsClass && !property.PropertyType.IsPrimitive)
                    {
                        var val = ConvertObject(property, row[index]);
                        property.SetValue(_object, val, null);
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(_object, row[index], null);
                        }
                        catch
                        {
                            property.SetValue(_object, default(T), null);
                        }
                    }
                }

            }
            return _object;
        }
        private object ConvertObject(PropertyInfo prop, object val)
        {
            object convertedValue = null;
            if (!prop.PropertyType.IsClass)
            {
                convertedValue = Convert.ChangeType(val, Type.GetType(prop.PropertyType.Name));
            }
            else
            {
                var propType = prop.PropertyType.Name;
                switch (propType)
                {
                    case nameof(TypeCode.String):
                        convertedValue = val.ToString();
                        break;
                    case nameof(TypeCode.Int32):
                        int i;
                        int.TryParse(val.ToString(), out i);
                        convertedValue = i;
                        break;
                    case nameof(TypeCode.Boolean):
                        bool b;
                        Boolean.TryParse(val.ToString(), out b);
                        convertedValue = b;
                        break;
                    case nameof(TypeCode.DateTime):
                        DateTime d;
                        DateTime.TryParse(val.ToString(), out d);
                        convertedValue = d;
                        break;
                    case nameof(System.Guid):
                        convertedValue = new Guid(val.ToString());
                        break;
                    case nameof(System.Type):
                        convertedValue = Type.GetType(val.ToString());
                        break;

                    default:
                        convertedValue = val.ToString();
                        break;
                }
            }
            return convertedValue;
        }


    }
}
