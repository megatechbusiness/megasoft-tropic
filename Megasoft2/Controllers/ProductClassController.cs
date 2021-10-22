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

namespace Megasoft2.Controllers
{
    public class ProductClassController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();
        private SysproEntities sdb = new SysproEntities("");
        //
        // GET: /ProductClass/Index
        [CustomAuthorize(Activity: "ProductClass")]
        public ActionResult Index()
        {
            return View(sdb.mtProductClasses.ToList());          
        }
        //
        // GET: /ProductClass/Create
        [CustomAuthorize(Activity: "ProductClass")]
        public ActionResult Create(string product)
        {
           mtProductClass objPc = new mtProductClass();
            objPc.ProductClass = product;
            return PartialView(objPc);
        }
        //
        // POST: /ProductClass/Create
        [CustomAuthorize(Activity: "ProductClass")]
        [HttpPost]
        public ActionResult Create(mtProductClass objPc)
        {
            if (ModelState.IsValid)
            {
                objPc.ProductClass = objPc.ProductClass.ToUpper();

                objPc.Description = objPc.Description.ToUpper();
                sdb.mtProductClasses.Add(objPc);
                sdb.SaveChanges();
                ModelState.AddModelError("", "Saved Succesfully");

                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "One or more values are incorrect");
            return RedirectToAction("Index");
        }
        // GET: /ProductClass/Edit/5
        [CustomAuthorize(Activity: "ProductClass")]
        public ActionResult Edit(string product)
        {
            mtProductClass ProductClass = (from a in sdb.mtProductClasses where a.ProductClass == product select a).FirstOrDefault();
            if (ProductClass == null)
            {
                return HttpNotFound();
            }
            return PartialView(ProductClass);
        }
        // POST: /ProductClass/Edit/5
        [CustomAuthorize(Activity: "ProductClass")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtProductClass model)
        {

            if (ModelState.IsValid)
            {
                model .ProductClass = model.ProductClass.ToUpper();
                model.Description = model.Description.ToUpper();
                sdb.Entry(model).State = EntityState.Modified;
                sdb.SaveChanges();
                ModelState.AddModelError("", "Saved Succesfully");

                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "One or more values are incorrect");
            return RedirectToAction("Index");
        }
        //GET: /ProductClass/Delete/5
        [CustomAuthorize(Activity: "ProductClass")]
        public ActionResult Delete(string product)
        {
            mtProductClass objPc = (from a in sdb.mtProductClasses where a.ProductClass == product select a).FirstOrDefault();
            if (objPc == null)
            {
                return HttpNotFound();
            }
            return PartialView(objPc);
        }

        //
        // POST: /PoductClass/Delete/5
        [CustomAuthorize(Activity: "ProductClass")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string product)
        {
            mtProductClass objPc = (from a in sdb.mtProductClasses where a.ProductClass == product select a).FirstOrDefault();
            sdb.mtProductClasses.Remove(objPc);
            sdb.SaveChanges();
            ModelState.AddModelError("", "Saved Succesfully");
            return RedirectToAction("Index");
        }            
        }
}  

