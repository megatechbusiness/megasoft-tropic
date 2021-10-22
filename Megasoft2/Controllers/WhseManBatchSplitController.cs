using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManBatchSplitController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        private LabelPrint objPrint = new LabelPrint();
        SysproCore objSyspro = new SysproCore();
        //
        // GET: /WhseManBatchSplit/
        [CustomAuthorize(Activity: "BatchSplit")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouses = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse }).ToList();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            ViewBag.LoadPallet = null;
            return View();
        }


        [CustomAuthorize(Activity: "BatchSplit")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "BatchSplit")]
        public ActionResult BatchSplit(BatchReceipt model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouses = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse }).ToList();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            try
            {
                ModelState.Clear();
                var result = wdb.sp_GetBatchDetailsForBatchSplit(model.BailNo).ToList();
                if (result.Count > 0)
                {
                    if (result.FirstOrDefault().TraceableType == "T" && result.FirstOrDefault().QtyOnHand == 0)
                    {
                        ModelState.AddModelError("", "Quantity on hand for Batch is zero.");
                        ViewBag.LoadPallet = null;
                    }
                    else
                    {
                        model.BailNo = result.FirstOrDefault().BatchId;
                        model.StockCode = result.FirstOrDefault().StockCode;
                        model.StockDescription = result.FirstOrDefault().Description;
                        model.QtyOnHand = (decimal)result.FirstOrDefault().QtyOnHand;
                        model.PackSize = (decimal)result.FirstOrDefault().PackSize;
                        model.Job = result.FirstOrDefault().Job;
                        model.TraceableType = result.FirstOrDefault().TraceableType;
                        model.Uom = result.FirstOrDefault().StockUom;
                        if (result.FirstOrDefault().QtyOnHand != 0)
                        {
                            model.NoOfLabels = (int)((decimal)result.FirstOrDefault().QtyOnHand / (decimal)result.FirstOrDefault().PackSize);
                        }
                        ViewBag.LoadPallet = "Y";
                    }



                }
                else
                {
                    ViewBag.LoadPallet = null;
                    ModelState.AddModelError("", "Batch not found.");
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [CustomAuthorize(Activity: "BatchSplit")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostBatchSplit")]
        public ActionResult PostBatchSplit(BatchReceipt model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouses = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse }).ToList();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            try
            {
                if(model.TraceableType == "T")
                {
                    var LotCheck = (from a in wdb.LotDetails where a.StockCode == model.StockCode && a.Lot == model.BailNo && a.Warehouse == model.Warehouse select a).FirstOrDefault();
                    if(LotCheck == null)
                    {
                        ModelState.AddModelError("", "Batch No. " + model.BailNo + " not found in Warehouse : " + model.Warehouse + ".");
                        return View("Index", model);
                    }
                    else
                    {
                        if(LotCheck.QtyOnHand == 0)
                        {
                            ModelState.AddModelError("", "Qty. on hand for Batch cannot be zero.");
                            return View("Index", model);
                        }
                    }

                    
                }
                else
                {
                    var StockCheck = (from a in wdb.InvWarehouses where a.StockCode == model.StockCode && a.Warehouse == model.Warehouse select a).FirstOrDefault();
                    if(StockCheck == null)
                    {
                        ModelState.AddModelError("", "Batch No. " + model.BailNo + " not found in Warehouse : " + model.Warehouse + ".");
                        return View("Index", model);
                    }
                    else
                    {
                        if (StockCheck.QtyOnHand == 0)
                        {
                            ModelState.AddModelError("", "Qty. on hand for Batch cannot be zero.");
                            return View("Index", model);
                        }
                    }
                }

                string OutMessage = SaveBatchAndPrintLabel(model.Job, model.NoOfLabels, model.PackSize, model.Printer, model.StockCode, model.Uom, model.TraceableType, model.BailNo, model.Warehouse);
                ModelState.AddModelError("", OutMessage);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        public string SaveBatchAndPrintLabel(string Job, int NoOfLabels, decimal PackSize, string Printer, string StockCode, string StockUom, string TraceableType, string BatchNo, string Warehouse)
        {
            try
            {
                
                int NextNo = 1;
                var NextPallet = wdb.sp_GetMaxJobPalletNumber(Job.PadLeft(15, '0')).ToList();
                if (NextPallet.Count > 0)
                {
                    NextNo = (int)NextPallet.FirstOrDefault().NumberOnly;
                }


                List<mtProductionLabel> LabelDetail = new List<mtProductionLabel>();
                for (int i = 0; i < NoOfLabels; i++)
                {
                    mtProductionLabel obj = new mtProductionLabel();
                    obj.Job = Job.PadLeft(15, '0');

                    obj.GrossQty = PackSize;
                    obj.NetQty = PackSize;
                    

                    //Bagging and Wicketting - No Core or Tare therefore NetQty = GrossQty - (Core + Tare)
                    obj.Core = 0;
                    obj.Tare = 0;

                    obj.NetQty = obj.GrossQty - (obj.Core + obj.Tare);


                    obj.NoOfLabels = NoOfLabels;
                    obj.Username = HttpContext.User.Identity.Name.ToUpper();
                    obj.LabelPrinted = "Y";
                    obj.DatePrinted = DateTime.Now;
                    var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                    if (setting.PalletNoReq == true)
                    {
                        
                        obj.BatchId = Job.TrimStart('0') + "-" + setting.DefaultPalletNo + "-" + NextNo.ToString().PadLeft(4, '0');
                        
                    }
                    else
                    {
                        obj.BatchId = Job.TrimStart('0') + "-" + NextNo.ToString().PadLeft(4, '0');
                    }

                    obj.LabelReceipted = "A";

                    obj.Department = "Bag";

                    obj.Reference = BatchNo;

                    //wdb.mtProductionLabels.Add(obj);
                    //wdb.SaveChanges();
                    LabelDetail.Add(obj);

                    //obj.PalletQty
                    NextNo++;
                }

                //PostAdjustment
                string ErrorMessage = PostAdjustment(LabelDetail, Warehouse, StockCode, StockUom, TraceableType, BatchNo);
                if(!string.IsNullOrEmpty(ErrorMessage))
                {
                    return ErrorMessage;
                }

                foreach(var item in LabelDetail)
                {
                    wdb.mtProductionLabels.Add(item);
                    wdb.SaveChanges();
                }

                string result = objPrint.PrintJobLabel(LabelDetail, Printer, PackSize.ToString(), "Bag", PackSize.ToString());
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostAdjustment(List<mtProductionLabel> model, string Warehouse, string StockCode, string StockUom, string TraceableType, string BatchNo)
        {
            try
            {

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Inventory Receipts Business Object");
                Document.Append("-->");
                Document.Append("<PostInvReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMRDOC.XSD\">");

                var Total = (from a in model select a.NetQty).Sum();

                if(StockUom == "TH")
                {
                    Total = Total / 1000;
                }

                
                Document.Append("<Item>");
                Document.Append("<Journal />");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Version />");
                Document.Append("<Release />");
                Document.Append("<Quantity>" + Total * -1 + "</Quantity>");
                Document.Append("<UnitOfMeasure />");
                Document.Append("<Units />");
                Document.Append("<Pieces />");
                Document.Append("<BinLocation></BinLocation>");
                Document.Append("<FifoBucket />");
                if(TraceableType == "T")
                {
                    Document.Append("<Lot><![CDATA[" + BatchNo + "]]></Lot>");
                }
                
                Document.Append("<Reference>COUNTERSALES</Reference>");
                Document.Append("<Notation>COUNTERSALES</Notation>");
                Document.Append("</Item>");

                foreach(var item in model)
                {
                    Document.Append("<Item>");
                    Document.Append("<Journal />");
                    Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                    Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                    Document.Append("<Version />");
                    Document.Append("<Release />");
                    decimal Qty=0;
                    if (StockUom == "TH")
                    {
                        Qty = (decimal)item.NetQty / 1000;
                    }
                    else
                    {
                        Qty = (decimal)item.NetQty;
                    }
                    Document.Append("<Quantity>" + Qty + "</Quantity>");
                    Document.Append("<UnitOfMeasure />");
                    Document.Append("<Units />");
                    Document.Append("<Pieces />");
                    Document.Append("<BinLocation></BinLocation>");
                    Document.Append("<FifoBucket />");
                    if (TraceableType == "T")
                    {
                        Document.Append("<Lot><![CDATA[" + item.BatchId + "]]></Lot>");
                        Document.Append("<LotConcession></LotConcession>");
                    }

                    Document.Append("<Reference>COUNTERSALES</Reference>");
                    Document.Append("<Notation>COUNTERSALES</Notation>");
                    Document.Append("</Item>");
                }

                Document.Append("</PostInvReceipts>");


                //Declaration
                StringBuilder Parameter = new StringBuilder();               

                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Inventory Receipts Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostInvReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMR.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<ManualSerialTransfersAllowed>N</ManualSerialTransfersAllowed>");
                Parameter.Append("<ReturnDetailedReceipt>N</ReturnDetailedReceipt>");
                Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostInvReceipts>");

                string Guid = objSyspro.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to login to Syspro.";
                }
                else
                {
                    string XmlOut = objSyspro.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "INVTMR");
                    objSyspro.SysproLogoff(Guid);
                    string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    return ErrorMessage;
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
