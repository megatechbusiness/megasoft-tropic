using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Megasoft2.Controllers
{
    public class WhseManDeliveryImportController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        //
        // GET: /WhseManDeliveryImport/

        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "DeliveryFileImport")]
        public ActionResult Index(WhseManDeliveryFileViewModel model)
        {
            try
            {
                ModelState.Clear();

                model.Detail = new List<WhseManDeliveryFile>();

                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/") + @"DeliveryFiles\", fileName);
                    file.SaveAs(path);

                    var xDoc = XDocument.Load(path);
                    var DetailList = (from p in xDoc.Descendants("Z1FS_DELIVERY_NOTIFICATION").Descendants("IDOC").Descendants("Z1FS_DELIVERYHEADER").Descendants("Z1FS_BATCHDETAILS") select p).ToList();
                    var DeliveryNumber = (from p in xDoc.Descendants("Z1FS_DELIVERY_NOTIFICATION").Descendants("IDOC").Descendants("Z1FS_DELIVERYHEADER") select p.Element("DELIVERY_NUMBER").Value).FirstOrDefault();
                    var Header = (from p in xDoc.Descendants("Z1FS_DELIVERY_NOTIFICATION").Descendants("IDOC").Descendants("EDI_DC40") select p).FirstOrDefault();
                    if (DetailList.Count > 0)
                    {
                        
                        foreach (var item in DetailList)
                        {
                            WhseManDeliveryFile objFile = new WhseManDeliveryFile();
                            objFile.DELIVERY_NUMBER = DeliveryNumber.ToString();
                            objFile.RECORD_TYPE = item.Element("RECORD_TYPE").Value;
                            objFile.RECORD_FUNCTION = item.Element("RECORD_FUNCTION").Value;
                            objFile.SUPPLIERSTKCODE = item.Element("SUPPLIERSTKCODE").Value;
                            objFile.FS_STKCODE = item.Element("FS_STKCODE").Value;
                            objFile.QUANTITY = Convert.ToDecimal(item.Element("QUANTITY").Value);
                            objFile.UOM = item.Element("UOM").Value;
                            objFile.QTYMTR = Convert.ToDecimal(item.Element("QTYMTR").Value);
                            objFile.GRAMMAGE = Convert.ToDecimal(item.Element("GRAMMAGE").Value);
                            objFile.BATCH_NUMBER = item.Element("BATCH_NUMBER").Value;
                            objFile.SUPPLIERCODE = item.Element("SUPPLIERCODE").Value;
                            objFile.PURCHASEORDER = item.Element("PURCHASEORDER").Value;
                            objFile.LINENO = Convert.ToInt16(item.Element("LINENO.").Value);
                            model.Detail.Add(objFile);
                        }

                        model.DeliveryNote = DeliveryNumber.ToString();
                        model.PurchaseOrder = DetailList.FirstOrDefault().Element("PURCHASEORDER").Value.PadLeft(15, '0');
                        model.FileName = file.FileName;
                        model.FileDate = Convert.ToDateTime(DateTime.ParseExact(Header.Element("CREDAT").Value, "yyyyMMdd", CultureInfo.InstalledUICulture));
                        model.FileTime = TimeSpan.ParseExact(Header.Element("CRETIM").Value, "hhmmss", CultureInfo.InvariantCulture);

                        //Validate File
                        var check = (from a in wdb.mtPorDeliveryImports where a.PurchaseOrder == model.PurchaseOrder && a.DeliveryNote == model.DeliveryNote select a).ToList();
                        if(check.Count > 0)
                        {
                            //File exists in table. IF file Grn'd we abort, if file not Grn' then display warning message.
                            if(!string.IsNullOrEmpty(check.FirstOrDefault().Grn))
                            {
                                ModelState.AddModelError("", "Purchase Order : " + model.PurchaseOrder + " Delivery Note : " + model.DeliveryNote + " already received in Syspro. Cannot continue!");
                                model = new WhseManDeliveryFileViewModel();
                            }
                            else
                            {
                                ModelState.AddModelError("", "A file already eists for Purchase Order : " + model.PurchaseOrder + " and Delivery Note : " + model.DeliveryNote + ". Saving this file will replace the original file.");
                            }
                        }

                        var po = (from a in wdb.PorMasterHdrs where a.PurchaseOrder == model.PurchaseOrder select a).FirstOrDefault();
                        if(po == null)
                        {
                            ModelState.AddModelError("", "Purchase Order : " + model.PurchaseOrder + " not found in Syspro.");
                            model = new WhseManDeliveryFileViewModel();
                        }

                        ViewBag.PoLine = (from a in wdb.PorMasterDetails.AsEnumerable() where a.PurchaseOrder == model.PurchaseOrder && a.LineType == "1" select new { Text = a.Line.ToString() + " - " + a.MWarehouse, Value = a.Line }).ToList();


                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }


                        System.IO.File.Delete(Path.Combine(@"\\10.10.6.20\FreedomShare\Factory\Mondi\", fileName));


                    } 
                    else
                    {
                        ModelState.AddModelError("", "No detail found in file : " + file.FileName + ".");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "File not found.");
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveFile")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "DeliveryFileImport")]
        public ActionResult SaveFile(WhseManDeliveryFileViewModel model)
        {
            try
            {
                ModelState.Clear();

                using(var ddb = new WarehouseManagementEntities(""))
                {
                    var check = (from a in ddb.mtPorDeliveryImports where a.PurchaseOrder == model.PurchaseOrder && a.DeliveryNote == model.DeliveryNote select a).ToList();
                    if (check.Count > 0)
                    {
                        foreach(var item in check)
                        {
                            ddb.Entry(item).State = System.Data.EntityState.Deleted;
                            ddb.SaveChanges();
                        }
                    }
                }

                using(var sdb = new WarehouseManagementEntities(""))
                {
                    foreach(var item in model.Detail)
                    {
                        mtPorDeliveryImport obj = new mtPorDeliveryImport();
                        obj.PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');
                        obj.DeliveryNote = model.DeliveryNote;
                        obj.Line = item.LINENO;
                        obj.FileName = model.FileName;
                        obj.FileDate = model.FileDate;
                        obj.FileTime = model.FileTime;
                        obj.RecordType = item.RECORD_TYPE.ToString();
                        obj.RecordFunction = item.RECORD_FUNCTION;
                        obj.StockCode = item.FS_STKCODE;
                        obj.SupplierStockCode = item.SUPPLIERSTKCODE;
                        obj.Lot = item.BATCH_NUMBER;
                        obj.Quantity = item.QUANTITY;
                        obj.Uom = item.UOM;
                        obj.Meters = item.QTYMTR;
                        obj.Grammage = item.GRAMMAGE;
                        obj.Username = HttpContext.User.Identity.Name.ToUpper();
                        obj.TrnDate = DateTime.Now;

                        var whse = (from a in wdb.PorMasterDetails.AsNoTracking() where a.PurchaseOrder == obj.PurchaseOrder && a.Line == item.LINENO select a.MWarehouse).FirstOrDefault();
                        if(whse == "MR")
                        {
                            obj.Scanned = "Y";
                            
                        }
                        else
                        {
                            obj.Scanned = "N";
                        }
                        sdb.Entry(obj).State = System.Data.EntityState.Added;
                        sdb.SaveChanges();

                        if(whse == "MR")
                        {
                            using (var jippo = new WarehouseManagementEntities(""))
                            {
                                jippo.sp_SaveDeliveryScan(item.BATCH_NUMBER, obj.PurchaseOrder, HttpContext.User.Identity.Name.ToUpper(), model.DeliveryNote);
                            }
                        }
                    }




                    ModelState.AddModelError("", "Saved successfully!");
                    model = new WhseManDeliveryFileViewModel();
                    
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [CustomAuthorize(Activity: "DeliveryFileScan")]
        public ActionResult DeliveryScan()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("DeliveryScan");
            }
        }
        
        public JsonResult CheckBarcode(string Barcode)
        {
            try
            {
                Barcode = Barcode.Substring(0, 9);
                var check = (from a in wdb.mtPorDeliveryImports where a.Lot == Barcode select a).FirstOrDefault();
                if(check != null)
                {
                    var result = wdb.sp_SaveDeliveryScan(Barcode, check.PurchaseOrder, HttpContext.User.Identity.Name.ToUpper(), check.DeliveryNote);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new List<sp_SaveDeliveryScan_Result>(), JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
