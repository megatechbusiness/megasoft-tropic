using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WmsSalesReleaseController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        //
        // GET: /WmsSalesRelease/
        [CustomAuthorize("SalesRelease")]
        public ActionResult Index()
        {
            WmsSalesReleaseViewModel model = new WmsSalesReleaseViewModel();
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
            try
            {
                ModelState.Clear();


                if(model.OrderLines != null)
                {

                    //Add new Entries:
                    //First deal with New IDs i.e Where WmsId = 0 and RelaseQty != 0;
                    //Group these entries by SalesOrder
                    //Add new WmsID For each Group
                    
                    var NewIds = (from a in model.OrderLines where a.WmsId == 0 && a.ReleaseQty != 0 select a).GroupBy(a => a.SalesOrder).ToList();
                    
                    foreach(var group in NewIds)
                    {
                        int WmsKeyId = GetMaxWmsId() + 1;
                        foreach(var so in group)
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
                        }
                    }


                    //Update Entries
                    //Find all entries that have a Wms Id && ReleaseQty != 0
                    //Update Release Qty for these entries
                    var UpdateIds = (from a in model.OrderLines where a.WmsId != 0 && a.ReleaseQty != 0 select a).ToList();
                    foreach(var upd in UpdateIds)
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
                    foreach(var delId in DeleteIds)
                    {
                        var dSalesOrder = delId.SalesOrder.PadLeft(15, '0');
                        var wms = (from a in wdb.mtWmsOrderMasters where a.WmsId == delId.WmsId && a.SalesOrder == dSalesOrder && a.SalesOrderLine == delId.SalesOrderLine select a).FirstOrDefault();
                        wdb.Entry(wms).State = System.Data.EntityState.Deleted;
                        wdb.SaveChanges();
                    }

                    //foreach(var item in model.OrderLines)
                    //{
                    //    if(item.ReleaseQty != 0)
                    //    {
                    //        if(item.WmsId == 0)
                    //        {
                    //            //Check 
                    //            mtWmsOrderMaster obj = new mtWmsOrderMaster();
                    //            obj.SalesOrder = item.SalesOrder.PadLeft(15, '0');
                    //            obj.SalesOrderLine = (int)item.SalesOrderLine;
                    //            obj.StockCode = item.MStockCode;
                    //            obj.Warehouse = item.MWarehouse;
                    //            obj.Uom = item.MStockingUom;
                    //            obj.SalesReleaseQty = (decimal)item.ReleaseQty;
                    //            obj.Comment = item.Comment;
                    //            obj.SalesReleaseUser = HttpContext.User.Identity.Name.ToUpper();
                    //            obj.SalesReleaseDate = DateTime.Now;
                    //            obj.TraceableType = item.TraceableType;
                    //            obj.WmsStatus = "2";
                    //            wdb.Entry(obj).State = System.Data.EntityState.Added;
                    //            wdb.SaveChanges();
                    //        }
                    //        else
                    //        {
                    //            var wms = (from a in wdb.mtWmsOrderMasters where a.WmsId == item.WmsId select a).FirstOrDefault();
                    //            wms.SalesReleaseQty = (decimal)item.ReleaseQty;
                    //            wms.Comment = item.Comment;
                    //            wms.WmsStatus = "2";
                    //            wdb.Entry(wms).State = System.Data.EntityState.Modified;
                    //            wdb.SaveChanges();
                    //        }
                            
                    //    }
                    //    else
                    //    {
                    //        if(item.WmsId != 0)
                    //        {
                    //            var wms = (from a in wdb.mtWmsOrderMasters where a.WmsId == item.WmsId select a).FirstOrDefault();
                    //            wdb.Entry(wms).State = System.Data.EntityState.Deleted;
                    //            wdb.SaveChanges();
                    //        }
                    //    }
                    //}
                }

                ModelState.AddModelError("", "Orders saved for Warehouse");


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

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }



        public int GetMaxWmsId()
        {
            try
            {
                int? result = wdb.mtWmsOrderMasters.Max(a => (int?)a.WmsId);
                if(result == null)
                {
                    return 0;
                }
                return (int)result;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        
    }
}
