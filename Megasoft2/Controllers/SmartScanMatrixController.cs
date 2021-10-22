using Megasoft2.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SmartScanMatrixController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities db = new MegasoftEntities();
        //
        // GET: /SmartScanMatrix/
        [CustomAuthorize(Activity: "SmartScanSetup")]
        public ActionResult Index()
        {
            return View(db.mtSmartScanMatrices.AsEnumerable());
        }

        public ActionResult Create(Guid SmartId)
        {
            ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
            ViewBag.Username = (from a in db.mtUsers select new { User = a.Username }).ToList();
            ViewBag.TrnType = (from a in db.mtSmartScanTypes select new { TrnType = a.TrnType, TrnTypeDesc = a.Description }).ToList();
            ViewBag.Printers = (from a in db.mtLabelPrinters select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();

            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.Bin = null;

            if (SmartId != Guid.Empty)
            {
                var v = db.mtSmartScanMatrices.Find(SmartId);
                SmartScanViewModel SmartScan = new SmartScanViewModel();
                SmartScan.SmartScan = v;
                return View(SmartScan);
            }
            else
            {
                SmartScanViewModel SmartScan = new SmartScanViewModel();
                return View(SmartScan);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save")]
        public ActionResult CreateSmartId(SmartScanViewModel model)
        {
            try{
                ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                ViewBag.Username = (from a in db.mtUsers select new { User = a.Username }).ToList();
                ViewBag.TrnType = (from a in db.mtSmartScanTypes select new { TrnType = a.TrnType, TrnTypeDesc = a.Description }).ToList();
                ViewBag.Printers = (from a in db.mtLabelPrinters select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.Bin = null;
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    if (model.SmartScan.SmartId != Guid.Empty  )
                    {
                        var v = db.mtSmartScanMatrices.Find(model.SmartScan.SmartId);
                        db.Entry(v).CurrentValues.SetValues((model.SmartScan));
                        db.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");

                        return View("Create", model);
                    }
                    else
                    {
                        Guid SmartGuid = Guid.NewGuid();
                        db.mtSmartScanMatrices.Add(new mtSmartScanMatrix
                        {
                            SmartId = SmartGuid,
                            Company = model.SmartScan.Company,
                            Username = model.SmartScan.Username,
                            TrnType = model.SmartScan.TrnType,
                            SourceWarehouse = model.SmartScan.SourceWarehouse,
                            SourceBin = model.SmartScan.SourceBin,
                            DestWarehouse = model.SmartScan.DestWarehouse,
                            DestBin = model.SmartScan.DestBin
                        });
                        db.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        model.SmartScan.SmartId = SmartGuid;
                        return View("Create", model);
                    }
                }
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View("Create", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Print")]
        public ActionResult PrintScanCard(SmartScanViewModel model)
        {
            try
            {
                if (model.SmartScan != null)
                {
                    var TrnType = (from a in db.mtSmartScanTypes where a.TrnType == model.SmartScan.TrnType select new { a.Description }).FirstOrDefault();
                    StreamReader reader = new StreamReader(HttpContext.Server.MapPath("~/SmartScanLabel/Labels/SmartScanLabel.txt").ToString());
                    string Template = reader.ReadToEnd();
                    Template = Template.Replace("<<DATE>>", DateTime.Now.Date.ToString("dd-MM-yyyy"));
                    Template = Template.Replace("<<Username>>", model.SmartScan.Username);
                    Template = Template.Replace("<<Transaction>>", TrnType.Description.ToString());
                    Template = Template.Replace("<<FromWh>>", model.SmartScan.SourceWarehouse);
                    Template = Template.Replace("<<FromBin>>", model.SmartScan.SourceBin);
                    Template = Template.Replace("<<ToBin>>", model.SmartScan.DestBin);
                    Template = Template.Replace("<<ToWh>>", model.SmartScan.DestWarehouse);
                    Template = Template.Replace("<<Barcode>>", "|" + model.SmartScan.SmartId);
                    reader.Close();
                    StreamWriter writer = new StreamWriter(HttpContext.Server.MapPath("~/SmartScanLabel/Labels/SmartLabelTemp.zpl").ToString(), false);
                    writer.WriteLine(Template);
                    writer.Close();
                    string Printer = model.Printer;
                    string PrinterPath = (from a in db.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                    System.IO.File.Copy(HttpContext.Server.MapPath("~/SmartScanLabel/Labels/SmartLabelTemp.zpl").ToString(), PrinterPath, true);
                    ModelState.AddModelError("", "Printed Successfully");
                    return RedirectToAction("Create", new { SmartId = model.SmartScan.SmartId });
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create", model);
            }
        }

        [HttpPost]
        public ActionResult GetBin(string Warehouse)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var BinList = wdb.sp_GetBins(Warehouse).ToList();
                var Bins = (from a in BinList where a.Warehouse == Warehouse select new { ID = a.Bin, Text = a.Bin }).ToList();
                var result = (from a in wdb.vw_InvWhControl where a.Warehouse == Warehouse select a).ToList();
                if (result.Count > 0)
                {
                    if (result.FirstOrDefault().UseMultipleBins == "Y")
                    {
                        return Json(Bins, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Delete")]
        public ActionResult Delete(Guid SmartId)
        {
            try
            {
                mtSmartScanMatrix mtSmartScanMatrix = db.mtSmartScanMatrices.Find(SmartId);
                db.mtSmartScanMatrices.Remove(mtSmartScanMatrix);
               db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }

        }
    }

    
}