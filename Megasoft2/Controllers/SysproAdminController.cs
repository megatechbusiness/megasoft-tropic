using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SysproAdminController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();

        //
        // GET: /SysproAdmin/
        [CustomAuthorize(Activity: "Companies")]
        public ActionResult Index()
        {
            return View(db.mtSysproAdmins.ToList());
        }


        //
        // GET: /SysproAdmin/Create
        [CustomAuthorize(Activity: "Companies")]
        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /SysproAdmin/Create
        [CustomAuthorize(Activity: "Companies")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtSysproAdmin mtsysproadmin)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.mtSysproAdmins.Add(mtsysproadmin);
                    db.SaveChanges();
                    return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
                }

                return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /SysproAdmin/Edit/5
        [CustomAuthorize(Activity: "Companies")]
        public ActionResult Edit(string id = null)
        {
            mtSysproAdmin mtsysproadmin = db.mtSysproAdmins.Find(id);
            if (mtsysproadmin == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtsysproadmin);
        }

        //
        // POST: /SysproAdmin/Edit/5
        [CustomAuthorize(Activity: "Companies")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtSysproAdmin mtsysproadmin)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    db.Entry(mtsysproadmin).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
                }
                return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
            }            
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /SysproAdmin/Delete/5
        [CustomAuthorize(Activity: "Companies")]
        public ActionResult Delete(string id = null)
        {
            mtSysproAdmin mtsysproadmin = db.mtSysproAdmins.Find(id);
            if (mtsysproadmin == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtsysproadmin);
        }

        //
        // POST: /SysproAdmin/Delete/5
        [CustomAuthorize(Activity: "Companies")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                mtSysproAdmin mtsysproadmin = db.mtSysproAdmins.Find(id);
                db.mtSysproAdmins.Remove(mtsysproadmin);
                db.SaveChanges();
                return RedirectToAction("Index");
            }            
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}