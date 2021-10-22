using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManScanCardController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        LabelPrint objPrint = new LabelPrint();
        //
        // GET: /WhseManScanCard/

        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "ScanCard")]
        public ActionResult Index()
        {
            PurchaseOrderLabel PoOut = new PurchaseOrderLabel();
            ViewBag.Printers = (from a in mdb.mtLabelPrinters select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            return View(PoOut);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "ScanCard")]
        public ActionResult PrintScanCard(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));

                objPrint.PrintScanCard(detail);

                return Json("Completed Successfully", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [CustomAuthorize(Activity: "ScanCard")]
        public ActionResult ValidateStockCode(string details)
        {
            try
            {

                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                string StockCode = detail.FirstOrDefault().StockCode;
                var result = (from a in wdb.InvMasters where a.StockCode == StockCode select a).FirstOrDefault();

                if (result != null)
                {

                    List<LabelPrintPoLine> PoOut = new List<LabelPrintPoLine>();
                    PoOut.Add(new LabelPrintPoLine { StockCode = result.StockCode, Description = result.Description });
                    return Json(PoOut, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("StockCode not found.", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [CustomAuthorize(Activity: "BinLabelPrint")]
        public ActionResult PrintBinLabel()
        {
            ViewBag.Printers = (from a in mdb.mtLabelPrinters select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "BinLabelPrint")]
        public ActionResult PrintBinLabel(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));

                objPrint.PrintBinLabel(detail);

                return Json("Completed Successfully", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
