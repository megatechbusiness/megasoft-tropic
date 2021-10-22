using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class PoEnquiryController : Controller
    {
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        Email _email = new Email();
        RequisitionBL BL = new RequisitionBL();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        //
        // GET: /PoEnquiry/
        [CustomAuthorize(Activity: "PoEnquiry")]
        public ActionResult Index()
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var PO = sdb.sp_GetPurchaseOrderEnquiryList(User, Company, "").ToList();
            return View(PO);

        }
        public JsonResult Search(string FilterText)
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var PO = sdb.sp_GetPurchaseOrderEnquiryList(User, Company, FilterText.ToUpper()).ToList();
            return Json(PO, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PoDetail(string PurchaseOrder)
        {
            try
            {
                var result = sdb.sp_GetPurchaseOrderLines(PurchaseOrder.PadLeft(15, '0')).ToList();//(from a in sdb.mtRequisitionDetails where a.Requisition == Requisition select a).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpGet]
        public ActionResult EmailPo(string PurchaseOrder)
        {
            try
            {
                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                var Supplier = (from a in sdb.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a).FirstOrDefault();
                var Useremail = (from a in mdb.mtUsers where a.Username == HttpContext.User.Identity.Name.ToUpper() select a.EmailAddress).FirstOrDefault();
                var ExportFile = new ExportFile();
                ExportFile = this.ExportPurchaseOrder(PurchaseOrder);
                var objPoMail = new PurchaseOrderEmail();
                objPoMail.PurchaseOrder = PurchaseOrder;
                objPoMail.Supplier = Supplier.Supplier;
                objPoMail.FromEmail = Useremail;
                objPoMail.CCEmail = Useremail;
                objPoMail.Subject = "Purchase Order : " + PurchaseOrder;
                objPoMail.ToEmail = BL.GetSupplierFirstContact(Supplier.Supplier);
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
        public ActionResult EmailPo(PurchaseOrderEmail model)
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
                _email.SendEmail(objMail, attachments);
                ModelState.AddModelError("", "Email Sent Successfully!");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ExportFile ExportPurchaseOrder(string PurchaseOrder)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "Requisition" && a.Report == "PurchaseOrder" select a.ReportPath).FirstOrDefault().Trim();
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

                rpt.SetParameterValue("@PurchaseOrder", PurchaseOrder);


                string FilePath = HttpContext.Server.MapPath("~/RequisitionSystem/PurchaseOrder/");

                string FileName = PurchaseOrder + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\RequisitionSystem\PurchaseOrder\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;

                try
                {
                    string[] files = Directory.GetFiles(HttpContext.Server.MapPath("~/RequisitionSystem/PurchaseOrder/"));

                    foreach (string delFile in files)
                    {
                        FileInfo fi = new FileInfo(delFile);
                        if (fi.LastWriteTime < DateTime.Now.AddDays(-1))
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
                throw new Exception("Report Export Error: " + ex.Message);
            }
        }

    }
}
