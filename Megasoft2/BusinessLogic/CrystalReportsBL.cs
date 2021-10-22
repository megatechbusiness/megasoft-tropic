using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.IO;
using CrystalDecisions.Shared;
using Megasoft2.Models;

namespace Megasoft2.BusinessLogic
{
    public class CrystalReportsBL
    {
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        public string PrintToPdf(string Guid, string Report, string Operator, string FilePath)
        {
            var ReportPath = (from a in wdb.mtReportMasters where a.Program == "TankSystem" && a.Report == Report select a.ReportPath).FirstOrDefault().Trim();
            ReportDocument rpt = new ReportDocument();
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

            rpt.SetParameterValue("@GUID", Guid);

            string FileName = FilePath + "\\" + Report + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf";

            //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
            rpt.ExportToDisk(ExportFormatType.PortableDocFormat, FileName);
            rpt.Close();
            rpt.Dispose();
            GC.Collect();

            return FileName;
        }



        public ExportFile ExportRequestForQuote(string Requisition, string Supplier)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "Requisition" && a.Report == "RequestForQuote" select a.ReportPath).FirstOrDefault().Trim();
                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportPath);

                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["SysproEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'SysproEntities' in web.config file");
                }
                string sysproConnectionString = sysproSettings.ConnectionString;
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                string password = sqlConnectionStringBuilder.Password;
                string userId = sqlConnectionStringBuilder.UserID;

                rpt.SetDatabaseLogon(userId, password);

                rpt.SetParameterValue("@Requisition", Requisition);
                rpt.SetParameterValue("@Supplier", Supplier);

                string FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/");

                string FileName = Requisition.Replace("/", "_") + "_" + Supplier + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\RequisitionSystem\RequestForQuote\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;

                try
                {
                    string[] files = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/"));

                    foreach (string delFile in files)
                    {
                        FileInfo fi = new FileInfo(delFile);
                        if (fi.LastAccessTime < DateTime.Now.AddDays(-1))
                            fi.Delete();
                    }
                }
                catch (Exception err)
                {

                }

                return file;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}