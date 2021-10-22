using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class ScaleSystemSetupController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "ScaleSetup")]
        public ActionResult Index()
        {
            return View(wdb.mtScales.AsEnumerable());
        }

        [CustomAuthorize(Activity: "ScaleSetup")]
        public ActionResult Create( string ScaleModelId ,string ToCopy)
        {
            try
            {
                ViewBag.Printer = (from a in mdb.mtLabelPrinters select new { Value = a.PrinterName, Text = a.PrinterName }).ToList();
                if(ToCopy == null) {
                    if (ScaleModelId == null)
                    {
                        ScaleSystemSetupViewModel scale = new ScaleSystemSetupViewModel();
                        return View(scale);
                    }
                    else
                    {
                        ScaleSystemSetupViewModel scale = new ScaleSystemSetupViewModel();
                        int Id = Convert.ToInt32(ScaleModelId);
                        scale.Scale = wdb.mtScales.Find(Id);
                        return View(scale);
                    }           
                }
                else
                {
                    int Id = Convert.ToInt32(ToCopy);
                    mtScale mtScales = wdb.mtScales.Find(Id);
                    ScaleSystemSetupViewModel scale = new ScaleSystemSetupViewModel();
                    scale.Scale = mtScales;
                    scale.Scale.ScaleModelId = 0;
                    scale.Scale.FriendlyName = "";
                    scale.Scale.PrinterName = "";
                    return View(scale);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Printer = (from a in mdb.mtLabelPrinters select new { Value = a.PrinterName, Text = a.PrinterName }).ToList();
                ModelState.AddModelError("", "Error Loading scale details:  "+ex.InnerException.Message);
                return View();
            }
        }


        [CustomAuthorize(Activity: "ScaleSetup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ScaleSystemSetupViewModel ScaleModel)
        {
           
            ModelState.Clear();
            ViewBag.Printer = (from a in mdb.mtLabelPrinters select new { Value = a.PrinterName, Text = a.PrinterName }).ToList();
            if (ModelState.IsValid)
            {
                //Check update or add
                var checkScale = (from a in wdb.mtScales.AsEnumerable() where a.ScaleModelId == ScaleModel.Scale.ScaleModelId select a).ToList();
                if (checkScale.Count > 0)
                {
                    try
                    {
                        var v = wdb.mtScales.Find(ScaleModel.Scale.ScaleModelId);
                        wdb.Entry(v).CurrentValues.SetValues(ScaleModel.Scale);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(ScaleModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Updating: " + ex.InnerException);
                        return View(ScaleModel);
                    }

                }
                else
                {
                    //Add New Entry
                    try
                    {
                        wdb.mtScales.Add(ScaleModel.Scale);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(ScaleModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Saving: " + ex.InnerException);
                        return View(ScaleModel);
                    }
                }
            }
            else
            { return View(ScaleModel); }
        }
        public ActionResult Delete(string ScaleModelId)
        {
            int Id = Convert.ToInt32(ScaleModelId);
            mtScale mtScales = wdb.mtScales.Find(Id);
            if (mtScales == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtScales);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string ScaleModelId)
        {
            int Id = Convert.ToInt32(ScaleModelId);
            mtScale mtScales = wdb.mtScales.Find(Id);
            wdb.mtScales.Remove(mtScales);
            wdb.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}
