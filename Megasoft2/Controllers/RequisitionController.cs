using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        Email _email = new Email();
        MegasoftAlertsBL ABL = new MegasoftAlertsBL();

        [CustomAuthorize(Activity: "Requisition")]
        public ActionResult Index()
        {
            try
            {
                RequisitionViewModel model = new RequisitionViewModel();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                if (requser == null)
                {
                    return View(model);
                }
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var reqList = wdb.sp_mtReqGetRequisitionList(requser, Company).ToList();
                model.ReqList = reqList;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }

        [CustomAuthorize(Activity: "CreateRequisition")]
        public ActionResult Create(string Requisition = "")
        {
            try
            {
                RequisitionViewModel model = new RequisitionViewModel();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var CostCentreList = wdb.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
                ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
                ViewBag.BranchList = (from a in wdb.sp_mtReqGetUserBranch(Company).ToList() select new { Branch = a.Branch, Description = a.Description }).ToList();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                else
                {
                    sp_mtReqGetRequisitionHeader_Result emptyheader = new sp_mtReqGetRequisitionHeader_Result();
                    model.Header = emptyheader;
                }
                ViewBag.CanChangeAddress = CanCreatePo(Requisition);
                ViewBag.CanCreatePo = CanCreatePo(Requisition);
                ViewBag.CanRoute = CanRoute(Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(Requisition);
                ViewBag.CanApprove = CanApprove(Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(Requisition);
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.CanChangeAddress = false;
                ViewBag.CanCreatePo = false;
                ViewBag.CanRoute = false;
                ViewBag.CanMaintainReq = false;
                ViewBag.CanApprove = false;
                ViewBag.CanRouteBack = false;
                RequisitionViewModel model = new RequisitionViewModel();
                model.Header = new sp_mtReqGetRequisitionHeader_Result();
                model.Lines = new List<sp_mtReqGetRequisitionLines_Result>();
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [CustomAuthorize(Activity: "CreateRequisition")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AddLine")]
        public ActionResult AddLine(RequisitionViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var sysSettings = (from a in mdb.mtSystemSettings where a.Id == 1 select a).FirstOrDefault();
            model.LocalCurrency = sysSettings.LocalCurrency;
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            var detail = wdb.sp_mtReqGetRequisitionLines(model.Header.Requisition, UserCode, Username, Company).ToList();
            model.Lines = detail;
            model.LineMethod = "Add";
            sp_mtReqGetRequisitionLines_Result line = new sp_mtReqGetRequisitionLines_Result();
            model.Line = line;


            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.WarehouseList = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.TaxCodeList = (from a in wdb.AdmTaxes select new { TaxCode = a.TaxCode, Description = a.TaxCode + " - " + a.Description }).ToList();
            ViewBag.BuyerList = (from a in wdb.InvBuyers select new { Buyer = a.Buyer, Description = a.Buyer + " - " + a.Name }).ToList();
            ViewBag.OrderUomList = LoadDefaultOrderUom();
            ViewBag.PriceMethodList = LoadPriceMethods();
            return View("AddLine", model);
        }

        public class OrderUom
        {
            public string Uom { get; set; }
            public string Desc { get; set; }
        }

        public List<OrderUom> LoadDefaultOrderUom()
        {
            var Uoms = new List<OrderUom>();
            var uom = new OrderUom();
            uom.Uom = "EA";
            uom.Desc = "EA";
            Uoms.Add(uom);
            return Uoms;
        }

        public List<sp_mtReqGetPricingData_Result> LoadPriceMethods()
        {
            var Uoms = new List<sp_mtReqGetPricingData_Result>();
            var uom = new sp_mtReqGetPricingData_Result();
            uom.PriceMethod = "Manual";
            uom.Price = "Manual";
            Uoms.Add(uom);
            return Uoms;
        }

        public ActionResult ChangeLine(string Requisition, int Line)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            RequisitionViewModel model = new RequisitionViewModel();
            var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
            var detail = wdb.sp_mtReqGetRequisitionLines(Requisition, UserCode, Username, Company).ToList().Where(a => a.Line == Line).FirstOrDefault();
            model.Header = header;
            model.Line = detail;
            model.LineMethod = "Change";
            if (detail.Warehouse != "**")
            {
                ViewBag.WarehouseList = (from a in wdb.InvWarehouses join b in wdb.vw_InvWhControl on a.Warehouse equals b.Warehouse where a.StockCode == detail.StockCode select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + b.Description }).ToList();
            }
            else
            {
                var NonStocked = new List<vw_InvWhControl>();
                var warehouse = new vw_InvWhControl();
                warehouse.Warehouse = "**";
                warehouse.Description = "**";
                NonStocked.Add(warehouse);
                ViewBag.WarehouseList = NonStocked;
            }
            ViewBag.TaxCodeList = (from a in wdb.AdmTaxes select new { TaxCode = a.TaxCode, Description = a.TaxCode + " - " + a.Description }).ToList();
            ViewBag.BuyerList = (from a in wdb.InvBuyers select new { Buyer = a.Buyer, Description = a.Buyer + " - " + a.Name }).ToList();
            ViewBag.OrderUomList = LoadDefaultOrderUom();
            ViewBag.PriceMethodList = LoadPriceMethods();
            var cat = (from a in wdb.PorSupStkInfoes where a.Supplier == model.Line.Supplier && a.StockCode == model.Line.StockCode select a.SupCatalogueNum).FirstOrDefault();
            model.Line.SupCatalogueNum = cat;
            model.SupCatNumber = model.Line.SupCatalogueNum;
            return View("AddLine", model);
        }

        public ActionResult StockCodeSearch(string CostCentre)
        {
            ViewBag.CostCentre = CostCentre;
            return PartialView();
        }

        public JsonResult StockCodeList(string CostCentre, string Warehouse)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var result = wdb.sp_mtReqGetStockCodes(Company, CostCentre, Warehouse).ToList();
            var Stock = (from a in result select new { MStockCode = a.StockCode, MStockDes = a.Description, MStockingUom = a.StockUom, ProductClass = a.ProductClass, WarehouseToUse = a.WarehouseToUse }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWarehouse(string StockCode)
        {
            var result = (from a in wdb.InvWarehouses join b in wdb.vw_InvWhControl on a.Warehouse equals b.Warehouse where a.StockCode == StockCode select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + b.Description }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetStockCodeUom(string StockCode)
        {
            var result = (from a in wdb.InvMasters where a.StockCode == StockCode select new { StockUom = a.StockUom, AlternateUom = a.AlternateUom }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPriceMethodsAndPrice(string StockCode, string Warehouse, string Supplier)
        {
            var result = wdb.sp_mtReqGetPricingData(StockCode, Warehouse, Supplier).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetStockCodeDetails(string StockCode)
        {
            //HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            //var result = (from a in wdb.InvMasters
            //              join p in wdb.ApSuppliers on a.Supplier equals p.Supplier into ps
            //              from z in ps.DefaultIfEmpty()
            //              where a.StockCode == StockCode
            //              select new { StockCode = a.StockCode, Description = a.Description, StockUom = a.StockUom, ProductClass = a.ProductClass, Warehouse = a.WarehouseToUse, Buyer = a.Buyer, TaxCode = a.TaxCode, Supplier = (z == null ? String.Empty : z.Supplier), SupplierName = (z == null ? String.Empty : z.SupplierName), Currency = (z == null ? String.Empty : z.Currency) }).ToList();
            var result = wdb.sp_mtReqGetStockCodeDetailsByStockCode(StockCode).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductClassSearch(string CostCentre)
        {
            ViewBag.CostCentre = CostCentre;
            return PartialView();
        }

        public JsonResult ProductClassList(string CostCentre)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var result = wdb.sp_mtReqGetProductClass(Company, CostCentre).ToList();
            var Stock = (from a in result select new { ProductClass = a.ProductClass, Description = a.Description }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ContractPriceSearch()
        {
            return PartialView();
        }

        public JsonResult ContractPriceList(string StockCode, string Supplier)
        {

            var result = wdb.sp_mtReqGetSupplierContractPricing(StockCode, Supplier).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SupplierSearch()
        {
            return PartialView();
        }

        public JsonResult SupplierList(string Requisition, string StockCode, string Warehouse)
        {
            var result = wdb.sp_mtReqGetSuppliers(Requisition, StockCode, Warehouse).ToList();
            var Stock = (from a in result select new { Supplier = a.Supplier, Name = a.SupplierName, Currency = a.Currency, ExchangeRate = a.ExchangeRate }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }



        public ActionResult JobSearch()
        {
            return PartialView();
        }

        public JsonResult JobList(bool SubContract)
        {
            if (SubContract)
            {
                var subcontract = wdb.sp_mtReqGetActiveSubContractJobs().ToList();
                return Json(subcontract, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var jobs = wdb.sp_mtReqGetActiveJobs().ToList();
                return Json(jobs, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GlCodeSearch(string CostCentre)
        {
            ViewBag.CostCentre = CostCentre;
            return PartialView();
        }

        public JsonResult GlCodeList(string CostCentre)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var result = wdb.sp_mtReqGetGlCodes(Company, CostCentre).ToList();
            var Stock = (from a in result select new { GlCode = a.GlCode, Description = a.Description }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }


        [CustomAuthorize(Activity: "CreateRequisition")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostLine")]
        public ActionResult PostLine(RequisitionViewModel model)
        {
            ModelState.Clear();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            string Username = HttpContext.User.Identity.Name.ToUpper();
            ViewBag.PriceMethodList = LoadPriceMethods();
            ViewBag.BuyerList = (from a in wdb.InvBuyers select new { Buyer = a.Buyer, Description = a.Buyer + " - " + a.Name }).ToList();
            var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            if (model.Line.Warehouse != "**")
            {
                ViewBag.WarehouseList = (from a in wdb.InvWarehouses join b in wdb.vw_InvWhControl on a.Warehouse equals b.Warehouse where a.StockCode == model.Line.StockCode select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + b.Description }).ToList();
            }
            else
            {
                var NonStocked = new List<vw_InvWhControl>();
                var warehouse = new vw_InvWhControl();
                warehouse.Warehouse = "**";
                warehouse.Description = "**";
                NonStocked.Add(warehouse);
                ViewBag.WarehouseList = NonStocked;
            }
            ViewBag.TaxCodeList = (from a in wdb.AdmTaxes select new { TaxCode = a.TaxCode, Description = a.TaxCode + " - " + a.Description }).ToList();
            try
            {
                if (!string.IsNullOrWhiteSpace(model.Header.Requisition))
                {
                    var check = (from a in wdb.sp_mtReqGetRequisitionLines(model.Header.Requisition, UserCode, Username, Company)
                                 where a.Line == 1
                                 select a).FirstOrDefault();
                    if (check != null)
                    {
                        if (model.Line.Line != 1)
                        {
                            if (model.Line.Buyer != check.Buyer)
                            {
                                ModelState.AddModelError("", "Error: Buyer on Requisition line 1 (" + check.Buyer + ") does not match buyer : " + model.Line.Buyer);
                                return View("AddLine", model);

                            }
                        }

                    }

                }


                string Requisition = "";

                //var DirectExpese = wdb.mt_DirectExpenseByStockCode(model.Line.StockCode).FirstOrDefault();
                //if (DirectExpese != null)
                //{
                //    if (DirectExpese.DirectExpenseIssue == "Y")
                //    {
                //        var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(model.Line.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();
                //        var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == "DIREXP" && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();


                //        if (string.IsNullOrWhiteSpace(GlCode))
                //        {
                //            ModelState.AddModelError("", "No GL code found in Matrix for Cost Centre: DIREXP Product Cass: " + TraceableType.ProductClass);
                //            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                //            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                //            var CostCentreList = wdb.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
                //            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
                //            ViewBag.BranchList = (from a in wdb.sp_mtReqGetUserBranch(Company).ToList() select new { Branch = a.Branch, Description = a.Description }).ToList();
                //            ViewBag.CanChangeAddress = CanCreatePo(Requisition);
                //            ViewBag.CanCreatePo = CanCreatePo(Requisition);
                //            ViewBag.CanRoute = CanRoute(Requisition);
                //            ViewBag.CanMaintainReq = CanMaintainReq(Requisition);
                //            ViewBag.CanApprove = CanApprove(Requisition);
                //            ViewBag.CanAlternateRoute = CanAlternateRoute(Requisition);
                //            sp_mtReqGetRequisitionHeader_Result emptyheader = new sp_mtReqGetRequisitionHeader_Result();
                //            model.Header = emptyheader;
                //            return View("Create", model);
                //        }
                //    }
                //}

                string Guid = sys.SysproLogin();
                //string Username = HttpContext.User.Identity.Name.ToUpper();
                var Requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                string ActionType = "";

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Requisition Entry Post Business Object");
                Document.Append("-->");
                Document.Append("<PostRequisition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRQDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<User>" + Requser + "</User>");
                Document.Append("<UserPassword/>");
                if (!string.IsNullOrWhiteSpace(model.Header.Requisition))
                {
                    //Updating existing Requisition
                    Document.Append("<RequisitionNumber>" + model.Header.Requisition + "</RequisitionNumber>");
                    ActionType = "U";
                    //wdb.sp_mtReqUpdateReqUserWarehouse(Requser, model.Line.Warehouse);
                }
                else
                {
                    ActionType = "A";
                }

                if (model.Line.Line != 0)
                {
                    Document.Append("<RequisitionLine>" + model.Line.Line + "</RequisitionLine>");

                }
                else
                {
                    Document.Append("<RequisitionLine/>");
                    Document.Append("<StockCode><![CDATA[" + model.Line.StockCode + "]]></StockCode>");
                }

                if (model.Line.Warehouse == "**")
                {
                    Document.Append("<Description><![CDATA[" + model.Line.StockDescription + "]]></Description>");
                    Document.Append("<UnitOfMeasure><![CDATA[" + model.Line.OrderUom + "]]></UnitOfMeasure>");
                    Document.Append("<LedgerCode><![CDATA[" + model.Line.GlCode + "]]></LedgerCode>");
                    Document.Append("<ProductClass><![CDATA[" + model.Line.ProductClass + "]]></ProductClass>");
                    Document.Append("<TaxCode><![CDATA[" + model.Line.TaxCode + "]]></TaxCode>");
                }

                Document.Append("<Quantity><![CDATA[" + model.Line.OrderQty + "]]></Quantity>");

                Document.Append("<DueDate><![CDATA[" + Convert.ToDateTime(model.Line.DueDate).ToString("yyyy-MM-dd") + "]]></DueDate>");
                Document.Append("<Reason><![CDATA[" + model.Line.ReasonForReqn + "]]></Reason>");
                Document.Append("<RequisitionType>N</RequisitionType>");
                //Document.Append("<RouteOn>Y</RouteOn>");
                Document.Append("<Buyer><![CDATA[" + model.Line.Buyer + "]]></Buyer>");
                Document.Append("<RouteToUser>" + Requser + "</RouteToUser>");
                Document.Append("<Supplier><![CDATA[" + model.Line.Supplier + "]]></Supplier>");
                Document.Append("<Price><![CDATA[" + model.Line.Price + "]]></Price>");
                Document.Append("<PriceUom><![CDATA[" + model.Line.PriceUom + "]]></PriceUom>");
                //if (model.Line.Line == 0)
                //{
                var Catalogue = (from a in wdb.PorSupStkInfoes where a.Supplier == model.Line.Supplier && a.StockCode == model.Line.StockCode select a.SupCatalogueNum).FirstOrDefault();
                Document.Append("<Warehouse><![CDATA[" + model.Line.Warehouse + "]]></Warehouse>");
                Document.Append("<Catalogue>" + Catalogue + "</Catalogue>");
                //}
                //Document.Append("<PurchaseOrderComments>TEST Comment</PurchaseOrderComments>");
                Document.Append("<Job/>");

                Document.Append("</Item>");
                Document.Append("</PostRequisition>");





                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Requisition Entry Post Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostRequisition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRQ.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<AllowNonStockedItems>Y</AllowNonStockedItems>");
                Parameter.Append("<AcceptGLCodeforStocked>N</AcceptGLCodeforStocked>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                if (!string.IsNullOrWhiteSpace(model.Header.Requisition))
                {
                    if (model.Line.Line == 0)
                    {
                        Parameter.Append("<ActionType>A</ActionType>");
                    }
                    else
                    {
                        Parameter.Append("<ActionType>C</ActionType>");
                    }
                }
                else
                {
                    Parameter.Append("<ActionType>A</ActionType>");
                }
                Parameter.Append("<GiveErrorWhenDuplicateFound>N</GiveErrorWhenDuplicateFound>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostRequisition>");

                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "PORTRQ");

                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {

                    Requisition = sys.GetFirstXmlValue(XmlOut, "Requisition");
                    if (ActionType == "A")
                    {
                        ABL.SaveMegasoftAlert("Requisition : " + Requisition + " created in Syspro.");
                    }


                    model.Header.Requisition = Requisition;
                    if (model.Line.ExchangeRate == null)
                    {
                        model.Line.ExchangeRate = 0;
                    }
                    PostReqCustomForm(Guid, model.Header.CostCentre, Requisition, model.Header.Branch, (decimal)model.Line.ExchangeRate, ActionType);
                    //Thread.Sleep(7000); //Wait 3 seconds for replication

                    ViewBag.CanChangeAddress = CanCreatePo(Requisition);
                    ViewBag.CanCreatePo = CanCreatePo(Requisition);
                    ViewBag.CanRoute = CanRoute(Requisition);
                    ViewBag.CanMaintainReq = CanMaintainReq(Requisition);
                    ViewBag.CanApprove = CanApprove(Requisition);
                    ViewBag.CanAlternateRoute = CanAlternateRoute(Requisition);

                    var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;

                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Posted successfully");
                    return View("Create", model);
                }
                else
                {
                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Syspro Error: " + ErrorMessage);
                    return View("AddLine", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("AddLine", model);
            }
        }


        public string PostReqCustomForm(string Guid, string CostCentre, string Requisition, string Branch, decimal ExchangeRate, string ActionType)
        {
            try
            {

                //SYSPRO 6.1

                //Declaration
                //StringBuilder Document = new StringBuilder();

                ////Building Document content
                //Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                //Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                //Document.Append("<!--");
                //Document.Append("Sample XML for the Custom Form Setup Business Object");
                //Document.Append("-->");
                //Document.Append("<SetupCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSFMDOC.XSD\">");
                //Document.Append("<Item>");
                //Document.Append("<Key>");
                //Document.Append("<FormType>REQ</FormType>");
                //Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                //Document.Append("<FieldName>COS001</FieldName>");
                //Document.Append("</Key>");
                //Document.Append("<AlphaValue><![CDATA[" + CostCentre + "]]></AlphaValue>");
                //Document.Append("</Item>");

                //Document.Append("<Item>");
                //Document.Append("<Key>");
                //Document.Append("<FormType>REQ</FormType>");
                //Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                //Document.Append("<FieldName>BRA001</FieldName>");
                //Document.Append("</Key>");
                //Document.Append("<AlphaValue><![CDATA[" + Branch + "]]></AlphaValue>");
                //Document.Append("</Item>");

                //Document.Append("<Item>");
                //Document.Append("<Key>");
                //Document.Append("<FormType>REQ</FormType>");
                //Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                //Document.Append("<FieldName>EXC001</FieldName>");
                //Document.Append("</Key>");
                //Document.Append("<AlphaValue><![CDATA[" + ExchangeRate + "]]></AlphaValue>");
                //Document.Append("</Item>");

                //Document.Append("</SetupCustomForm>");

                ////Declaration
                //StringBuilder Parameter = new StringBuilder();

                ////Building Parameter content
                //Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                //Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                //Parameter.Append("<!--");
                //Parameter.Append("Sample XML for the Custom Form Setup Business Object");
                //Parameter.Append("-->");
                //Parameter.Append("<SetupCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSFM.XSD\">");
                //Parameter.Append("<Parameters>");
                //Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                //Parameter.Append("</Parameters>");
                //Parameter.Append("</SetupCustomForm>");

                //string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "COMSFM");
                //return sys.GetXmlErrors(XmlOut);


                ////SYSPRO 7

                ////Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Custom Form Post Business Object");
                Document.Append("-->");
                Document.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFMDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyFields>");
                Document.Append("<Requisition><![CDATA[" + Requisition + "]]></Requisition>");
                Document.Append("</KeyFields>");
                Document.Append("</Key>");
                Document.Append("<Fields>");
                Document.Append("<CostCentre><![CDATA[" + CostCentre + "]]></CostCentre>");
                Document.Append("<Branch><![CDATA[" + Branch + "]]></Branch>");
                Document.Append("<ExchangeRate><![CDATA[" + ExchangeRate + "]]></ExchangeRate>");
                Document.Append("</Fields>");
                Document.Append("</Item>");
                Document.Append("</PostCustomForm>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Parameters used in the Custom Form Post Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFM.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<Function>" + ActionType + "</Function>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostCustomForm>");



                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "COMTFM");
                return sys.GetXmlErrors(XmlOut);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CanMaintainReq(string Requisition)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
                var requser = User.ReqPrefix;
                bool isAdmin = User.Administrator;

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return true;
                }

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();
                if (ReqDetail.Count > 0)
                {
                    var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
                    if (po.Count == 0)
                    {
                        return false;
                    }
                }


                var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();


                if (header.ReqnStatus == "*")
                {
                    return false;
                }



                if (header.ReqnStatus == "R")
                {
                    return false;
                }
                if (string.IsNullOrWhiteSpace(header.ReqHolder))
                {
                    if (isAdmin)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (requser == header.ReqHolder)
                {
                    return true;
                }
                else
                {
                    //if (isAdmin)
                    //{
                    //    return true;
                    //}
                    //else
                    //{
                    return false;
                    //}
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CanRoute(string Requisition)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return false;
                }
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();
                if (ReqDetail.Count > 0)
                {
                    var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
                    if (po.Count == 0)
                    {
                        return false;
                    }
                }
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
                var requser = User.ReqPrefix;
                bool isAdmin = User.Administrator;
                var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                if (header.ReqnStatus == "P")
                {
                    return false;
                }
                if (header.ReqnStatus != "R")
                {
                    if (isAdmin)
                    {
                        if (header.ReqHolder.Trim() == header.OriginatorCode.Trim())
                        {
                            return true;
                        }
                        return false;
                    }
                    else
                    {

                        if (header.ReqHolder.Trim() == requser && header.ReqHolder.Trim() == header.OriginatorCode.Trim())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //public bool CanCreatePo(string Requisition)
        //{
        //    try
        //    {

        //        if (string.IsNullOrWhiteSpace(Requisition))
        //        {
        //            return false;
        //        }

        //        HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
        //        var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
        //        var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();
        //        if (ReqDetail.Count > 0)
        //        {
        //            var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
        //            if (po.Count == 0)
        //            {
        //                return false;
        //            }
        //        }
        //        string Username = HttpContext.User.Identity.Name.ToUpper();
        //        var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
        //        var requser = User.ReqPrefix;
        //        bool isAdmin = User.Administrator;
        //        var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
        //        var PoUser = (from a in wdb.sp_mtReqGetRequisitionUsers() where a.UserCode == requser select a).FirstOrDefault();

        //        if (header == null)
        //        {
        //            ModelState.AddModelError("", "Megasoft: Reading replication data. Unable to find requisition header.");
        //        }

        //        if (header.ReqnStatus == "R")
        //        {
        //            if (PoUser.CanCreateOrder == "Y")
        //            {
        //                if (header.ReqHolder == requser)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    if (isAdmin)
        //                    {
        //                        return true;
        //                    }
        //                }
        //                return false;
        //            }
        //            return false;
        //        }
        //        return false;



        //        //if (header.ReqnStatus == "Approved")
        //        //{
        //        //    if (isAdmin)
        //        //    {
        //        //        return true;
        //        //    }
        //        //    else
        //        //    {

        //        //        if (string.IsNullOrWhiteSpace(PoUser))
        //        //        {
        //        //            //No PO User defined against operator. Allow originator to create PO if Originater is current holder
        //        //            if (header.Holder == header.Originator)
        //        //            {
        //        //                return true;
        //        //            }
        //        //            else
        //        //                return false;
        //        //        }
        //        //        else
        //        //        {
        //        //            if (PoUser.UserForPorder == requser)
        //        //            {
        //        //                return true;
        //        //            }
        //        //            else
        //        //                return false;
        //        //        }
        //        //    }
        //        //}
        //        //else
        //        //    return false;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}


        public bool CanCreatePo(string Requisition)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return false;
                }

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();
                if (ReqDetail.Count > 0)
                {
                    var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
                    if (po.Count == 0)
                    {
                        return false;
                    }
                }
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
                var requser = User.ReqPrefix;
                bool isAdmin = User.Administrator;
                var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                var PoUser = (from a in wdb.sp_mtReqGetRequisitionUsers() where a.UserCode == requser select a).FirstOrDefault();
                var LineStatus = (from a in wdb.sp_mtReqGetRequisitionLines(Requisition, requser, Username, Company) where a.ReqnStatus == "R" select a).ToList();
                if (header == null)
                {
                    ModelState.AddModelError("", "Megasoft: Reading replication data. Unable to find requisition header.");
                }

                if (LineStatus.Count > 0)
                {
                    if (PoUser.CanCreateOrder == "Y")
                    {
                        if (header.ReqHolder == requser)
                        {
                            return true;
                        }
                        else if (LineStatus.Count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            if (isAdmin)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    return false;
                }
                return false;



                //if (header.ReqnStatus == "Approved")
                //{
                //    if (isAdmin)
                //    {
                //        return true;
                //    }
                //    else
                //    {

                //        if (string.IsNullOrWhiteSpace(PoUser))
                //        {
                //            //No PO User defined against operator. Allow originator to create PO if Originater is current holder
                //            if (header.Holder == header.Originator)
                //            {
                //                return true;
                //            }
                //            else
                //                return false;
                //        }
                //        else
                //        {
                //            if (PoUser.UserForPorder == requser)
                //            {
                //                return true;
                //            }
                //            else
                //                return false;
                //        }
                //    }
                //}
                //else
                //    return false;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //public bool CanApprove(string Requisition)
        //{
        //    try
        //    {

        //        if (string.IsNullOrWhiteSpace(Requisition))
        //        {
        //            return false;
        //        }

        //        HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
        //        var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
        //        var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();
        //        if (ReqDetail.Count > 0)
        //        {
        //            var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
        //            if (po.Count == 0)
        //            {
        //                return false;
        //            }
        //        }

        //        string Username = HttpContext.User.Identity.Name.ToUpper();
        //        var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
        //        var requser = User.ReqPrefix.Trim();
        //        bool isAdmin = User.Administrator;
        //        var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();

        //        var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.RoutedTo == requser && a.Company == Company && a.GuidActive == "Y" select a).ToList();

        //        if (header.ReqnStatus != "R")
        //        {
        //            if (Tracking.Count > 0 && header.ReqHolder.Trim() != header.OriginatorCode.Trim())
        //            {
        //                return true;
        //            }
        //            else return false;
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public bool CanApprove(string Requisition)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return false;
                }
                var AppSettings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                //if (AppSettings.ReqNumberPadZeros == true)
                //{
                //    Requisition = Requisition.PadLeft(10, '0');
                //}
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                //var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();

                //if (ReqDetail.Count > 0)
                //{
                //    var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
                //    if (po.Count == 0)
                //    {
                //        return false;
                //    }
                //}



                string Username = HttpContext.User.Identity.Name.ToUpper();
                var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
                var requser = User.ReqPrefix.Trim();
                bool isAdmin = User.Administrator;
                var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                var LineHolderCheck = (from a in wdb.sp_mtReqGetRequisitionLines(Requisition, requser, Username, Company) select a).ToList();

                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.RoutedTo == requser && a.Company == Company && a.GuidActive == "Y" select a).ToList();
                if (header.OriginatorCode.Trim() == requser)
                {
                    return false;
                }
                if (CanCreatePo(Requisition) == true)
                {
                    return false;
                }
                if (header.ReqnStatus != "R")
                {
                    if (Tracking.Count > 0 && header.ReqHolder.Trim() != header.OriginatorCode.Trim())
                    {
                        return true;
                    }
                    else if (LineHolderCheck.Count > 0 && (header.ReqHolder.Trim() != header.OriginatorCode.Trim() || LineHolderCheck.FirstOrDefault().CurrentHolder.Trim() != header.OriginatorCode.Trim()))
                    {
                        return true;
                    }

                    //else if (LineHolderCheck.Count>0)
                    //{
                    //    if (LineHolderCheck.FirstOrDefault().ReqnStatus==" "|| LineHolderCheck.FirstOrDefault().ReqnStatus=="R")
                    //    {

                    //    }
                    //    return true;
                    //}
                    else return false;
                }

                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool RoutedToCanApprove(string Requisition, string RoutedTo)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return false;
                }

                //string Username = HttpContext.User.Identity.Name.ToUpper();

                //var User = (from a in mdb.mtUsers where a.Username == RoutedTo select a).FirstOrDefault();
                //var requser = User.ReqPrefix;

                var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.RoutedTo == RoutedTo && a.Company == Company && a.GuidActive == "Y" select a).ToList();



                if (header.ReqnStatus != "R")
                {
                    if (Tracking.Count > 0 && header.ReqHolder != header.OriginatorCode)
                    {
                        return true;
                    }
                    else return false;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CanAlternateRoute(string Requisition)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return false;
                }

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var ReqDetail = (from a in wdb.mtReqPurchaseOrders where a.Requisition == Requisition && a.Company == Company select a).ToList();
                if (ReqDetail.Count > 0)
                {
                    var po = (from a in ReqDetail where a.PurchaseOrder == "" select a).ToList();
                    if (po.Count == 0)
                    {
                        return false;
                    }
                }

                string Username = HttpContext.User.Identity.Name.ToUpper();
                var User = (from a in mdb.mtUsers where a.Username == Username select a).FirstOrDefault();
                var requser = User.ReqPrefix;
                bool isAdmin = User.Administrator;
                var header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                var ReqDetailHolder = wdb.sp_mtReqGetRequisitionLines(Requisition, requser, Username, Company).ToList();
                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.RoutedTo == requser && a.Company == Company && a.GuidActive == "Y" select a).ToList();
                if (header.ReqnStatus == "P")
                {
                    return false;
                }

                if (isAdmin)
                {
                    return true;
                }

                //if (Tracking.Count > 0 && (header.ReqHolder != header.OriginatorCode || header.ReqnStatus == "R"))
                if (ReqDetailHolder.Count > 0)
                {
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public ActionResult RequisitionRouting(string Requisition)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Routing = wdb.sp_mtReqGetRouteOnUsers(HttpContext.User.Identity.Name.ToUpper(), Company).ToList();
                var ReqHeader = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();


                RequisitionViewModel model = new RequisitionViewModel();
                model.Requisition = Requisition;
                model.RouteOn = Routing;



                return PartialView("RequisitionRouting", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("RequisitionRouting");
            }
        }

        public ActionResult RequisitionAltRouting(string Requisition)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.RouteToList = (from a in wdb.sp_mtReqGetRequisitionUsers().ToList() select new { UserCode = a.UserCode, Name = a.UserName }).ToList();

                RequisitionViewModel model = new RequisitionViewModel();
                model.Requisition = Requisition;


                return PartialView("RequisitionAltRouting", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("RequisitionAltRouting");
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RequisitionAltRouting")]
        public ActionResult RequisitionAltRouting(RequisitionViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {

                var Username = HttpContext.User.Identity.Name.ToUpper();
                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var reqheader = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();


                if (ReqName != null)
                {
                    string sysGuid = sys.SysproLogin();

                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("This is an example XML instance to demonstrate");
                    Document.Append("use of the Requisition Route To User Posting Business Object");
                    Document.Append("-->");
                    Document.Append("<PostReqRoute xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRRDOC.XSD\">");
                    Document.Append("<Item>");
                    if (reqheader.ReqHolder.Trim() != ReqName.Trim())
                    {
                        Document.Append("<User><![CDATA[" + reqheader.ReqHolder.Trim() + "]]></User>");
                    }
                    else
                    {
                        Document.Append("<User><![CDATA[" + ReqName + "]]></User>");
                    }

                    Document.Append("<UserPassword/>");
                    Document.Append("<RequisitionNumber><![CDATA[" + model.Requisition + "]]></RequisitionNumber>");
                    Document.Append("<RequisitionLine>0</RequisitionLine>");
                    Document.Append("<RouteToUser><![CDATA[" + model.RouteTo + "]]></RouteToUser>");
                    Document.Append("<RouteNotation><![CDATA[" + model.RouteNote + "]]></RouteNotation>");
                    //Document.Append("<eSignature/>");
                    Document.Append("</Item>");
                    Document.Append("</PostReqRoute>");


                    //Declaration
                    StringBuilder Parameter = new StringBuilder();

                    //Building Parameter content
                    Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                    Parameter.Append("<!--");
                    Parameter.Append("This is an example XML instance to demonstrate");
                    Parameter.Append("use of the Requisition Route To User Posting Business Object");
                    Parameter.Append("There are no parameters required");
                    Parameter.Append("-->");
                    Parameter.Append("<PostReqRoute xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRR.XSD\">");
                    Parameter.Append("<Parameters>");
                    Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                    Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                    Parameter.Append("</Parameters>");
                    Parameter.Append("</PostReqRoute>");

                    string XmlOut = sys.SysproPost(sysGuid, Parameter.ToString(), Document.ToString(), "PORTRR");
                    sys.SysproLogoff(sysGuid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        //Thread.Sleep(5000);
                        ClearActiveTracking(model.Requisition, Company);

                        Guid eGuid = Guid.NewGuid();
                        using (var edb = new MegasoftEntities())
                        {
                            mtReqRoutingTracking obj = new mtReqRoutingTracking();
                            obj.MegasoftGuid = eGuid;
                            obj.Company = Company;
                            obj.Requisition = model.Requisition;
                            obj.Originator = ReqName.Trim();
                            obj.RoutedTo = model.RouteTo.Trim();
                            obj.DateRouted = DateTime.Now;
                            obj.Username = HttpContext.User.Identity.Name.ToUpper();
                            obj.RouteNote = model.RouteNote;
                            obj.GuidActive = "Y";
                            obj.NoOfApprovals = 1;
                            obj.ProcessApiRequest = "N";
                            edb.Entry(obj).State = System.Data.EntityState.Added;
                            edb.SaveChanges();
                        }

                        SendEmail(model.Requisition, ReqName, model.RouteTo, eGuid, model.RouteNote);

                        ModelState.AddModelError("", "Requistion routed.");
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                    }


                }
                else
                {
                    ModelState.AddModelError("", "Failed to get requisition details.");
                }

                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
            catch (Exception ex)
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                ModelState.AddModelError("", ex.Message);
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
        }

        public void ClearActiveTracking(string Requisition, string Company)
        {
            try
            {
                var tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.Company == Company select a).ToList();
                foreach (var tr in tracking)
                {
                    tr.GuidActive = "N";
                    mdb.Entry(tr).State = System.Data.EntityState.Modified;
                    mdb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RequisitionRouting")]
        public ActionResult RequisitionRouting(RequisitionViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {

                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                bool OkToApprove = true;

                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == model.Requisition && a.Company == Company && a.GuidActive == "Y" select a).ToList();
                if (Tracking.Count > 0)
                {
                    if (Tracking.FirstOrDefault().NoOfApprovals <= 1)
                    {
                        OkToApprove = true;
                        foreach (var tr in Tracking)
                        {
                            tr.Approved = "Y";
                            tr.DateApproved = DateTime.Now;
                            tr.GuidActive = "N";
                            mdb.Entry(tr).State = System.Data.EntityState.Modified;
                            mdb.SaveChanges();
                        }
                    }
                    else if (Tracking.FirstOrDefault().NoOfApprovals > 1)
                    {
                        var ApprovalsOutstanding = (from a in Tracking where a.Approved == "N" select a).ToList();
                        if (ApprovalsOutstanding.Count == 1)
                        {
                            OkToApprove = true; // only 1 approval outstanding which is the current approval
                        }
                        else
                        {
                            OkToApprove = false;
                        }

                        var ItemToFlag = (from a in ApprovalsOutstanding where a.RoutedTo == Username select a).FirstOrDefault();
                        ItemToFlag.Approved = "Y";
                        ItemToFlag.DateApproved = DateTime.Now;
                        ItemToFlag.GuidActive = "N";
                        mdb.Entry(ItemToFlag).State = System.Data.EntityState.Modified;
                        mdb.SaveChanges();
                    }

                }


                if (OkToApprove)
                {
                    var reqheader = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    if (reqheader != null)
                    {
                        string sysGuid = sys.SysproLogin();

                        //Declaration
                        StringBuilder Document = new StringBuilder();

                        //Building Document content
                        Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("This is an example XML instance to demonstrate");
                        Document.Append("use of the Requisition Route To User Posting Business Object");
                        Document.Append("-->");
                        Document.Append("<PostReqRoute xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRRDOC.XSD\">");
                        Document.Append("<Item>");
                        Document.Append("<User><![CDATA[" + reqheader.OriginatorCode + "]]></User>");
                        Document.Append("<UserPassword/>");
                        Document.Append("<RequisitionNumber><![CDATA[" + model.Requisition + "]]></RequisitionNumber>");
                        Document.Append("<RequisitionLine>0</RequisitionLine>");
                        Document.Append("<RouteToUser><![CDATA[" + model.RouteOn.FirstOrDefault().UserCode + "]]></RouteToUser>");
                        Document.Append("<RouteNotation><![CDATA[" + model.RouteNote + "]]></RouteNotation>");
                        //Document.Append("<eSignature/>");
                        Document.Append("</Item>");
                        Document.Append("</PostReqRoute>");


                        //Declaration
                        StringBuilder Parameter = new StringBuilder();

                        //Building Parameter content
                        Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                        Parameter.Append("<!--");
                        Parameter.Append("This is an example XML instance to demonstrate");
                        Parameter.Append("use of the Requisition Route To User Posting Business Object");
                        Parameter.Append("There are no parameters required");
                        Parameter.Append("-->");
                        Parameter.Append("<PostReqRoute xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRR.XSD\">");
                        Parameter.Append("<Parameters>");
                        Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                        Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                        Parameter.Append("</Parameters>");
                        Parameter.Append("</PostReqRoute>");

                        string XmlOut = sys.SysproPost(sysGuid, Parameter.ToString(), Document.ToString(), "PORTRR");
                        sys.SysproLogoff(sysGuid);
                        string ErrorMessage = sys.GetXmlErrors(XmlOut);
                        if (string.IsNullOrWhiteSpace(ErrorMessage))
                        {
                            //Thread.Sleep(5000);
                            ClearActiveTracking(model.Requisition, Company);
                            using (var edb = new MegasoftEntities())
                            {


                                foreach (var item in model.RouteOn)
                                {

                                    Guid eGuid = Guid.NewGuid();
                                    mtReqRoutingTracking obj = new mtReqRoutingTracking();
                                    obj.MegasoftGuid = eGuid;
                                    obj.Company = Company;
                                    obj.Requisition = model.Requisition;
                                    obj.Originator = reqheader.OriginatorCode.Trim();
                                    obj.RoutedTo = item.UserCode.Trim();
                                    obj.DateRouted = DateTime.Now;
                                    obj.Username = Username;
                                    obj.RouteNote = model.RouteNote;
                                    obj.GuidActive = "Y";
                                    obj.NoOfApprovals = model.RouteOn.FirstOrDefault().NoOfApprovals;
                                    obj.Approved = "N";
                                    obj.ProcessApiRequest = "N";
                                    edb.Entry(obj).State = System.Data.EntityState.Added;
                                    edb.SaveChanges();

                                    SendEmail(model.Requisition, reqheader.OriginatorCode.Trim(), item.UserCode.Trim(), eGuid, model.RouteNote);

                                }
                            }


                            ModelState.AddModelError("", "Requistion routed.");
                        }
                        else
                        {

                            //Approval failed so we need last tracking to become active again
                            using (var ldb = new MegasoftEntities())
                            {
                                var LastTracking = (from a in ldb.mtReqRoutingTrackings where a.Requisition == model.Requisition && a.Company == Company && a.GuidActive == "N" select a).OrderByDescending(a => a.Id).FirstOrDefault();
                                LastTracking.GuidActive = "Y";
                                LastTracking.Approved = "N";
                                LastTracking.DateApproved = null;
                                ldb.Entry(LastTracking).State = System.Data.EntityState.Modified;
                                ldb.SaveChanges();
                            }

                            ModelState.AddModelError("", ErrorMessage);
                        }


                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to get requisition details.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Routing request denied due to invalid tracking information found.");
                }


                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
        }


        public ActionResult RequisitionApproval(string Requisition, string TrnType)
        {
            try
            {

                ViewBag.TrnType = TrnType;
                string Username = HttpContext.User.Identity.Name.ToUpper();

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Routing = wdb.sp_mtReqGetRouteOnUsers(Username, Company).ToList();

                var ReqTotal = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault().ReqnValue;

                var CostCentre = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault().CostCentre;

                var SpendLimit = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Company == Company && a.Username == Username && a.CostCentre == CostCentre select a.SpendLimit).FirstOrDefault();

                RequisitionViewModel model = new RequisitionViewModel();
                model.Requisition = Requisition;



                if (SpendLimit >= ReqTotal)
                {
                    //Approval limit met. Ok to Approve
                    model.FinalApproval = true;
                    model.RouteOn = null;
                }
                else
                {
                    //Approval limit not met. Route On
                    model.FinalApproval = false;
                    model.RouteOn = Routing;
                }




                return PartialView("RequisitionApproval", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("RequisitionRouting");
            }
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RequisitionApproval")]
        public ActionResult RequisitionApproval(RequisitionViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();

                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();

                bool OkToApprove = false;

                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == model.Requisition && a.Company == Company && a.GuidActive == "Y" select a).ToList();
                if (Tracking.Count > 0)
                {
                    if (Tracking.FirstOrDefault().NoOfApprovals == 1)
                    {
                        OkToApprove = true;
                        foreach (var tr in Tracking)
                        {
                            tr.Approved = "Y";
                            tr.DateApproved = DateTime.Now;
                            tr.GuidActive = "N";
                            mdb.Entry(tr).State = System.Data.EntityState.Modified;
                            mdb.SaveChanges();
                        }
                    }
                    else
                    {
                        var ApprovalsOutstanding = (from a in Tracking where a.Approved == "N" select a).ToList();
                        if (ApprovalsOutstanding.Count == 1)
                        {
                            OkToApprove = true; // only 1 approval outstanding which is the current approval
                        }
                        else
                        {
                            OkToApprove = false;
                        }

                        var ItemToFlag = (from a in ApprovalsOutstanding where a.RoutedTo.Trim() == ReqName select a).FirstOrDefault();
                        ItemToFlag.Approved = "Y";
                        ItemToFlag.DateApproved = DateTime.Now;
                        ItemToFlag.GuidActive = "N";
                        mdb.Entry(ItemToFlag).State = System.Data.EntityState.Modified;
                        mdb.SaveChanges();
                    }
                }




                if (OkToApprove)
                {
                    string sysGuid = sys.SysproLogin();

                    if (!string.IsNullOrWhiteSpace(ReqName))
                    {
                        //Thread.Sleep(5000);
                        string ErrorMessage = PostApprovalClear(sysGuid, ReqName, model.Requisition, "A");
                        if (string.IsNullOrWhiteSpace(ErrorMessage))
                        {
                            //Thread.Sleep(5000);
                            ModelState.AddModelError("", "Requisition approved.");
                            var ReqOriginator = (from a in wdb.sp_mtReqGetRequisitionHeader(model.Requisition) select a.OriginatorCode).FirstOrDefault();
                            //SendEmail(model.Requisition, ReqName.Trim(), ReqOriginator.Trim(), new Guid(), "Requisition Approved.");
                            //Check if notification required to send
                            var ReqHeader = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                            var LocalCurrency = (from a in mdb.mtSystemSettings where a.Id == 1 select a.LocalCurrency).FirstOrDefault();
                            //var NotificationSettings = (from a in mdb.mtDistributionSetups where a.CompanyCode == Company select a).FirstOrDefault();
                            //decimal NotificationLimitLocalCurrency = (decimal)NotificationSettings.RequisitionNotificationLimit;
                            //if (NotificationSettings.RequisitionLocalCurrency != LocalCurrency)
                            //{
                            //    NotificationLimitLocalCurrency = (decimal)NotificationSettings.RequisitionNotificationLimit * (decimal)NotificationSettings.RequisitionNotificationExchangeRate;
                            //}
                            //if (ReqHeader.ReqnValue > NotificationLimitLocalCurrency)
                            //{
                            //    //Send notification email
                            //    SendNotifcationEmail(model.Requisition, NotificationSettings.RequisitionNotificationEmailAddress);
                            //}

                            //JR - 2021-06-03 - Added tracking for Po creation  user. This will allow Po user to route to alternate user.
                            var RouteTo = (from a in wdb.sp_mtReqGetRequisitionUsers() where a.UserCode == ReqName select a.UserForPorder).FirstOrDefault();

                            ClearActiveTracking(model.Requisition, Company);

                            Guid eGuid = Guid.NewGuid();
                            using (var edb = new MegasoftEntities())
                            {
                                mtReqRoutingTracking obj = new mtReqRoutingTracking();
                                obj.MegasoftGuid = eGuid;
                                obj.Company = Company;
                                obj.Requisition = model.Requisition;
                                obj.Originator = ReqName.Trim();
                                obj.RoutedTo = RouteTo.Trim();
                                obj.DateRouted = DateTime.Now;
                                obj.Username = HttpContext.User.Identity.Name.ToUpper();
                                obj.RouteNote = "Req Approved";
                                obj.GuidActive = "Y";
                                obj.NoOfApprovals = 1;
                                obj.ProcessApiRequest = "N";
                                edb.Entry(obj).State = System.Data.EntityState.Added;
                                edb.SaveChanges();
                            }

                        }
                        else
                        {
                            //Approval failed so we need last tracking to become active again
                            using (var ldb = new MegasoftEntities())
                            {
                                var LastTracking = (from a in ldb.mtReqRoutingTrackings where a.Requisition == model.Requisition && a.Company == Company && a.GuidActive == "N" select a).OrderByDescending(a => a.Id).FirstOrDefault();
                                LastTracking.GuidActive = "Y";
                                LastTracking.Approved = "N";
                                LastTracking.DateApproved = null;
                                ldb.Entry(LastTracking).State = System.Data.EntityState.Modified;
                                ldb.SaveChanges();
                            }

                            ModelState.AddModelError("", ErrorMessage);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Requisition Username not found for " + Username);
                    }

                    sys.SysproLogoff(sysGuid);
                }
                else
                {
                    ModelState.AddModelError("", "Approval processed successfully, however further approval still required.");
                }


                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, ReqName, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);

                //if (model.Header.ApprovedBy == model.Header.Holder)
                //{
                //    return View("Create", model);
                //}
                //else
                //{

                //}

                var reqList = wdb.sp_mtReqGetRequisitionList(ReqName, Company).ToList();
                model.ReqList = reqList;
                return View("Index");


            }
            catch (Exception ex)
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                ModelState.AddModelError("", ex.Message);
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RequisitionClear")]
        public ActionResult RequisitionClear(RequisitionViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {
                string sysGuid = sys.SysproLogin();
                string Username = HttpContext.User.Identity.Name.ToUpper();


                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(ReqName))
                {

                    string ErrorMessage = PostApprovalClear(sysGuid, ReqName, model.Requisition, "C");
                    if (string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        //Thread.Sleep(5000);
                        ModelState.AddModelError("", "Requisition cleared.");
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Requisition Username not found for " + Username);
                }

                sys.SysproLogoff(sysGuid);

                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, ReqName, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);

                if (ReqName == model.Header.ReqHolder)
                {
                    return View("Create", model);
                }
                else
                {
                    var reqList = wdb.sp_mtReqGetRequisitionList(ReqName, Company).ToList();
                    model.ReqList = reqList;
                    return View("Index");
                }


            }
            catch (Exception ex)
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                ModelState.AddModelError("", ex.Message);
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
        }


        public string PostApprovalClear(string sysGuid, string ReqName, string Requisition, string TrnType)
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
                Document.Append("use of the Requisition Approve/Clear Business Object");
                Document.Append("-->");
                Document.Append("<PostReqApprove xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRADOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<User><![CDATA[" + ReqName + "]]></User>");
                Document.Append("<UserPassword />");
                Document.Append("<RequisitionNumber><![CDATA[" + Requisition + "]]></RequisitionNumber>");
                Document.Append("<RequisitionLine>0</RequisitionLine>");
                //Document.Append("<eSignature />");
                Document.Append("</Item>");
                Document.Append("</PostReqApprove>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Requisition Approve/Clear Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostReqApprove xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRA.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ActionType>" + TrnType + "</ActionType>");
                Parameter.Append("<IgnoreCancelledLines>Y</IgnoreCancelledLines>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostReqApprove>");


                string XmlOut = sys.SysproPost(sysGuid, Parameter.ToString(), Document.ToString(), "PORTRA");


                return sys.GetXmlErrors(XmlOut);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public ActionResult Comments(string Requisition)
        {
            ViewBag.Requisition = Requisition;
            return PartialView();
        }

        public JsonResult CommentsData(string Requisition)
        {
            var result = wdb.sp_mtReqGetRequisitionComments(Requisition).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveComment(string Requisition, string Comment)
        {
            if (!string.IsNullOrWhiteSpace(Requisition))
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                mtReqRequisitionComment _comm = new mtReqRequisitionComment();
                _comm.Company = Company;
                _comm.Requisition = Requisition;
                _comm.Comment = Comment;
                _comm.Username = HttpContext.User.Identity.Name.ToUpper();
                _comm.TrnDate = DateTime.Now;
                wdb.mtReqRequisitionComments.Add(_comm);
                wdb.SaveChanges();
                return Json("Saved", JsonRequestBehavior.AllowGet);
            }
            return Json("Requisition must be created first.", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostPo")]
        public ActionResult PostPo(RequisitionViewModel model)
        {
            ModelState.Clear();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {
                model.Requisition = model.Header.Requisition;

                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var LocalCurrency = (from a in mdb.mtSystemSettings where a.Id == 1 select a.LocalCurrency).FirstOrDefault();

                var reqHeader = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                if (reqHeader != null)
                {
                    var reqDetail = wdb.sp_mtReqGetRequisitionLinesForPo(model.Requisition, Company).ToList();
                    var reqCurrency = (from a in reqDetail select a.Currency).FirstOrDefault().Trim();

                    if (!string.IsNullOrWhiteSpace(UserCode))
                    {
                        foreach (var item in reqDetail)
                        {
                            var DirectExpese = wdb.mt_DirectExpenseByStockCode(item.StockCode).FirstOrDefault();
                            if (DirectExpese != null)
                            {
                                if (DirectExpese.DirectExpenseIssue == "Y")
                                {
                                    var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(item.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();
                                    var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == "DIREXP" && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();


                                    if (string.IsNullOrWhiteSpace(GlCode))
                                    {
                                        ModelState.AddModelError("", "No GL code found in Matrix for Cost Centre: DIREXP Product Cass: " + TraceableType.ProductClass);
                                        var CostCentreList = wdb.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
                                        if (!string.IsNullOrWhiteSpace(model.Requisition))
                                        {
                                            var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                                            var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                                            model.Header = header;
                                            model.Lines = detail;
                                        }
                                        ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
                                        ViewBag.BranchList = (from a in wdb.sp_mtReqGetUserBranch(Company).ToList() select new { Branch = a.Branch, Description = a.Description }).ToList();
                                        ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                                        ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                                        ViewBag.CanRoute = CanRoute(model.Requisition);
                                        ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                                        ViewBag.CanApprove = CanApprove(model.Requisition);
                                        ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                                        return View("Create", model);
                                    }
                                }
                            }

                        }
                        //Save req in Megasoft
                        foreach (var l in reqDetail)
                        {
                            var ReqCheck = (from a in wdb.mtReqPurchaseOrders where a.Requisition == model.Requisition && a.Line == l.Line select a).ToList();
                            if (ReqCheck.Count == 0)
                            {
                                mtReqPurchaseOrder obj = new mtReqPurchaseOrder();
                                obj.Company = Company;
                                obj.Requisition = model.Requisition;
                                obj.Line = (int)l.Line;
                                obj.PurchaseOrder = "";
                                wdb.Entry(obj).State = System.Data.EntityState.Added;
                                wdb.SaveChanges();

                            }
                        }

                        string sysGuid = sys.SysproLogin();

                        string PurchaseOrder = "";
                        string ReturnMessage = "";
                        var SupplierList = (from a in reqDetail select a.Supplier).Distinct().ToList();
                        foreach (var supplier in SupplierList)
                        {
                            var ReqDetailBySupplier = (from a in reqDetail where a.Supplier == supplier select a).ToList();
                            if (ReqDetailBySupplier.Count > 0)
                            {
                                for (int i = 0; i < ReqDetailBySupplier.Count; i++)
                                {
                                    //Declaration
                                    StringBuilder Document = new StringBuilder();

                                    //Building Document content
                                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                                    Document.Append("<!-- Copyright 1994-2016 SYSPRO Ltd.-->");
                                    Document.Append("<!--");
                                    Document.Append("This is an example XML instance to demonstrate");
                                    Document.Append("use of the Requisition Issues Business Object");
                                    Document.Append("-->");
                                    Document.Append("<PostRequisitionCreatePo xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTTPDOC.xsd\">");

                                    Document.Append("<Item>");
                                    Document.Append("<User>" + UserCode + "</User>");
                                    Document.Append("<OrderHeader>");
                                    if (i == 0)
                                    {
                                        Document.Append("<OrderActionType>A</OrderActionType>");
                                        Document.Append("<PurchaseOrder></PurchaseOrder>");
                                    }
                                    else
                                    {
                                        Document.Append("<OrderActionType>C</OrderActionType>");
                                        Document.Append("<PurchaseOrder>" + PurchaseOrder + "</PurchaseOrder>");
                                    }


                                    Document.Append("<ExchRateFixed/>");

                                    if (reqCurrency != LocalCurrency)
                                    {
                                        Document.Append("<ExchangeRate>" + ReqDetailBySupplier.FirstOrDefault().ExchangeRate + "</ExchangeRate>");
                                    }

                                    Document.Append("<Customer></Customer>");

                                    if (reqCurrency != LocalCurrency)
                                    {
                                        Document.Append("<TaxStatus>E</TaxStatus>");
                                    }
                                    else
                                    {
                                        Document.Append("<TaxStatus>N</TaxStatus>");
                                    }

                                    Document.Append("<PaymentTerms/>");
                                    //Document.Append("<InvoiceTerms>" + model.Line.TermsCode + "</InvoiceTerms>");
                                    Document.Append("<InvoiceTerms>" + ReqDetailBySupplier.FirstOrDefault().TermsCode + "</InvoiceTerms>");
                                    Document.Append("<CustomerPoNumber></CustomerPoNumber>");
                                    Document.Append("<ShippingInstrs></ShippingInstrs>");
                                    Document.Append("<OrderDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</OrderDate>");
                                    Document.Append("<DueDate>" + Convert.ToDateTime(ReqDetailBySupplier.LastOrDefault().DueDate).ToString("yyyy-MM-dd") + "</DueDate>");
                                    //Document.Append("<MemoDate>2016-12-04</MemoDate>");
                                    //Document.Append("<ApplyDueDateToLines>A</ApplyDueDateToLines>");
                                    //Document.Append("<MemoCode/>");
                                    Document.Append("<Buyer><![CDATA[" + ReqDetailBySupplier.FirstOrDefault().Buyer + "]]></Buyer>");

                                    Document.Append("</OrderHeader>");
                                    Document.Append("<OrderDetails>");

                                    Document.Append("<RequisitionDetail>");
                                    Document.Append("<Requisition>" + model.Requisition + "</Requisition>");
                                    Document.Append("<RequisitionLine>" + ReqDetailBySupplier[i].Line + "</RequisitionLine>");
                                    Document.Append("</RequisitionDetail>");

                                    Document.Append("</OrderDetails>");
                                    Document.Append("</Item>");
                                    Document.Append("</PostRequisitionCreatePo>");

                                    //Declaration
                                    StringBuilder Parameter = new StringBuilder();

                                    //Building Parameter content
                                    Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                                    Parameter.Append("<!-- Copyright 1994-2016 SYSPRO Ltd.-->");
                                    Parameter.Append("<!--");
                                    Parameter.Append("This is an example XML instance to demonstrate");
                                    Parameter.Append("use of the Counts in Inspection Business Object");
                                    Parameter.Append("-->");
                                    Parameter.Append("<PostRequisitionCreatePo xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTTP.XSD\">");
                                    Parameter.Append("<Parameters>");
                                    Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
                                    Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                                    Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                                    Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                                    Parameter.Append("<WarehouseForNonStk />");
                                    Parameter.Append("<IncludeCustomerInValidationCriteria>Y</IncludeCustomerInValidationCriteria>");
                                    Parameter.Append("<IncludeCustomerPOInValidationCriteria>Y</IncludeCustomerPOInValidationCriteria>");
                                    Parameter.Append("<IncludeApprovedMRPOnly>Y</IncludeApprovedMRPOnly>");
                                    Parameter.Append("<DefaultDeliveryAddress />");
                                    Parameter.Append("<IgnoreSupplierMinimums>Y</IgnoreSupplierMinimums>");
                                    Parameter.Append("<CopyCustomForms>Y</CopyCustomForms>");
                                    Parameter.Append("<ConvertQtyToAltUm>N</ConvertQtyToAltUm>");
                                    Parameter.Append("</Parameters>");
                                    Parameter.Append("</PostRequisitionCreatePo>");

                                    string XmlOut = sys.SysproPost(sysGuid, Parameter.ToString(), Document.ToString(), "PORTTP");
                                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                                    if (string.IsNullOrWhiteSpace(ErrorMessage))
                                    {
                                        PurchaseOrder = sys.GetFirstXmlValue(XmlOut, "PurchaseOrder");
                                        if (!ReturnMessage.Contains(PurchaseOrder + ";"))
                                        {
                                            ReturnMessage += PurchaseOrder + ";";
                                        }
                                        using (var pdb = new WarehouseManagementEntities(""))
                                        {
                                            var ReqNo = ReqDetailBySupplier[i].Requisition;
                                            var ReqLine = ReqDetailBySupplier[i].Line;
                                            var req = (from a in pdb.mtReqPurchaseOrders where a.Company == Company && a.Requisition == ReqNo && a.Line == ReqLine select a).FirstOrDefault();
                                            req.PurchaseOrder = PurchaseOrder;
                                            req.DateCreated = DateTime.Now;
                                            req.Username = HttpContext.User.Identity.Name.ToUpper();
                                            pdb.Entry(req).State = System.Data.EntityState.Modified;
                                            pdb.SaveChanges();
                                        }

                                    }
                                    else
                                    {
                                        ModelState.AddModelError("", "Failed to create P/O for Requisition :" + ReqDetailBySupplier[i].Requisition + "-" + ReqDetailBySupplier[i].Line + ". " + ErrorMessage);
                                    }
                                }
                            }

                            try
                            {
                                string CommentMsg = AddCommentsToPo(PurchaseOrder, ReqDetailBySupplier.FirstOrDefault().StockCode, sysGuid);
                                if (!string.IsNullOrWhiteSpace(CommentMsg))
                                {
                                    ModelState.AddModelError("", "Failed to add comments to P/O :" + PurchaseOrder + ". " + CommentMsg);
                                }

                            }
                            catch (Exception pocom)
                            {
                                ModelState.AddModelError("", "Failed to add comments to P/O :" + PurchaseOrder + ". " + pocom);
                            }

                        }

                        if (!string.IsNullOrWhiteSpace(sysGuid))
                        {
                            sys.SysproLogoff(sysGuid);
                        }

                        if (!string.IsNullOrWhiteSpace(ReturnMessage))
                        {
                            ModelState.AddModelError("", "The following P/O(s) have been created : " + ReturnMessage);
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Requisition user not found.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Requisition not found.");
                }

                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var reqList = wdb.sp_mtReqGetRequisitionList(requser, Company).ToList();
                model.ReqList = reqList;
                return View("Index", model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, UserCode, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
        }




        public string GetEmailTemplate(string Requisition, string RoutedBy, string RoutedTo, Guid RouteGuid, string Company, string RouteNote)
        {
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            var Header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
            var detail = wdb.sp_mtReqGetRequisitionLines(Requisition, RoutedTo, Username, Company).ToList();

            bool CanApprove = RoutedToCanApprove(Requisition, RoutedTo);
            var RoutedByName = (from a in wdb.sp_mtReqGetRequisitionUsers() where a.UserCode == RoutedBy select a).FirstOrDefault().UserName;

            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<!doctype html>");
            Document.Append("<html>");
            Document.Append("<head>");
            Document.Append("<meta name=\"viewport\" content=\"width=device-width\">");
            Document.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
            Document.Append("<title>Requisition powered by Megasoft</title>");
            Document.Append("<style>");
            Document.Append("/* -------------------------------------");
            Document.Append("INLINED WITH htmlemail.io/inline");
            Document.Append("------------------------------------- */");
            Document.Append("/* -------------------------------------");
            Document.Append("RESPONSIVE AND MOBILE FRIENDLY STYLES");
            Document.Append("------------------------------------- */");
            Document.Append("@media only screen and (max-width: 720px) {");
            Document.Append("table[class=body] h1 {");
            Document.Append("font-size: 28px !important;");
            Document.Append("margin-bottom: 10px !important;");
            Document.Append("}");
            Document.Append("table[class=body] p,");
            Document.Append("table[class=body] ul,");
            Document.Append("table[class=body] ol,");
            Document.Append("table[class=body] td,");
            Document.Append("table[class=body] span,");
            Document.Append("table[class=body] a {");
            Document.Append("font-size: 16px !important;");
            Document.Append("}");
            Document.Append("table[class=body] .wrapper,");
            Document.Append("table[class=body] .article {");
            Document.Append("padding: 10px !important;");
            Document.Append("}");
            Document.Append("table[class=body] .content {");
            Document.Append("padding: 0 !important;");
            Document.Append("}");
            Document.Append("table[class=body] .container {");
            Document.Append("padding: 0 !important;");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .main {");
            //Document.Append("border-left-width: 0 !important;");
            //Document.Append("border-radius: 0 !important;");
            //Document.Append("border-right-width: 0 !important;");
            Document.Append("}");
            Document.Append("table[class=body] .btn table {");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .btn a {");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .img-responsive {");
            Document.Append("height: auto !important;");
            Document.Append("max-width: 100% !important;");
            Document.Append("width: auto !important;");
            Document.Append("}");
            Document.Append("}");
            Document.Append("/* -------------------------------------");
            Document.Append("PRESERVE THESE STYLES IN THE HEAD");
            Document.Append("------------------------------------- */");
            Document.Append("@media all {");
            Document.Append(".ExternalClass {");
            Document.Append("width: 100%;");
            Document.Append("}");
            Document.Append(".ExternalClass,");
            Document.Append(".ExternalClass p,");
            Document.Append(".ExternalClass span,");
            Document.Append(".ExternalClass font,");
            Document.Append(".ExternalClass td,");
            Document.Append(".ExternalClass div {");
            Document.Append("line-height: 100%;");
            Document.Append("}");
            Document.Append(".apple-link a {");
            Document.Append("color: inherit !important;");
            Document.Append("font-family: inherit !important;");
            Document.Append("font-size: inherit !important;");
            Document.Append("font-weight: inherit !important;");
            Document.Append("line-height: inherit !important;");
            Document.Append("text-decoration: none !important;");
            Document.Append("}");
            Document.Append("#MessageViewBody a {");
            Document.Append("color: inherit;");
            Document.Append("text-decoration: none;");
            Document.Append("font-size: inherit;");
            Document.Append("font-family: inherit;");
            Document.Append("font-weight: inherit;");
            Document.Append("line-height: inherit;");
            Document.Append("}");
            Document.Append(".btn-primary table td:hover {");
            Document.Append("background-color: #34495e !important;");
            Document.Append("}");
            Document.Append(".btn-primary a:hover {");
            Document.Append("background-color: #34495e !important;");
            Document.Append("border-color: #34495e !important;");
            Document.Append("}");
            Document.Append("}");
            Document.Append("     ");
            Document.Append("     ");
            Document.Append("</style>");
            Document.Append("</head>");
            Document.Append("<body class=\"\" style=\"background-color: #f6f6f6; font-family: sans-serif; -webkit-font-smoothing: antialiased; font-size: 14px; line-height: 1.4; margin: 0; padding: 0; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #f6f6f6;\">");
            Document.Append("<tr>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">&nbsp;</td>");
            Document.Append("<td class=\"container\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; display: block; Margin: 0 auto; max-width: 780px; padding: 10px; width: 780px;\">");
            Document.Append("<div class=\"content\" style=\"box-sizing: border-box; display: block; Margin: 0 auto; max-width: 780px; padding: 10px;\">");
            Document.Append("<!-- START CENTERED WHITE CONTAINER -->");
            Document.Append("<span class=\"preheader\" style=\"color: transparent; display: none; height: 0; max-height: 0; max-width: 0; opacity: 0; overflow: hidden; mso-hide: all; visibility: hidden; width: 0;\"></span>");
            Document.Append("<table class=\"main\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background: #ffffff; border-radius: 3px;\">");
            Document.Append("<!-- START MAIN CONTENT AREA -->");
            Document.Append("<tr>");
            Document.Append("<td class=\"wrapper\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tr>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">Hi there,</p>");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">The below requisition has been routed for your attention.</p>");
            Document.Append("");
            Document.Append("                                        ");
            Document.Append("                                        <table class=\"grtable\" style=\"width:100%\">");
            Document.Append("                                          <caption style=\"font-weight:bold;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Requisition Details</caption>");
            Document.Append("                                          <thead>");
            Document.Append("                                               <tr style=\"text-align:left\">");
            Document.Append("                                                <td style=\"font-weight:bold;\">Company</td>");
            Document.Append("                                                <td>" + Company + "</td>");
            Document.Append("                                                <td style=\"font-weight:bold;\">Site</td>");
            Document.Append("                                                <td>" + Header.CostCentre + "</td>");
            Document.Append("                                              </tr>");
            Document.Append("                                          </thead>");
            Document.Append("                                          <tbody>");
            Document.Append("                                              <tr style=\"text-align:left\">");
            Document.Append("                                                <td style=\"font-weight:bold;\">Requisition</td>");
            Document.Append("                                                <td>" + Requisition + "</td>");
            Document.Append("                                                <td style=\"font-weight:bold;\"></td>");
            Document.Append("                                                <td></td>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr style=\"text-align:left\">");
            Document.Append("                                                <td style=\"font-weight:bold;\">Date: </td>");
            Document.Append("                                                <td>" + Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy") + "</td>");
            Document.Append("                                                <td style=\"font-weight:bold;\"></td>");
            Document.Append("                                                <td></td>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr style=\"text-align:left\">");
            Document.Append("                                                <td style=\"font-weight:bold;\">Supplier</td>");
            Document.Append("                                                <td>" + detail.FirstOrDefault().SupplierName + "</td>");
            Document.Append("                                                <td style=\"font-weight:bold;\">Currency</td>");
            Document.Append("                                                <td>" + detail.FirstOrDefault().Currency + "</td>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr style=\"text-align:left\">");
            Document.Append("                                                <td style=\"font-weight:bold;\">Requisition Value</td>");
            Document.Append("                                                <td>" + string.Format("{0:##,###,##0.00}", Header.ReqnValue) + "</td>");
            Document.Append("                                                <td style=\"font-weight:bold;\">Local Currency Value</td>");
            Document.Append("                                                <td>" + string.Format("{0:##,###,##0.00}", Header.ReqnValue) + "</td>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr style=\"text-align:left\">");
            Document.Append("                                                <td style=\"font-weight:bold;\">Originator</td>");
            Document.Append("                                                <td>" + Header.Originator + "</td>");
            Document.Append("                                                <td style=\"font-weight:bold;\">Routed By</td>");
            Document.Append("                                                <td>" + RoutedBy + " - " + RoutedByName + "</td>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr>");
            Document.Append("                                                <td style=\"font-weight:bold;\">Route Note</td>");
            Document.Append("                                                <td colspan=\"3\">" + RouteNote + "</td>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr>");
            Document.Append("                                              </tr>");
            Document.Append("                                              <tr>");
            Document.Append("                                                     <td colspan=\"4\">");
            Document.Append("                                                             <table class=\"grtable\" style=\"width:100%\">");
            Document.Append("                                                                   <tr>");
            Document.Append("                                                                          <th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Line</th>");
            Document.Append("                                                                          <th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">StockCode</th>");
            Document.Append("                                                                          <th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Desc</th>");
            Document.Append("                                                                          <th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Qty</th>");
            Document.Append("                                                                          <th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Uom</th>");
            Document.Append("                                                                          <th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Warehouse</th>");
            Document.Append("                                                                          <th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Price</th>");
            Document.Append("                                                                          <th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Value</th>");
            Document.Append("                                                                          <th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Due Date</th>");
            Document.Append("                                                                   </tr>");

            foreach (var item in detail)
            {
                Document.Append("                                                               <tr>");
                Document.Append("                                                                      <td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.Line + "</td>");
                Document.Append("                                                                      <td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.StockCode + "</td>");
                Document.Append("                                                                      <td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.StockDescription + "</td>");
                Document.Append("                                                                      <td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.000}", item.OrderQty) + "</td>");
                Document.Append("                                                                      <td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.OrderUom + "</td>");
                Document.Append("                                                                      <td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.Warehouse + "</td>");
                Document.Append("                                                                      <td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.00}", item.Price) + "</td>");
                Document.Append("                                                                      <td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.00}", item.OrderQty * item.Price) + "</td>");
                Document.Append("                                                                      <td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + Convert.ToDateTime(item.DueDate).ToString("dd-MM-yyyy") + "</td>");
                Document.Append("                                                               </tr>");
            }

            Document.Append("                                                            </table>");
            Document.Append("                                                     </td>");
            Document.Append("                                              </tr>");
            Document.Append("                                          </tbody>");
            Document.Append("                                        </table>");
            Document.Append("                                        ");
            Document.Append("                                        <p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">For more information click the \"View\" button below</p>");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">or click the \"Approve\" button.</p>");
            Document.Append("                                        ");
            Document.Append("                                        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; box-sizing: border-box;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");
            Document.Append("<td align=\"left\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");


            string HostUrl = Request.Url.Host;
            if (HostUrl == "localhost")
            {
                HostUrl = "localhost:52696";
            }
            string ViewUrl = @"http://" + HostUrl + "/Megasoft/Requisition/Create?Requisition=" + Requisition;
            Document.Append("                                                            <td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #3498db; border-radius: 5px; text-align: center;\"> <a href=\"" + ViewUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #3498db; border: solid 1px #3498db; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #3498db;\">View</a> </td>");
            Document.Append("                                                     <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>");
            if (CanApprove)
            {
                //string ApproveUrl = @"http://" + HostUrl + "/api/RequisitionApi/" + RouteGuid;
                string ApproveUrl = "http://192.168.0.22/MegasoftApi/api/RequisitionApi/" + RouteGuid;
                Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #5cb85c; border-radius: 5px; text-align: center;\"> <a href=\"" + ApproveUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #5cb85c; border: solid 1px #5cb85c; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #5cb85c;\">Approve</a> </td>");

            }
            Document.Append("</tr>");
            Document.Append("</tbody>");
            Document.Append("</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</tbody>");
            Document.Append("</table>");
            Document.Append("                                        ");
            Document.Append("                                        ");
            Document.Append("");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("<!-- END MAIN CONTENT AREA -->");
            Document.Append("</table>");
            Document.Append("<!-- START FOOTER -->");
            Document.Append("<div class=\"footer\" style=\"clear: both; Margin-top: 10px; text-align: center; width: 100%;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<td class=\"content-block powered-by\" style=\"font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 12px; color: #999999; text-align: center;\">");
            Document.Append("Powered by <a href=\"http://www.mega-tech.co.za\" style=\"color: #999999; font-size: 12px; text-align: center; text-decoration: none;\">Megasoft</a>.");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</div>");
            Document.Append("<!-- END FOOTER -->");
            Document.Append("<!-- END CENTERED WHITE CONTAINER -->");
            Document.Append("</div>");
            Document.Append("</td>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">&nbsp;</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</body>");
            Document.Append("</html>");






            return Document.ToString();
        }


        public ActionResult testemail()
        {
            SendEmail("0000006797", "JR", "JR", Guid.Parse("9AC82DC0-681C-4916-9EB4-D963EDE4C735"), "TEST1");
            return View();
        }


        public void SendEmail(string Requisition, string RoutedBy, string RoutedTo, Guid RouteGuid, string RouteNote)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.FriendlyName).FirstOrDefault();
                var ToUser = (from a in wdb.sp_mtReqGetRequisitionUsers() where a.UserCode == RoutedTo select a).FirstOrDefault();
                var FromAddress = (from a in mdb.mtSystemSettings where a.Id == 1 select a.FromAddress).FirstOrDefault();
                Mail objMail = new Mail();
                objMail.From = FromAddress;
                objMail.To = ToUser.Email;
                objMail.Subject = "Requisition for " + Company;
                objMail.Body = GetEmailTemplate(Requisition, RoutedBy, RoutedTo, RouteGuid, Company, RouteNote);
                //objMail.CC =


                List<string> attachments = new List<string>();
                //attachments.Add(item.AttachmentPath);
                _email.SendEmail(objMail, attachments);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SendNotifcationEmail(string Requisition, string emailaddress)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.FriendlyName).FirstOrDefault();


                Mail objMail = new Mail();
                objMail.From = "requisitions@astrapak.com";
                objMail.To = emailaddress;
                objMail.Subject = "Requisition for " + Company;
                objMail.Body = GetNotificationTemplate(Requisition, Company);
                //objMail.CC =


                List<string> attachments = new List<string>();
                //attachments.Add(item.AttachmentPath);
                _email.SendEmail(objMail, attachments);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string GetNotificationTemplate(string Requisition, string Company)
        {
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var UserCode = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            var Header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
            var detail = wdb.sp_mtReqGetRequisitionLines(Requisition, UserCode, Username, Company).ToList();


            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<!doctype html>");
            Document.Append("<html>");
            Document.Append("<head>");
            Document.Append("<meta name=\"viewport\" content=\"width=device-width\">");
            Document.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
            Document.Append("<title>Requisition powered by Megasoft</title>");
            Document.Append("<style>");
            Document.Append("/* -------------------------------------");
            Document.Append("INLINED WITH htmlemail.io/inline");
            Document.Append("------------------------------------- */");
            Document.Append("/* -------------------------------------");
            Document.Append("RESPONSIVE AND MOBILE FRIENDLY STYLES");
            Document.Append("------------------------------------- */");
            Document.Append("@media only screen and (max-width: 720px) {");
            Document.Append("table[class=body] h1 {");
            Document.Append("font-size: 28px !important;");
            Document.Append("margin-bottom: 10px !important;");
            Document.Append("}");
            Document.Append("table[class=body] p,");
            Document.Append("table[class=body] ul,");
            Document.Append("table[class=body] ol,");
            Document.Append("table[class=body] td,");
            Document.Append("table[class=body] span,");
            Document.Append("table[class=body] a {");
            Document.Append("font-size: 16px !important;");
            Document.Append("}");
            Document.Append("table[class=body] .wrapper,");
            Document.Append("table[class=body] .article {");
            Document.Append("padding: 10px !important;");
            Document.Append("}");
            Document.Append("table[class=body] .content {");
            Document.Append("padding: 0 !important;");
            Document.Append("}");
            Document.Append("table[class=body] .container {");
            Document.Append("padding: 0 !important;");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .main {");
            //Document.Append("border-left-width: 0 !important;");
            //Document.Append("border-radius: 0 !important;");
            //Document.Append("border-right-width: 0 !important;");
            Document.Append("}");
            Document.Append("table[class=body] .btn table {");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .btn a {");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .img-responsive {");
            Document.Append("height: auto !important;");
            Document.Append("max-width: 100% !important;");
            Document.Append("width: auto !important;");
            Document.Append("}");
            Document.Append("}");
            Document.Append("/* -------------------------------------");
            Document.Append("PRESERVE THESE STYLES IN THE HEAD");
            Document.Append("------------------------------------- */");
            Document.Append("@media all {");
            Document.Append(".ExternalClass {");
            Document.Append("width: 100%;");
            Document.Append("}");
            Document.Append(".ExternalClass,");
            Document.Append(".ExternalClass p,");
            Document.Append(".ExternalClass span,");
            Document.Append(".ExternalClass font,");
            Document.Append(".ExternalClass td,");
            Document.Append(".ExternalClass div {");
            Document.Append("line-height: 100%;");
            Document.Append("}");
            Document.Append(".apple-link a {");
            Document.Append("color: inherit !important;");
            Document.Append("font-family: inherit !important;");
            Document.Append("font-size: inherit !important;");
            Document.Append("font-weight: inherit !important;");
            Document.Append("line-height: inherit !important;");
            Document.Append("text-decoration: none !important;");
            Document.Append("}");
            Document.Append("#MessageViewBody a {");
            Document.Append("color: inherit;");
            Document.Append("text-decoration: none;");
            Document.Append("font-size: inherit;");
            Document.Append("font-family: inherit;");
            Document.Append("font-weight: inherit;");
            Document.Append("line-height: inherit;");
            Document.Append("}");
            Document.Append(".btn-primary table td:hover {");
            Document.Append("background-color: #34495e !important;");
            Document.Append("}");
            Document.Append(".btn-primary a:hover {");
            Document.Append("background-color: #34495e !important;");
            Document.Append("border-color: #34495e !important;");
            Document.Append("}");
            Document.Append("}");
            Document.Append("	");
            Document.Append("	");
            Document.Append("</style>");
            Document.Append("</head>");
            Document.Append("<body class=\"\" style=\"background-color: #f6f6f6; font-family: sans-serif; -webkit-font-smoothing: antialiased; font-size: 14px; line-height: 1.4; margin: 0; padding: 0; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #f6f6f6;\">");
            Document.Append("<tr>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">&nbsp;</td>");
            Document.Append("<td class=\"container\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; display: block; Margin: 0 auto; max-width: 780px; padding: 10px; width: 780px;\">");
            Document.Append("<div class=\"content\" style=\"box-sizing: border-box; display: block; Margin: 0 auto; max-width: 780px; padding: 10px;\">");
            Document.Append("<!-- START CENTERED WHITE CONTAINER -->");
            Document.Append("<span class=\"preheader\" style=\"color: transparent; display: none; height: 0; max-height: 0; max-width: 0; opacity: 0; overflow: hidden; mso-hide: all; visibility: hidden; width: 0;\"></span>");
            Document.Append("<table class=\"main\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background: #ffffff; border-radius: 3px;\">");
            Document.Append("<!-- START MAIN CONTENT AREA -->");
            Document.Append("<tr>");
            Document.Append("<td class=\"wrapper\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tr>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">Hi,</p>");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">The below requisition has been approved and a Purchase Order will be created as follows:</p>");
            Document.Append("");
            Document.Append("						");
            Document.Append("						<table class=\"grtable\" style=\"width:100%\">");
            Document.Append("						  <caption style=\"font-weight:bold;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Requisition Details</caption>");
            Document.Append("						  <thead>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Company</td>");
            Document.Append("							  <td>" + Company + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Site</td>");
            Document.Append("							  <td>" + Header.CostCentre + "</td>");
            Document.Append("							</tr>");
            Document.Append("						  </thead>");
            Document.Append("						  <tbody>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Requisition</td>");
            Document.Append("							  <td>" + Requisition + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Supplier</td>");
            Document.Append("							  <td>" + detail.FirstOrDefault().SupplierName + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Currency</td>");
            Document.Append("							  <td>" + detail.FirstOrDefault().Currency + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Requisition Value</td>");
            Document.Append("							  <td>" + string.Format("{0:##,###,##0.00}", Header.ReqnValue) + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Local Currency Value</td>");
            Document.Append("							  <td>" + string.Format("{0:##,###,##0.00}", Header.ReqnValue) + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Originator</td>");
            Document.Append("							  <td>" + Header.Originator + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\"></td>");
            Document.Append("							  <td></td>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            Document.Append("							  <td style=\"font-weight:bold;\">Route Note</td>");
            Document.Append("							  <td colspan=\"3\">Requisition Approved</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            Document.Append("								<td colspan=\"4\">");
            Document.Append("									<table class=\"grtable\" style=\"width:100%\">");
            Document.Append("										<tr>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Line</th>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">StockCode</th>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Desc</th>");
            Document.Append("											<th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Qty</th>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Uom</th>");
            Document.Append("											<th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Price</th>");
            Document.Append("											<th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Value</th>");
            Document.Append("										</tr>");

            foreach (var item in detail)
            {
                Document.Append("										<tr>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.Line + "</td>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.StockCode + "</td>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.StockDescription + "</td>");
                Document.Append("											<td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.000}", item.OrderQty) + "</td>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.OrderUom + "</td>");
                Document.Append("											<td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.00}", item.Price) + "</td>");
                Document.Append("											<td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.00}", item.OrderQty * item.Price) + "</td>");
                Document.Append("										</tr>");
            }

            Document.Append("									</table>");
            Document.Append("								</td>");
            Document.Append("							</tr>");
            Document.Append("						  </tbody>");
            Document.Append("						</table>");
            Document.Append("						");
            Document.Append("						<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">For more information click the \"View\" button below</p>");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">or click the \"Approve\" button.</p>");
            Document.Append("						");
            Document.Append("						<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; box-sizing: border-box;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");
            Document.Append("<td align=\"left\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");

            string HostUrl = Request.Url.Host;
            if (HostUrl == "localhost")
            {
                HostUrl = "localhost:52696";
            }
            string ViewUrl = @"http://" + HostUrl + "/Requisition/Create?Requisition=" + Requisition;
            Document.Append("									<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #3498db; border-radius: 5px; text-align: center;\"> <a href=\"" + ViewUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #3498db; border: solid 1px #3498db; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #3498db;\">View</a> </td>");
            Document.Append("									<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>");
            Document.Append("<td</td>");
            Document.Append("</tr>");
            Document.Append("</tbody>");
            Document.Append("</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</tbody>");
            Document.Append("</table>");
            Document.Append("						");
            Document.Append("						");
            Document.Append("");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("<!-- END MAIN CONTENT AREA -->");
            Document.Append("</table>");
            Document.Append("<!-- START FOOTER -->");
            Document.Append("<div class=\"footer\" style=\"clear: both; Margin-top: 10px; text-align: center; width: 100%;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<td class=\"content-block powered-by\" style=\"font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 12px; color: #999999; text-align: center;\">");
            Document.Append("Powered by <a href=\"http://www.mega-tech.co.za\" style=\"color: #999999; font-size: 12px; text-align: center; text-decoration: none;\">Megasoft</a>.");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</div>");
            Document.Append("<!-- END FOOTER -->");
            Document.Append("<!-- END CENTERED WHITE CONTAINER -->");
            Document.Append("</div>");
            Document.Append("</td>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">&nbsp;</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</body>");
            Document.Append("</html>");






            return Document.ToString();
        }


        public ActionResult StockCodeImage()
        {
            return PartialView();
        }

        public ActionResult ImageSearch(string StockCode)
        {
            var result = (from a in wdb.mtInvMasterImages where a.StockCode == StockCode select a).ToList();
            StockCodeImagesViewModel model = new StockCodeImagesViewModel();
            var InvMaster = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc }).FirstOrDefault();
            model.ImageList = new List<string>();
            foreach (var item in result)
            {
                model.StockCode = item.StockCode;
                string imreBase64Data = Convert.ToBase64String(item.Image);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                model.ImageList.Add(imgDataURL);
            }
            model.Description = InvMaster.Description;
            return PartialView("StockCodeImage", model);
        }


        [CustomAuthorize(Activity: "CreateRequisition")]
        [MultipleButton(Name = "action", Argument = "DeleteLine")]
        public ActionResult DeleteLine(RequisitionViewModel model)
        {
            try
            {
                string Guid = sys.SysproLogin();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var Requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                string ActionType = "";

                // RequisitionViewModel model = new RequisitionViewModel();
                //var AllLines= from a in w


                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Requisition Cancel Business Object");
                Document.Append("-->");
                Document.Append("<PostReqCancel xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRCDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<User>" + Requser + "</User>");
                Document.Append("<UserPassword/>");


                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    //Updating existing Requisition
                    Document.Append("<RequisitionNumber>" + model.Requisition + "</RequisitionNumber>");
                    ActionType = "D";

                }
                else
                {
                    ActionType = "D";
                }
                if (model.DeleteSingleOrAll == "Single")
                {
                    Document.Append("<RequisitionLine>" + model.ReqLines + "</RequisitionLine>");
                }
                else
                {
                    Document.Append("<RequisitionLine>0</RequisitionLine>");
                }
                Document.Append("<eSignature/>");
                Document.Append("</Item>");
                Document.Append("</PostReqCancel>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Requisition Cancel Posting Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostReqCancel xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRC.XSD\">");
                Parameter.Append("<Parameters/>");
                Parameter.Append("</PostReqCancel>");
                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "PORTRCDOC");

                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {

                    string Req = sys.GetFirstXmlValue(XmlOut, "Requisition");
                    if (ActionType == "D")
                    {
                        ABL.SaveMegasoftAlert("Requisition : " + Req + " Deleted in Syspro.");
                    }

                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Deleted successfully");



                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    ViewBag.CostCentreList = (from a in wdb.sp_mtReqGetUserCostCentres(Company).ToList() select new { CostCentre = a.CostCentre, Description = a.Description }).ToList();
                    ViewBag.BranchList = (from a in wdb.sp_mtReqGetUserBranch(Company).ToList() select new { Branch = a.Branch, Description = a.Description }).ToList();
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, Requser, Username, Company).ToList();
                    if (!string.IsNullOrWhiteSpace(model.Requisition))
                    {

                        model.Header = header;
                        model.Lines = detail;
                    }
                    else
                    {
                        sp_mtReqGetRequisitionHeader_Result emptyheader = new sp_mtReqGetRequisitionHeader_Result();
                        model.Header = emptyheader;
                    }
                    if (model.DeleteSingleOrAll == "All")
                    {
                        var GetLine = wdb.sp_mtReqGetLinesForCancellation(model.Requisition).ToList();
                        foreach (var item in GetLine)
                        {
                            model.mtRequisitionDeletedLine = new mtRequisitionDeletedLine();
                            model.mtRequisitionDeletedLine.Line = Convert.ToInt32(item.Line);
                            model.mtRequisitionDeletedLine.Requisition = item.Requisition;
                            model.mtRequisitionDeletedLine.DateDeleted = Convert.ToDateTime(DateTime.Now.ToString("dd-MM-yyyy"));
                            model.mtRequisitionDeletedLine.DeletedBy = Username;
                            model.mtRequisitionDeletedLine.Reason = model.Reason;
                            model.mtRequisitionDeletedLine.StockCode = item.StockCode;
                            model.mtRequisitionDeletedLine.Uom = item.PriceUom;
                            model.mtRequisitionDeletedLine.Description = item.StockDescription;
                            model.mtRequisitionDeletedLine.Quantity = item.OrderQty;
                            wdb.mtRequisitionDeletedLines.Add(model.mtRequisitionDeletedLine);
                            wdb.SaveChanges();
                        }
                    }

                    else
                    {
                        model.mtRequisitionDeletedLine = new mtRequisitionDeletedLine();
                        model.mtRequisitionDeletedLine.Line = Convert.ToInt32(model.ReqLines);
                        model.mtRequisitionDeletedLine.Requisition = model.Requisition;
                        model.mtRequisitionDeletedLine.DateDeleted = Convert.ToDateTime(DateTime.Now.ToString("dd-MM-yyyy"));
                        model.mtRequisitionDeletedLine.DeletedBy = Username;
                        model.mtRequisitionDeletedLine.Reason = model.Reason;
                        model.mtRequisitionDeletedLine.StockCode = detail.FirstOrDefault().StockCode;
                        model.mtRequisitionDeletedLine.Uom = detail.FirstOrDefault().PriceUom;
                        model.mtRequisitionDeletedLine.Description = detail.FirstOrDefault().StockDescription;
                        model.mtRequisitionDeletedLine.Quantity = detail.FirstOrDefault().OrderQty;
                        wdb.mtRequisitionDeletedLines.Add(model.mtRequisitionDeletedLine);
                        wdb.SaveChanges();
                    }


                    ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                    ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                    ViewBag.CanRoute = CanRoute(model.Requisition);
                    ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                    ViewBag.CanApprove = CanApprove(model.Requisition);
                    ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                    return View("Create", model);
                }
                else
                {
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    ViewBag.CostCentreList = (from a in wdb.sp_mtReqGetUserCostCentres(Company).ToList() select new { CostCentre = a.CostCentre, Description = a.Description }).ToList();
                    ViewBag.BranchList = (from a in wdb.sp_mtReqGetUserBranch(Company).ToList() select new { Branch = a.Branch, Description = a.Description }).ToList();

                    sys.SysproLogoff(Guid);
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, Requser, Username, Company).ToList();
                    ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                    ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                    ViewBag.CanRoute = CanRoute(model.Requisition);
                    ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                    ViewBag.CanApprove = CanApprove(model.Requisition);
                    ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                    model.Header = header;
                    model.Lines = detail;
                    ModelState.AddModelError("", "Syspro Error: " + ErrorMessage);
                    return View("Create", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }
        }




        public string AddCommentsToPo(string PurchaseOrder, string StockCode, string Guid)
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
                Document.Append("use of the Purchase Order Transaction Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostPurchaseOrders xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTOIDOC.XSD\">");
                Document.Append("<Orders>");
                Document.Append("<OrderHeader>");
                Document.Append("<OrderActionType>C</OrderActionType>");

                Document.Append("<PurchaseOrder><![CDATA[" + PurchaseOrder + "]]></PurchaseOrder>"); ;
                Document.Append("</OrderHeader>");
                Document.Append("<OrderDetails>");


                Document.Append("<CommentLine>");
                Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                Document.Append("<LineActionType>A</LineActionType>");
                Document.Append("<Comment><![CDATA[.]]></Comment>");////////   \\DESKTOP-TKO1Q82\ZSyspro
                Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                Document.Append("<DeleteAttachedCommentLines/>");
                Document.Append("<ChangeSingleCommentLine/>");
                Document.Append("</CommentLine>");

                var Narrations = (from a in wdb.InvNarrations where a.StockCode == StockCode && a.TextType == "P" select a).ToList();
                if (Narrations.Count > 0)
                {
                    foreach (var line in Narrations)
                    {
                        Document.Append("<CommentLine>");
                        Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<Comment><![CDATA[" + line.Text.Trim() + "]]></Comment>");
                        Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                        Document.Append("<DeleteAttachedCommentLines/>");
                        Document.Append("<ChangeSingleCommentLine/>");
                        Document.Append("</CommentLine>");
                    }

                }
                Document.Append("</OrderDetails>");
                Document.Append("</Orders>");
                Document.Append("</PostPurchaseOrders>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Purchase Order Transaction Posting Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostPurchaseOrders xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTOI.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<AllowNonStockItems>S</AllowNonStockItems>");
                Parameter.Append("<AllowZeroPrice>Y</AllowZeroPrice>");
                Parameter.Append("<ValidateWorkingDays>N</ValidateWorkingDays>");
                Parameter.Append("<AllowPoWhenBlanketPo>N</AllowPoWhenBlanketPo>");
                Parameter.Append("<DefaultMemoCode>S</DefaultMemoCode>");
                Parameter.Append("<FixedExchangeRate>N</FixedExchangeRate>");
                Parameter.Append("<DefaultMemoDays>12</DefaultMemoDays>");
                Parameter.Append("<AllowBlankLedgerCode>Y</AllowBlankLedgerCode>");
                Parameter.Append("<DefaultDeliveryAddress/>");
                Parameter.Append("<CalcDueDate>N</CalcDueDate>");
                Parameter.Append("<InsertDangerousGoodsText>N</InsertDangerousGoodsText>");
                Parameter.Append("<InsertAdditionalPOText>N</InsertAdditionalPOText>");
                Parameter.Append("<OutputItemforDetailLines>N</OutputItemforDetailLines>");
                Parameter.Append("<Status>1</Status>");
                Parameter.Append("<StatusInProcess/>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostPurchaseOrders>");

                if (Narrations.Count > 0)
                {
                    string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "PORTOI");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    return ErrorMessage;
                }
                return "";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public ActionResult PurchaseHistory(string StockCode)
        {
            var result = wdb.mt_GetLast5PurchasesByStockCode(StockCode).ToList();

            return PartialView(result);
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostHeaderCustomForms")]
        public ActionResult PostHeaderCustomForms(RequisitionViewModel model)
        {
            ModelState.Clear();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {
                model.Requisition = model.Header.Requisition;

                var Username = HttpContext.User.Identity.Name.ToUpper();
                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var CostCentreList = wdb.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
                ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
                ViewBag.BranchList = (from a in wdb.sp_mtReqGetUserBranch(Company).ToList() select new { Branch = a.Branch, Description = a.Description }).ToList();
                var branchCost = wdb.sp_mtReqGetRequisitionHeaderCustomForm(model.Requisition).ToList();
                //var check = (from a in branchCost where a.Branch == "" || a.CostCentre == "" select a).ToList();

                ////Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Custom Form Post Business Object");
                Document.Append("-->");
                Document.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFMDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyFields>");
                Document.Append("<Requisition><![CDATA[" + model.Requisition + "]]></Requisition>");
                Document.Append("</KeyFields>");
                Document.Append("</Key>");
                Document.Append("<Fields>");
                Document.Append("<Branch><![CDATA[" + model.Header.Branch + "]]></Branch>");
                Document.Append("<CostCentre><![CDATA[" + model.Header.CostCentre + "]]></CostCentre>");
                Document.Append("<RequestedBy1><![CDATA[" + model.Header.RequestedBy1 + "]]></RequestedBy1>");
                Document.Append("<RequestedBy2><![CDATA[" + model.Header.RequestedBy2 + "]]></RequestedBy2>");
                Document.Append("<RequestedBy3><![CDATA[" + model.Header.RequestedBy3 + "]]></RequestedBy3>");
                Document.Append("<PurchDepartment><![CDATA[" + model.Header.PurchDepartment + "]]></PurchDepartment>");
                Document.Append("<PurchaseCategory><![CDATA[" + model.Header.PurchaseCategory + "]]></PurchaseCategory>");
                Document.Append("</Fields>");
                Document.Append("</Item>");
                Document.Append("</PostCustomForm>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Parameters used in the Custom Form Post Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFM.XSD\">");
                Parameter.Append("<Parameters>");

                if (branchCost.Count == 0)
                {
                    Parameter.Append("<Function>A</Function>");
                }
                else
                {
                    Parameter.Append("<Function>U</Function>");
                }

                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostCustomForm>");


                string Guid = sys.SysproLogin();
                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "COMTFM");
                sys.SysproLogoff(Guid);
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    ModelState.AddModelError("", "Custom forms posted successfully!");
                }
                else
                {
                    ModelState.AddModelError("", ErrorMessage);
                }



                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, ReqName, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);

            }
            catch (Exception ex)
            {
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                ModelState.AddModelError("", ex.Message);
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = wdb.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = wdb.sp_mtReqGetRequisitionLines(model.Requisition, ReqName, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                ViewBag.CanChangeAddress = CanCreatePo(model.Requisition);
                ViewBag.CanCreatePo = CanCreatePo(model.Requisition);
                ViewBag.CanRoute = CanRoute(model.Requisition);
                ViewBag.CanMaintainReq = CanMaintainReq(model.Requisition);
                ViewBag.CanApprove = CanApprove(model.Requisition);
                ViewBag.CanAlternateRoute = CanAlternateRoute(model.Requisition);
                return View("Create", model);
            }
        }


        public ActionResult ReqCustomFormSearch(string FieldName)
        {
            var result = (from a in wdb.AdmFormValidations
                          where a.FormType == "POR" && a.FieldName == FieldName
                          select a).ToList();

            return PartialView(result);
        }

        public ActionResult QuantityOnHand(string StockCode)
        {
            var result = wdb.sp_mtReqGetQtyOnHandByStockCode(StockCode).ToList();
            return PartialView(result);
        }




        public ActionResult CancelReq(string Requisition, decimal Line)
        {
            RequisitionViewModel model = new RequisitionViewModel();
            model.Requisition = Requisition;
            model.ReqLines = Line;
            return PartialView(model);
        }


    }
}
