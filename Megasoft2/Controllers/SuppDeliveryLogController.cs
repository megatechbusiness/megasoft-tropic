using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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
    public class SuppDeliveryLogController : Controller
    {
        //
        // GET: /SuppDeliveryLog/
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");

        [HttpGet]
        [CustomAuthorize(Activity: "SupplierDeliveryLog")]
        public ActionResult Index()
        {
            var Employees = db.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "RECEIVER" select new { Employee = a.Employee, Description = a.Employee }).ToList();
            SuppDeliveryLogViewModel model = new SuppDeliveryLogViewModel();
            model.Date = DateTime.Now;
            ViewBag.Access = false;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Index")]
        [HttpPost]
        public ActionResult Index(SuppDeliveryLogViewModel model)
        {
            var Employees = db.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "RECEIVER" select new { Employee = a.Employee, Description = a.Employee }).ToList();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            string Po = model.PurchaseOrder.PadLeft(15, '0');
            var result = db.sp_GetPoLabelLines(Po, Username, Company).ToList();

            try
            {

                var obj = new mtSuppDeliveryLog()
                {
                    Supplier = model.Supplier,
                    TransactionDate = model.Date.Date + ((DateTime)model.Time).TimeOfDay,
                    PurchaseOrder = model.PurchaseOrder,
                    StockCode = model.SuppLog.StockCode,
                    Quantity = model.SuppLog.Quantity,
                    Uom = model.SuppLog.Uom,
                    Line = model.SuppLog.Line,
                    ProductApperance = model.SuppLog.ProductApperance,
                    ProductCondition = model.SuppLog.ProductCondition,
                    DeliveryTruckCondition = model.SuppLog.DeliveryTruckCondition,
                    AcceptedRejected = model.SuppLog.AcceptedRejected,
                    lblPrint = model.SuppLog.lblPrint,
                    Username = Username,
                    SystemDate = DateTime.Now,
                    Reciever = model.Employee,
                    Comments = model.SuppLog.Comments,
                    SupplierRef = model.SuppLog.SupplierRef,
                    Description = model.SuppLog.Description
                };

                db.mtSuppDeliveryLogs.Add(obj);
                db.SaveChanges();

                var ListResult = db.sp_SupplierDeliveryGetLogByReciever(obj.TransactionDate, obj.Reciever).ToList();
                model.GetLogReciever = ListResult;
                model.Date = obj.TransactionDate;
                model.Employee = obj.Reciever;
                ModelState.AddModelError("", "Capture Logged Successfully");
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

        [MultipleButton(Name = "action", Argument = "LoadReciever")]
        [HttpPost]
        public ActionResult LoadReciever(SuppDeliveryLogViewModel model)
        {
            var Employees = db.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "RECEIVER" select new { Employee = a.Employee, Description = a.Employee }).ToList();

            try
            {
                var result = db.sp_SupplierDeliveryGetLogByReciever(model.Date, model.Employee).ToList();
                model.GetLogReciever = result;
                ModelState.AddModelError("", "Logs Loaded Successfully");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }

        public ActionResult DeleteDetailLine(int Id)
        {
            SuppDeliveryLogViewModel Vmodel = new SuppDeliveryLogViewModel();
            var Employees = db.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "RECEIVER" select new { Employee = a.Employee, Description = a.Employee }).ToList();

            try
            {
                var DeleteLine = db.mtSuppDeliveryLogs.Find(Id);
                db.mtSuppDeliveryLogs.Remove(DeleteLine);
                db.SaveChanges();

                var ListResult = db.sp_SupplierDeliveryGetLogByReciever(DeleteLine.TransactionDate, DeleteLine.Reciever).ToList();
                Vmodel.GetLogReciever = ListResult;
                Vmodel.Date = DeleteLine.TransactionDate;
                Vmodel.Employee = DeleteLine.Reciever;
                return View("Index", Vmodel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", Vmodel);
            }

        }

        public ActionResult SupplierSearch()
        {
            return PartialView();
        }

        public ActionResult SupplierList(string FilterText)
        {
            var result = db.sp_GetSuppliers(FilterText.ToUpper()).ToList();

            try
            {
                if (result.Count > 0)
                {
                    ViewBag.supplier = result.Where(x => x.Supplier == FilterText);
                }
            }
            catch (Exception)
            {
                throw;
            }
            ViewBag.supplier = result.FirstOrDefault().Supplier;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SupplierPO()
        {
            return PartialView();
        }

        public ActionResult SupplierPOList(string FilterText)
        {
            var result = db.sp_GetPurchaseOrderLinesBySupplier(FilterText);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PrintPdf")]
        public ActionResult PrintPdf(SuppDeliveryLogViewModel model)
        {
            var Employees = db.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "RECEIVER" select new { Employee = a.Employee, Description = a.Employee }).ToList();

            model.PrintPdf = ExportPdf(model.Date, model.Employee);
            return View("Index", model);
        }

        public ExportFile ExportPdf(DateTime TransactionDate, string Reciever)
        {
            try
            {
                var ReportPath = (from a in db.mtReportMasters where a.Program == "DynamicReports" && a.Report == "SupplierDeliveryLog" select a.ReportPath).FirstOrDefault().Trim();
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

                rpt.SetParameterValue("@TransactionDate", TransactionDate.ToString("yyyy-MM-dd"));
                rpt.SetParameterValue("@Receiver", Reciever);

                string FilePath = HttpContext.Server.MapPath("~/Reports/SupplierDeliveryLog/");

                string FileName = "DeliveryLog_" + DateTime.Now.ToString("yyyy_MM_dd") + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);
                ViewBag.Access = true;
                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\Reports\SupplierDeliveryLog\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
