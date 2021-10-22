using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class LabelPrinterController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();

        //
        // GET: /LabelPrinter/

        public ActionResult Index()
        {
            return View(db.mtLabelPrinters.ToList());
        }

     

        //
        // GET: /LabelPrinter/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /LabelPrinter/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtLabelPrinter mtlabelprinter)
        {
            try
            {
                var check = (from a in db.mtLabelPrinters where a.PrinterName == mtlabelprinter.PrinterName select a).ToList();
                if (check.Count>0)
                {
                    ModelState.AddModelError("", "Printer Already Exists");
                    return View("");
                }
                else
                {
                    mtLabelPrinter obj = new mtLabelPrinter()
                    {
                        PrinterName = mtlabelprinter.PrinterName,
                        PrinterPath = mtlabelprinter.PrinterPath,
                    };
                    db.mtLabelPrinters.Add(obj);
                    db.SaveChanges();
                    return View("Index", db.mtLabelPrinters.ToList());
                }
                //return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index",mtlabelprinter);
            }
        }

        //
        // GET: /LabelPrinter/Edit/5

        public ActionResult Edit(string id = null)
        {
            mtLabelPrinter mtlabelprinter = db.mtLabelPrinters.Find(id);
            if (mtlabelprinter == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtlabelprinter);
        }

        //
        // POST: /LabelPrinter/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtLabelPrinter mtlabelprinter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtlabelprinter).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
            }
            return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /LabelPrinter/Delete/5

        public ActionResult Delete(string id = null)
        {
            mtLabelPrinter mtlabelprinter = db.mtLabelPrinters.Find(id);
            if (mtlabelprinter == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtlabelprinter);
        }

        //
        // POST: /LabelPrinter/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            mtLabelPrinter mtlabelprinter = db.mtLabelPrinters.Find(id);
            db.mtLabelPrinters.Remove(mtlabelprinter);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}

    }
}