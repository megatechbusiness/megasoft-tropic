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
    public class WhseManPoLabelController : Controller
    {
        //
        // GET: /WhseManPoLabel/
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        LabelPrint objPrint = new LabelPrint();

        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult Index(string PurchaseOrder = null, string Program = null)
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            PurchaseOrderLabel model = new PurchaseOrderLabel();
            model.Program = Program;
            if (PurchaseOrder != null)
            {
                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                var PoCheck = (from a in db.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a).ToList();
                if (PoCheck.Count > 0)
                {
                    model.PoLines = db.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company).ToList();
                    model.PurchaseOrder = PurchaseOrder;
                    if (model.PoLines.Count == 0)
                    {
                        ModelState.AddModelError("", "No outstanding lines found for Purchase Order");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Purchase Order not found.");
                }
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult Index(PurchaseOrderLabel model)
        {
            try
            {
                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                if (!string.IsNullOrEmpty(model.PurchaseOrder))
                {
                    ModelState.Clear();
                    string PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');
                    var PoCheck = (from a in db.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a).ToList();
                    if (PoCheck.Count > 0)
                    {
                        model.PoLines = db.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company).ToList();
                        model.PurchaseOrder = PurchaseOrder;
                        if (model.PoLines.Count == 0)
                        {
                            ModelState.AddModelError("", "No outstanding lines found for Purchase Order");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Purchase Order not found.");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Purchase Order number cannot be blank!");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult DisplayLabel(string PurchaseOrder, decimal Line, string Program)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                LabelPrintPoLine line = new LabelPrintPoLine();
                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                var result = db.sp_GetPackingSlipPoLine(PurchaseOrder, (int)Line).FirstOrDefault();
                var grnSus = db.sp_GetGrnSuspenseAccByPoLine(PurchaseOrder, (int)Line).FirstOrDefault();
                if (result != null)
                {
                    line.PurchaseOrder = result.PurchaseOrder;
                    line.Line = (decimal)result.Line;
                    line.StockCode = result.StockCode;
                    line.Description = result.Description;
                    if (result.MWarehouse!="**")
                    {
                        line.CostMultiplier = (decimal)result.CostMultiplier;
                        line.CostMultiplierRequired = result.HasCostMultiplier;
                        line.CostMultiplierPrice = (decimal)result.CostMultiplierPrice;
                        line.ReelQuantity = (decimal)result.ConvFactOthUom;
                    line.UseMultipleBins = result.UseMultipleBins;

                    }
                    else
                    {
                        line.UseMultipleBins = "N";

                    }
                    line.Price = (decimal)result.Price;
                    line.Warehouse = result.MWarehouse;
                    line.NoOfLables = 1;

                    line.GrnSuspense = grnSus.GrnLdgAcc;
                    line.LabelMultiplier = 1;
                    line.Program = Program;
                    //line.DeliveryDate = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy"));
                    var whcontrol = (from a in db.vw_InvWhControl where a.Warehouse == result.MWarehouse select a).FirstOrDefault();
                    if (whcontrol != null)
                    {
                        if (whcontrol.UseMultipleBins == "Y")
                        {
                            ViewBag.BinList = (from a in db.sp_GetBinsByWarehouse(result.MWarehouse) select new { Text = a.Bin, Value = a.Bin }).ToList();
                        }
                        else
                        {
                            ViewBag.BinList = new List<SelectListItem>();
                        }
                    }
                    else
                    {
                        ViewBag.BinList = new List<SelectListItem>();
                    }
                }
                else
                {
                    ViewBag.BinList = new List<SelectListItem>();
                }
                ViewBag.Header = "P/O : " + PurchaseOrder + " Line : " + Line;
                return View(line);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult SaveReel(string details)
        {
            try
            {
                ModelState.Clear();
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                //string PoReceipt = objPrint.PostPoReceipt(detail);
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0');
                decimal Line = detail.FirstOrDefault().Line;
                //string DelNote = detail.FirstOrDefault().DeliveryNote;
                //DateTime DelDate = detail.FirstOrDefault().DeliveryDate;
                string StockCode = detail.FirstOrDefault().StockCode;
                string Description = detail.FirstOrDefault().Description;
                string ReelNo = detail.FirstOrDefault().ReelNumber;
                string Bin = detail.FirstOrDefault().Bin;
                decimal ReelQty = detail.FirstOrDefault().ReelQuantity;
                int NoOfLabels = detail.FirstOrDefault().NoOfLables;
                bool AutoReel = detail.FirstOrDefault().AutoReel;
                string FileImport = detail.FirstOrDefault().FileImport;
                string DeliveryNote = detail.FirstOrDefault().DeliveryNote;

                if (AutoReel == false)
                {
                    var result = (from a in db.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line == Line && a.ReelNumber == ReelNo select a).ToList();
                    if (result.Count > 0)
                    {
                        return Json("Reel : " + ReelNo + " already printed for Purchase Order : " + PurchaseOrder, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        using (var ldb = new WarehouseManagementEntities(""))
                        {

                            mtPurchaseOrderLabel objLabel = new mtPurchaseOrderLabel();
                            objLabel.PurchaseOrder = PurchaseOrder;
                            objLabel.Line = Line;
                            //objLabel.DeliveryNote = DelNote;
                            //objLabel.DeliveryDate = Convert.ToDateTime(DelDate.ToString("yyyy-MM-dd"));
                            objLabel.StockCode = StockCode;
                            objLabel.Description = Description;
                            objLabel.Bin = Bin;
                            objLabel.ReelQty = ReelQty;
                            objLabel.ReelNumber = ReelNo;
                            objLabel.NoOfLabels = NoOfLabels;
                            objLabel.Username = HttpContext.User.Identity.Name.ToUpper();
                            objLabel.TrnDate = DateTime.Now;
                            objLabel.LabelPrinted = "N";
                            objLabel.GrnPosted = "N";
                            if (FileImport == "Y")
                            {
                                objLabel.FileImport = "Y";
                                objLabel.ValidScan = "N";
                            }
                            else
                            {
                                objLabel.FileImport = "N";
                                objLabel.ValidScan = "N";
                            }
                            ldb.mtPurchaseOrderLabels.Add(objLabel);
                            ldb.SaveChanges();

                        }
                        if (FileImport == "Y")
                        {
                            using (var sdb = new WarehouseManagementEntities(""))
                            {
                                //Add to mtPorDeliveryImport table if FileImport = "Y"
                                mtPorDeliveryImport obj = new mtPorDeliveryImport();
                                obj.PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                                obj.DeliveryNote = DeliveryNote;
                                obj.Line = Line;
                                obj.FileName = "";
                                obj.FileDate = DateTime.Now;
                                obj.FileTime = TimeSpan.Parse(DateTime.Now.ToString());
                                obj.RecordType = "R";
                                obj.RecordFunction = "A";
                                obj.StockCode = StockCode;
                                obj.SupplierStockCode = "";
                                obj.Lot = ReelNo;
                                obj.Quantity = ReelQty;
                                obj.Uom = null;
                                obj.Meters = null;
                                obj.Grammage = null;
                                obj.Username = HttpContext.User.Identity.Name.ToUpper();
                                obj.TrnDate = DateTime.Now;
                                obj.Scanned = "Y";
                                sdb.Entry(obj).State = System.Data.EntityState.Added;
                                sdb.SaveChanges();
                            }
                        }


                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {

                    //Changes done to accomodate multiple date formats
                    var dateformat = (from a in db.mtWhseManSettings where a.SettingId == 1 select a.LabelDateFormat).FirstOrDefault();

                    string DateField = DateTime.Now.ToString("yy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd");
                    if (!string.IsNullOrEmpty(dateformat))
                    {
                        DateField = DateTime.Now.ToString(dateformat.Substring(0, 2)) + DateTime.Now.ToString(dateformat.Substring(2, 2)) + DateTime.Now.ToString(dateformat.Substring(4, 2));
                    }




                    if (!string.IsNullOrEmpty(detail.FirstOrDefault().ReelNumber))
                    {
                        DateField = ReelNo + "-" + DateField;
                    }


                    var lastReel = db.sp_GetMaxReelNumber(PurchaseOrder, (int)Line, DateField).FirstOrDefault();


                    int NextReelNo = 1;
                    if (lastReel != null)
                    {
                        NextReelNo = Convert.ToInt32(lastReel.MaxReel) + 1;
                    }


                    for (int i = 0; i < NoOfLabels; i++)
                    {
                        using (var ldb = new WarehouseManagementEntities(""))
                        {

                            mtPurchaseOrderLabel objLabel = new mtPurchaseOrderLabel();
                            objLabel.PurchaseOrder = PurchaseOrder;
                            objLabel.Line = Line;
                            //objLabel.DeliveryNote = DelNote;
                            //objLabel.DeliveryDate = Convert.ToDateTime(DelDate.ToString("yyyy-MM-dd"));
                            objLabel.StockCode = StockCode;
                            objLabel.Description = Description;
                            objLabel.Bin = Bin;
                            objLabel.ReelQty = ReelQty;
                            objLabel.ReelNumber = DateField + "-" + (i + NextReelNo).ToString();
                            objLabel.NoOfLabels = detail.FirstOrDefault().LabelMultiplier;
                            objLabel.Username = HttpContext.User.Identity.Name.ToUpper();
                            objLabel.TrnDate = DateTime.Now;
                            objLabel.LabelPrinted = "N";
                            objLabel.GrnPosted = "N";
                            if (FileImport == "Y")
                            {
                                objLabel.FileImport = "Y";
                                objLabel.ValidScan = "N";
                            }
                            else
                            {
                                objLabel.FileImport = "N";
                                objLabel.ValidScan = "N";
                            }
                            ldb.mtPurchaseOrderLabels.Add(objLabel);
                            ldb.SaveChanges();

                        }

                    }


                    return Json("", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult DeleteReel(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder;
                decimal Line = detail.FirstOrDefault().Line;
                string ReelNumber = detail.FirstOrDefault().ReelNumber;
                db.sp_DeletePurchaseOrderReel(PurchaseOrder.PadLeft(15, '0'), (int)Line, ReelNumber, HttpContext.User.Identity.Name.ToUpper());
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult PrintLabel(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));


                if (detail.FirstOrDefault().Reprint == "N")
                {
                    var check = (from a in detail where a.Printed == "N" select a).ToList();
                    if (check.Count == 0)
                    {
                        return Json("Labels Printed already. Please use Label Reprint Program to re-print labels.", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        detail = (from a in detail where a.Printed == "N" select a).ToList();
                    }
                }

                objPrint.PrintLabel(detail);

                using (var cdb = new WarehouseManagementEntities(""))
                {
                    string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder;
                    decimal Line = detail.FirstOrDefault().Line;
                    foreach (var item in detail)
                    {
                        string ItemNo = item.ReelNumber;
                        var result = (from a in cdb.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line == Line && a.ReelNumber == ItemNo select a).FirstOrDefault();
                        result.LabelPrinted = "Y";
                        cdb.Entry(result).State = System.Data.EntityState.Modified;
                        cdb.SaveChanges();
                    }
                }

                //string PoReceipt = objPrint.PostPoReceipt(detail);
                //if(!string.IsNullOrEmpty(PoReceipt))
                //{
                //    return Json(PoReceipt, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{

                //foreach(var line in detail)
                //{
                //    string PurchaseOrder = line.PurchaseOrder;
                //    decimal Line = line.Line;
                //    string ReelNo = line.ReelNumber;

                //    var result = (from a in db.mtPurchaseOrderLabels where a.PurchaseOrder == PurchaseOrder && a.Line == Line && a.ReelNumber == ReelNo select a).ToList();
                //    if (result.Count > 0)
                //    {
                //        return Json("Reel : " + ReelNo + " already printed for Purchase Order : " + PurchaseOrder, JsonRequestBehavior.AllowGet);
                //    }
                //    else
                //    {
                //        //return Json("", JsonRequestBehavior.AllowGet);
                //    }
                //}


                //using (var db = new WarehouseManagementEntities(""))
                //{
                //    foreach (var item in detail)
                //    {
                //        mtPurchaseOrderLabel objLabel = new mtPurchaseOrderLabel();
                //        objLabel.PurchaseOrder = item.PurchaseOrder;
                //        objLabel.Line = item.Line;
                //        objLabel.DeliveryNote = item.DeliveryNote;
                //        objLabel.DeliveryDate = Convert.ToDateTime(item.DeliveryDate.ToString("yyyy-MM-dd"));
                //        objLabel.StockCode = item.StockCode;
                //        objLabel.Description = item.Description;
                //        objLabel.ReelQty = item.ReelQuantity;
                //        objLabel.ReelNumber = item.ReelNumber;
                //        objLabel.NoOfLabels = item.NoOfLables;
                //        objLabel.Username = HttpContext.User.Identity.Name.ToUpper();
                //        objLabel.TrnDate = DateTime.Now;
                //        objLabel.LabelPrinted = "Y";
                //        objLabel.GrnPosted = "N";
                //        db.mtPurchaseOrderLabels.Add(objLabel);
                //        db.SaveChanges();
                //    }
                //}
                //    objPrint.PrintLabel();
                //    return Json("Completed Successfully", JsonRequestBehavior.AllowGet);

                //}

                return Json("Completed Successfully", JsonRequestBehavior.AllowGet);
                //return Json(objPrint.PrintPoLabel("","",""), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize(Activity: "PackingSlip")]
        public JsonResult GetGridData(string PurchaseOrder, int Line)
        {
            try
            {
                var data = db.sp_GetPackingSlipData(PurchaseOrder, Line).ToList();
                var result = (from a in data select new { PurchaseOrder = a.PurchaseOrder, Line = a.Line, StockCode = a.MStockCode, Description = a.MStockDes, ReelQty = a.ReelQty, ReelNumber = a.ReelNumber, NoOfLables = a.NoOfLabels, Bin = a.Bin, Printed = a.LabelPrinted, DeliveryNote = a.DeliveryNote, FileImport = a.FileImport }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize(Activity: "PackingSlip")]
        public JsonResult GetNonMerchData(string PurchaseOrder, int Line)
        {
            try
            {

                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                var result = db.sp_GetNonMerchCosts(PurchaseOrder, Line).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public JsonResult SupplierList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = db.sp_GetSuppliers(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SupplierSearch(string ControlId)
        {
            ViewBag.ControlId = ControlId;
            return PartialView();
        }


        public ActionResult GlCodeList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = db.sp_GetGlCodes(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GlCodeSearch(string ControlId)
        {
            ViewBag.ControlId = ControlId;
            return PartialView();
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PackingSlip")]
        public ActionResult SaveNonMerchCost(string details)
        {
            try
            {
                List<mtPurchaseOrderNonMerchCost> detail = (List<mtPurchaseOrderNonMerchCost>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtPurchaseOrderNonMerchCost>));
                if (detail.Count > 0)
                {
                    string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0');
                    decimal Line = detail.FirstOrDefault().Line;
                    db.sp_DeleteNonMerchCosts(PurchaseOrder, (int)Line);

                    using (var ldb = new WarehouseManagementEntities(""))
                    {
                        foreach (var item in detail)
                        {
                            mtPurchaseOrderNonMerchCost objLabel = new mtPurchaseOrderNonMerchCost();
                            objLabel.PurchaseOrder = PurchaseOrder;
                            objLabel.Line = (int)Line;
                            objLabel.Reference = item.Reference;
                            objLabel.Supplier = item.Supplier;
                            objLabel.GlCode = item.GlCode;
                            objLabel.Amount = item.Amount;
                            objLabel.DateSaved = DateTime.Now;
                            objLabel.SavedBy = HttpContext.User.Identity.Name.ToUpper();
                            objLabel.Posted = "N";
                            ldb.Entry(objLabel).State = System.Data.EntityState.Added;
                            //ldb.mtPurchaseOrderNonMerchCosts.Add(objLabel);
                            ldb.SaveChanges();
                        }
                    }
                    return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No data found!", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
