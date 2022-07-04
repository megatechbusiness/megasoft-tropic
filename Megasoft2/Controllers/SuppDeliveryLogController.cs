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
using System.Data;

namespace Megasoft2.Controllers
{
    public class SuppDeliveryLogController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");

        [CustomAuthorize(Activity: "SupplierDeliveryLog")]
        public ActionResult Index(string Supplier = null, string DeliveryNote = null)
        {
            SuppDeliveryLogViewModel model = new SuppDeliveryLogViewModel();

            try
            {
                if (!string.IsNullOrEmpty(Supplier) && !string.IsNullOrEmpty(DeliveryNote))
                {
                    model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == Supplier && a.SupplierRef == DeliveryNote) select a).ToList();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            SetViewBags(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(SuppDeliveryLogViewModel model)
        {
            ModelState.Clear();

            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {
                //make sure user inputs supplier and delivery note values
                if (string.IsNullOrEmpty(model.Supplier) || string.IsNullOrEmpty(model.DeliveryNote))
                {
                    throw new Exception("Please enter BOTH a Supplier and Delivery Note");
                }

                //check if supplier exists
                var suppCheck = db.sp_GetSuppliers("").Where(a => a.Supplier == model.Supplier).FirstOrDefault();
                if (suppCheck == null)
                {
                    throw new Exception("Supplier does not exist");
                }

                //load entries
                if (!string.IsNullOrEmpty(model.Supplier) && !string.IsNullOrEmpty(model.DeliveryNote))
                {
                    model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == model.Supplier && a.SupplierRef == model.DeliveryNote) select a).ToList();

                    if (model.DeliveryLogList.Count > 0)
                    {
                        model.SuppLog = new mtSuppDeliveryLog();
                        model.SuppLog.TransactionDate = model.DeliveryLogList.FirstOrDefault().TransactionDate;
                        model.SuppLog.Reciever = model.DeliveryLogList.FirstOrDefault().Reciever;
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            SetViewBags(model);
            return View(model);
        }

        public ActionResult LogEntry(int Id)
        {
            SuppDeliveryLogViewModel model = new SuppDeliveryLogViewModel();

            try
            {
                //build drop down
                model.EmployeeList = (from a in db.sp_BaggingLabelEmployees() where a.ProcessTask == "RECEIVER" select a).ToList();
                
                model.SuppLog = new mtSuppDeliveryLog();

                //fill field values
                if (Id != -1)
                {
                    var entry = (from a in db.mtSuppDeliveryLogs where a.Id == Id select a).FirstOrDefault();

                    if (entry != null)
                    {
                        model.SuppLog = entry;
                    }

                    model.SuppLog.ValidPO = "N";
                }
                else
                {
                    model.SuppLog.TransactionDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {

            }

            return PartialView(model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveLogEntry")]
        public ActionResult SaveLogEntry(SuppDeliveryLogViewModel model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                string Username = HttpContext.User.Identity.Name.ToUpper();

                //check if entry already exists
                var check = (from a in db.mtSuppDeliveryLogs where a.Id == model.SuppLog.Id select a).FirstOrDefault();

                if (check != null) //update existing
                {
                    //add -NO PO to purchase order if not valid
                    if (model.SuppLog.ValidPO == "N")
                    {
                        if (!model.SuppLog.PurchaseOrder.Contains("-NO PO"))
                        {
                            model.SuppLog.PurchaseOrder = model.SuppLog.PurchaseOrder.TrimStart('0');
                            model.SuppLog.PurchaseOrder += "-NO PO";
                            model.SuppLog.PurchaseOrder = model.SuppLog.PurchaseOrder.PadLeft(15, '0');
                        }
                    }

                    check.Supplier = model.SuppLog.Supplier;
                    check.TransactionDate = model.SuppLog.TransactionDate;
                    check.PurchaseOrder = model.SuppLog.PurchaseOrder;
                    check.StockCode = model.SuppLog.StockCode;
                    check.Quantity = model.SuppLog.Quantity;
                    check.Uom = model.SuppLog.Uom;
                    check.Line = model.SuppLog.Line;
                    check.ProductApperance = model.SuppLog.ProductApperance;
                    check.ProductCondition = model.SuppLog.ProductCondition;
                    check.DeliveryTruckCondition = model.SuppLog.DeliveryTruckCondition;
                    check.AcceptedRejected = model.SuppLog.AcceptedRejected;
                    check.lblPrint = model.SuppLog.lblPrint;
                    check.Username = Username;
                    check.SystemDate = DateTime.Now;
                    check.Reciever = model.SuppLog.Reciever;
                    check.Comments = model.SuppLog.Comments;
                    check.SupplierRef = model.SuppLog.SupplierRef;
                    check.Description = model.SuppLog.Description;
                    check.ValidPO = model.SuppLog.ValidPO;

                    db.Entry(check).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var obj = new mtSuppDeliveryLog()
                    {
                        Supplier = model.SuppLog.Supplier,
                        TransactionDate = model.SuppLog.TransactionDate,
                        PurchaseOrder = model.SuppLog.PurchaseOrder,
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
                        Reciever = model.SuppLog.Reciever,
                        Comments = model.SuppLog.Comments,
                        SupplierRef = model.SuppLog.SupplierRef,
                        Description = model.SuppLog.Description,
                        ValidPO = model.SuppLog.ValidPO
                    };

                    db.mtSuppDeliveryLogs.Add(obj);
                    db.SaveChanges();
                }

                //rebuild log list
                if (!string.IsNullOrEmpty(model.Supplier) && !string.IsNullOrEmpty(model.DeliveryNote))
                {
                    model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == model.Supplier && a.SupplierRef == model.DeliveryNote) select a).ToList();
                }
                else
                {
                    model.Supplier = model.SuppLog.Supplier;
                    model.DeliveryNote = model.SuppLog.SupplierRef;
                    model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == model.SuppLog.Supplier && a.SupplierRef == model.SuppLog.SupplierRef) select a).ToList();
                }

                ModelState.AddModelError("", "Capture Logged Successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                //rebuild log list
                if (!string.IsNullOrEmpty(model.Supplier) && !string.IsNullOrEmpty(model.DeliveryNote))
                {
                    model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == model.Supplier && a.SupplierRef == model.DeliveryNote) select a).ToList();
                }
                else
                {
                    model.Supplier = model.SuppLog.Supplier;
                    model.DeliveryNote = model.SuppLog.SupplierRef;
                    model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == model.SuppLog.Supplier && a.SupplierRef == model.SuppLog.SupplierRef) select a).ToList();
                }
            }

            SetViewBags(model);
            return View("Index", model);
        }

        public ActionResult DeleteDetailLine(int Id)
        {
            SuppDeliveryLogViewModel model = new SuppDeliveryLogViewModel();

            try
            {
                var DeleteLine = db.mtSuppDeliveryLogs.Find(Id);
                db.mtSuppDeliveryLogs.Remove(DeleteLine);
                db.SaveChanges();
                
                model.Supplier = DeleteLine.Supplier;
                model.DeliveryNote = DeleteLine.SupplierRef;

                //rebuild log list
                model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == DeleteLine.Supplier && a.SupplierRef == DeleteLine.SupplierRef) select a).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            SetViewBags(model);
            return View("Index", model);
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

        public ActionResult PoLineSearch()
        {
            return PartialView();
        }

        public ActionResult PoLineList(string PurchaseOrder)
        {
            var result = (from a in db.PorMasterDetails
                          join b in db.PorMasterHdrs
                          on a.PurchaseOrder equals b.PurchaseOrder
                          where b.PurchaseOrder == PurchaseOrder
                          select new { a.PurchaseOrder, a.Line, a.MStockCode, a.MStockDes, a.MWarehouse, a.MOrderUom, a.MOrderQty, a.MReceivedQty, OutstandingQty = a.MOrderQty - a.MReceivedQty, b.Supplier })
                          .ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PrintPdf")]
        public ActionResult PrintPdf(SuppDeliveryLogViewModel model)
        {
            model.PrintPdf = ExportPdf(model.SuppLog.TransactionDate, model.SuppLog.Reciever);

            //rebuild log list
            model.DeliveryLogList = (from a in db.mtSuppDeliveryLogs where (a.Supplier == model.Supplier && a.SupplierRef == model.DeliveryNote) select a).ToList();
            SetViewBags(model);

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
                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @".\Reports\SupplierDeliveryLog\" + FileName;
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ActionResult PurchaseOrderValidation(string PurchaseOrder)
        {
            PurchaseOrder = PurchaseOrder.PadLeft(15, '0');

            var check = (from a in db.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a).ToList();
            bool valid = false;

            if (check.Count > 0)
            {
                valid = true;
            }

            return Json(valid, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSupplierForPo(string PurchaseOrder)
        {
            PurchaseOrder = PurchaseOrder.PadLeft(15, '0');

            var supplier = (from a in db.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a.Supplier).FirstOrDefault();
            return Json(supplier, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Activity: "SupplierDeliveryLog")]
        public ActionResult SupplierDeliveryReport()
        {
            SuppDeliveryLogViewModel model = new SuppDeliveryLogViewModel();

            try
            {
                var result = db.mt_SupplierDeliveryLogReport().ToList();
                model.ReportList = result;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        public void SetViewBags(SuppDeliveryLogViewModel model)
        {
            //can show table or not
            if (model.DeliveryLogList != null)
            {
                ViewBag.ShowTable = true;
            }

            //search field values are valid
            if (!string.IsNullOrEmpty(model.Supplier) && !string.IsNullOrEmpty(model.DeliveryNote))
            {
                var suppCheck = db.sp_GetSuppliers("").Where(a => a.Supplier == model.Supplier).FirstOrDefault();
                if (suppCheck == null)
                {
                    ViewBag.IsValid = false;
                }
                else
                {
                    ViewBag.IsValid = true;
                }
            }
            else
            {
                ViewBag.IsValid = false;
            }
        }
    }
}
