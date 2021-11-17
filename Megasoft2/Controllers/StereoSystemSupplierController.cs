using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.ViewModel;

namespace Megasoft2.Controllers
{
    public class StereoSystemSupplierController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        // GET: /StereoSystemSupplier/

        public ActionResult Index()
        {
            return View(wdb.mtStereoSuppliers.AsEnumerable());
        }

        [CustomAuthorize(Activity: "CreateSupplier")]
        public ActionResult Create(string Supplier)
        {
            try
            {
                if (Supplier == null)
                {
                    StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
                    return View(model);
                }
                else
                {
                    mtStereoSupplier mtStereoSupplier = new mtStereoSupplier();
                    if (mtStereoSupplier == null)
                    {
                        return HttpNotFound();
                    }
                    StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
                    model.tblSupplier = wdb.mtStereoSuppliers.Find(Supplier);

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }
        [CustomAuthorize(Activity: "CreateSupplier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StereoSystemAddStereoViewModel StereoModel)
        {
            try
            {
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    var checkUser = (from a in wdb.mtStereoSuppliers.AsEnumerable() where a.Supplier == StereoModel.tblSupplier.Supplier  select a).ToList();
                    if (checkUser.Count > 0)
                    {
                        var v = wdb.mtStereoSuppliers.Find(StereoModel.tblSupplier.Supplier);
                        wdb.Entry(v).CurrentValues.SetValues(StereoModel.tblSupplier);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(StereoModel);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(StereoModel.tblSupplier.Supplier))
                        {
                            ModelState.AddModelError("", "Please select a Supplier");
                            return View(StereoModel);
                        }
                        if (string.IsNullOrEmpty(StereoModel.tblSupplier.CustomerExpenseGlCode))
                        {
                            ModelState.AddModelError("", "Please select a Gl Code");
                            return View(StereoModel);
                        }
                        if (string.IsNullOrEmpty(StereoModel.tblSupplier.ProductClass))
                        {
                            ModelState.AddModelError("", "Please select a Product Class");
                            return View(StereoModel);
                        }
                        if (string.IsNullOrEmpty(StereoModel.tblSupplier.TaxCode))
                        {
                            ModelState.AddModelError("", "Please select a Tax Code");
                            return View(StereoModel);
                        }
                        if (string.IsNullOrEmpty(StereoModel.tblSupplier.Email))
                        {
                            ModelState.AddModelError("", "Please enter a Email");
                            return View(StereoModel);
                        }

                        wdb.mtStereoSuppliers.Add(new mtStereoSupplier
                        {
                            Supplier= StereoModel.tblSupplier.Supplier,
                            CustomerExpenseGlCode = StereoModel.tblSupplier.CustomerExpenseGlCode,
                            ProductClass = StereoModel.tblSupplier.ProductClass,
                            Taxable = StereoModel.tblSupplier.Taxable,
                            TaxCode = StereoModel.tblSupplier.TaxCode,
                            Email = StereoModel.tblSupplier.Email

                        });
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(StereoModel);
                    }
                }

                return View(StereoModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View(StereoModel);
            }
        }

        [CustomAuthorize(Activity: "CreateSupplier")]
        public ActionResult Delete(string Supplier)
        {
            mtStereoSupplier mtStereoSuppliers = wdb.mtStereoSuppliers.Find(Supplier);
            wdb.mtStereoSuppliers.Remove(mtStereoSuppliers);
            wdb.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult GlCodeSearch ()
        {
            return PartialView();
        }
        public ActionResult GlCodeList(string FilterText)
        {
            var result = wdb.sp_GetStereoGlCodes(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductClassSearch()
        {
            return PartialView();
        }
        public ActionResult ProductClassList(string FilterText)
        {
            var result = wdb.sp_GetStereoProductClass(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TaxCodeSearch()
        {
            return PartialView();
        }
        public ActionResult TaxCodeList(string FilterText)
        {
            var result = wdb.sp_GetStereoTaxCode(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SupplierSearch()
        {
            return PartialView();
        }
        public ActionResult SupplierList(string FilterText)
        {
            var result = wdb.sp_GetStereoSysproSupplier(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
