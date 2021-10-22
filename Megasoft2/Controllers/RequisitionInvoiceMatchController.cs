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
    public class RequisitionInvoiceMatchController : Controller
    {
        //
        // GET: /RequisitionInvoiceMatch/
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        RequisitionBL BL = new RequisitionBL();
        SysproCore sys = new SysproCore();
        RequisitionPurchaseOrder PBL = new RequisitionPurchaseOrder();
        RequisitionAudit AU = new RequisitionAudit();
        Email _email = new Email();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        [CustomAuthorize(Activity: "InvoiceMatching")]
        public ActionResult Index()
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            InvoiceMatchingList obj = new InvoiceMatchingList();
            obj.StartIndex = "A";
            obj.EndIndex = "A";
            ViewBag.Alphabet = LoadAlphabet();
            var Pref = (from a in mdb.mtUserPreferences where a.Username == User && a.Company == Company select a).ToList();
            if (Pref.Count > 0)
            {
                obj.StartIndex = Pref.FirstOrDefault().InvoicingSupplierStartIndex;
                obj.EndIndex = Pref.FirstOrDefault().InvoicingSupplierEndIndex;
            }

            return View(obj);

        }


        public List<SelectListItem> LoadAlphabet()
        {
            List<SelectListItem> Alphabet = new List<SelectListItem>
            {
                new SelectListItem{Text = "0", Value="0"},
                new SelectListItem{Text = "1", Value="1"},
                new SelectListItem{Text = "2", Value="2"},
                new SelectListItem{Text = "3", Value="3"},
                new SelectListItem{Text = "4", Value="4"},
                new SelectListItem{Text = "5", Value="5"},
                new SelectListItem{Text = "6", Value="6"},
                new SelectListItem{Text = "7", Value="7"},

                new SelectListItem{Text = "8", Value="8"},
                new SelectListItem{Text = "9", Value="9"},
                new SelectListItem{Text = "A", Value="A"},
                new SelectListItem{Text = "B", Value="B"},
                new SelectListItem{Text = "C", Value="C"},
                new SelectListItem{Text = "D", Value="D"},
                new SelectListItem{Text = "E", Value="E"},
                new SelectListItem{Text = "F", Value="F"},
                new SelectListItem{Text = "G", Value="G"},
                new SelectListItem{Text = "H", Value="H"},
                new SelectListItem{Text = "I", Value="I"},
                new SelectListItem{Text = "J", Value="J"},
                new SelectListItem{Text = "K", Value="K"},
                new SelectListItem{Text = "L", Value="L"},
                new SelectListItem{Text = "M", Value="M"},
                new SelectListItem{Text = "N", Value="N"},
                new SelectListItem{Text = "O", Value="O"},
                new SelectListItem{Text = "P", Value="P"},
                new SelectListItem{Text = "Q", Value="Q"},
                new SelectListItem{Text = "R", Value="R"},
                new SelectListItem{Text = "S", Value="S"},
                new SelectListItem{Text = "T", Value="T"},
                new SelectListItem{Text = "U", Value="U"},
                new SelectListItem{Text = "V", Value="V"},
                new SelectListItem{Text = "W", Value="W"},
                new SelectListItem{Text = "X", Value="X"},
                new SelectListItem{Text = "Y", Value="Y"},
                new SelectListItem{Text = "Z", Value="Z"}



            };
            return Alphabet;
        }

        public JsonResult InvoiceList(string StartIndex, string EndIndex)
        {
            try
            {

                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                mdb.sp_UpdateInvoicingPreference(User, Company, StartIndex, EndIndex);
                var GrnList = sdb.sp_GetGrnListForInvoicing(User, Company).ToList();
                return Json(GrnList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        [CustomAuthorize(Activity: "InvoiceMatching")]
        public ActionResult Create(string Grn)
        {
            try
            {

                InvoiceMatching Inv = new InvoiceMatching();
                var GrnLines = sdb.sp_GetGrnLinesForInvoicing(Grn).ToList();
                Inv.PoLines = GrnLines;
                Inv.PurchaseOrder = GrnLines.FirstOrDefault().PurchaseOrder;
                Inv.Invoice = GrnLines.FirstOrDefault().Invoice;
                Inv.InvoiceAmount = (decimal)GrnLines.FirstOrDefault().InvoiceAmount;
                Inv.InvoiceDate = GrnLines.FirstOrDefault().InvoiceDate;
                Inv.Grn = Grn;
                Inv.DeliveryNote = GrnLines.FirstOrDefault().DeliveryNote;
                Inv.DeliveryNoteDate = GrnLines.FirstOrDefault().DeliveryNoteDate;
                Inv.Requisition = GrnLines.FirstOrDefault().Requisition;
                Inv.Branch = GrnLines.FirstOrDefault().Branch;
                Inv.Site = GrnLines.FirstOrDefault().Site;
                Inv.PoAttached = GrnLines.FirstOrDefault().PoAttached;
                Inv.GrnAttached = GrnLines.FirstOrDefault().GrnAttached;
                Inv.PodAttached = GrnLines.FirstOrDefault().PodAttached;
                Inv.CastAndExtensions = GrnLines.FirstOrDefault().CastAndExtensions;
                Inv.GlCodeChecked = GrnLines.FirstOrDefault().GlCodeChecked;
                Inv.SupplierCodeChecked = GrnLines.FirstOrDefault().SupplierCodeChecked;
                var Totals = sdb.sp_GetGrnTotals(Grn).ToList();
                Inv.SubTotal = (decimal)Totals.FirstOrDefault().SubTotal;
                Inv.Vat = (decimal)Totals.FirstOrDefault().Vat;
                Inv.Total = (decimal)Totals.FirstOrDefault().Total;
                Inv.Supplier = GrnLines.FirstOrDefault().Supplier;
                Inv.ReceivedBy = GrnLines.FirstOrDefault().ReceivedBy;
                Inv.Currency = GrnLines.FirstOrDefault().Currency.Trim();
                ViewBag.IsAdmin = BL.isAdmin();
                ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");

                ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();

                return View(Inv);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [CustomAuthorize(Activity: "InvoiceMatching")]
        [HttpPost]
        public ActionResult Create(InvoiceMatching model)
        {
            ModelState.Clear();
            string Guid = "";
            try
            {


                DateTime InvoiceDateForVat = DateTime.Now;
                if (model.InvoiceDate != null)
                {
                    InvoiceDateForVat = (DateTime)model.InvoiceDate;
                }


                //Validation
                bool HasErrors = false;

                if (BL.CheckApBranchDefined(model.Branch, model.Site) == false)
                {
                    ModelState.AddModelError("", "Ap Invoice Posting Branch not defined for Site. Please contact your administrator!");
                    HasErrors = true;
                }

                if (BL.CheckSupplierValid(model.Supplier) == false)
                {
                    ModelState.AddModelError("", "Invalid Supplier!");
                    HasErrors = true;
                }


                if (model.DeliveryNote == null)
                {
                    ModelState.AddModelError("", "Delivery Note cannot be blank.");
                    HasErrors = true;
                }

                string Branch = model.Branch;
                string Site = model.Site;

                var SuspenseAccount = (from a in sdb.mtBranchSites where a.Branch == Branch && a.Site == Site select a).ToList();
                if (SuspenseAccount.Count == 0)
                {
                    ModelState.AddModelError("", "Failed to retrieve Branch/Site information.");
                    HasErrors = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(SuspenseAccount.FirstOrDefault().AccrualSuspenseAcc))
                    {
                        ModelState.AddModelError("", "No Accrual Suspense Account defined against Site Setup.");
                        HasErrors = true;
                    }
                }

                if (model.DeliveryNoteDate > DateTime.Now.Date)
                {
                    ModelState.AddModelError("", "Delivery Date cannot be after today!");
                    HasErrors = true;
                }

                if (model.InvoiceDate > DateTime.Now.Date)
                {
                    ModelState.AddModelError("", "Invoice Date cannot be after today!");
                    HasErrors = true;
                }


                var PostDates = sdb.sp_GetPostingPeriod(model.DeliveryNoteDate).ToList();
                if (PostDates.Count == 0)
                {
                    ModelState.AddModelError("", "Delivery Date out of range of Posting Period.");
                    HasErrors = true;
                }


                foreach (var line in model.PoLines)
                {
                    line.Supplier = model.Supplier;
                    if (BL.CheckStockCodeValid(line.StockCode) == false)
                    {
                        ModelState.AddModelError("", "StockCode not found for line " + line.Line + ".");
                        HasErrors = true;
                    }

                    if (BL.isStockCodeNonStocked(line.StockCode) == true)
                    {
                        if (BL.CheckGlCodeValid(line.GlCode) == false)
                        {
                            ModelState.AddModelError("", "Gl Code : " + line.GlCode + " not found for line " + line.Line + ".");
                            HasErrors = true;
                        }

                        if (Branch != line.GlCode.Substring(0, 3).Trim())
                        {
                            ModelState.AddModelError("", "Invalid Gl Code : " + line.GlCode + " for branch : " + Branch + " for line " + line.Line + ".");
                            HasErrors = true;
                        }

                        if (Site != line.GlCode.Substring(3, 3).Trim())
                        {
                            ModelState.AddModelError("", "Invalid Gl Code : " + line.GlCode + " for Site : " + Site + " for line " + line.Line + ".");
                            HasErrors = true;
                        }


                        if (BL.CheckGlCodeHasTaxCode(line.GlCode) == false)
                        {
                            ModelState.AddModelError("", "Purchasing Tax Code not defined for Gl Code : " + line.GlCode + ".");
                            HasErrors = true;
                        }

                    }

                    //if (BL.isAdmin() == false)
                    //{

                    if ((line.QtyReceived + line.PrevRecQty) > line.OrderQty)
                    {
                        ModelState.AddModelError("", "Over receipting not allowed for Line :" + line.Line + ". Received Qty :" + line.QtyReceived + " Outstanding Qty :" + line.OutstandingQty);
                        HasErrors = true;
                    }


                    if (line.QtyReceived < 0)
                    {
                        ModelState.AddModelError("", "Quantity received cannot be negative for line :" + line.Line);
                        HasErrors = true;
                    }
                    //}

                    string GlCode = line.GlCode;
                    var _glCode = (from a in sdb.GenMasters where a.GlCode == GlCode select a).ToList();
                    if (_glCode.Count > 0)
                    {
                        if (_glCode[0].AnalysisRequired == "Y")
                        {
                            if (string.IsNullOrEmpty(line.HierachyCategory))
                            {
                                ModelState.AddModelError("", "Please select an Analysis Category.");
                                HasErrors = true;
                            }

                        }
                        else
                        {
                            var JobList = (from a in sdb.CusGenMaster_ where a.GlCode == GlCode && a.PurchasingCategory == "WIP" select a).ToList();
                            if (JobList.Count > 0)
                            {
                                if (string.IsNullOrEmpty(line.Job))
                                {
                                    ModelState.AddModelError("", "Please select a Job.");
                                    HasErrors = true;
                                }

                                if (string.IsNullOrEmpty(line.HierachyCategory))
                                {
                                    ModelState.AddModelError("", "Please select a Job Heirachy.");
                                    HasErrors = true;
                                }
                            }
                        }
                    }

                }

                bool completeCheckTotals = false;
                if (model.PodAttached == true && model.CastAndExtensions == true && model.GlCodeChecked == true && model.PoAttached == true && model.GrnAttached == true && model.SupplierCodeChecked == true)
                {
                    completeCheckTotals = true;
                    if (string.IsNullOrEmpty(model.Invoice))
                    {
                        ModelState.AddModelError("", "Invoice number required.");
                        HasErrors = true;
                    }

                    var Supplier = model.PoLines.FirstOrDefault().Supplier;
                    var result = sdb.sp_GetSupplierInvoiceValidation(model.Supplier, model.Invoice, model.Grn).ToList();
                    if (result.Count > 0)
                    {
                        ModelState.AddModelError("", "Invoice : " + model.Invoice + " already exists for Supplier : " + Supplier + ".");
                        HasErrors = true;
                    }

                    if (model.InvoiceDate == null)
                    {
                        ModelState.AddModelError("", "Invoice date required.");
                        HasErrors = true;
                    }

                    if (model.InvoiceAmount == 0)
                    {
                        ModelState.AddModelError("", "Invoice amount cannot be blank or zero.");
                        HasErrors = true;
                    }

                    if (model.Currency.Trim() != "R")
                    {
                        ModelState.AddModelError("", "Currency required in Rands. Please contact your administrator.");
                        HasErrors = true;
                    }



                    var Totals = sdb.sp_GetGrnTotals(model.Grn).ToList();
                    if (model.InvoiceAmount != Totals.FirstOrDefault().Total)
                    {
                        ModelState.AddModelError("", "Invoice amount cannot be less than or greater than the Grn Total.");
                        HasErrors = true;
                    }

                    //var PoTotal = Convert.ToDecimal(Convert.ToDecimal(model.PoLines.Sum(x => x.QtyReceived * x.Price)).ToString("0.##"));
                    //if (model.InvoiceAmount != PoTotal)
                    //{
                    //    ModelState.AddModelError("", "Invoice amount cannot be less than or greater than the Grn Total of R" + PoTotal);
                    //    HasErrors = true;
                    //}
                }



                if (HasErrors == true)
                {
                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                    ViewBag.IsAdmin = BL.isAdmin();
                    ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                    return View("Create", model);
                }

                //Update PO
                string ErrorMessage = "";
                if (model.PoLines != null)
                {
                    bool UpdatePoLine = false;
                    Guid = sys.SysproLogin();
                    var OldValue = sdb.sp_GetGrnLinesForInvoicing(model.Grn).ToList();
                    foreach (var Item in model.PoLines)
                    {
                        if (Item.Line == 0 || Item.Line == null)
                        {
                            //Add Line

                            ErrorMessage = PBL.InvoiceActionPoLine("A", Guid, Item.PurchaseOrder, Item.Supplier, 0, Item.StockCode, Item.Description, (decimal)Item.OrderQty, Item.Uom, (decimal)Item.Price, Item.GlCode, Item.Job, Item.HierachyCategory, Item.Warehouse, Item.TaxCode, Item.GrnLine, Branch, Site, InvoiceDateForVat);
                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {
                                ModelState.AddModelError("", ErrorMessage);
                                ViewBag.IsAdmin = BL.isAdmin();
                                ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                                ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                return View("Create", model);
                            }
                            else
                            {
                                using (var LineDb = new SysproEntities(""))
                                {
                                    string Po = Item.PurchaseOrder.PadLeft(15, '0');
                                    var Line = (from a in LineDb.PorMasterDetails where a.PurchaseOrder == Po select a.Line).Max();
                                    mtPurchaseOrderAnalysisCat objCat = new mtPurchaseOrderAnalysisCat();
                                    objCat.PurchaseOrder = Po;
                                    objCat.Line = Convert.ToInt16(Line + 1);
                                    objCat.Requisition = Item.Requisition;
                                    objCat.GlCode = Item.GlCode;
                                    var AnalysisRequired = (from a in LineDb.GenMasters where a.GlCode == Item.GlCode select a.AnalysisRequired).FirstOrDefault();
                                    if (AnalysisRequired == "Y")
                                    {
                                        objCat.AnalysisCategory = Item.HierachyCategory;
                                    }
                                    else
                                    {
                                        objCat.AnalysisCategory = "";
                                    }
                                    LineDb.mtPurchaseOrderAnalysisCats.Add(objCat);
                                    LineDb.SaveChanges();
                                }
                                UpdatePoLine = true;
                            }
                        }
                        else
                        {
                            var Line = (from a in OldValue where a.Line == Item.Line select a).FirstOrDefault();
                            List<EntityChanges> changes = AuditHelper.EnumeratePropertyDifferences(Line, Item);
                            if (changes.Count > 0 || Item.TaxCode.Trim() != Item.PoTaxCode.Trim() || Item.GlCode.Substring(0, 6) != Branch.Trim() + Site.Trim())
                            {
                                var supplierChange = (from a in changes where a.KeyField == "Supplier" select a.KeyField).Count();
                                if (supplierChange > 0)
                                {
                                    string SupplierError = PBL.CreateSupplier(Guid, Item.Supplier);
                                    if (!string.IsNullOrEmpty(SupplierError))
                                    {
                                        ModelState.AddModelError("", "Failed to create Supplier. " + SupplierError);
                                        ViewBag.IsAdmin = BL.isAdmin();
                                        ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                                        ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                        return View("Create", model);
                                    }
                                    PBL.UpdatePoSupplier(Item.PurchaseOrder, Item.Supplier);
                                }
                                var StockChange = (from a in changes where a.KeyField == "StockCode" select a.KeyField).Count();
                                if (StockChange > 0)
                                {
                                    //StockCOde Change -- Delete Line and Add New Line
                                    ErrorMessage = PBL.DeletePoLine(Guid, Item.PurchaseOrder, (int)Item.Line);
                                    if (!string.IsNullOrEmpty(ErrorMessage))
                                    {
                                        ModelState.AddModelError("", "StockCode Change Detected. Failed to Delete Line : " + Item.Line + ". " + ErrorMessage);
                                        ViewBag.IsAdmin = BL.isAdmin();
                                        ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                                        ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                        return View("Create", model);
                                    }
                                    else
                                    {
                                        using (var ddb = new SysproEntities(""))
                                        {
                                            var Category = (from a in ddb.mtPurchaseOrderAnalysisCats where a.PurchaseOrder == Item.PurchaseOrder && a.Line == Item.Line select a).FirstOrDefault();
                                            if (Category != null)
                                            {
                                                ddb.mtPurchaseOrderAnalysisCats.Remove(Category);
                                                ddb.SaveChanges();
                                            }
                                        }


                                        ErrorMessage = PBL.InvoiceActionPoLine("A", Guid, Item.PurchaseOrder, Item.Supplier, (int)Item.Line, Item.StockCode, Item.Description, (decimal)Item.OrderQty, Item.Uom, (decimal)Item.Price, Item.GlCode, Item.Job, Item.HierachyCategory, Item.Warehouse, "", Item.GrnLine, Branch, Site, InvoiceDateForVat);
                                        if (!string.IsNullOrEmpty(ErrorMessage))
                                        {
                                            ModelState.AddModelError("", "StockCode Change Detected. Line : " + Item.Line + " deleted successfully but failed to add new line with StockCode : " + Item.StockCode + ". " + ErrorMessage);
                                            ViewBag.IsAdmin = BL.isAdmin();
                                            ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                                            ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                            return View("Create", model);
                                        }
                                        else
                                        {
                                            using (var LineDb = new SysproEntities(""))
                                            {
                                                var LineNo = (from a in LineDb.PorMasterDetails where a.PurchaseOrder == Item.PurchaseOrder select a.Line).Max();
                                                mtPurchaseOrderAnalysisCat objCat = new mtPurchaseOrderAnalysisCat();
                                                objCat.PurchaseOrder = Item.PurchaseOrder;
                                                objCat.Line = Convert.ToInt16(LineNo + 1);
                                                objCat.Requisition = Item.Requisition;
                                                objCat.GlCode = Item.GlCode;
                                                var AnalysisRequired = (from a in LineDb.GenMasters where a.GlCode == Item.GlCode select a.AnalysisRequired).FirstOrDefault();
                                                if (AnalysisRequired == "Y")
                                                {
                                                    objCat.AnalysisCategory = Item.HierachyCategory;
                                                }
                                                else
                                                {
                                                    objCat.AnalysisCategory = "";
                                                }
                                                LineDb.mtPurchaseOrderAnalysisCats.Add(objCat);
                                                LineDb.SaveChanges();
                                            }
                                            UpdatePoLine = true;
                                        }
                                    }
                                }
                                else
                                {
                                    var OtherChanges = (from a in changes where a.KeyField != "StockCode" select a.KeyField).Count();
                                    if (OtherChanges > 0)
                                    {
                                        //Update Line
                                        ErrorMessage = PBL.InvoiceActionPoLine("C", Guid, Item.PurchaseOrder, Item.Supplier, (int)Item.Line, Item.StockCode, Item.Description, (decimal)Item.OrderQty, Item.Uom, (decimal)Item.Price, Item.GlCode, Item.Job, Item.HierachyCategory, Item.Warehouse, Item.TaxCode, 0, Branch, Site, InvoiceDateForVat);
                                        if (!string.IsNullOrEmpty(ErrorMessage))
                                        {
                                            ModelState.AddModelError("", "An error occured updating Line : " + Item.Line + ". " + ErrorMessage);
                                            ViewBag.IsAdmin = BL.isAdmin();
                                            ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                                            ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                            return View("Create", model);
                                        }
                                        else
                                        {
                                            using (var ddb = new SysproEntities(""))
                                            {
                                                var Category = (from a in ddb.mtPurchaseOrderAnalysisCats where a.PurchaseOrder == Item.PurchaseOrder && a.Line == Item.Line select a).FirstOrDefault();
                                                if (Category != null)
                                                {
                                                    ddb.mtPurchaseOrderAnalysisCats.Remove(Category);
                                                    ddb.SaveChanges();
                                                }
                                            }
                                            using (var LineDb = new SysproEntities(""))
                                            {

                                                mtPurchaseOrderAnalysisCat objCat = new mtPurchaseOrderAnalysisCat();
                                                objCat.PurchaseOrder = Item.PurchaseOrder;
                                                objCat.Line = (int)Item.Line;
                                                objCat.Requisition = Item.Requisition;
                                                objCat.GlCode = Item.GlCode;
                                                var AnalysisRequired = (from a in LineDb.GenMasters where a.GlCode == Item.GlCode select a.AnalysisRequired).FirstOrDefault();
                                                if (AnalysisRequired == "Y")
                                                {
                                                    objCat.AnalysisCategory = Item.HierachyCategory;
                                                }
                                                else
                                                {
                                                    objCat.AnalysisCategory = "";
                                                }
                                                LineDb.mtPurchaseOrderAnalysisCats.Add(objCat);
                                                LineDb.SaveChanges();
                                            }


                                            //if item not capex item then we clear the MJob flag on the PorMasterDetail table, as the PORTOI BO does not do this.
                                            var JobList = (from a in sdb.CusGenMaster_ where a.GlCode == Item.GlCode && a.PurchasingCategory == "WIP" select a).ToList();
                                            if (JobList.Count > 0)
                                            {
                                                //Job required. Do nothing
                                            }
                                            else
                                            {
                                                //Clear job from PoDetail table and PrjProjHier table.
                                                sdb.sp_UpdatePoDLinkJob(Item.PurchaseOrder, (int)Item.Line);
                                            }
                                        }
                                    }
                                }

                            }
                        }


                    }

                    var grnDet = new List<mtGrnDetail>();
                    using (var ndb = new SysproEntities(""))
                    {
                        grnDet = (from a in ndb.mtGrnDetails where a.Grn == model.Grn select a).ToList(); //still need grnDet for Audit Trail 2017-08-03
                    }

                    //Update Grn
                    using (var db = new SysproEntities(""))
                    {

                        //changed function to update lines instead of deleting and inserting 2017-08-03
                        //grnDet = (from a in db.mtGrnDetails where a.Grn == model.Grn select a).ToList();
                        //foreach (var line in grnDet)
                        //{
                        //    db.Entry(line).State = System.Data.EntityState.Deleted;
                        //    db.SaveChanges();
                        //}

                        //int newLineNo = 1;//changed function to update lines instead of deleting and inserting 2017-08-03
                        foreach (var line in model.PoLines)
                        {
                            if (line.QtyReceived != 0)
                            {
                                mtGrnDetail det = new mtGrnDetail();
                                det.Grn = model.Grn;
                                //det.GrnLine = newLineNo;//changed function to update lines instead of deleting and inserting 2017-08-03
                                det.GrnLine = line.GrnLine;
                                det.PurchaseOrder = line.PurchaseOrder;
                                det.PurchaseOrderLin = (decimal)line.Line;
                                det.Supplier = line.Supplier;
                                det.OrigReceiptDate = line.OrigReceiptDate;
                                det.ReqGrnMonth = (decimal)PostDates.FirstOrDefault().PostMonth;
                                det.ReqGrnYear = (decimal)PostDates.FirstOrDefault().PostYear;
                                det.StockCode = line.StockCode;
                                det.StockDescription = line.Description;
                                if (BL.isStockCodeNonStocked(line.StockCode) == true)
                                {
                                    det.Warehouse = "**";
                                }
                                else
                                {
                                    det.Warehouse = BL.GetWarehouseForStockCode(line.StockCode, Branch, Site);
                                }
                                det.QtyReceived = (decimal)line.QtyReceived;
                                det.QtyUom = line.Uom;
                                if (string.IsNullOrEmpty(line.Job))
                                {
                                    det.Job = "";
                                }
                                else
                                {
                                    det.Job = line.Job;
                                }

                                det.DeliveryNote = model.DeliveryNote;
                                det.DeliveryNoteDate = model.DeliveryNoteDate;
                                det.ProductClass = line.ProductClass;
                                det.TaxCode = PBL.GetTaxCodeForStockCode(line.StockCode, line.Supplier, line.GlCode);
                                det.GlCode = line.GlCode;


                                var OldPrice = (from a in sdb.PorMasterDetails where a.PurchaseOrder == line.PurchaseOrder && a.Line == line.Line select a.MPrice).FirstOrDefault();
                                decimal NewPrice = (decimal)line.Price;
                                if (OldPrice == (decimal)line.Price)
                                {
                                    var PurchasePrice = sdb.sp_GetPriceForPoLine(line.PurchaseOrder, line.Line, line.GlCode, line.StockCode, InvoiceDateForVat).ToList();

                                    if (PurchasePrice.Count > 0)
                                    {
                                        NewPrice = (decimal)PurchasePrice.FirstOrDefault().Price;
                                    }
                                }


                                det.Price = NewPrice;
                                if (line.HierachyCategory == null)
                                {
                                    det.AnalysisEntry = "";
                                }
                                else
                                {
                                    det.AnalysisEntry = line.HierachyCategory;
                                }

                                det.SuspenseAccount = (from a in db.mtBranchSites where a.Branch == Branch && a.Site == Site select a).FirstOrDefault().AccrualSuspenseAcc;
                                det.Requisition = line.Requisition;
                                det.Branch = Branch;
                                det.Site = Site;
                                det.Invoice = model.Invoice;
                                det.InvoiceDate = model.InvoiceDate;
                                det.InvoiceAmount = model.InvoiceAmount;
                                det.PoAttached = model.PoAttached;
                                det.GrnAttached = model.GrnAttached;
                                det.PodAttached = model.PodAttached;
                                det.CastAndExtensions = model.CastAndExtensions;
                                det.GlCodeChecked = model.GlCodeChecked;
                                det.SupplierCodeChecked = model.SupplierCodeChecked;
                                det.ReceivedBy = model.ReceivedBy;
                                det.PostStatus = line.PostStatus;
                                det.GrnPostDate = line.GrnPostDate;
                                det.InvoicePostDate = line.InvoicePostDate;
                                det.SysproGrn = line.SysproGrn;
                                det.GrnError = line.GrnError;
                                det.InvoiceError = line.InvoiceError;
                                det.Journal = line.Journal;
                                det.MaterialAllocationError = line.MaterialAllocationError;
                                det.MaterialIssueError = line.MaterialIssueError;
                                det.GrnAdjustmentError = line.GrnAdjustmentError;
                                det.JournalUpdated = line.JournalUpdated;
                                det.AuthorizedLevel1 = line.AuthorizedLevel1;
                                det.AuthorizedLevel2 = line.AuthorizedLevel2;
                                det.Level1AuthorizedBy = line.Level1AuthorizedBy;
                                det.Level2AuthorizedBy = line.Level2AuthorizedBy;
                                det.GrnJournalYear = line.GrnJournalYear;
                                det.GrnJournalMonth = line.GrnJournalMonth;
                                if (line.OriginalInvoiceDate != null)
                                {
                                    det.OriginalInvoiceDate = line.OriginalInvoiceDate;
                                }
                                else
                                {
                                    det.OriginalInvoiceDate = DateTime.Now;
                                }
                                det.ApJournal = line.ApJournal;
                                det.ApJournalYear = line.ApJournalYear;
                                det.ApJournalMonth = line.ApJournalMonth;
                                det.IssueJournal = line.IssueJournal;
                                det.IssueJournalYear = line.IssueJournalYear;
                                det.IssueJournalMonth = line.IssueJournalMonth;
                                det.Currency = model.Currency.Trim();
                                det.GrnDoneBy = line.GrnDoneBy;

                                //changed function to update lines instead of deleting and inserting 2017-08-03
                                db.Entry(det).State = System.Data.EntityState.Modified;
                                db.SaveChanges();
                                //db.mtGrnDetails.Add(det);
                                //db.SaveChanges();
                                //newLineNo++;
                            }
                            else
                            {
                                //changed function to update lines instead of deleting and inserting 2017-08-03
                                var delLine = (from a in db.mtGrnDetails where a.Grn == model.Grn && a.GrnLine == line.GrnLine select a).FirstOrDefault();
                                if (delLine != null)
                                {
                                    db.Entry(delLine).State = System.Data.EntityState.Deleted;
                                    db.SaveChanges();
                                }


                            }

                        }
                    }

                    //update GlCode for Stocked Items based on warehouse
                    sdb.sp_UpdateStockedWhCtlGlAcc(grnDet.FirstOrDefault().PurchaseOrder);

                    //Audit Trail
                    var newGrn = new mtGrnDetail();
                    foreach (var grnLine in grnDet)
                    {
                        using (var ndb = new SysproEntities(""))
                        {
                            newGrn = (from a in ndb.mtGrnDetails where a.Grn == grnLine.Grn && a.GrnLine == grnLine.GrnLine select a).FirstOrDefault();
                        }
                        if (newGrn != null)
                        {
                            List<EntityChanges> auChanges = AuditHelper.EnumeratePropertyDifferences(grnLine, newGrn);
                            if (auChanges.Count > 0)
                            {
                                foreach (var ch in auChanges)
                                {
                                    AU.AuditGrnManual(grnLine.Grn, grnLine.GrnLine, "C", "InvoiceMatching", ch.KeyField, ch.OldValue, ch.NewValue);
                                }
                            }
                        }
                        else
                        {
                            AU.AuditGrnManual(grnLine.Grn, grnLine.GrnLine, "D", "InvoiceMatching", "QtyReceived", grnLine.QtyReceived.ToString(), "0");
                        }

                    }




                    if (UpdatePoLine == true)
                    {
                        sdb.sp_UpdatePoLineInGrn(model.PurchaseOrder);
                    }


                    //Check new totals match invoice amount
                    if (completeCheckTotals == true)
                    {
                        using (var checkdb = new SysproEntities(""))
                        {
                            var CheckTotals = checkdb.sp_GetGrnTotals(model.Grn).ToList();
                            if (model.InvoiceAmount != CheckTotals.FirstOrDefault().Total)
                            {
                                ModelState.AddModelError("", "Invoice amount cannot be less than or greater than the Grn Total.");
                                var checkTotal = (from a in checkdb.mtGrnDetails where a.Grn == model.Grn select a).ToList();
                                if (checkTotal.Count > 0)
                                {
                                    foreach (var ch in checkTotal)
                                    {
                                        ch.InvoiceAmount = 0;
                                        ch.CastAndExtensions = false;
                                        checkdb.Entry(ch).State = System.Data.EntityState.Modified;
                                        checkdb.SaveChanges();
                                    }
                                }
                                ViewBag.IsAdmin = BL.isAdmin();
                                ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                                model.SubTotal = (decimal)CheckTotals.FirstOrDefault().SubTotal;
                                model.Vat = (decimal)CheckTotals.FirstOrDefault().Vat;
                                model.Total = (decimal)CheckTotals.FirstOrDefault().Total;
                                model.CastAndExtensions = false;
                                ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                return View("Create", model);

                            }
                        }
                    }




                    ViewBag.IsAdmin = BL.isAdmin();
                    ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                    ModelState.AddModelError("", "Saved Successfully.");
                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();


                    model.PoLines = sdb.sp_GetGrnLinesForInvoicing(model.Grn).ToList();
                    if (model.PoLines.Count > 0)
                    {
                        var Totals = sdb.sp_GetGrnTotals(model.Grn).ToList();
                        model.SubTotal = (decimal)Totals.FirstOrDefault().SubTotal;
                        model.Vat = (decimal)Totals.FirstOrDefault().Vat;
                        model.Total = (decimal)Totals.FirstOrDefault().Total;
                        return View("Create", model);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }

                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.IsAdmin = BL.isAdmin();
                ViewBag.CanChangeBranch = BL.ProgramAccess("ChangeInvoiceBranch");
                ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                return View(model);
            }
            finally
            {
                if (model != null)
                {
                    if (!string.IsNullOrEmpty(Guid))
                    {
                        sys.SysproLogoff(Guid);
                    }

                }
            }
        }


        public ActionResult GlCodeList(string Branch, string Site, string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = sdb.sp_GetGlCodesByBranchSite(Branch, Site, HttpContext.User.Identity.Name.ToUpper(), Company, FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GlCodeSearch(int RowIndex)
        {

            ViewBag.ControlId = "PoLines_" + RowIndex + "__GlCode";
            ViewBag.JobBtn = "PoLines_" + RowIndex + "__btnJob";
            ViewBag.CatBtn = "PoLines_" + RowIndex + "__btnCat";
            ViewBag.RowIndex = RowIndex;
            return PartialView();
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

                }


                return file;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




        [HttpGet]
        public ActionResult EmailPo(string PurchaseOrder, string Grn)
        {
            try
            {
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
                ViewBag.Grn = Grn;
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
                throw new Exception(ex.Message);
            }
        }




    }
}
