using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionCostCentreController : Controller
    {
        MegasoftEntities db = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        //[CustomAuthorize(Activity: "Req")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            var CostCentreList = (from a in wdb.mtReqCostCentres where a.Company == Company select a).ToList();
            return View(CostCentreList);
        }

        public ActionResult Create(string CostCentre = "")
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                var model = (from a in wdb.mtReqCostCentres where a.CostCentre == CostCentre && a.Company == Company select a).FirstOrDefault();
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        [HttpPost]
        public ActionResult Create(mtReqCostCentre model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                ModelState.Clear();
                var CostCentre = (from a in wdb.mtReqCostCentres where a.CostCentre == model.CostCentre && a.Company == Company select a).FirstOrDefault();
                if (CostCentre == null)
                {
                    model.Company = Company;
                    wdb.Entry(model).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }
                else
                {
                    CostCentre.Description = model.Description;
                    wdb.Entry(CostCentre).State = System.Data.EntityState.Modified;
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


        public ActionResult Delete(string CostCentre)
        {

            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {

                var check = (from a in wdb.mtReqCostCentres where a.Company == Company select a).ToList();

                var costCentre = (from a in wdb.mtReqCostCentres where a.Company == Company && a.CostCentre == CostCentre select a).FirstOrDefault();
                wdb.Entry(costCentre).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var CostCentreList = (from a in wdb.mtReqCostCentres where a.Company == Company select a).ToList();
                return View("Index", CostCentreList);
            }
            catch (Exception ex)
            {
                var CostCentreList = (from a in wdb.mtReqCostCentres where a.Company == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", CostCentreList);
            }
        }

    }
}
