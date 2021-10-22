using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionCostCentreProductClassController : Controller
    {
        MegasoftEntities db = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");


        //[CustomAuthorize(Activity: "reqBranch")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            var ReqCostCentreProducthList = (from a in wdb.mtReqCostCentreProductClasses where a.Company == Company select a).ToList();
            return View(ReqCostCentreProducthList);
        }

        public ActionResult Create()
        {
            string Username = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var CostCentreList = wdb.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
            try
            {


                return View("Create");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        [HttpPost]
        public ActionResult Create(mtReqCostCentreProductClass model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.CostCentreList = new SelectList(wdb.mtReqCostCentres.Where(a => a.Company == Company).ToList(), "CostCentre", "CostCentre");

                ModelState.Clear();
                var ccprod = (from a in wdb.mtReqCostCentreProductClasses where a.CostCentre == model.CostCentre && a.ProductClass == model.ProductClass && a.Company == Company select a).FirstOrDefault();
                model.Company = Company;
                if (ccprod == null)
                {
                    model.Company = Company;
                    wdb.Entry(model).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }
                else
                {
                    ccprod.CostCentre = model.CostCentre;
                    wdb.Entry(ccprod).State = System.Data.EntityState.Modified;
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

        public ActionResult ProductClassSearch()
        {
            return PartialView();
        }

        public JsonResult ProductClassList()
        {
            var result = (from a in wdb.SalProductClassDes.AsNoTracking() select a).ToList();
            var ProdClass = (from a in result select new { ProductClass = a.ProductClass, Description = a.Description }).Distinct().ToList();
            return Json(ProdClass, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Delete(string CostCentre, string ProductClass)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {

                var pclass = (from a in wdb.mtReqCostCentreProductClasses where a.CostCentre == CostCentre && a.ProductClass == ProductClass && a.Company == Company select a).FirstOrDefault();
                db.Entry(pclass).State = System.Data.EntityState.Deleted;
                db.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var ReqCostCentreProducthList = (from a in wdb.mtReqCostCentreProductClasses where a.Company == Company select a).ToList();
                return View("Index", ReqCostCentreProducthList);
            }
            catch (Exception ex)
            {
                var ReqCostCentreProducthList = (from a in wdb.mtReqCostCentreProductClasses where a.Company == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", ReqCostCentreProducthList);
            }
        }

    }
}
