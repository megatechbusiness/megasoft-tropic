using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System.Data;
using System.Data.Entity;
using System.Web.Security;
using Megasoft2.BusinessLogic;

namespace Megasoft2.Controllers
{

    public class SupplierController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();
        private SysproEntities sdb = new SysproEntities("");
        RequisitionBL BL = new RequisitionBL();
        //
        // GET: /Supplier/
        [CustomAuthorize("SupplierSetup", "SupplierView")]
        public ActionResult Index()
        {
              return View(sdb.vwApSuppliers.ToList().Take(100));
        }

        //
        // GET: /Supplier/Create
        [CustomAuthorize("SupplierSetup", "SupplierView")]
        public ActionResult Create(string supplier = null)
        {
            SupplierViewModel model = new SupplierViewModel();
            try
            {
                List<SelectListItem> YesNoList = new List<SelectListItem>
                {                
                    new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                    new SelectListItem{Text = "No", Value = "N"}
                };
                

                ViewBag.YesNoList = YesNoList;
                ViewBag.SetupSupplier = BL.SetupProgramAccess("SupplierSetup");
                if(supplier == null)
                {
                    //We are adding a new supplier
                    vwApSupplier objVw = new vwApSupplier();
                    model.Suppliers = objVw;
                    model.Suppliers.OnHold = "N";
                    return View(model);
                }
                else
                {
                    model.SupplierCode = supplier;
                    model.Suppliers = (from a in sdb.vwApSuppliers where a.Supplier == supplier select a).FirstOrDefault();
                    model.SupplierContact = (from a in sdb.mtSupplierContacts where a.Supplier == supplier select a).ToList();
                    //Check if supplier resides in Syspro - ApSupplier
                    var supCheck = (from a in sdb.ApSuppliers where a.Supplier == supplier select a).ToList();
                    if(supCheck.Count > 0)
                    {
                        //Supplier exists in Syspro 
                        model.isSysproSupplier = "Y";
                    }
                    else
                    {
                        //Supplier must exist in mtApSupplier
                        model.isSysproSupplier = "N";
                    }
                    return View(model);
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }           
        }

         //
        // POST: /Supplier/Create
        [CustomAuthorize(Activity: "SupplierSetup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( SupplierViewModel supplierModel)
        {
            try
            {
                List<SelectListItem> YesNoList = new List<SelectListItem>
                {                
                    new SelectListItem{Text = "Yes", Value = "Y", Selected = false},
                    new SelectListItem{Text = "No", Value = "N"}
                };

                ViewBag.YesNoList = YesNoList;
                ViewBag.SetupSupplier = BL.SetupProgramAccess("SupplierSetup");
                if (supplierModel.SupplierCode == null)
                {
                    //Check if supplier code already exists.
                    var checkSup = (from a in sdb.ApSuppliers where a.Supplier == supplierModel.Suppliers.Supplier select a).ToList();
                    if (checkSup.Count > 0)
                    {
                        ModelState.AddModelError("", "Supplier already exists");
                    }
                    else
                    {
                        //Adding new Supplier in mtApSupplier
                        mtApSupplier objmtAp = new mtApSupplier();
                        objmtAp.Supplier = supplierModel.Suppliers.Supplier;
                        objmtAp.SupplierName = supplierModel.Suppliers.SupplierName;
                        objmtAp.Branch = supplierModel.Suppliers.Branch;
                        objmtAp.TermsCode = supplierModel.Suppliers.TermsCode;
                        objmtAp.Address1 = supplierModel.Suppliers.Address1;
                        objmtAp.Address2 = supplierModel.Suppliers.Address2;
                        objmtAp.Address3 = supplierModel.Suppliers.Address3;
                        objmtAp.Address4 = supplierModel.Suppliers.Address4;
                        objmtAp.Address5 = supplierModel.Suppliers.Address5;
                        objmtAp.TaxRegnNum = supplierModel.Suppliers.TaxRegnNum;
                        objmtAp.OnHold = supplierModel.Suppliers.OnHold;
                        if (objmtAp.Branch == null)
                        {
                            objmtAp.Branch = "HO";                      
                        }
                        if (objmtAp.TermsCode == null)
                        {
                            objmtAp.TermsCode = "P";      
                        }
                        sdb.mtApSuppliers.Add(objmtAp);
                        sdb.SaveChanges();
                        //Output Saved Successfully
                        ModelState.AddModelError("", "Saved Succesfully");
                    }

                }
                else
                {
                    if (supplierModel.isSysproSupplier == "Y")
                    {
                        //Cannot update Syspro table therefore no Update needed.
                    }
                    else
                    {
                        //Update Supplier
                        mtApSupplier objmtAp = new mtApSupplier();
                        objmtAp.Supplier = supplierModel.Suppliers.Supplier;
                        objmtAp.SupplierName = supplierModel.Suppliers.SupplierName;
                        objmtAp.Branch = supplierModel.Suppliers.Branch;
                        objmtAp.TermsCode = supplierModel.Suppliers.TermsCode;
                        objmtAp.Address1 = supplierModel.Suppliers.Address1;
                        objmtAp.Address2 = supplierModel.Suppliers.Address2;
                        objmtAp.Address3 = supplierModel.Suppliers.Address3;
                        objmtAp.Address4 = supplierModel.Suppliers.Address4;
                        objmtAp.Address5 = supplierModel.Suppliers.Address5;
                        objmtAp.TaxRegnNum = supplierModel.Suppliers.TaxRegnNum;
                        objmtAp.OnHold = supplierModel.Suppliers.OnHold;
                        if (objmtAp.Branch == null)
                        {
                            objmtAp.Branch = "HO";
                        }
                        if (objmtAp.TermsCode == null)
                        {
                            objmtAp.TermsCode = "P";
                        }
                        sdb.Entry(objmtAp).State = EntityState.Modified;
                        sdb.SaveChanges();
                        //Output Saved Successfully
                        ModelState.AddModelError("", "Saved Succesfully");
                    }
                }    
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(supplierModel);
            }
            return View(supplierModel);
        }

        public JsonResult BranchSiteList()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = sdb.sp_GetApBranch();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BranchSearch()
        {
            return PartialView();
        }
        public JsonResult ApTermsList()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = sdb.sp_GetApTerms().ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApTermsSearch()
        {
            return PartialView();
        }

        // GET: /SupplierController/Edit/5
        public ActionResult Edit(string Supplier,int id)
        {
            mtSupplierContact supplier = (from a in sdb.mtSupplierContacts where a.Supplier == Supplier && a.ContactId == id select a).FirstOrDefault();
           if (supplier == null)
            {
                return HttpNotFound();
            }
           return PartialView(supplier);
        }

        //
         //POST: /SupplierController/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtSupplierContact model)
        {

            if (ModelState.IsValid)
            {
                sdb.Entry(model).State = EntityState.Modified;
                sdb.SaveChanges();
                ModelState.AddModelError("", "Saved Succesfully");

                return RedirectToAction("Create",model);
            }
            ModelState.AddModelError("", "One or more values are incorrect");
            return RedirectToAction("Create",model);
        }

        
         //GET: /SupplierController/Delete/5
        public ActionResult Delete(string Supplier, int id)
        {
            mtSupplierContact supplier = (from a in sdb.mtSupplierContacts where a.Supplier == Supplier && a.ContactId == id select a).FirstOrDefault();
           if (supplier == null)
            {
                return HttpNotFound();
            }
           return PartialView(supplier);
        
        }

        //
        // POST: /SupplierController/Delete/5
        [CustomAuthorize(Activity: "")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Supplier,int id)
        {
            mtSupplierContact supplier = (from a in sdb.mtSupplierContacts where a.Supplier == Supplier && a.ContactId == id select a).FirstOrDefault();
            sdb.mtSupplierContacts.Remove(supplier);
            sdb.SaveChanges();
            ModelState.AddModelError("", "Saved Succesfully");
            return RedirectToAction("Create",supplier);
        }

       // GET: /SupplierController/CreateContact/5      
        public ActionResult CreateContact(string Supplier)
        {

            mtSupplierContact objmtAp = new mtSupplierContact();
            objmtAp.Supplier = Supplier;
            return PartialView(objmtAp);
        }

        // POST: /SupplierController/CreateContact/5  
        [HttpPost]
        public ActionResult CreateContact(mtSupplierContact objmtAp)
        {
            
          if (ModelState.IsValid)
            {
                
                sdb.mtSupplierContacts.Add(objmtAp);
                sdb.SaveChanges();
                ModelState.AddModelError("", "Saved Succesfully");

                return RedirectToAction("Create", objmtAp);
            }
            ModelState.AddModelError("", "One or more values are incorrect");
            return RedirectToAction("Create", objmtAp);
        }
        public JsonResult GetSupplier(string FilterText)
        {
            FilterText = FilterText.Replace("&amp;", "&");
            var result = sdb.sp_GetAllSuppliers(FilterText.ToUpper()).Take(100);
            return Json(result, JsonRequestBehavior.AllowGet);
            
        }
        
        public ActionResult SupplierDetails(string supplier)
        {
            try
            {
                SupplierViewModel SV = new SupplierViewModel();
                SV.GetSupplier = supplier;
                SV.GrnSupplier = sdb.sp_GetSupplierPurchaseHistoryHeader(supplier, "").ToList();
                return View(SV);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
        public JsonResult SupplierPurchaseOrderHistory(string FilterText, string supplier)
        {
            supplier = supplier.Replace("&amp;", "&");
            var result = sdb.sp_GetSupplierPurchaseHistoryHeader(supplier, FilterText.ToUpper()).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SupplierPurchaseOrderDetail(string Requisition)
        {
            var result = sdb.sp_GetSupplierPurchaseHistoryDetail(Requisition).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            sdb.Dispose();
            base.Dispose(disposing);
        }        
    }  
}
