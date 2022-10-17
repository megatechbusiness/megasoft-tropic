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
        [CustomAuthorize(Activity: "PackLabelPrint")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            PackLabelPrintViewModel model = new PackLabelPrintViewModel();
            model.Job = "";
            return View("Index", model);
        }

        [CustomAuthorize(Activity: "PackLabelPrint")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadBatchID")]
        public ActionResult LoadBatchID(PackLabelPrintViewModel model)
        {

            ModelState.Clear();
            model.Job = model.Job.PadLeft(15, '0');
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();

            model.BatchIdList = (from a in wdb.mtProductionLabels where a.Job == model.Job select a.BatchId).ToList();
            if (model.BatchIdList.Count() > 0)
            {
                var packLabelDetails = (from a in wdb.mt_ProductionPackLabelDetailsByJob(model.Job) select a).FirstOrDefault();
                if (packLabelDetails.BagPerPack > 0 && packLabelDetails.BagPerBale > 0)
                {
                    model.PackSize = packLabelDetails.BagPerPack;
                    model.NoOfLabels = (int)(packLabelDetails.BagPerBale / packLabelDetails.BagPerPack);

                    model.LabelDetails = new List<mtProductionPackLabelPrint>();

                    for (int i = 0; i < model.BatchIdList.Count(); i++)
                    {
                        model.LabelDetails.Add(new mtProductionPackLabelPrint
                        {
                            BatchId = model.BatchIdList[i],
                            PackSize = model.PackSize,
                            NoOfLabels = model.NoOfLabels
                        });
                    }
                }
                else
                {
                    if (packLabelDetails.BagPerPack == null || packLabelDetails.BagPerPack == 0)
                    {
                        model.ErrorMessage = "Bag per pack needs a value";
                    }
                    if (packLabelDetails.BagPerBale == null || packLabelDetails.BagPerBale == 0)
                    {
                        model.ErrorMessage = model.ErrorMessage == "" ? "Bag per bale needs a value" : model.ErrorMessage + ". " + "Bag per bale needs a value";
                    }

                    ModelState.AddModelError("", model.ErrorMessage);
                    if (model.LabelDetails != null)
                    {
                        model.LabelDetails.Clear();
                    }
                    return View("Index", model);
                }
            }
            else
            {
                ModelState.AddModelError("", "No batches exist for Job: " + model.Job);
                if (model.LabelDetails !=null)
                {
                    model.LabelDetails.Clear();
                }
                
                return View("Index", model);
            }


            return View("Index", model);
        }

        public ActionResult Print(string details)
        {
            try
            {
                List<mtProductionPackLabelPrint> myDeserializedObject = (List<mtProductionPackLabelPrint>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtProductionPackLabelPrint>));
                PackLabelPrintViewModel model = new PackLabelPrintViewModel();
                model.LabelDetails = myDeserializedObject;
                string Username = User.Identity.Name.ToString().ToUpper();
                foreach (var item in model.LabelDetails)
                {                    
                    var pack = (from a in wdb.mtProductionPackLabelPrints where a.Job == item.Job && a.BatchId == item.BatchId select a.PackNo).ToList();
                    int max =  pack.Count == 0? 0: pack.Max();
                    for (int i = max+1; i <= item.NoOfLabels+max; i++)
                    {
                        List<mtProductionPackLabelPrint> packDetails = new List<mtProductionPackLabelPrint>();
                        mtProductionPackLabelPrint obj = new mtProductionPackLabelPrint();
                        var BatchPackNo = item.BatchId + "-" + i.ToString().PadLeft(2, '0');
                        obj.Job = item.Job;
                        obj.BatchId = item.BatchId;
                        obj.PackNo = i ;
                        obj.ExtruderNo = item.ExtruderNo;
                        obj.ExtruderRoll = item.ExtruderRoll;
                        obj.PrintRoll = item.PrintRoll;
                        obj.OpCode = item.OpCode;
                        obj.BatchPackNo = BatchPackNo;
                        obj.PackSize = item.PackSize;
                        obj.NoOfLabels = item.NoOfLabels;
                        obj.Printer = item.Printer;
                        obj.Username = Username;
                        obj.TrnDate = DateTime.Now;
                        obj.PrinterOp = item.PrinterOp;

                        wdb.Entry(obj).State = System.Data.EntityState.Added;
                        wdb.SaveChanges();
                        packDetails.Add(obj);
                        var result = objPrint.PrintPackLabel(packDetails);
                        if (result != "Label Printed Successfully!")
                        {
                            break;
                        }
                    }

                }
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                model.ErrorMessage = "Label Printed Successfully!";
                return Json(model.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                PackLabelPrintViewModel model = new PackLabelPrintViewModel();
                model.ErrorMessage = ex.Message;
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                string Username = User.Identity.Name.ToString().ToUpper();
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            
            
        }

    }
}
