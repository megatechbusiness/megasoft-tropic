using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WmsDespatchController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        DespatchBL BL = new DespatchBL();
        //
        // GET: /WmsDespatch/

        [CustomAuthorize("Despatch")]
        public ActionResult DispatchList()
        {
            WmsBackOrderReleaseViewModel model = new WmsBackOrderReleaseViewModel();
            var result = wdb.sp_GetWmsDespatchLines().ToList();
            model.LinesForDespatch = result;
            if(result.Count == 0)
            {
                ModelState.AddModelError("", "No items found for Dispatch.");
            }
            return View("DispatchList", model);
        }

        [CustomAuthorize("Despatch")]
        public ActionResult Index(int WmsId = 0, int SalesOrderLine = 0)
        {
            ViewBag.PickersList = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            WmsBackOrderReleaseViewModel model = new WmsBackOrderReleaseViewModel();
            if (WmsId != 0)
            {


                model.DespatchLines = wdb.sp_GetWmsDespatchItems(WmsId, SalesOrderLine).ToList();
                if (model.DespatchLines.Count == 0)
                {
                    ModelState.AddModelError("", "No outstanding lines found for Despatch");
                }
                else
                {
                    model.WmsId = WmsId;
                    model.MStockCode = model.DespatchLines.FirstOrDefault().StockCode;
                    model.MStockDes = model.DespatchLines.FirstOrDefault().MStockDes;
                    model.MWarehouse = model.DespatchLines.FirstOrDefault().MWarehouse;
                    model.MStockingUom = model.DespatchLines.FirstOrDefault().Uom;
                    model.MOrderQty = model.DespatchLines.FirstOrDefault().MOrderQty;
                    model.MBackOrderQty = model.DespatchLines.FirstOrDefault().MBackOrderQty;
                    model.SalesOrder = model.DespatchLines.FirstOrDefault().SalesOrder;
                    model.Line = (int)model.DespatchLines.FirstOrDefault().SalesOrderLine;
                    model.SalesReleaseQty = (decimal)model.DespatchLines.FirstOrDefault().SalesReleaseQty;
                    model.Comment = model.DespatchLines.FirstOrDefault().Comment;
                    model.Picker = model.DespatchLines.FirstOrDefault().Picker;
                }
            }
            return View(model);
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "Despatch")]
        public ActionResult Index(WmsBackOrderReleaseViewModel model)
        {
            ViewBag.PickersList = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                ModelState.Clear();
                
                if (model.WmsId != 0)
                {

                    model.DespatchLines = wdb.sp_GetWmsDespatchItems(model.WmsId, model.Line).ToList();

                    if (model.DespatchLines.Count == 0)
                    {
                        ModelState.AddModelError("", "No outstanding lines found for Despatch");
                    }
                    else
                    {
                        model.WmsId = model.WmsId;
                        model.MStockCode = model.DespatchLines.FirstOrDefault().StockCode;
                        model.MStockDes = model.DespatchLines.FirstOrDefault().MStockDes;
                        model.MWarehouse = model.DespatchLines.FirstOrDefault().MWarehouse;
                        model.MStockingUom = model.DespatchLines.FirstOrDefault().Uom;
                        model.MOrderQty = model.DespatchLines.FirstOrDefault().MOrderQty;
                        model.MBackOrderQty = model.DespatchLines.FirstOrDefault().MBackOrderQty;
                        model.SalesOrder = model.DespatchLines.FirstOrDefault().SalesOrder;
                        model.Line = (int)model.DespatchLines.FirstOrDefault().SalesOrderLine;
                        model.SalesReleaseQty = (decimal)model.DespatchLines.FirstOrDefault().SalesReleaseQty;
                        model.Comment = model.DespatchLines.FirstOrDefault().Comment;
                    }

                }
                else
                {
                    ModelState.AddModelError("", "WMS Id cannot be blank or zero!");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveDespatch")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "Despatch")]
        public ActionResult SaveDespatch(WmsBackOrderReleaseViewModel model)
        {
            ViewBag.PickersList = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                if(model.DespatchLines.Count > 0)
                {
                    foreach(var item in model.DespatchLines)
                    {
                        using (var udb = new WarehouseManagementEntities(""))
                        {
                            var result = (from a in udb.mtWmsOrderDetails where a.WmsId == item.WmsId && a.WmsLine == a.WmsLine &&  a.SalesOrderLine == model.Line select a).FirstOrDefault();
                            result.QuantityReleased = item.QuantityReleased;
                            result.Picker = item.Picker;
                            udb.Entry(result).State = System.Data.EntityState.Modified;
                            udb.SaveChanges();
                        }
                    }
                    model.DespatchLines = wdb.sp_GetWmsDespatchItems(model.WmsId, model.Line).ToList();
                    ModelState.AddModelError("", "Saved Successfully.");
                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                }
                return View("Index", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostDespatch")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "Despatch")]
        public ActionResult PostDespatch(WmsBackOrderReleaseViewModel model)
        {
            ViewBag.PickersList = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                if (model.DespatchLines.Count > 0)
                {
                    string Guid = sys.SysproLogin();
                    int WMSID = Convert.ToInt32(model.WmsId);
                    string XmlOut;
                    string ErrorMessage = "";


                    ErrorMessage = BL.ZeroShipQty(Guid, WMSID, model.Line);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("Index", model);
                    }



                    var checkBackOrderRequired = (from a in wdb.mtWmsOrderDetails where a.WmsId == WMSID && a.SalesOrderLine == model.Line && a.QuantityPicked != 0 && a.CheckoutDone == "Y" select a).ToList();
                    if (checkBackOrderRequired.Count > 0)
                    {
                        //Commented out to test if Dispatch Works without posting back order release.
                        //XmlOut = sys.SysproPost(Guid, BL.BuildReleaseParameter(), BL.BuildReleaseDocument(Convert.ToInt32(model.WmsId), "Depatch", model.Line), "SORTBO");
                        //ErrorMessage = sys.GetXmlErrors(XmlOut);

                        if (string.IsNullOrEmpty(ErrorMessage))
                        {
                            string username = HttpContext.User.Identity.Name.ToUpper();

                            using (var udb = new WarehouseManagementEntities(""))
                            {
                                var result = (from a in udb.mtWmsOrderDetails where a.WmsId == WMSID && a.SalesOrderLine == model.Line && a.QuantityPicked != 0 && a.CheckoutDone != "Y" select a).ToList();
                                foreach (var item in result)
                                {
                                    item.CheckoutDate = DateTime.Now;
                                    item.CheckoutDone = "Y";
                                    udb.Entry(item).State = System.Data.EntityState.Modified;
                                    udb.SaveChanges();
                                }
                            }
                        }
                    }



                    if (string.IsNullOrEmpty(ErrorMessage))
                    {                        
                        string DespatchOut = BL.PostDespatchCreation(Guid, model.WmsId, model.Line);
                        ErrorMessage = sys.GetXmlErrors(DespatchOut);
                        if (string.IsNullOrEmpty(ErrorMessage))
                        {
                            string SalesOrder = sys.GetXmlValue(DespatchOut, "SalesOrder");
                            string DespatchNoteNumber = sys.GetXmlValue(DespatchOut, "DispatchNoteNumber");
                            using (var hdb = new WarehouseManagementEntities(""))
                            {
                                var header = (from a in hdb.mtWmsOrderMasters where a.WmsId == WMSID && a.SalesOrderLine == model.Line select a).FirstOrDefault();
                                header.WmsStatus = "4";
                                header.DispatchNote = DespatchNoteNumber;
                                hdb.Entry(header).State = System.Data.EntityState.Modified;
                                hdb.SaveChanges();
                            }
                            ModelState.AddModelError("", "Sales Order : " + SalesOrder + ". Despatch Note : " + DespatchNoteNumber + " created successfully.");
                            return View("DispatchList");
                        }
                        else
                        {
                            ModelState.AddModelError("", ErrorMessage);
                        }
                        
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                }
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        public ActionResult DeleteItem(int WmsId, int WmsLine)
        {
            ViewBag.PickersList = (from a in mdb.mtUsers where a.Picker == true select new { Value = a.Username, Text = a.Username }).ToList();
            try
            {
                var line = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId && a.WmsLine == WmsLine select a).FirstOrDefault();
                wdb.Entry(line).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
                ModelState.AddModelError("", "Line Deleted.");
                return RedirectToAction("Index", new { WmsId = WmsId, SalesOrderLine = line.SalesOrderLine});
            }
            catch(Exception ex)
            {
                var line = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId && a.WmsLine == WmsLine select a).FirstOrDefault();
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index", new { WmsId = WmsId, SalesOrderLine = line.SalesOrderLine });
            }
        }

        public ActionResult DeleteDispatch(int WmsId, int SalesOrderLine)
        {            
            try
            {
                var line = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine select a).ToList();
                foreach(var item in line)
                {
                    wdb.Entry(item).State = System.Data.EntityState.Deleted;
                    wdb.SaveChanges();
                }

                using (var hdb = new WarehouseManagementEntities(""))
                {
                    var header = (from a in hdb.mtWmsOrderMasters where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine select a).FirstOrDefault();
                    header.WmsStatus = "5";                   
                    hdb.Entry(header).State = System.Data.EntityState.Modified;
                    hdb.SaveChanges();
                }

                ModelState.AddModelError("", "Wms Entry Deleted.");
                return RedirectToAction("DispatchList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("DispatchList");
            }
        }

    }
}
