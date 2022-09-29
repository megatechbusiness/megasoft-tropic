using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.ViewModel;

namespace Megasoft2.Controllers
{
    public class PackLabelPrintController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
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
            return View("Index",model);
        }

    }
}
