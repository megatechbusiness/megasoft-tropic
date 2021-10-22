using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.EntityClient;
using System.Web;



namespace Megasoft2
{
    public class clsData
    {
        public string BuildSysproConnection()
        {
            try
            {
                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["WarehouseManagementEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'SysproEntities' in web.config file");
                }
                string sysproConnectionString = sysproSettings.ConnectionString;
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                string DataSource = sqlConnectionStringBuilder.DataSource;
                string Database = HttpContext.Current.Request.Cookies.Get("SysproDatabase").Value;
                string password = sqlConnectionStringBuilder.Password;
                string userId = sqlConnectionStringBuilder.UserID;
                return @"Data Source=" + DataSource + ";Initial Catalog=" + Database + ";Persist Security Info=True;User ID=" + userId + ";Password=" + password + ";Connect Timeout=120;Asynchronous Processing=True;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;Packet Size=32768";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public DataTable SelectData(string strSQL)
        {
            try
            {
                DataTable dt = new DataTable();

                SqlConnection con = new SqlConnection();
                con.ConnectionString = BuildSysproConnection();
                SqlDataAdapter adap = new SqlDataAdapter();

                SqlCommand comm = new SqlCommand();
                comm.CommandText = strSQL;
                comm.Connection = con;
                adap.SelectCommand = comm;
                adap.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " DataAccessLayer.clsData.SelectData");
            }
        }


        public void Execute(string strSQL)
        {
            try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = BuildSysproConnection();
                con.Open();

                SqlCommand comm = new SqlCommand();
                comm.Connection = con;
                comm.CommandText = strSQL;
                comm.ExecuteNonQuery();

                comm.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}