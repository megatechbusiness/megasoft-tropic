using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WmsSalesReleaseExpressController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        DespatchBL BL = new DespatchBL();
        //
        // GET: /WmsSalesRelease/
        [CustomAuthorize("SalesRelease")]
        public ActionResult Index()
        {
            WmsSalesReleaseViewModel model = new WmsSalesReleaseViewModel();
            ViewBag.Pickers = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            model.SalesOrderSelection = "All";
            model.StockCodeSelection = "All";
            model.ShipDateSelection = "All";
            return View("Index", model);
        }

        [CustomAuthorize("SalesRelease")]
        public ActionResult ReviewCriteria()
        {
            WmsSalesReleaseViewModel model = new WmsSalesReleaseViewModel();
            model.SalesOrderSelection = "All";
            model.StockCodeSelection = "All";
            model.ShipDateSelection = "All";
            return PartialView("ReviewCriteria", model);
        }

        public JsonResult StockCodeList()
        {
            var result = wdb.sp_GetWmsOrdersForRelease().ToList();
            var Stock = (from a in result select new { MStockCode = a.MStockCode, MStockDes = a.MStockDes, MStockingUom = a.MStockingUom }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SalesOrderList()
        {
            var result = wdb.sp_GetWmsOrdersForRelease().ToList();
            var orders = (from a in result select new { SalesOrder = a.SalesOrder, Customer = a.Customer, Name = a.Name, OrderDate = Convert.ToDateTime(a.OrderDate).ToString("yyyy-MM-dd") }).Distinct().ToList();
            return Json(orders, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Review")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "SalesRelease")]
        public ActionResult Review(WmsSalesReleaseViewModel model)
        {
            ViewBag.Pickers = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                ModelState.Clear();

                bool isValid = true;

                if (model.SalesOrderSelection == "Single")
                {
                    if (string.IsNullOrEmpty(model.SalesOrder))
                    {
                        ModelState.AddModelError("", "Please enter a Sales Order Number.");
                        isValid = false;
                    }
                }

                if (model.StockCodeSelection == "Single")
                {
                    if (string.IsNullOrEmpty(model.StockCode))
                    {
                        ModelState.AddModelError("", "Please enter a Stock Code");
                        isValid = false;
                    }
                }

                if (model.ShipDateSelection == "Single")
                {
                    if (model.StartShipDate == null)
                    {
                        ModelState.AddModelError("", "Please enter a Start Ship Date");
                        isValid = false;
                    }
                }

                if (model.ShipDateSelection == "Range")
                {
                    if (model.StartShipDate == null)
                    {
                        ModelState.AddModelError("", "Please enter a Start Ship Date");
                        isValid = false;
                    }
                    if (model.EndShipDate == null)
                    {
                        ModelState.AddModelError("", "Please enter an End Ship Date");
                        isValid = false;
                    }

                    if (model.StartShipDate != null && model.EndShipDate != null)
                    {
                        if (model.EndShipDate < model.StartShipDate)
                        {
                            ModelState.AddModelError("", "End Ship Date cannot be before Start Ship Date");
                            isValid = false;
                        }
                    }
                }

                if (!isValid)
                {
                    return View("Index", model);
                }

                var result = wdb.sp_GetWmsOrdersForRelease().ToList();

                if (model.SalesOrderSelection == "Single")
                {
                    result = (from a in result where a.SalesOrder == model.SalesOrder select a).ToList();
                }

                if (model.StockCodeSelection == "Single")
                {
                    result = (from a in result where a.MStockCode == model.StockCode select a).ToList();
                }

                if (model.ShipDateSelection == "Single")
                {
                    result = (from a in result where a.MLineShipDate == model.StartShipDate select a).ToList();
                }
                else if (model.ShipDateSelection == "Range")
                {
                    result = (from a in result where a.MLineShipDate >= model.StartShipDate && a.MLineShipDate <= model.EndShipDate select a).ToList();
                }

                model.OrderLines = result;

                return View("Index", model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveRelease")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "SalesRelease")]
        public ActionResult SaveRelease(WmsSalesReleaseViewModel model)
        {
            ViewBag.Pickers = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                ModelState.Clear();


                if (model.OrderLines != null)
                {
                    //Validate On Hand vs Release Qty.
                    bool ValidEntry = true;
                    foreach(var oh in model.OrderLines)
                    {
                        if(oh.QtyOnHand < oh.ReleaseQty)
                        {
                            ModelState.AddModelError("", "Quantity to release cannot be more than Quantity on Hand for Order : " + oh.SalesOrder + "-" + oh.SalesOrderLine + ".");
                            ValidEntry = false;
                        }
                    }
                    if(ValidEntry == false)
                    {
                        return View("Index", model);
                    }


                    //Add new Entries:
                    //First deal with New IDs i.e Where WmsId = 0 and RelaseQty != 0;
                    //Group these entries by SalesOrder
                    //Add new WmsID For each Group

                    List<mtWmsOrderMaster> ReleasedOrders = new List<mtWmsOrderMaster>();

                    var NewIds = (from a in model.OrderLines where a.WmsId == 0 && a.ReleaseQty != 0 select a).GroupBy(a => a.SalesOrder).ToList();
                    foreach (var group in NewIds)
                    {
                        int WmsKeyId = GetMaxWmsId() + 1;
                        foreach (var so in group)
                        {
                            mtWmsOrderMaster obj = new mtWmsOrderMaster();
                            obj.WmsId = WmsKeyId;
                            obj.SalesOrder = so.SalesOrder.PadLeft(15, '0');
                            obj.SalesOrderLine = (int)so.SalesOrderLine;
                            obj.StockCode = so.MStockCode;
                            obj.Warehouse = so.MWarehouse;
                            obj.Uom = so.MStockingUom;
                            obj.SalesReleaseQty = (decimal)so.ReleaseQty;
                            obj.Comment = so.Comment;
                            obj.SalesReleaseUser = HttpContext.User.Identity.Name.ToUpper();
                            obj.SalesReleaseDate = DateTime.Now;
                            obj.TraceableType = so.TraceableType;
                            obj.WmsStatus = "2";
                            wdb.Entry(obj).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();
                            ReleasedOrders.Add(obj);
                        }
                    }


                    //Update Entries
                    //Find all entries that have a Wms Id && ReleaseQty != 0
                    //Update Release Qty for these entries
                    var UpdateIds = (from a in model.OrderLines where a.WmsId != 0 && a.ReleaseQty != 0 select a).ToList();
                    foreach (var upd in UpdateIds)
                    {
                        var uSalesOrder = upd.SalesOrder.PadLeft(15, '0');
                        var wms = (from a in wdb.mtWmsOrderMasters where a.WmsId == upd.WmsId && a.SalesOrder == uSalesOrder && a.SalesOrderLine == upd.SalesOrderLine select a).FirstOrDefault();
                        wms.SalesReleaseQty = (decimal)upd.ReleaseQty;
                        wms.Comment = upd.Comment;
                        wms.WmsStatus = "2";
                        wdb.Entry(wms).State = System.Data.EntityState.Modified;
                        wdb.SaveChanges();
                    }


                    //Delete Entries
                    //Find all entries with release qty of 0 and WmsId != 0
                    //Delete these entries
                    var DeleteIds = (from a in model.OrderLines where a.WmsId != 0 && a.ReleaseQty == 0 select a).ToList();
                    foreach (var delId in DeleteIds)
                    {
                        var dSalesOrder = delId.SalesOrder.PadLeft(15, '0');
                        var wms = (from a in wdb.mtWmsOrderMasters where a.WmsId == delId.WmsId && a.SalesOrder == dSalesOrder && a.SalesOrderLine == delId.SalesOrderLine select a).FirstOrDefault();
                        wdb.Entry(wms).State = System.Data.EntityState.Deleted;
                        wdb.SaveChanges();
                    }

                    var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                    //Automatically build release items
                    if(setting.SalesReleaseAutoAllocation == true)
                    {
                        foreach (var line in ReleasedOrders)
                        {
                            WmsBackOrderReleaseViewModel released = new WmsBackOrderReleaseViewModel();
                            released.ReleaseItems = wdb.sp_GetWmsBackOrderReleaseItems(line.WmsId, line.SalesOrderLine).ToList();
                            if (released.ReleaseItems.Count == 0)
                            {
                                ModelState.AddModelError("", "No items found for order " + line.SalesOrder + "-" + line.SalesOrderLine + ".");
                                DeleteWwmsId(line.WmsId, line.SalesOrder, line.SalesOrderLine);
                            }
                            else
                            {
                                if (released.ReleaseItems.FirstOrDefault().TraceableType == "S")
                                {
                                    ModelState.AddModelError("", "Manual picking required for serialised item. Sales Order " + line.SalesOrder + "-" + line.SalesOrderLine + ".");
                                    released.ReleaseItems = null;
                                    DeleteWwmsId(line.WmsId, line.SalesOrder, line.SalesOrderLine);
                                }
                                else
                                {
                                    if (released.ReleaseItems.FirstOrDefault().TraceableType == "T")
                                    {
                                        var countLots = (from a in released.ReleaseItems where a.Lot != "" && a.Lot != null select a.Lot).ToList().Count();
                                        if (countLots == 0)
                                        {
                                            ModelState.AddModelError("", "No lots found for Stock Code " + released.ReleaseItems.FirstOrDefault().MStockCode + ". Sales Order " + line.SalesOrder + "-" + line.SalesOrderLine + ".");
                                            released.ReleaseItems = null;
                                            DeleteWwmsId(line.WmsId, line.SalesOrder, line.SalesOrderLine);
                                        }
                                        else
                                        {
                                            released.MStockCode = released.ReleaseItems.FirstOrDefault().MStockCode;
                                            released.MStockDes = released.ReleaseItems.FirstOrDefault().MStockDes;
                                            released.MWarehouse = released.ReleaseItems.FirstOrDefault().MWarehouse;
                                            released.MStockingUom = released.ReleaseItems.FirstOrDefault().MStockingUom;
                                            released.MOrderQty = released.ReleaseItems.FirstOrDefault().MOrderQty;
                                            released.MBackOrderQty = released.ReleaseItems.FirstOrDefault().MBackOrderQty;
                                            released.SalesOrder = released.ReleaseItems.FirstOrDefault().SalesOrder;
                                            released.Line = (int)released.ReleaseItems.FirstOrDefault().SalesOrderLine;
                                            released.SalesReleaseQty = (decimal)released.ReleaseItems.FirstOrDefault().SalesReleaseQty;
                                            released.Comment = released.ReleaseItems.FirstOrDefault().Comment;

                                            var TotalRelease = released.ReleaseItems.FirstOrDefault().SalesReleaseQty;
                                            decimal RunnningTotal = 0;
                                            foreach (var item in released.ReleaseItems)
                                            {
                                                if (RunnningTotal < TotalRelease)
                                                {
                                                    item.ReleaseItem = true;
                                                    RunnningTotal += (decimal)item.QtyOnHand;
                                                }
                                            }


                                        }
                                    }
                                    else
                                    {
                                        released.MStockCode = released.ReleaseItems.FirstOrDefault().MStockCode;
                                        released.MStockDes = released.ReleaseItems.FirstOrDefault().MStockDes;
                                        released.MWarehouse = released.ReleaseItems.FirstOrDefault().MWarehouse;
                                        released.MStockingUom = released.ReleaseItems.FirstOrDefault().MStockingUom;
                                        released.MOrderQty = released.ReleaseItems.FirstOrDefault().MOrderQty;
                                        released.MBackOrderQty = released.ReleaseItems.FirstOrDefault().MBackOrderQty;
                                        released.SalesOrder = released.ReleaseItems.FirstOrDefault().SalesOrder;
                                        released.Line = (int)released.ReleaseItems.FirstOrDefault().SalesOrderLine;
                                        released.SalesReleaseQty = (decimal)released.ReleaseItems.FirstOrDefault().SalesReleaseQty;
                                        released.Comment = released.ReleaseItems.FirstOrDefault().Comment;
                                    }


                                }

                            }


                            //Auto Allocate Lots and release for Picking
                            bool itemsReleased = false;

                            if (released.ReleaseItems.Count > 0)
                            {
                                var itemsToSave = (from a in released.ReleaseItems where a.ReleaseItem == true select a).ToList();
                                foreach (var item in itemsToSave)
                                {
                                    if (item.ReleaseItem == true)
                                    {
                                        mtWmsOrderDetail obj = new mtWmsOrderDetail();
                                        string PalletNo = item.PalletId;
                                        if (item.TraceableType == "T")
                                        {
                                            var countItemsOnPallet = (from a in released.ReleaseItems where a.PalletId == PalletNo select a).ToList().Count();
                                            var countItemsReleased = (from a in released.ReleaseItems where a.ReleaseItem == true && a.PalletId == PalletNo select a).ToList().Count();
                                            if (countItemsOnPallet == countItemsReleased)
                                            {
                                                //Full Pallet released
                                                obj.ScanType = "P";
                                            }
                                            else
                                            {
                                                //Batch item released
                                                obj.ScanType = "B";
                                            }
                                        }
                                        else
                                        {
                                            obj.ScanType = "S";
                                        }
                                        obj.WmsId = line.WmsId;
                                        obj.WmsLine = NextWmsLine(line.WmsId);
                                        obj.SalesOrderLine = (int)released.Line;
                                        obj.StockCode = item.MStockCode;
                                        obj.Bin = item.Bin;
                                        obj.Lot = item.Lot;
                                        obj.PalletNo = item.PalletId;
                                        obj.QuantityReleased = item.QtyOnHand;
                                        obj.QuantityPicked = 0;
                                        var SalesOrderShort = line.SalesOrder.TrimStart(new Char[] { '0' });
                                        obj.Picker = (from a in model.OrderLines where a.SalesOrder == SalesOrderShort && a.SalesOrderLine == line.SalesOrderLine select a.Picker).FirstOrDefault();
                                        obj.DateReleased = DateTime.Now;
                                        obj.ReleasedBy = HttpContext.User.Identity.Name.ToUpper();
                                        wdb.mtWmsOrderDetails.Add(obj);
                                        wdb.SaveChanges();

                                        itemsReleased = true;


                                    }
                                }

                                if (itemsReleased)
                                {
                                    using (var hdb = new WarehouseManagementEntities(""))
                                    {
                                        var ordMaster = (from a in hdb.mtWmsOrderMasters where a.WmsId == line.WmsId && a.SalesOrder == line.SalesOrder && a.SalesOrderLine == line.SalesOrderLine select a).FirstOrDefault();
                                        ordMaster.WmsStatus = "3";
                                        hdb.Entry(ordMaster).State = System.Data.EntityState.Modified;
                                        hdb.SaveChanges();
                                    }

                                    ModelState.AddModelError("", "Items released for picking.");
                                    WmsBackOrderReleaseViewModel modelOut = new WmsBackOrderReleaseViewModel();
                                    modelOut.SoLines = wdb.sp_GetWmsBackOrderReleaseLines(0).ToList();
                                    //return View("Index", modelOut);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "No data found.");
                            }
                        }
                    }
                    else
                    {
                        bool CheckShipQtyToZero = false;
                        string ErrorMessage = "";
                        //Check if we need to zero any sales orders ship quantity
                        foreach (var line in ReleasedOrders)
                        {
                            var SorDetail = wdb.sp_GetSalesOrderDetailLine(line.SalesOrder, line.SalesOrderLine).FirstOrDefault();
                            if (SorDetail != null)
                            {
                                if (SorDetail.MShipQty > 0)
                                {
                                    //we do
                                    CheckShipQtyToZero = true;
                                }
                            }
                        }
                        if(CheckShipQtyToZero == true)
                        {
                            string Guid = sys.SysproLogin();
                            string XmlOut;
                            //
                            XmlOut = sys.SysproPost(Guid, BL.BuildReleaseParameter(), BL.BuildReleaseShipQtyDocument(ReleasedOrders), "SORTBO");
                            ErrorMessage = sys.GetXmlErrors(XmlOut);
                        }
                        if(string.IsNullOrEmpty(ErrorMessage))
                        {
                            foreach (var line in ReleasedOrders)
                            {

                                if (string.IsNullOrEmpty(ErrorMessage))
                                {
                                    mtWmsOrderMaster obj = new mtWmsOrderMaster();
                                    obj = wdb.mtWmsOrderMasters.Find(line.WmsId, line.SalesOrder, line.SalesOrderLine);
                                    obj.WmsStatus = "3";
                                    var SalesOrderShort = line.SalesOrder.TrimStart(new Char[] { '0' });
                                    obj.ExpressPicker = (from a in model.OrderLines where a.SalesOrder == SalesOrderShort && a.SalesOrderLine == line.SalesOrderLine select a.Picker).FirstOrDefault();
                                    wdb.Entry(obj).State = EntityState.Modified;
                                    wdb.SaveChanges();
                                }
                            }
                                ModelState.AddModelError("", "Items released for picking.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Error releasing ship quantity: " + ErrorMessage);
                        }
                        //WmsBackOrderReleaseViewModel modelOut = new WmsBackOrderReleaseViewModel();
                        //modelOut.SoLines = wdb.sp_GetWmsBackOrderReleaseLines(0).ToList();
                    }
                }
                //ModelState.AddModelError("", "Orders saved for Warehouse");

                var result = wdb.sp_GetWmsOrdersForRelease().ToList();

                if (model.SalesOrderSelection == "Single")
                {
                    result = (from a in result where a.SalesOrder == model.SalesOrder select a).ToList();
                }

                if (model.StockCodeSelection == "Single")
                {
                    result = (from a in result where a.MStockCode == model.StockCode select a).ToList();
                }

                if (model.ShipDateSelection == "Single")
                {
                    result = (from a in result where a.MLineShipDate == model.StartShipDate select a).ToList();
                }
                else if (model.ShipDateSelection == "Range")
                {
                    result = (from a in result where a.MLineShipDate >= model.StartShipDate && a.MLineShipDate <= model.EndShipDate select a).ToList();
                }

                model.OrderLines = result;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public int NextWmsLine(int WmsId)
        {
            try
            {
                var Wms = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId select a).ToList();
                if (Wms.Count > 0)
                {
                    return (from a in Wms select a.WmsLine).Max() + 1;
                }
                else
                {
                    return 1;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int GetMaxWmsId()
        {
            try
            {
                int? result = wdb.mtWmsOrderMasters.Max(a => (int?)a.WmsId);
                if (result == null)
                {
                    return 0;
                }
                return (int)result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteWwmsId(int WmsId, string SalesOrder, int SalesOrderLine)
        {
            try
            {
                var wms = (from a in wdb.mtWmsOrderMasters where a.WmsId == WmsId && a.SalesOrder == SalesOrder && a.SalesOrderLine == SalesOrderLine select a).FirstOrDefault();
                wdb.Entry(wms).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
