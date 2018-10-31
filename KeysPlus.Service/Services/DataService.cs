using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KeysPlus.Service.Services
{
    
    public static class DataService
    {
        static string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //private static SqlConnection Conn = new SqlConnection(connString);
        public static void ExecuteStoredProcedure(string procedureName)
        {
            SqlConnection sqlConnObj = new SqlConnection(connString);

            SqlCommand sqlCmd = new SqlCommand(procedureName, sqlConnObj);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlConnObj.Open();
            sqlCmd.ExecuteNonQuery();
            sqlConnObj.Close();
        }

        public static List<T>  ExecuteStoredProcedure<T>(string procedureName, object model) where T : new()
        {
            List<T> objects = new List<T>();
            SqlDataReader rdr = null;
            var parameters = GenerateSQLParameters(model);
            SqlConnection sqlConnObj = new SqlConnection(connString);

            SqlCommand sqlCmd = new SqlCommand(procedureName, sqlConnObj);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            
            foreach (var param in parameters)
            {
                sqlCmd.Parameters.Add(param);
            }
            sqlConnObj.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCmd);
            DataSet outputDataSet = new DataSet();
            try
            {
                sqlDataAdapter.Fill(outputDataSet, "resultset");
            }
            catch (SystemException ex)
            {
                throw ex; 
            }
            rdr = sqlCmd.ExecuteReader();
            IDataReader reader = (IDataReader)rdr;
            while (reader.Read())
            {
                T tempObject = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetValue(i) != DBNull.Value)
                    {
                        var name = reader.GetName(i);
                        PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                        if(propertyInfo != null)
                            propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                    }
                }

                objects.Add(tempObject);
            }
            sqlConnObj.Close();
            reader.Close();
            return objects;
        }

        private static List<SqlParameter> GenerateSQLParameters(object model)
        {
            var paramList = new List<SqlParameter>();
            Type modelType = model.GetType();
            var properties = modelType.GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(model) == null)
                {
                    paramList.Add(new SqlParameter(property.Name, DBNull.Value));
                }
                else
                {
                    paramList.Add(new SqlParameter(property.Name, property.GetValue(model)));
                }
            }
            return paramList;

        }
    }
}
