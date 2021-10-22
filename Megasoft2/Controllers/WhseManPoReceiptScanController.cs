using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManPoReceiptScanController : Controller
    {
        //
        // GET: /WhseManPoReceiptScan/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "PoReceiptsScan")]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [CustomAuthorize(Activity: "PoReceiptsScan")]
        public ActionResult ValidatePo(string details)
        {
            try
            {

                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15,'0');
                var result = (from a in wdb.PorMasterDetails where a.PurchaseOrder == PurchaseOrder && a.MOrderQty - a.MReceivedQty > 0 select a).FirstOrDefault();
                
                if(result != null)
                {
                    var Supplier = (from a in wdb.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a.Supplier).FirstOrDefault();                    
                    string Warehouse = result.MWarehouse;
                    string UseMultipleBins = (from a in wdb.vw_InvWhControl where a.Warehouse == Warehouse select a.UseMultipleBins).FirstOrDefault();
                    List<LabelPrintPoLine> PoOut = new List<LabelPrintPoLine>();
                    PoOut.Add(new LabelPrintPoLine { PurchaseOrder = result.PurchaseOrder, Line = result.Line, StockCode = result.MStockCode, Description = result.MStockDes, UseMultipleBins = UseMultipleBins, Supplier = Supplier, QtyDesc = "Ord. Qty:" + String.Format("{0:##,###,###.##}", result.MOrderQty.ToString()) + " | Rec. Qty:" + result.MReceivedQty  });
                    return Json(PoOut, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No outstanding lines found.", JsonRequestBehavior.AllowGet);
                }
                
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [CustomAuthorize(Activity: "PoReceiptsScan")]
        public ActionResult NavigatePoLine(string details)
        {
            try
            {

                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                List<NavigatePoLine> detail = (List<NavigatePoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<NavigatePoLine>));
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0');
                string NavDirection = detail.FirstOrDefault().NavDirection;
                decimal Line = detail.FirstOrDefault().Line;
                sp_GetPurchaseOrderLinesForLabel_Result result;
                if(NavDirection == "Next")
                {
                    var LastLine = (from a in wdb.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company) select a.Line).Max();
                    if(LastLine == Line)
                    {
                        result = (from a in wdb.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company) select a).OrderBy(a => a.Line).FirstOrDefault();
                    }
                    else
                    {
                        result = (from a in wdb.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company) where a.Line > Line select a).FirstOrDefault();
                    }
                }
                else
                {
                    var FirstLine = (from a in wdb.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company) select a.Line).Min();
                    if (FirstLine == Line)
                    {
                        result = (from a in wdb.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company) select a).OrderByDescending(a => a.Line).FirstOrDefault();
                    }
                    else
                    {
                        result = (from a in wdb.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company) where a.Line < Line select a).FirstOrDefault();
                    }
                }
                
                
                if(result != null)
                {
                    string Warehouse = result.MWarehouse;
                    string UseMultipleBins = (from a in wdb.vw_InvWhControl where a.Warehouse == Warehouse select a.UseMultipleBins).FirstOrDefault();
                    List<LabelPrintPoLine> PoOut = new List<LabelPrintPoLine>();
                    PoOut.Add(new LabelPrintPoLine { PurchaseOrder = result.PurchaseOrder, Line = result.Line, StockCode = result.MStockCode, Description = result.MStockDes, UseMultipleBins = UseMultipleBins });
                    return Json(PoOut, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error: No outstanding lines found.", JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PoReceiptsScan")]
        public ActionResult SaveReel(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                //string PoReceipt = objPrint.PostPoReceipt(detail);
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15,'0');
                decimal Line = detail.FirstOrDefault().Line;
                string StockCode = detail.FirstOrDefault().StockCode;
                string Description = detail.FirstOrDefault().Description;
                string ReelNo = detail.FirstOrDefault().ReelNumber;
                decimal ReelQty = detail.FirstOrDefault().ReelQuantity;
                string Bin = detail.FirstOrDefault().Bin;

                var result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line == Line && a.ReelNumber == ReelNo select a).ToList();
                if (result.Count > 0)
                {
                    return Json("Reel : " + ReelNo + " already scanned for Purchase Order : " + PurchaseOrder, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var Outstanding = (wdb.sp_GetOustandingPoQty(PurchaseOrder, (int)Line)).ToList();
                    if (Outstanding.Count > 0)
                    {

                        var ItemsAlreadyScannedQty = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line == Line && a.GrnPosted != "Y" select a).ToList();
                        decimal AlreadyScannedQty = 0;
                        if(ItemsAlreadyScannedQty.Count > 0)
                        {
                            AlreadyScannedQty = (decimal)(from a in ItemsAlreadyScannedQty select a.ReelQty).Sum();
                        }
                            


                        if((Outstanding.FirstOrDefault().OutstandingQty + AlreadyScannedQty) - ReelQty < 0)
                        {
                            return Json("Over Receipt not allowed for Purchase Order : " + PurchaseOrder + ". P/O Oustanding Qty: " + Outstanding.FirstOrDefault().OutstandingQty.ToString() + ". Current scanned Quantity : " + AlreadyScannedQty + ".", JsonRequestBehavior.AllowGet);
                        }

                        using (var ldb = new WarehouseManagementEntities(""))
                        {
                            mtPurchaseOrderLabel objLabel = new mtPurchaseOrderLabel();
                            objLabel.PurchaseOrder = PurchaseOrder;
                            objLabel.Line = Line;
                            objLabel.DeliveryNote = "";
                            objLabel.StockCode = StockCode;
                            objLabel.Description = Description;
                            objLabel.ReelQty = ReelQty;
                            objLabel.ReelNumber = ReelNo;
                            objLabel.NoOfLabels = 1;
                            objLabel.Username = HttpContext.User.Identity.Name.ToUpper();
                            objLabel.TrnDate = DateTime.Now;
                            objLabel.LabelPrinted = "N";
                            objLabel.GrnPosted = "N";
                            objLabel.Bin = Bin;
                            ldb.mtPurchaseOrderLabels.Add(objLabel);
                            ldb.SaveChanges();
                        }
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("No information found for Purchase Order : " + PurchaseOrder, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetGridData(string PurchaseOrder)
        {
            try
            {
                var username = HttpContext.User.Identity.Name.ToUpper();
                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                var result = (from a in wdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.GrnPosted == "N" && a.Username == username select new { PurchaseOrder = a.PurchaseOrder, Line = a.Line, StockCode = a.StockCode, Description = a.Description, ReelQty = a.ReelQty, ReelNumber = a.ReelNumber, NoOfLables = a.NoOfLabels, TrnDate = a.TrnDate }).OrderByDescending(a => a.TrnDate).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
