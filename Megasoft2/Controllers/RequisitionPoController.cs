using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionPoController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        RequisitionPurchaseOrder PBL = new RequisitionPurchaseOrder();
        SysproCore sys = new SysproCore();
        RequisitionBL BL = new RequisitionBL();
        Email _email = new Email();
        RequisitionAudit AU = new RequisitionAudit();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        [CustomAuthorize(Activity: "PurchaseOrders")]
        public ActionResult Index()
        {
            var PoList = sdb.sp_PurchaseOrderList().ToList();
            return View(PoList);
        }

        [CustomAuthorize(Activity: "PurchaseOrders")]
        public ActionResult EditPo(string PurchaseOrder)
        {
            var Po = sdb.sp_GetPurchaseOrderLines(PurchaseOrder.PadLeft(15, '0')).ToList();
            var PoModel = new PoMaintenance();
            PoModel.PoLines = Po;
            PoModel.Supplier = Po.FirstOrDefault().Supplier;
            PoModel.SupplierName = Po.FirstOrDefault().SupplierName;
            PoModel.Currency = Po.FirstOrDefault().Currency.Trim();
            var Totals = sdb.sp_GetRequisitionPoTotals(PurchaseOrder).FirstOrDefault();
            PoModel.SubTotal = Totals.SubTotal;
            PoModel.Vat = Totals.Vat;
            PoModel.Total = Totals.Total;
            ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
            ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
            return View(PoModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPo(PoMaintenance model)
        {

            string Guid = "";
            try
            {
                string ErrorMessage = "";
                if (model.PoLines != null)
                {
                    foreach (var line in model.PoLines)
                    {
                        if (line.LineType == "S")
                        {
                            line.Supplier = model.Supplier;
                            line.SupplierName = model.SupplierName;
                            line.Currency = model.Currency;
                        }

                    }

                    if (BL.CheckSupplierValid(model.Supplier) == false)
                    {
                        ModelState.AddModelError("", "Invalid Supplier!");
                        ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                        ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                        return View("EditPo", model);
                    }

                    Guid = sys.SysproLogin();
                    var OldValue = sdb.sp_GetPurchaseOrderLines(model.PoLines.FirstOrDefault().PurchaseOrder).ToList();
                    foreach (var Item in model.PoLines)
                    {
                        if (Item.LineType == "S")
                        {
                            if (Item.Line == 0 || Item.Line == null)
                            {
                                //Add Line

                                ErrorMessage = PBL.ActionPoLine("A", Guid, Item.PurchaseOrder, Item.Supplier, 0, Item.StockCode, Item.Description, (decimal)Item.OrderQty, Item.Uom, (decimal)Item.Price, Item.GlCode, Item.Job, Item.HierachyCategory, Item.Warehouse, "", 0, Item.Branch, Item.Site);
                                if (!string.IsNullOrEmpty(ErrorMessage))
                                {
                                    ModelState.AddModelError("", ErrorMessage);
                                    ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                    return View("EditPo", model);
                                }
                                else
                                {
                                    AU.AuditPurchaseOrderManual(Item.PurchaseOrder, 0, "A", "P/O Maintenance", "StockCode", "", Item.StockCode);
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
                                }
                            }
                            else
                            {

                                var Line = (from a in OldValue where a.Line == Item.Line select a).FirstOrDefault();
                                List<EntityChanges> changes = AuditHelper.EnumeratePropertyDifferences(Line, Item);
                                if (changes.Count > 0)
                                {
                                    var supplierChange = (from a in changes where a.KeyField == "Supplier" select a.KeyField).Count();
                                    if (supplierChange > 0)
                                    {
                                        string SupplierError = PBL.CreateSupplier(Guid, Item.Supplier);
                                        if (!string.IsNullOrEmpty(SupplierError))
                                        {
                                            ModelState.AddModelError("", "Failed to create Supplier. " + SupplierError);
                                            ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                                            ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                            return View("EditPo", model);
                                        }
                                        PBL.UpdatePoSupplier(Item.PurchaseOrder, Item.Supplier);
                                        AU.AuditPurchaseOrderManual(Item.PurchaseOrder, 0, "C", "P/O Maintenance", "Supplier", Line.Supplier, Item.Supplier);
                                    }

                                    var CurrencyChange = (from a in changes where a.KeyField == "Currency" select a.KeyField).Count();
                                    if (CurrencyChange > 0)
                                    {
                                        //Update Currency In Req Header.
                                        PBL.UpdateRequisitionCurrency(Line.Requisition, Item.Currency);
                                        AU.AuditPurchaseOrderManual(Item.PurchaseOrder, 0, "C", "P/O Maintenance", "Currency", Line.Currency, Item.Currency);
                                    }


                                    //check if Po Line Complete. If Complete, do not update.
                                    var poComplete = (from a in sdb.PorMasterDetails where a.PurchaseOrder == Item.PurchaseOrder && a.Line == Item.Line && a.MCompleteFlag != "Y" select a).ToList();
                                    if (poComplete.Count > 0)
                                    {
                                        var StockChange = (from a in changes where a.KeyField == "StockCode" select a.KeyField).Count();
                                        if (StockChange > 0)
                                        {
                                            //Check if Po has been Grn'd
                                            using (var grndb = new SysproEntities(""))
                                            {
                                                var grnLine = (from a in grndb.mtGrnDetails where a.PurchaseOrder == Item.PurchaseOrder && a.PurchaseOrderLin == Item.Line select a).ToList();
                                                if (grnLine.Count > 0)
                                                {
                                                    ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                                                    ModelState.AddModelError("", "Cannot change StockCode on Purchase Order as it has been receipted.Please reverse Grn first.");
                                                    model.PoLines = sdb.sp_GetPurchaseOrderLines(model.PoLines.FirstOrDefault().PurchaseOrder).ToList();
                                                    model.Supplier = model.PoLines.FirstOrDefault().Supplier;
                                                    model.SupplierName = model.PoLines.FirstOrDefault().SupplierName;
                                                    var newTotals = sdb.sp_GetRequisitionPoTotals(model.PoLines.FirstOrDefault().PurchaseOrder).FirstOrDefault();
                                                    model.SubTotal = newTotals.SubTotal;
                                                    model.Vat = newTotals.Vat;
                                                    model.Total = newTotals.Total;
                                                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                                    return View("EditPo", model);
                                                }
                                            }
                                            //StockCOde Change -- Delete Line and Add New Line
                                            ErrorMessage = PBL.DeletePoLine(Guid, Item.PurchaseOrder, (int)Item.Line);
                                            if (!string.IsNullOrEmpty(ErrorMessage))
                                            {
                                                ModelState.AddModelError("", "StockCode Change Detected. Failed to Delete Line : " + Item.Line + ". " + ErrorMessage);
                                                ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                                                ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                                return View("EditPo", model);
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


                                                ErrorMessage = PBL.ActionPoLine("A", Guid, Item.PurchaseOrder, Item.Supplier, (int)Item.Line, Item.StockCode, Item.Description, (decimal)Item.OrderQty, Item.Uom, (decimal)Item.Price, Item.GlCode, Item.Job, Item.HierachyCategory, Item.Warehouse, "", 0, Item.Branch, Item.Site);
                                                if (!string.IsNullOrEmpty(ErrorMessage))
                                                {
                                                    ModelState.AddModelError("", "StockCode Change Detected. Line : " + Item.Line + " deleted successfully but failed to add new line with StockCode : " + Item.StockCode + ". " + ErrorMessage);
                                                    ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                                                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                                    return View("EditPo", model);
                                                }
                                                else
                                                {
                                                    AU.AuditPurchaseOrderManual(Item.PurchaseOrder, 0, "C", "P/O Maintenance", "StockCode", Line.StockCode, Item.StockCode);
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
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var OtherChanges = (from a in changes where a.KeyField != "StockCode" select a.KeyField).Count();
                                            if (OtherChanges > 0)
                                            {
                                                //Update Line
                                                ErrorMessage = PBL.ActionPoLine("C", Guid, Item.PurchaseOrder, Item.Supplier, (int)Item.Line, Item.StockCode, Item.Description, (decimal)Item.OrderQty, Item.Uom, (decimal)Item.Price, Item.GlCode, Item.Job, Item.HierachyCategory, Item.Warehouse, "", 0, Item.Branch, Item.Site);
                                                if (!string.IsNullOrEmpty(ErrorMessage))
                                                {
                                                    ModelState.AddModelError("", "An error occured updating Line : " + Item.Line + ". " + ErrorMessage);
                                                    ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                                                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                                                    return View("EditPo", model);
                                                }
                                                else
                                                {
                                                    var LogChanges = (from a in changes where a.KeyField != "StockCode" select a);
                                                    foreach (var change in LogChanges)
                                                    {
                                                        AU.AuditPurchaseOrderManual(Item.PurchaseOrder, (int)Line.Line, "C", "P/O Maintenance", change.KeyField, change.OldValue, change.NewValue);
                                                    }

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


                                                    //Update StockDescription in Requisition Line so that PO Report pulls correct description, PO Report uses req description because syspro description has a limited no. of characters.
                                                    sdb.sp_UpdateRequisitionLineDescription(Item.Requisition, Item.ReqLine, Item.Description);


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
                        }



                    }




                    ViewBag.MaintainPo = BL.ProgramAccess("MaintainPo");
                    ModelState.AddModelError("", "Purchase Order Updated Successfully.");
                    model.PoLines = sdb.sp_GetPurchaseOrderLines(model.PoLines.FirstOrDefault().PurchaseOrder).ToList();
                    model.Supplier = model.PoLines.FirstOrDefault().Supplier;
                    model.SupplierName = model.PoLines.FirstOrDefault().SupplierName;
                    var Totals = sdb.sp_GetRequisitionPoTotals(model.PoLines.FirstOrDefault().PurchaseOrder).FirstOrDefault();
                    model.SubTotal = Totals.SubTotal;
                    model.Vat = Totals.Vat;
                    model.Total = Totals.Total;
                    ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                    return View("EditPo", model);

                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.CurrencyList = (from a in sdb.TblCurrencies select new SelectListItem { Value = a.Currency.Trim(), Text = a.Description }).ToList();
                return View("EditPo", model);
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

        public ActionResult GlCodeSearch(string Branch, string Site, int RowIndex)
        {
            ViewBag.Branch = Branch;
            ViewBag.Site = Site;
            ViewBag.ControlId = "PoLines_" + RowIndex + "__GlCode";
            ViewBag.JobBtn = "PoLines_" + RowIndex + "__btnJob";
            ViewBag.CatBtn = "PoLines_" + RowIndex + "__btnCat";
            return PartialView();
        }

        public JsonResult JobList(string GlCode, string FilterText)
        {
            var result = sdb.sp_GetJobsByGlCode(GlCode, FilterText);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JobSearch(string ControlId, string GlCodeId, string CatBtnId)
        {
            ViewBag.ControlId = ControlId;
            ViewBag.GlCodeId = GlCodeId;
            ViewBag.CatBtnId = CatBtnId;
            return PartialView();
        }


        public JsonResult CategoryList(string GlCode, string Job, string FilterText)
        {

            var _glCode = (from a in sdb.GenMasters where a.GlCode == GlCode select a).ToList();
            if (_glCode.Count > 0)
            {
                if (_glCode[0].AnalysisRequired == "Y")
                {
                    var result = sdb.sp_GetCategoryByGlCode(GlCode, FilterText.ToUpper());
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = sdb.sp_GetCategoryByJob(Job, FilterText);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Error. No data found.", JsonRequestBehavior.AllowGet);
            }



        }

        public ActionResult CategorySearch(string ControlId, string GlCodeId, string JobId)
        {
            ViewBag.ControlId = ControlId;
            ViewBag.GlCodeId = GlCodeId;
            ViewBag.JobId = JobId;
            return PartialView();
        }


        public JsonResult GetNewRow(string PurchaseOrder, string Requisition, string Branch, string Site)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                var Po = sdb.sp_GetPurchaseOrderLines(PurchaseOrder.PadLeft(15, '0')).ToList();
                int RowCount = Po.Count();

                int RowIndex = RowCount;


                //Building Document content
                Document.Append("<tr>");
                Document.Append("	<td>");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__Line\" name=\"PoLines[" + RowIndex + "].Line\" type=\"hidden\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__PurchaseOrder\" name=\"PoLines[" + RowIndex + "].PurchaseOrder\" type=\"hidden\" value=\"" + PurchaseOrder + "\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__Requisition\" name=\"PoLines[" + RowIndex + "].Requisition\" type=\"hidden\" value=\"" + Requisition + "\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__Branch\" name=\"PoLines[" + RowIndex + "].Branch\" type=\"hidden\" value=\"" + Branch + "\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__Site\" name=\"PoLines[" + RowIndex + "].Site\" type=\"hidden\" value=\"" + Site + "\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__Status\" name=\"PoLines[" + RowIndex + "].Status\" type=\"hidden\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__ReqLine\" name=\"PoLines[" + RowIndex + "].ReqLine\" type=\"hidden\">");
                Document.Append("		<input id=\"PoLines_" + RowIndex + "__LineType\" name=\"PoLines[" + RowIndex + "].LineType\" type=\"hidden\" value=\"S\">");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<div class=\"input-group add-on\">");
                Document.Append("			<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__StockCode\" name=\"PoLines[" + RowIndex + "].StockCode\" type=\"text\">");
                Document.Append("			<div class=\"input-group-btn\">");
                Document.Append("				<a href=\"/Requisition/StockSearch?Branch=" + Branch + "&amp;Site=" + Site + "&amp;StockId=PoLines_" + RowIndex + "__StockCode&amp;DescId=PoLines_" + RowIndex + "__Description&amp;UomId=PoLines_" + RowIndex + "__Uom\" class=\"modal-link btn btn-default btn-sm\"><i class=\"glyphicon glyphicon-search\"></i></a>");
                Document.Append("			</div>");
                Document.Append("		</div>");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<input class=\"form-control input-sm tdtextbox\" id=\"PoLines_" + RowIndex + "__Description\" name=\"PoLines[" + RowIndex + "].Description\" type=\"text\" >");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__OrderQty\" name=\"PoLines[" + RowIndex + "].OrderQty\" type=\"text\" >");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__Uom\" name=\"PoLines[" + RowIndex + "].Uom\" type=\"text\" >");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__Price\" name=\"PoLines[" + RowIndex + "].Price\" type=\"text\" >");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<div class=\"input-group add-on\">");
                Document.Append("			<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__GlCode\" name=\"PoLines[" + RowIndex + "].GlCode\" type=\"text\" >");
                Document.Append("			<div class=\"input-group-btn\">");
                Document.Append("				<a href=\"/RequisitionPo/GlCodeSearch?Branch=" + Branch + "&amp;Site=" + Site + "&amp;RowIndex=" + RowIndex + "\" class=\"modal-link btn btn-default btn-sm\"><i class=\"glyphicon glyphicon-search\"></i></a>");
                Document.Append("			</div>");
                Document.Append("		</div>");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<div class=\"input-group add-on\">");
                Document.Append("			<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__Job\" name=\"PoLines[" + RowIndex + "].Job\" type=\"text\">");
                Document.Append("			<div class=\"input-group-btn\">");
                Document.Append("				<a href=\"/RequisitionPo/JobSearch?ControlId=PoLines_" + RowIndex + "__Job&amp;GlCodeId=PoLines_" + RowIndex + "__GlCode&amp;CatBtnId=PoLines_" + RowIndex + "__btnCat\" class=\"modal-link btn btn-default btn-sm\" id=\"PoLines_" + RowIndex + "__btnJob\"><i class=\"glyphicon glyphicon-search\"></i></a>");
                Document.Append("			</div>");
                Document.Append("		</div>");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<div class=\"input-group add-on\">");
                Document.Append("			<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__HierachyCategory\" name=\"PoLines[" + RowIndex + "].HierachyCategory\" type=\"text\" >");
                Document.Append("			<div class=\"input-group-btn\">");
                Document.Append("				<a href=\"/RequisitionPo/CategorySearch?ControlId=PoLines_" + RowIndex + "__HierachyCategory&amp;GlCodeId=PoLines_" + RowIndex + "__GlCode&amp;JobId=PoLines_" + RowIndex + "__Job\" class=\"modal-link btn btn-default btn-sm\" id=\"PoLines_" + RowIndex + "__btnCat\"><i class=\"glyphicon glyphicon-search\"></i></a>");
                Document.Append("			</div>");
                Document.Append("		</div>");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<div class=\"input-group add-on\">");
                Document.Append("			<input class=\"form-control input-sm\" id=\"PoLines_" + RowIndex + "__Warehouse\" name=\"PoLines[" + RowIndex + "].Warehouse\" type=\"text\">");
                Document.Append("			<div class=\"input-group-btn\">");
                Document.Append("				<a href=\"/Requisition/ReasonSearch?ReasonType=SupplierOverride&amp;ControlId=Quote_" + RowIndex + "__SupplierOverrideReason\" class=\"modal-link btn btn-default btn-sm\"><i class=\"glyphicon glyphicon-search\"></i></a>");
                Document.Append("			</div>");
                Document.Append("		</div>");
                Document.Append("	</td>");
                Document.Append("	<td>");
                Document.Append("		<a href=\"/RequisitionPo/DeleteLine?Requisition=" + PurchaseOrder + "&amp;Line=2\" class=\"btn btn-danger btn-xs\" style=\"pointer-events:none;\">");
                Document.Append("			<span class=\"fa fa-trash-o\" aria-hidden=\"true\" title=\"Delete Line\"></span>");
                Document.Append("		</a>");
                Document.Append("	</td>");
                Document.Append("</tr>");

                return Json(Document.ToString(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SupplierList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = sdb.sp_GetSysproSuppliers(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SupplierSearch(string ControlId, string NameId = "")
        {
            ViewBag.ControlId = ControlId;
            ViewBag.NameId = NameId;
            return PartialView();
        }


        public ActionResult DeletePoLine(string PurchaseOrder, int Line)
        {
            try
            {
                string Guid = sys.SysproLogin();
                string ErrorMessage = PBL.DeletePoLine(Guid, PurchaseOrder, Line);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    ModelState.AddModelError("", ErrorMessage);
                }
                else
                {
                    ModelState.AddModelError("", "Purchase Order Line Deleted Successfully.");
                    using (var ddb = new SysproEntities(""))
                    {
                        var Category = (from a in ddb.mtPurchaseOrderAnalysisCats where a.PurchaseOrder == PurchaseOrder && a.Line == Line select a).FirstOrDefault();
                        if (Category != null)
                        {
                            ddb.mtPurchaseOrderAnalysisCats.Remove(Category);
                            ddb.SaveChanges();
                        }
                    }
                }
                PoMaintenance model = new PoMaintenance();

                model.PoLines = sdb.sp_GetPurchaseOrderLines(PurchaseOrder).ToList();
                model.Supplier = model.PoLines.FirstOrDefault().Supplier;
                model.SupplierName = model.PoLines.FirstOrDefault().SupplierName;
                return View("EditPo", model);
            }
            catch (Exception ex)
            {
                PoMaintenance model = new PoMaintenance();
                ModelState.AddModelError("", ex.Message);
                model.PoLines = sdb.sp_GetPurchaseOrderLines(PurchaseOrder).ToList();
                model.Supplier = model.PoLines.FirstOrDefault().Supplier;
                model.SupplierName = model.PoLines.FirstOrDefault().SupplierName;
                return View("EditPo", model);
            }
        }



        [HttpGet]
        public ActionResult EmailPo(string PurchaseOrder)
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


        [CustomAuthorize("CancelPurchaseOrder")]

        public JsonResult CancelPurchaseOrder(string PurchaseOrder, string Requisition)
        {
            try
            {
                string PostResult = PBL.CancelPurchaseOrder(PurchaseOrder, Requisition);
                return Json(PostResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string PostResult = ex.Message;
                return Json(PostResult, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult TextLine(string PurchaseOrder, string Requisition, int Line)
        {
            RequisitionTextLine objText = new RequisitionTextLine();
            List<int> Lines = (from a in sdb.mtRequisitionDetails where a.Requisition == Requisition select a.Line).ToList();
            objText.Requisition = Requisition;
            objText.TextLine = Line;
            if (Line != 0)
            {
                var SelectedLine = (from a in sdb.mtRequisitionTextLines where a.Requisition == Requisition && a.TextLine == Line select a).FirstOrDefault();

                objText.NText = SelectedLine.NText;
                if (SelectedLine.RequisitionLine != 0)
                {
                    objText.LinkedToLine = true;
                    objText.SelectedLine = (int)SelectedLine.RequisitionLine;
                }
                else
                {
                    objText.LinkedToLine = false;
                    objText.SelectedLine = Lines.FirstOrDefault();
                }
            }

            objText.Lines = new SelectList(Lines);

            objText.PurchaseOrder = PurchaseOrder;

            return PartialView(objText);
        }

        [HttpPost]
        public ActionResult TextLine(RequisitionTextLine model)
        {
            if (model.TextLine != 0)
            {
                //Update
                mtRequisitionTextLine result = (from a in sdb.mtRequisitionTextLines where a.Requisition == model.Requisition && a.TextLine == model.TextLine select a).FirstOrDefault();
                result.NText = model.NText;
                result.RequisitionLine = model.SelectedLine;
                sdb.Entry(result).State = EntityState.Modified;
                sdb.SaveChanges();
            }
            else
            {
                //Add
                mtRequisitionTextLine line = new mtRequisitionTextLine();
                line.Requisition = model.Requisition;
                var NextLine = (from a in sdb.mtRequisitionTextLines where a.Requisition == model.Requisition select a.TextLine).ToList();
                if (NextLine.Count == 0)
                {
                    line.TextLine = 1;
                }
                else
                {
                    line.TextLine = NextLine.Max() + 1;
                }

                line.NText = model.NText;
                if (model.LinkedToLine == true)
                {
                    line.RequisitionLine = model.SelectedLine;
                }
                else
                {
                    line.RequisitionLine = 0;
                }
                sdb.mtRequisitionTextLines.Add(line);
                sdb.SaveChanges();
            }
            var Header = (from a in sdb.mtRequisitionHeaders where a.Requisition == model.Requisition select a).FirstOrDefault();
            return RedirectToAction("EditPo", new { PurchaseOrder = model.PurchaseOrder });
        }

    }
}
