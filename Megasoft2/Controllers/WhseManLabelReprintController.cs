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
    public class WhseManLabelReprintController : Controller
    {
        //
        // GET: /WhseManLabelReprint/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        LabelPrint objPrint = new LabelPrint();

        [CustomAuthorize(Activity: "PoLabelReprint")]
        public ActionResult Index(string Program = null)
        {
            PurchaseOrderLabel PoOut = new PurchaseOrderLabel();
            PoOut.Program = Program;
            string Username = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            return View(PoOut);
        }


        [HttpPost]
        [CustomAuthorize(Activity: "PoLabelReprint")]
        public ActionResult ValidatePo(string details)
        {
            try
            {

                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0');
                var result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder select a).FirstOrDefault();

                if (result != null)
                {

                    List<LabelPrintPoLine> PoOut = new List<LabelPrintPoLine>();
                    PoOut.Add(new LabelPrintPoLine { PurchaseOrder = result.PurchaseOrder, Line = result.Line, StockCode = result.StockCode, Description = result.Description });
                    return Json(PoOut, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No labels found to re-print.", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "PoLabelReprint")]
        public ActionResult NavigatePoLine(string details)
        {
            try
            {

                List<NavigatePoLine> detail = (List<NavigatePoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<NavigatePoLine>));
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0');
                string NavDirection = detail.FirstOrDefault().NavDirection;
                decimal Line = detail.FirstOrDefault().Line;
                mtPurchaseOrderLabel result;
                if (NavDirection == "Next")
                {
                    var LastLine = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder select a.Line).Max();
                    if (LastLine == Line)
                    {
                        result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder select a).OrderBy(a => a.Line).FirstOrDefault();
                    }
                    else
                    {
                        result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line > Line select a).FirstOrDefault();
                    }
                }
                else
                {
                    var FirstLine = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder select a.Line).Min();
                    if (FirstLine == Line)
                    {
                        result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder select a).OrderByDescending(a => a.Line).FirstOrDefault();
                    }
                    else
                    {
                        result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line < Line select a).FirstOrDefault();
                    }
                }


                if (result != null)
                {
                    List<LabelPrintPoLine> PoOut = new List<LabelPrintPoLine>();
                    PoOut.Add(new LabelPrintPoLine { PurchaseOrder = result.PurchaseOrder, Line = result.Line, StockCode = result.StockCode, Description = result.Description });
                    return Json(PoOut, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error: No lines found.", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetGridData(string PurchaseOrder, int Line)
        {
            try
            {
                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                var result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line == Line select new { PurchaseOrder = a.PurchaseOrder, Line = a.Line, StockCode = a.StockCode, Description = a.Description, ReelQty = a.ReelQty, ReelNumber = a.ReelNumber, NoOfLables = a.NoOfLabels, TrnDate = a.TrnDate }).OrderByDescending(a => a.TrnDate).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "PoLabelReprint")]
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

        [HttpPost]
        [CustomAuthorize(Activity: "PoLabelReprint")]
        public ActionResult AutoReel(string details)
        {
            try
            {

                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                string StockCode = detail.FirstOrDefault().StockCode;
                string Description = detail.FirstOrDefault().Description;
                decimal ReelQty = detail.FirstOrDefault().ReelQuantity;
                int ReelNo = 1;

                ////Changes done to accomodate multiple date formats
                //Changes done to accomodate multiple date formats
                var dateformat = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.LabelDateFormat).FirstOrDefault();

                string DateField = DateTime.Now.ToString("yy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd");
                if (!string.IsNullOrEmpty(dateformat))
                {
                    DateField = DateTime.Now.ToString(dateformat.Substring(0, 2)) + DateTime.Now.ToString(dateformat.Substring(2, 2)) + DateTime.Now.ToString(dateformat.Substring(4, 2));
                }

                string dateOnly = DateField;

                var result = wdb.sp_GetMaxStockReelNumber(StockCode, DateField).ToList();

                if (!string.IsNullOrEmpty(detail.FirstOrDefault().ReelNumber))
                {
                    DateField = detail.FirstOrDefault().ReelNumber + "-" + DateField + "-";
                }


                if (result.Count > 0)
                {
                    ReelNo = Convert.ToInt16(result.FirstOrDefault().Sequence) + 1;
                }

                List<LabelPrintPoLine> obj = new List<LabelPrintPoLine>();

                for (int i = 0; i < detail.FirstOrDefault().NoOfLables; i++)
                {

                    LabelPrintPoLine pr = new LabelPrintPoLine();
                    pr.StockCode = StockCode;
                    pr.Description = Description;
                    pr.ReelQuantity = ReelQty;
                    pr.ReelNumber = DateField + ReelNo.ToString();
                    pr.NoOfLables = detail.FirstOrDefault().LabelMultiplier;
                    pr.Bin = detail.FirstOrDefault().Bin;
                    obj.Add(pr);
                    ReelNo++;
                }

                wdb.sp_UpdateStockReelNumber(StockCode, dateOnly, (ReelNo - 1).ToString());

                return Json(obj, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PoLabelReprint")]
        public ActionResult PrintStockLabel(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));

                objPrint.PrintStockLabel(detail);


                //Temporary store of label data for stock take take on process
                //if (detail.Count > 0)
                //{
                //    foreach (var item in detail)
                //    {
                //        using (var sdb = new WarehouseManagementEntities(""))
                //        {
                //            mtStockLabel obj = new mtStockLabel();
                //            obj.StockCode = item.StockCode.Trim();
                //            obj.Lot = item.ReelNumber.Trim();
                //            obj.Quantity = item.ReelQuantity;
                //            obj.Warehouse = "";
                //            obj.TrnDate = DateTime.Now;
                //            obj.Bin = item.Bin;
                //            sdb.Entry(obj).State = System.Data.EntityState.Added;
                //            sdb.SaveChanges();
                //        }
                //    }
                //}

                return Json("Completed Successfully", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
