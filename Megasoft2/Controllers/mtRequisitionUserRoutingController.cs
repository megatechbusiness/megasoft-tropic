using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionUserRoutingController : Controller
    {
        MegasoftEntities db = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        public string Company { get; private set; }

        //[CustomAuthorize(Activity: "mtReqUserRouting")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            var UserRoutingList = (from a in wdb.mtReqUserRoutings where a.Company == Company select a).ToList();
            return View(UserRoutingList);
        }

        public ActionResult Create(string Username = "")
        {
            ViewBag.UserList = new SelectList(db.mtUsers.ToList(), "Username", "Username");
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                var model = (from a in wdb.mtReqUserRoutings where a.Username == Username && a.Company == Company select a).FirstOrDefault();
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        [HttpPost]
        public ActionResult Create(mtReqUserRouting model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ModelState.Clear();
                var Supp = (from a in wdb.mtReqUserRoutings where a.Username == model.Username && a.RouteTo == model.RouteTo && a.Company == Company && a.NoOfApprovals == 1 select a).FirstOrDefault();
                model.Company = Company;
                int NoOfApprovals = 1;
                if (Supp == null)
                {
                    model.NoOfApprovals = 1;
                    model.Company = Company;
                    wdb.Entry(model).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }
                else
                {
                    Supp.NoOfApprovals = model.NoOfApprovals;
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

        public ActionResult Delete(string Username)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {
                var pclass = (from a in wdb.mtReqUserRoutings where a.Username == Username && a.Company == Company select a).FirstOrDefault();
                wdb.Entry(pclass).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var UserRoutingList = (from a in wdb.mtReqUserRoutings where a.Company == Company select a).ToList();
                return View("Index", UserRoutingList);
            }
            catch (Exception ex)
            {
                var UserRoutingList = (from a in wdb.mtReqUserRoutings where a.Company == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", UserRoutingList);
            }
        }
    }
}