using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionProductClassGlCodeController : Controller
    {
        MegasoftEntities db = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        //[CustomAuthorize(Activity: "reqBranch")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            var ProductClassList = (from a in wdb.mtReqProductClassGlCodes where a.Company == Company select a).ToList();
            return View(ProductClassList);
        }

        public ActionResult Create(string ProductClass = "")
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.ProductClassList = new SelectList(wdb.mtReqCostCentreProductClasses.Where(a => a.Company == Company).Select(a => a.ProductClass).Distinct().ToList(), "ProductClass", "ProductClass");
            try
            {

                var model = (from a in wdb.mtReqProductClassGlCodes where a.ProductClass == ProductClass && a.Company == Company select a).FirstOrDefault();
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }


        public ActionResult GlCodeSearch()
        {
            return PartialView();
        }

        public JsonResult GlCodeList()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = wdb.sp_mtReqGetGenMaster(Company).ToList();
            var ProdClass = (from a in result select new { GlCode = a.GlCode, Description = a.Description }).Distinct().ToList();
            return Json(ProdClass, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(mtReqProductClassGlCode model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                ModelState.Clear();
                var Supp = (from a in wdb.mtReqProductClassGlCodes where a.ProductClass == model.ProductClass && a.GlCode == model.GlCode && a.Company == Company select a).FirstOrDefault();
                model.Company = Company;
                if (Supp == null)
                {
                    model.Company = Company;
                    wdb.Entry(model).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }
                else
                {
                    Supp.GlCode = model.GlCode;
                    wdb.Entry(Supp).State = System.Data.EntityState.Modified;
                    wdb.SaveChanges();
                }
                ModelState.AddModelError("", "Saved.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }




        public ActionResult Delete(string ProductClass)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {

                var pclass = (from a in wdb.mtReqProductClassGlCodes where a.ProductClass == ProductClass && a.Company == Company select a).FirstOrDefault();
                wdb.Entry(pclass).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var ProductClassList = (from a in wdb.mtReqProductClassGlCodes select a).ToList();
                return View("Index", ProductClassList);
            }
            catch (Exception ex)
            {
                var ProductClassList = (from a in wdb.mtReqProductClassGlCodes select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", ProductClassList);
            }
        }

    }
}