using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WmsWarehouseReleaseController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        //
        // GET: /WhseManBackOrderRelease/

        [CustomAuthorize("WarehouseRelease")]
        public ActionResult Index(int WmsId = 0)
        {
            WmsBackOrderReleaseViewModel model = new WmsBackOrderReleaseViewModel();
            model.SoLines = wdb.sp_GetWmsBackOrderReleaseLines(WmsId).ToList();            
            if (model.SoLines.Count == 0)
            {
                ModelState.AddModelError("", "No released orders found.");
            }
            else
            {

            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "WarehouseRelease")]
        public ActionResult Index(WmsBackOrderReleaseViewModel model)
        {
            try
            {
                ModelState.Clear();
                
                if (model.WmsId != 0)
                {
                    
                    model.SoLines = wdb.sp_GetWmsBackOrderReleaseLines(model.WmsId).ToList();
                    
                    if (model.SoLines.Count == 0)
                    {
                        ModelState.AddModelError("", "No outstanding lines found for Sales Order");
                    }
                    else
                    {
                        model.SalesOrder = model.SoLines.FirstOrDefault().SalesOrder;
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Sales Order number cannot be blank!");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [CustomAuthorize("WarehouseRelease")]
        public ActionResult ReleaseLine(int WmsId, string SalesOrder, int SalesOrderLine)
        {
            try
            {

                ViewBag.Pickers = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
                WmsBackOrderReleaseViewModel model = new WmsBackOrderReleaseViewModel();
                model.ReleaseItems = wdb.sp_GetWmsBackOrderReleaseItems(WmsId, SalesOrderLine).ToList();
                if (model.ReleaseItems.Count == 0)
                {
                    ModelState.AddModelError("", "No items found for order.");
                }
                else
                {
                    if(model.ReleaseItems.FirstOrDefault().TraceableType == "S")
                    {
                        ModelState.AddModelError("", "Manual picking required for serialised item.");
                        model.ReleaseItems = null;
                    }
                    else
                    {
                        if(model.ReleaseItems.FirstOrDefault().TraceableType == "T")
                        {
                            var countLots = (from a in model.ReleaseItems where a.Lot != "" && a.Lot != null select a.Lot).ToList().Count();
                            if(countLots == 0)
                            {
                                ModelState.AddModelError("", "No lots found for Stock Code " + model.ReleaseItems.FirstOrDefault().MStockCode + ".");
                                model.ReleaseItems = null;
                            }
                            else
                            {
                                model.MStockCode = model.ReleaseItems.FirstOrDefault().MStockCode;
                                model.MStockDes = model.ReleaseItems.FirstOrDefault().MStockDes;
                                model.MWarehouse = model.ReleaseItems.FirstOrDefault().MWarehouse;
                                model.MStockingUom = model.ReleaseItems.FirstOrDefault().MStockingUom;
                                model.MOrderQty = model.ReleaseItems.FirstOrDefault().MOrderQty;
                                model.MBackOrderQty = model.ReleaseItems.FirstOrDefault().MBackOrderQty;
                                model.SalesOrder = model.ReleaseItems.FirstOrDefault().SalesOrder;
                                model.Line = (int)model.ReleaseItems.FirstOrDefault().SalesOrderLine;
                                model.SalesReleaseQty = (decimal)model.ReleaseItems.FirstOrDefault().SalesReleaseQty;
                                model.Comment = model.ReleaseItems.FirstOrDefault().Comment;

                                var TotalRelease = model.ReleaseItems.FirstOrDefault().SalesReleaseQty;
                                decimal RunnningTotal = 0;
                                foreach(var item in model.ReleaseItems)
                                {
                                    if(RunnningTotal < TotalRelease)
                                    {
                                        item.ReleaseItem = true;
                                        RunnningTotal += (decimal)item.QtyOnHand;
                                    }
                                }


                            }
                        }
                        else
                        {
                            model.MStockCode = model.ReleaseItems.FirstOrDefault().MStockCode;
                            model.MStockDes = model.ReleaseItems.FirstOrDefault().MStockDes;
                            model.MWarehouse = model.ReleaseItems.FirstOrDefault().MWarehouse;
                            model.MStockingUom = model.ReleaseItems.FirstOrDefault().MStockingUom;
                            model.MOrderQty = model.ReleaseItems.FirstOrDefault().MOrderQty;
                            model.MBackOrderQty = model.ReleaseItems.FirstOrDefault().MBackOrderQty;
                            model.SalesOrder = model.ReleaseItems.FirstOrDefault().SalesOrder;
                            model.Line = (int)model.ReleaseItems.FirstOrDefault().SalesOrderLine;
                            model.SalesReleaseQty = (decimal)model.ReleaseItems.FirstOrDefault().SalesReleaseQty;
                            model.Comment = model.ReleaseItems.FirstOrDefault().Comment;
                        }
                        
                        
                    }
                    
                }
                model.WmsId = WmsId;
                return View("ReleaseLine", model);
            }
            catch (Exception ex)
            {
                ViewBag.Pickers = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("ReleaseLine");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "WarehouseRelease")]
        public ActionResult ReleaseLine(WmsBackOrderReleaseViewModel model)
        {
            ViewBag.Pickers = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                ModelState.Clear();

                bool itemsReleased = false;

                if (model.ReleaseItems.Count > 0)
                {
                    var itemsToSave = (from a in model.ReleaseItems where a.ReleaseItem == true select a).ToList();
                    foreach(var item in itemsToSave)
                    {
                        if(item.ReleaseItem == true)
                        {
                            mtWmsOrderDetail obj = new mtWmsOrderDetail();
                            string PalletNo = item.PalletId;
                            if(item.TraceableType == "T")
                            {
                                var countItemsOnPallet = (from a in model.ReleaseItems where a.PalletId == PalletNo select a).ToList().Count();
                                var countItemsReleased = (from a in model.ReleaseItems where a.ReleaseItem == true && a.PalletId == PalletNo select a).ToList().Count();
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
                            obj.WmsId = model.WmsId;
                            obj.WmsLine = NextWmsLine(model.WmsId);
                            obj.SalesOrderLine = (int)model.Line;
                            obj.StockCode = item.MStockCode;
                            obj.Bin = item.Bin;
                            obj.Lot = item.Lot;
                            obj.PalletNo = item.PalletId;
                            obj.QuantityReleased = item.QtyOnHand;
                            obj.QuantityPicked = 0;
                            obj.Picker = item.Picker;
                            obj.DateReleased = DateTime.Now;
                            wdb.mtWmsOrderDetails.Add(obj);
                            wdb.SaveChanges();

                            itemsReleased = true;

                            
                        }
                    }

                    if(itemsReleased)
                    {
                        using (var hdb = new WarehouseManagementEntities(""))
                        {
                            var ordMaster = (from a in hdb.mtWmsOrderMasters where a.WmsId == model.WmsId && a.SalesOrder == model.SalesOrder && a.SalesOrderLine == model.Line select a).FirstOrDefault();
                            ordMaster.WmsStatus = "3";
                            hdb.Entry(ordMaster).State = System.Data.EntityState.Modified;
                            hdb.SaveChanges();
                        }

                        ModelState.AddModelError("", "Items released for picking.");
                        WmsBackOrderReleaseViewModel modelOut = new WmsBackOrderReleaseViewModel();
                        modelOut.SoLines = wdb.sp_GetWmsBackOrderReleaseLines(0).ToList();
                        return View("Index", modelOut);
                    }
                    

                    
                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                }

                return View("ReleaseLine", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ReleaseLine", model);
            }
        }

        public int NextWmsLine(int WmsId)
        {
            try
            {
                var Wms = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId select a).ToList();
                if(Wms.Count > 0)
                {
                    return (from a in Wms select a.WmsLine).Max() + 1;
                }
                else
                {
                    return 1;
                }
                
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
