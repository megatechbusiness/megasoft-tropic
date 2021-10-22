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
    public class WhseManBackOrderReleaseController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        //
        // GET: /WhseManBackOrderRelease/

        [CustomAuthorize("BackOrderRelease")]
        public ActionResult Index(string SalesOrder = null)
        {
            WhseManBackOrderReleaseViewModel model = new WhseManBackOrderReleaseViewModel();
            if (SalesOrder != null)
            {
                
                SalesOrder = SalesOrder.PadLeft(15, '0');
                model.SoLines = wdb.sp_GetBackOrderReleaseLines(SalesOrder).ToList();
                model.SalesOrder = SalesOrder;
                if (model.SoLines.Count == 0)
                {
                    ModelState.AddModelError("", "No outstanding lines found for Sales Order");
                }
                else
                {

                }
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "BackOrderRelease")]
        public ActionResult Index(WhseManBackOrderReleaseViewModel model)
        {
            try
            {
                ModelState.Clear();
                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                if (!string.IsNullOrEmpty(model.SalesOrder))
                {
                    string SalesOrder = model.SalesOrder.PadLeft(15, '0');
                    model.SoLines = wdb.sp_GetBackOrderReleaseLines(SalesOrder).ToList();
                    model.SalesOrder = SalesOrder;
                    if (model.SoLines.Count == 0)
                    {
                        ModelState.AddModelError("", "No outstanding lines found for Sales Order");
                    }
                    else
                    {
                        
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


        public ActionResult ReleaseLine(string SalesOrder, int Line)
        {
            try
            {
                WhseManBackOrderReleaseViewModel model = new WhseManBackOrderReleaseViewModel();
                model.ReleaseItems = wdb.sp_GetBackOrderReleaseItems(SalesOrder, Line).ToList();
                if(model.ReleaseItems.Count == 0)
                {
                    ModelState.AddModelError("", "No items found for order.");
                }
                else
                {
                    model.MStockCode = model.ReleaseItems.FirstOrDefault().MStockCode;
                    model.MStockDes = model.ReleaseItems.FirstOrDefault().MStockDes;
                    model.MWarehouse = model.ReleaseItems.FirstOrDefault().MWarehouse;
                    model.MStockingUom = model.ReleaseItems.FirstOrDefault().MStockingUom;
                    model.MOrderQty = model.ReleaseItems.FirstOrDefault().MOrderQty;
                    model.MBackOrderQty = model.ReleaseItems.FirstOrDefault().MBackOrderQty;
                }
                model.SalesOrder = SalesOrder;
                model.Line = Line;
                return View("ReleaseLine", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ReleaseLine");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "BackOrderRelease")]
        public ActionResult ReleaseLine(WhseManBackOrderReleaseViewModel model)
        {
            try
            {
                ModelState.Clear();

                if(model.ReleaseItems.Count > 0)
                {
                    string Guid = sys.SysproLogin();
                    string XmlOut = sys.SysproPost(Guid, this.BuildReleaseParameter(), BuildReleaseDoc(model), "SORTBO");
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        //Posted.
                        ModelState.AddModelError("", "Posted Successfully.");

                        //Reload Grid

                        model.ReleaseItems = wdb.sp_GetBackOrderReleaseItems(model.SalesOrder, model.Line).ToList();
                        if (model.ReleaseItems.Count == 0)
                        {
                            ModelState.AddModelError("", "No items found for order.");
                        }
                        else
                        {
                            model.MStockCode = model.ReleaseItems.FirstOrDefault().MStockCode;
                            model.MStockDes = model.ReleaseItems.FirstOrDefault().MStockDes;
                            model.MWarehouse = model.ReleaseItems.FirstOrDefault().MWarehouse;
                            model.MStockingUom = model.ReleaseItems.FirstOrDefault().MStockingUom;
                            model.MOrderQty = model.ReleaseItems.FirstOrDefault().MOrderQty;
                            model.MBackOrderQty = model.ReleaseItems.FirstOrDefault().MBackOrderQty;
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

                return View("ReleaseLine", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ReleaseLine", model);
            }
        }

        public string BuildReleaseDoc(WhseManBackOrderReleaseViewModel model)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Sales Order Back Order Release Business Object");
                Document.Append("-->");
                Document.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBODOC.XSD\">");

                foreach(var item in model.ReleaseItems)
                {
                    if(item.ReleaseItem == true)
                    {
                        Document.Append("<Item>");
                        //Document.Append("<LatestAcceptedLotExpiryDate>2006-09-16</LatestAcceptedLotExpiryDate>");
                        //Document.Append("<LatestAcceptedSerialExpiryDate>2006-09-16</LatestAcceptedSerialExpiryDate>");
                        //Document.Append("<LatestAcceptedSerialScrapDate>2006-09-16</LatestAcceptedSerialScrapDate>");
                        //Document.Append("<ScheduleAllocateDate>2006-10-16</ScheduleAllocateDate>");
                        //Document.Append("<ScheduleLineShipDate>2006-10-16</ScheduleLineShipDate>");
                        //Document.Append("<Customer>000008</Customer>");
                        Document.Append("<SalesOrder>" + model.SalesOrder + "</SalesOrder>");
                        //Document.Append("<StockCode>LOT100</StockCode>");
                        //Document.Append("<Warehouse>FG</Warehouse>");
                        Document.Append("<Quantity>" + item.QtyOnHand + "</Quantity>");
                        //Document.Append("<ActualShipQty>" + item.QtyOnHand + "</ActualShipQty>");
                        //Document.Append("");
                        Document.Append("<UnitOfMeasure />");
                        Document.Append("<Units />");
                        Document.Append("<Pieces />");
                        Document.Append("<ReleaseFromMultipleLines>N</ReleaseFromMultipleLines>");
                        Document.Append("<SalesOrderLine>" + model.Line + "</SalesOrderLine>");
                        Document.Append("<CompleteLine>N</CompleteLine>");
                        Document.Append("<AdjustOrderQuantity>N</AdjustOrderQuantity>");
                        //Document.Append("<Serials>");
                        //Document.Append("<SerialNumber />");
                        //Document.Append("<SerialQuantity />");
                        //Document.Append("<SerialCreationDate />");
                        //Document.Append("<SerialExpiryDate />");
                        //Document.Append("<SerialScrapDate />");
                        //Document.Append("<SerialLocation />");
                        //Document.Append("<SerialUnits />");
                        //Document.Append("<SerialPieces />");
                        //Document.Append("</Serials>");
                        Document.Append("<Lot>" + item.Lot + "</Lot>");
                        //Document.Append("<Bins>");
                        //Document.Append("<BinLocation>A</BinLocation>");
                        //Document.Append("<BinQuantity>3.000</BinQuantity>");
                        //Document.Append("<BinUnits />");
                        //Document.Append("<BinPieces />");
                        //Document.Append("</Bins>");
                        Document.Append("<OrderStatus>3</OrderStatus>");
                        Document.Append("<ReleaseFromShip>N</ReleaseFromShip>");
                        Document.Append("<ZeroShipQuantity>N</ZeroShipQuantity>");
                        Document.Append("<AllocateSerialNumbers>N</AllocateSerialNumbers>");
                        Document.Append("<eSignature>");
                        Document.Append("</eSignature>");
                        Document.Append("</Item>");
                    }
                    
                }
                

                Document.Append("</PostSorBackOrderRelease>");

                return Document.ToString();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildReleaseParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the Sales Order Back Order Release Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBO.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<AddQuantityToBatchSerial>N</AddQuantityToBatchSerial>");
            Parameter.Append("<IgnoreAutoDepletion>Y</IgnoreAutoDepletion>");
            Parameter.Append("<ShipKitFromDefaultBin>N</ShipKitFromDefaultBin>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostSorBackOrderRelease>");

            return Parameter.ToString();
        }





        }
}
