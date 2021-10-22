using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.Shared;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.EntityClient;



namespace Megasoft2.CrystalReports
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        ReportDocument rpt = new ReportDocument();
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if(!Page.IsPostBack)
                {
                    LoadReport();
                }
                
            }
            catch(Exception ex)
            {
                lblError.Text = ex.Message;
            }            
        }

        public void LoadReport()
        {
            try
            {
                string entryGuid = Request.QueryString["entryGuid"];
                string report = Request.QueryString["Report"];

                if(!string.IsNullOrEmpty(entryGuid))
                {


                    var ReportPath = (from a in wdb.mtReportMasters where  a.Report == report select a.ReportPath).FirstOrDefault().Trim();

                    rpt.Load(ReportPath);

                    ConnectionStringSettings megasoftSettings = ConfigurationManager.ConnectionStrings["MegasoftEntities"];
                    if (megasoftSettings == null || string.IsNullOrEmpty(megasoftSettings.ConnectionString))
                    {
                        throw new Exception("Fatal error: Missing connection string 'MegasoftEntities' in web.config file");
                    }
                    string megasoftConnectionString = megasoftSettings.ConnectionString;
                    EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(megasoftConnectionString);
                    SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                    string password = sqlConnectionStringBuilder.Password;
                    string userId = sqlConnectionStringBuilder.UserID;
                    rpt.SetDatabaseLogon(userId, password);
                    if (report != "TransportScheduleEmailDocument")
                    {
                        rpt.SetParameterValue("@GUID", entryGuid);
                    }
                    else
                    {
                        string Date = entryGuid.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        string Customer = entryGuid.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        string Location = entryGuid.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)[2];
                        rpt.SetParameterValue("@FromDate", Date.Trim());
                        rpt.SetParameterValue("@ToDate", Date.Trim());
                        rpt.SetParameterValue("Customer", Customer.Trim());
                        rpt.SetParameterValue("Location", Location.Trim());
                    }

                    CrystalReportViewer1.ReportSource = rpt;
                }
                else
                {
                    var ReportPath = (from a in wdb.mtReportMasters where  a.Report == report select a.ReportPath).FirstOrDefault().Trim();
                    rpt.Load(ReportPath);

                    ConnectionStringSettings megasoftSettings = ConfigurationManager.ConnectionStrings["MegasoftEntities"];
                    if (megasoftSettings == null || string.IsNullOrEmpty(megasoftSettings.ConnectionString))
                    {
                        throw new Exception("Fatal error: Missing connection string 'MegasoftEntities' in web.config file");
                    }
                    string megasoftConnectionString = megasoftSettings.ConnectionString;
                    EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(megasoftConnectionString);
                    SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                    string password = sqlConnectionStringBuilder.Password;
                    string userId = sqlConnectionStringBuilder.UserID;

                    //string Server = sqlConnectionStringBuilder.DataSource;
                    //string Database = sqlConnectionStringBuilder.InitialCatalog;


                    rpt.SetDatabaseLogon(userId, password);

                    CrystalReportViewer1.ReportSource = rpt;
                
                }        
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected void CrystalReportViewer1_Init(object sender, EventArgs e)
        {
            try
            {
                LoadReport();

            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (rpt != null)
            {
                this.CrystalReportViewer1.Dispose();
                this.CrystalReportViewer1 = null;
                rpt.Close();
                rpt.Dispose();
                GC.Collect();
            }
        }

       
    }
}