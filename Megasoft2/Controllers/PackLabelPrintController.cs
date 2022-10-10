using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.ViewModel;
using Megasoft2.BusinessLogic;

namespace Megasoft2.Controllers
{
    public class PackLabelPrintController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private LabelPrint objPrint = new LabelPrint();
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            return View();
        }

        public JsonResult LoadDetails(string BatchId)
        {            
            PackLabelPrintViewModel model = new PackLabelPrintViewModel();
            model.ErrorMessage = "";
            model.BatchId = BatchId;
            model.Job = (from a in wdb.mtProductionLabels where a.BatchId == BatchId select a.Job).FirstOrDefault();
            model.BatchNo = BatchId;
            var packLabelDetails = (from a in wdb.mt_ProductionPackLabelDetailsByJob(model.Job) select a).FirstOrDefault();
            if (packLabelDetails.BagPerPack >0 && packLabelDetails.BagPerBale > 0)
            {
            model.PackSize = packLabelDetails.BagPerPack;
            model.NoOfLabels = (int)(packLabelDetails.BagPerBale / packLabelDetails.BagPerPack);
            }
            else
            {
                if (packLabelDetails.BagPerPack == null || packLabelDetails.BagPerPack ==0)
                {
                    model.ErrorMessage = "Bag per pack needs a value";
                }
                if (packLabelDetails.BagPerBale == null || packLabelDetails.BagPerBale == 0)
                {
                    model.ErrorMessage = model.ErrorMessage ==""? "Bag per bale needs a value" : model.ErrorMessage + ". " + "Bag per bale needs a value";
                }                
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Print")]
        public ActionResult Print(PackLabelPrintViewModel model)
        {
            try
            {
                string Username = User.Identity.Name.ToString().ToUpper();
                var pack = (from a in wdb.mtProductionPackLabelPrints where a.Job == model.Job && a.BatchId == model.BatchId select a.PackNo).ToList();
                int max =  pack.Count == 0? 0: pack.Max();
                for (int i = max+1; i <= model.NoOfLabels+max; i++)
                {
                    List<mtProductionPackLabelPrint> packDetails = new List<mtProductionPackLabelPrint>();
                    mtProductionPackLabelPrint obj = new mtProductionPackLabelPrint();
                    obj.Job = model.Job;
                    obj.BatchId = model.BatchId;
                    obj.PackNo = i ;
                    obj.ExtruderNo = model.ExtruderNo;
                    obj.ExtruderRoll = model.ExtRoll;
                    obj.PrintRoll = model.PrintRoll;
                    obj.OpCode = model.OpCode;
                    obj.BatchNo = model.BatchNo;
                    obj.PackSize = model.PackSize;
                    obj.NoOfLabels = model.NoOfLabels;
                    obj.Printer = model.Printer;
                    obj.Packer = model.Packer;
                    obj.Username = Username;
                    obj.TrnDate = DateTime.Now;
                    obj.PrinterOp = model.PrinterOp;

                    wdb.Entry(obj).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                    packDetails.Add(obj);
                    var result = objPrint.PrintPackLabel(packDetails);
                    if (result != "Label Printed Successfully!")
                    {
                        break;
                    }
                }
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                ModelState.AddModelError("", "Label Printed Successfully!");
                return View("Index", model);
            }
            catch (Exception ex)
            {

                model.ErrorMessage = ex.Message;
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                string Username = User.Identity.Name.ToString().ToUpper();
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                return View("Index", model);

            }
            
            
        }

    }
}
