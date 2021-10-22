using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TankMasterController : Controller
    {
        //
        // GET: /TankMaster/
        private MegasoftEntities db = new MegasoftEntities();
        //Adr_LoggingEntities ldb = new Adr_LoggingEntities();

        [CustomAuthorize(Activity: "Tanks")]
        public ActionResult Index()
        {
            return View(db.mtTankMasters.ToList());
        }

      
        //
        // GET: /TankMaster/Create
        [CustomAuthorize(Activity: "Tanks")]
        public ActionResult Create()
        {
            List<SelectListItem> TankTypes = new List<SelectListItem>();
            TankTypes.Add(new SelectListItem() { Text = "Feeder", Value = "Feeder" });
            TankTypes.Add(new SelectListItem() { Text = "Blend", Value = "Blend" });
            
            ViewBag.TankType = new SelectList(TankTypes, "Value", "Text");
            return PartialView();
        }

        //
        // POST: /TankMaster/Create
        [CustomAuthorize(Activity: "Tanks")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtTankMaster mtTankMaster)
        {
            if (ModelState.IsValid)
            {
                mtTankMaster.Operator = HttpContext.User.Identity.Name.ToUpper();
                mtTankMaster.DateLastUpdated = DateTime.Now;
                
                db.mtTankMasters.Add(mtTankMaster);
                db.SaveChanges();
                
                return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
            }

            return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /TankMaster/Edit/5
        [CustomAuthorize(Activity: "Tanks")]
        public ActionResult Edit(string tank = null)
        {
            try
            {

                mtTankMaster mtTankMaster = db.mtTankMasters.Find(tank);
                if (mtTankMaster == null)
                {
                    return HttpNotFound();
                }

                List<SelectListItem> TankTypes = new List<SelectListItem>();
                TankTypes.Add(new SelectListItem() { Text = "Feeder", Value = "Feeder" });
                TankTypes.Add(new SelectListItem() { Text = "Blend", Value = "Blend" });

                ViewBag.TankTypes = new SelectList(TankTypes, "Value", "Text");

                return PartialView(mtTankMaster);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /TankMaster/Edit/5
        [CustomAuthorize(Activity: "Tanks")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtTankMaster mtTankMaster)
        {
            if (ModelState.IsValid)
            {
                mtTankMaster.Operator = HttpContext.User.Identity.Name.ToUpper();
                mtTankMaster.DateLastUpdated = DateTime.Now;
                db.Entry(mtTankMaster).State = EntityState.Modified;
                bool ProductChanged = false;
                var OldProduct = (from a in db.mtTankMasters where a.Tank == mtTankMaster.Tank select a.Product).FirstOrDefault();
                if (OldProduct != mtTankMaster.Product)
                {
                    ProductChanged = true;
                }
                db.SaveChanges();
                if (ProductChanged == true)
                {
                    mtTankProductHistory objHistory = new mtTankProductHistory
                    {
                        Tank = mtTankMaster.Tank,
                        Product = mtTankMaster.Product,
                        DateUpdated = DateTime.Now,
                        Operator = HttpContext.User.Identity.Name.ToUpper()
                    };
                    db.mtTankProductHistories.Add(objHistory);
                    db.SaveChanges();
                }
                return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
            }
            return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /TankMaster/Delete/5
        [CustomAuthorize(Activity: "Tanks")]
        public ActionResult Delete(string tank = null)
        {
            mtTankMaster mtTankMaster = db.mtTankMasters.Find(tank);
            if (mtTankMaster == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtTankMaster);
        }

        //
        // POST: /User/Delete/5
        [CustomAuthorize(Activity: "Tanks")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string tank)
        {
            mtTankMaster mtTankMaster = db.mtTankMasters.Find(tank);
            db.mtTankMasters.Remove(mtTankMaster);
            db.SaveChanges();
            return RedirectToAction("Index");
            
        }
    }
}
