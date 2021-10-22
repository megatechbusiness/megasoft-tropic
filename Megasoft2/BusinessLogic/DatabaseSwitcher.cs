using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public static class DatabaseSwitcher
    {
        
        public static string Connstr(string EntityName)//Entity Name determines which .Edmx we using i.e. Syspro.edmx or WarehouseManagement.edmx //Metadata is also based on this.
        {
            MegasoftEntities mdb = new MegasoftEntities();
            HttpCookie database = HttpContext.Current.Request.Cookies.Get("SysproDatabase");

            

            ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings[EntityName];
            if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
            {
                throw new Exception("Fatal error: Missing connection string 'SysproEntities' in web.config file");
            }
            string sysproConnectionString = sysproSettings.ConnectionString;
            EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);

            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

            string password = sqlConnectionStringBuilder.Password;
            string userId = sqlConnectionStringBuilder.UserID;
            string Datasource = sqlConnectionStringBuilder.DataSource;
            string Database = sqlConnectionStringBuilder.InitialCatalog;

            string DatabaseToUse = "";
            if (database == null) //Added for Web Api Purpose
            {
                DatabaseToUse = Database;
            }
            else
            {
                DatabaseToUse = database.Value;
            }
            
            EntityConnectionStringBuilder ef = new EntityConnectionStringBuilder();
            if (EntityName == "SysproEntities")
            {
                ef.Metadata = "res://*/Syspro.csdl|res://*/Syspro.ssdl|res://*/Syspro.msl";
            }
            else if (EntityName == "WarehouseManagementEntities")
            {
                ef.Metadata = "res://*/WarehouseManagement.csdl|res://*/WarehouseManagement.ssdl|res://*/WarehouseManagement.msl";
            }
            else
            {
                ef.Metadata = "res://*/StockTake.csdl|res://*/StockTake.ssdl|res://*/StockTake.msl";
            }
            ef.Provider = "System.Data.SqlClient";
            ef.ProviderConnectionString = "data source=" + Datasource + ";initial catalog=" + DatabaseToUse + ";persist security info=True;user id=" + userId + ";password=" + password + ";MultipleActiveResultSets=True;App=EntityFramework&quot;";


            return ef.ToString();
        }
    }
}