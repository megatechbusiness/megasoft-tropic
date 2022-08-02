using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using CrystalDecisions.CrystalReports.Engine;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using CrystalDecisions.Shared;
using Megasoft2.BusinessLogic;

namespace Megasoft2.Controllers
{
    public class GrnEnquiryController : Controller
    {
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        Email _email = new Email();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        //
        // GET: /GrnEnquiry/
        [CustomAuthorize(Activity: "GrnEnquiry")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Grn = sdb.sp_GetGrnEnquiryHeader(HttpContext.User.Identity.Name.ToUpper(), Company, "").ToList();
            return View(Grn);
        }
        public JsonResult Search(string FilterText)
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var GRN = sdb.sp_GetGrnEnquiryHeader(User, Company, FilterText.ToUpper()).ToList();
            return Json(GRN, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GrnDetail(string Grn)
        {
            try
            {
                var result = sdb.sp_GetGrnLinesForInvoicing(Grn).ToList();//(from a in sdb.mtRequisitionDetails where a.Requisition == Requisition select a).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpGet]
        public ActionResult EmailGrn(string Grn)
        {
            try
            {

                var Useremail = (from a in mdb.mtUsers where a.Username == HttpContext.User.Identity.Name.ToUpper() select a.EmailAddress).FirstOrDefault();
                var ExportFile = new ExportFile();
                ExportFile = this.ExportGrn(Grn);
                var objPoMail = new PurchaseOrderEmail();
                objPoMail.PurchaseOrder = Grn;
                objPoMail.Supplier = "";
                objPoMail.FromEmail = Useremail;
                objPoMail.AttachmentPath = ExportFile.FilePath;
                objPoMail.FileName = ExportFile.FileName;
                return View(objPoMail);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new PurchaseOrderEmail());
            }
        }


        [HttpPost]
        public ActionResult EmailGrn(PurchaseOrderEmail model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.ToEmail))
                {
                    ModelState.AddModelError("", "TO email required!");
                    return View(model);
                }

                Mail objMail = new Mail();
                objMail.From = model.FromEmail;
                objMail.To = model.ToEmail;
                objMail.Subject = model.Subject;
                objMail.Body = model.MessageBody;
                if (!string.IsNullOrEmpty(model.CCEmail))
                {
                    objMail.CC = model.CCEmail;
                }


                List<string> attachments = new List<string>();
                attachments.Add(model.AttachmentPath);
                _email.SendEmail(objMail, attachments, "GeneralEmail");
                ModelState.AddModelError("", "Email Sent Successfully!");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ExportFile ExportGrn(string Grn)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "Requisition" && a.Report == "Grn" select a.ReportPath).FirstOrDefault().Trim();
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

                rpt.SetParameterValue("@Grn", Grn);


                string FilePath = HttpContext.Server.MapPath("~/RequisitionSystem/Grn/");

                string FileName = Grn + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\RequisitionSystem\Grn\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;


                try
                {
                    string[] files = Directory.GetFiles(HttpContext.Server.MapPath("~/RequisitionSystem/Grn/"));

                    foreach (string delFile in files)
                    {
                        FileInfo fi = new FileInfo(delFile);
                        if (fi.LastWriteTime < DateTime.Now.AddDays(-1))
                            fi.Delete();
                    }
                }
                catch (Exception err)
                {
                    throw new Exception(err.Message);
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
