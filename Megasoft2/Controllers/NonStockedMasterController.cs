using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class NonStockedMasterController : Controller
    {
        private SysproEntities db = new SysproEntities("");
        RequisitionBL BL = new RequisitionBL();
        //
        // GET: /NonStockedMaster/
        [CustomAuthorize("StockCodeSetup", "StockCodeView")]
        public ActionResult Index()
        {
            return View(db.sp_GetAllStockCodes("").Take(100).ToList());
        }


        public JsonResult StockCodeList(string Search1, string Search2, string Search3, string Search4, string Search5)
        {

            var result = db.sp_GetAllStockCodesFiltered(Search1.ToUpper(), Search2.ToUpper(), Search3.ToUpper(), Search4.ToUpper(), Search5.ToUpper()).Take(100).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /NonStockedMaster/Create
        [CustomAuthorize("StockCodeSetup", "StockCodeView")]
        public ActionResult Create(string StockedItem,string StockCode = null)
        {
            var _stock = (from a in db.mtInvMasters where a.StockCode == StockCode select a).FirstOrDefault();
            

            if(_stock == null)
            {
                if(StockCode != null)
                {
                    var Stocked = (from a in db.InvMasters where a.StockCode == StockCode select a).FirstOrDefault();
                    _stock = new mtInvMaster { StockCode = Stocked.StockCode, Description = Stocked.Description, ProductClass = Stocked.ProductClass, OnHold = Stocked.StockOnHold, Uom = Stocked.StockUom };            
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_stock.ProductClass))
                {
                    _stock.ProductClass = "ZZZZ";
                }
            }
            



            if(StockCode == null)
            {
                ViewBag.NewStockCode = "Y";

            }
            else
            {
                ViewBag.NewStockCode = "N";
            }
            
            NonStockedMaster _master = new NonStockedMaster();
            if (StockCode == null)
            {
                mtInvMaster objInv = new mtInvMaster();
                _stock = new mtInvMaster();
                _stock.OnHold = "N";
                _master.NewStockCode = "Y";
                ViewBag.NewStockCode = "Y";
            }
            else
            {
                _master.NewStockCode = "N";
                ViewBag.NewStockCode = "N";
            }


            List<SelectListItem> YesNoList = new List<SelectListItem>
            {                
                new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                new SelectListItem{Text = "No", Value = "N"}
            };

            ViewBag.YesNoList = YesNoList;

            _master.InvMaster = _stock;
            _master.InvBranch = db.sp_GetBranchByStockCode(StockCode).ToList();
            _master.Contract = db.sp_GetContractPricingByStockCode(StockCode).ToList();
            _master.StockedItem = StockedItem;
            _master.InvWarehouse = db.sp_GetWarehouseByStockCode(StockCode).ToList();
            var BranchList = (List<SelectListItem>)(from a in db.mtBranches select new SelectListItem { Text = a.Branch, Value = a.Branch + " - " + a.BranchName }).ToList();
            ViewBag.Branch = new SelectList(BranchList, "Text", "Value");
            ViewBag.SetupStockCode = BL.SetupProgramAccess("StockCodeSetup");
            return View(_master);
        }

        //
        // POST: /NonStockedMaster/Create
        [CustomAuthorize("StockCodeSetup", "StockCodeView")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NonStockedMaster mtinvmaster)
        {
            try
            {
                ViewBag.SetupStockCode = BL.SetupProgramAccess("StockCodeSetup");
                if (ModelState.IsValid)
                {

                    if(mtinvmaster.StockedItem == "N")
                    {

                        if(mtinvmaster.NewStockCode == "Y")
                        {
                            var validcheck = (from a in db.mtInvMasters where a.StockCode == mtinvmaster.InvMaster.StockCode select a).ToList();
                            if (validcheck.Count > 0)
                            {
                                ModelState.AddModelError("", "Cannot create StockCode. StockCode already exists.");
                                ViewBag.NewStockCode = mtinvmaster.NewStockCode;
                                var newBranchList = (List<SelectListItem>)(from a in db.mtBranches select new SelectListItem { Text = a.Branch, Value = a.Branch + " - " + a.BranchName }).ToList();
                                ViewBag.Branch = new SelectList(newBranchList, "Text", "Value");
                                List<SelectListItem> YesNoList = new List<SelectListItem>
                                {                
                                    new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                                    new SelectListItem{Text = "No", Value = "N"}
                                };

                                ViewBag.YesNoList = YesNoList;
                                return View(mtinvmaster);
                            }
                        }


                        var check = (from a in db.mtInvMasters where a.StockCode == mtinvmaster.InvMaster.StockCode select a).ToList();
                        if (check.Count > 0)
                        {
                            using (var sdb = new SysproEntities(""))
                            {
                                mtinvmaster.InvMaster.ProductClass = "_OTH";
                                sdb.Entry(mtinvmaster.InvMaster).State = EntityState.Modified;
                                sdb.SaveChanges();
                            }
                        }
                        else
                        {
                            mtinvmaster.InvMaster.ProductClass = "_OTH";
                            db.mtInvMasters.Add(mtinvmaster.InvMaster);
                            db.SaveChanges();
                        }


                        if(mtinvmaster.InvBranch != null)
                        {
                            foreach (var item in mtinvmaster.InvBranch)
                            {

                                using (var sdb = new SysproEntities(""))
                                {
                                    var brcheck = (from a in db.mtInvBranches where a.StockCode == item.StockCode && a.Branch == item.Branch && a.Site == item.Site select a).ToList();
                                    if (brcheck.Count > 0)
                                    {
                                        var mt = new mtInvBranch();
                                        mt.StockCode = mtinvmaster.InvMaster.StockCode;
                                        mt.Branch = item.Branch;
                                        mt.Site = item.Site;
                                        if (item.StockedBranch == false)
                                        {
                                            sdb.Entry(mt).State = EntityState.Deleted;
                                            sdb.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        var mt = new mtInvBranch();
                                        mt.StockCode = mtinvmaster.InvMaster.StockCode;
                                        mt.Branch = item.Branch;
                                        mt.Site = item.Site;
                                        if (item.StockedBranch == true)
                                        {
                                            sdb.mtInvBranches.Add(mt);
                                            sdb.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                    
                    if(mtinvmaster.Contract != null)
                    {
                        foreach (var item in mtinvmaster.Contract)
                        {
                            using (var sdb = new SysproEntities(""))
                            {
                                var crcheck = (from a in db.mtInvContractPricings where a.StockCode == mtinvmaster.InvMaster.StockCode && a.Branch == item.Branch && a.Site == item.Site select a).ToList();
                                if (crcheck.Count > 0)
                                {
                                    var mt = new mtInvContractPricing();
                                    mt.StockCode = mtinvmaster.InvMaster.StockCode;
                                    mt.Branch = item.Branch;
                                    mt.Site = item.Site;
                                    mt.Warehouse = item.Warehouse;
                                    mt.Supplier = item.Supplier;
                                    mt.ContractExpiryDate = item.ContractExpiryDate;
                                    mt.ContractPrice = item.ContractPrice;
                                    if (!string.IsNullOrEmpty(item.Warehouse) || !string.IsNullOrEmpty(item.Supplier))
                                    {
                                        sdb.Entry(mt).State = EntityState.Modified;
                                        sdb.SaveChanges();
                                    }
                                    else
                                    {
                                        sdb.Entry(mt).State = EntityState.Deleted;
                                        sdb.SaveChanges();
                                    }

                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(item.Warehouse) || !string.IsNullOrEmpty(item.Supplier))
                                    {
                                        var mt = new mtInvContractPricing();
                                        mt.StockCode = mtinvmaster.InvMaster.StockCode;
                                        mt.Branch = item.Branch;
                                        mt.Site = item.Site;
                                        mt.Warehouse = item.Warehouse;
                                        mt.Supplier = item.Supplier;
                                        mt.ContractExpiryDate = item.ContractExpiryDate;
                                        mt.ContractPrice = item.ContractPrice;
                                        sdb.mtInvContractPricings.Add(mt);
                                        sdb.SaveChanges();
                                    }

                                }
                            }
                        }
                    }
                    

                    if(mtinvmaster.InvWarehouse != null)
                    {
                        foreach (var item in mtinvmaster.InvWarehouse)
                        {
                            using (var wdb = new SysproEntities(""))
                            {

                                var wcheck = (from a in wdb.InvWarehouse_ where a.StockCode == item.StockCode && a.Warehouse == item.Warehouse select a).FirstOrDefault();
                                if (wcheck != null)
                                {
                                    wcheck.BranchSite = item.BranchSite;
                                    wdb.Entry(wcheck).State = EntityState.Modified;
                                    wdb.SaveChanges();
                                }
                                else
                                {
                                    InvWarehouse_ invWh = new InvWarehouse_();
                                    invWh.StockCode = item.StockCode;
                                    invWh.Warehouse = item.Warehouse;
                                    invWh.BranchSite = item.BranchSite;
                                    wdb.InvWarehouse_.Add(invWh);
                                    wdb.SaveChanges();
                                }
                            }
                        }
                    }
                    


                    ModelState.AddModelError("", "Saved Successfully.");
                    var BranchList = (List<SelectListItem>)(from a in db.mtBranches select new SelectListItem { Text = a.Branch, Value = a.Branch + " - " + a.BranchName }).ToList();
                    ViewBag.Branch = new SelectList(BranchList, "Text", "Value");
                    List<SelectListItem> bYesNoList = new List<SelectListItem>
                    {                
                        new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                        new SelectListItem{Text = "No", Value = "N"}
                    };

                    ViewBag.YesNoList = bYesNoList;
                    return View(mtinvmaster);
                }
                ModelState.AddModelError("", "Model State Invalid");
                var returnBranchList = (List<SelectListItem>)(from a in db.mtBranches select new SelectListItem { Text = a.Branch, Value = a.Branch + " - " + a.BranchName }).ToList();
                ViewBag.Branch = new SelectList(returnBranchList, "Text", "Value");
                List<SelectListItem> aYesNoList = new List<SelectListItem>
                {                
                    new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                    new SelectListItem{Text = "No", Value = "N"}
                };

                ViewBag.YesNoList = aYesNoList;
                return View(mtinvmaster);


            }
            catch(Exception ex)
            {
                var returnBranchList = (List<SelectListItem>)(from a in db.mtBranches select new SelectListItem { Text = a.Branch, Value = a.Branch + " - " + a.BranchName }).ToList();
                ViewBag.Branch = new SelectList(returnBranchList, "Text", "Value");
                ViewBag.SetupStockCode = BL.SetupProgramAccess("StockCodeSetup");
                ModelState.AddModelError("", ex.Message);
                List<SelectListItem> YesNoList = new List<SelectListItem>
                {                
                    new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                    new SelectListItem{Text = "No", Value = "N"}
                };

                ViewBag.YesNoList = YesNoList;
                return View(mtinvmaster);
            }
            
        }

        public JsonResult BranchSiteList()
        {
            var result = (from a in db.mtBranchSites select new { Branch = a.Branch, Site = a.Site, SiteName = a.SiteName }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BranchSiteSearch(string ControlId)
        {
            ViewBag.ControlId = ControlId;
            return PartialView();
        }


        public JsonResult AdmTaxList()
        {
            var result = (from a in db.AdmTaxes select new { TaxCode = a.TaxCode, Description = a.Description }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AdmTaxSearch()
        {
            return PartialView();
        }

        public JsonResult mtProductClassList()
        {
            var result = (from a in db.mtProductClasses select a).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult mtProductClassSearch()
        {
            return PartialView();
        }


        //
        // GET: /NonStockedMaster/Edit/5

        public ActionResult Edit(string id = null)
        {
            mtInvMaster mtinvmaster = db.mtInvMasters.Find(id);
            if (mtinvmaster == null)
            {
                return HttpNotFound();
            }
            return View(mtinvmaster);
        }

        //
        // POST: /NonStockedMaster/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtInvMaster mtinvmaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtinvmaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mtinvmaster);
        }

        //
        // GET: /NonStockedMaster/Delete/5

        public ActionResult Delete(string id = null)
        {
            mtInvMaster mtinvmaster = db.mtInvMasters.Find(id);
            if (mtinvmaster == null)
            {
                return HttpNotFound();
            }
            return View(mtinvmaster);
        }

        //
        // POST: /NonStockedMaster/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            mtInvMaster mtinvmaster = db.mtInvMasters.Find(id);
            db.mtInvMasters.Remove(mtinvmaster);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        //GET: /NonStockedMaster/StockCodeSuppliers
        public ActionResult StockCodeSuppliers(string StockCode)
        {         
            try
            {
                
                NonStockedMaster Stock = new NonStockedMaster();
                Stock.Code = StockCode;                  
                Stock.StockCodeSupplier = db.sp_GetStockCodePurchaseHistory(StockCode,"").ToList();
                return View(Stock);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }           
        }
        public JsonResult PurchaseOrderList(string FilterText,string Code)
       {
            var result = db.sp_GetStockCodePurchaseHistory(Code, FilterText.ToUpper()).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}