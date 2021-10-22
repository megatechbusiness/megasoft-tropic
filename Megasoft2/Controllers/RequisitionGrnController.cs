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
    public class RequisitionGrnController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        PurchaseOrderGrn objGrn = new PurchaseOrderGrn();
        RequisitionBL BL = new RequisitionBL();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        [CustomAuthorize("Grn", "GrnViewOnly")]
        public ActionResult Index()
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            List<sp_PurchaseOrderGrnList_Result> PoList = new List<sp_PurchaseOrderGrnList_Result>();

            //PoList = sdb.sp_PurchaseOrderGrnList(Company, User).ToList();


            return View(PoList);
        }

        [CustomAuthorize("Grn", "GrnViewOnly")]
        public ActionResult Create(string PurchaseOrder, string Grn = null)
        {

            try
            {
                PoGrn GrnDetail = new PoGrn();
                GrnDetail.GrnLines = sdb.sp_GetPurchaseOrderLinesForGrn(PurchaseOrder.PadLeft(15, '0'), Grn).ToList();
                GrnDetail.PurchaseOrder = PurchaseOrder;
                GrnDetail.Grn = Grn;
                ViewBag.check = BL.ProgramAccess("Grn");
                return View(GrnDetail);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [CustomAuthorize(Activity: "Grn")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PoGrn model)
        {
            try
            {
                ModelState.Clear();
                if (ModelState.IsValid == true)
                {
                    bool HasErrors = false;
                    if (model.DeliveryNote == null)
                    {
                        ModelState.AddModelError("", "Delivery Note cannot be blank.");
                        HasErrors = true;
                    }

                    if (model.ReceivedBy == null)
                    {
                        ModelState.AddModelError("", "Received By cannot be blank.");
                        HasErrors = true;
                    }

                    string Branch = model.GrnLines.FirstOrDefault().Branch;
                    string Site = model.GrnLines.FirstOrDefault().Site;

                    var SuspenseAccount = (from a in sdb.mtBranchSites where a.Branch == Branch && a.Site == Site select a).ToList();
                    if (SuspenseAccount.Count == 0)
                    {
                        ModelState.AddModelError("", "Failed to retrieve Branch/Site information. Please contact your administrator.");
                        HasErrors = true;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(SuspenseAccount.FirstOrDefault().AccrualSuspenseAcc))
                        {
                            ModelState.AddModelError("", "No Accrual Suspense Account defined against Site Setup. Please contact your administrator.");
                            HasErrors = true;
                        }
                    }

                    if (model.DeliveryNoteDate > DateTime.Now.Date)
                    {
                        ModelState.AddModelError("", "Delivery Date cannot be after today!");
                        HasErrors = true;
                    }


                    var PostDates = sdb.sp_GetPostingPeriod(model.DeliveryNoteDate).ToList();
                    if (PostDates.Count == 0)
                    {
                        ModelState.AddModelError("", "Delivery Date out of range of Posting Period.");
                        HasErrors = true;
                    }


                    var DelNote = (from a in sdb.mtGrnDetails where a.PurchaseOrder == model.PurchaseOrder && a.DeliveryNote == model.DeliveryNote select a).ToList();
                    if (DelNote.Count > 0)
                    {
                        ModelState.AddModelError("", "Delivery note : " + model.DeliveryNote + " already exists for Purchase Order : " + model.PurchaseOrder + ".");
                        HasErrors = true;
                    }

                    var TotalRecQty = (model.GrnLines.Sum(x => x.GrnQty));
                    if (TotalRecQty == 0)
                    {
                        ModelState.AddModelError("", "Grn quantity required for one or more lines.");
                        HasErrors = true;
                    }


                    //if(BL.isAdmin() == false)
                    //{
                    foreach (var line in model.GrnLines)
                    {

                        if (line.GrnQty < 0)
                        {
                            ModelState.AddModelError("", "Grn Quantity cannot be negative for line :" + line.Line);
                            HasErrors = true;
                        }

                        if (line.GrnQty > line.OutstandingQty)
                        {
                            ModelState.AddModelError("", "Over receipting not allowed for Line :" + line.Line);
                            HasErrors = true;
                        }
                    }
                    //}

                    //check added to prevent double grn if user clicks refresh on the browser.
                    //if results return nothing, it means the PO has already been fully Grn'd.
                    //if it returns data then we check the grn qty against the outstanding qty
                    using (var vdb = new SysproEntities(""))
                    {
                        var dupcheck = sdb.sp_GetPurchaseOrderLinesForGrn(model.PurchaseOrder.PadLeft(15, '0'), null).ToList();
                        if (dupcheck.Count > 0)
                        {
                            foreach (var line in dupcheck)
                            {
                                var gline = (from a in model.GrnLines where a.Line == line.Line select a).FirstOrDefault();
                                if (gline.GrnQty > line.OutstandingQty)
                                {
                                    ModelState.AddModelError("", "Over receipting not allowed for Line :" + line.Line);
                                    HasErrors = true;
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "No outstanding lines found for Purchase Order");
                            HasErrors = true;
                        }
                    }



                    //var Supplier = model.GrnLines.FirstOrDefault().Supplier;
                    //var Sup = (from a in sdb.ApSuppliers where a.Supplier == Supplier && a.GrnMatchReqd == "Y" select a).ToList();
                    //if(Sup.Count == 0)
                    //{
                    //    //ModelState.AddModelError("", "Grn Matching required for Supplier :" + Supplier + ". Please contact your administrator.");
                    //    //HasErrors = true;
                    //}

                    PoGrn GrnDetail = new PoGrn();
                    if (HasErrors == false)
                    {
                        GrnDetail.Grn = objGrn.SaveGrn(model);
                    }

                    if (GrnDetail.Grn != null)
                    {
                        GrnDetail.GrnReport = this.ExportGrn(GrnDetail.Grn);
                        ModelState.AddModelError("", "Grn Completed Successfully. Grn : " + GrnDetail.Grn);
                        GrnDetail.GrnLines = sdb.sp_GetPurchaseOrderLinesForGrn(model.PurchaseOrder.PadLeft(15, '0'), GrnDetail.Grn).ToList();
                        GrnDetail.PurchaseOrder = model.PurchaseOrder;
                        ViewBag.check = BL.ProgramAccess("Grn");
                        return View(GrnDetail);
                    }
                    ViewBag.check = BL.ProgramAccess("Grn");
                    return View(model);

                }
                throw new Exception("Model contains errors!");


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message + " - " + ex.InnerException.Message);
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
                return file;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public JsonResult GrnList()
        {
            try
            {

                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var GrnList = sdb.sp_PurchaseOrderGrnList(Company, User).ToList();
                return Json(GrnList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        protected override void Dispose(bool disposing)
        {
            mdb.Dispose();
            sdb.Dispose();
            base.Dispose(disposing);
        }
    }
}
