using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManExpenseIssueController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore objSyspro = new SysproCore();

        [CustomAuthorize(Activity: "ExpenseIssue")]
        public ActionResult Index(string ProgramMode = "")
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.WarehouseList = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            var Employees = wdb.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "MAINT" select new { Employee = a.Employee, Description = a.Employee }).ToList();
            var CostCentreList = (from a in mdb.mtUserDepartments where a.Company == Company && a.Username == Username select new { CostCentre = a.CostCentre, Description = a.CostCentre }).ToList();
            ViewBag.CostCentreList = CostCentreList;
            ViewBag.WorkCentreList = new List<SelectListItem>();
            ViewBag.AnalysisCode1 = new List<SelectListItem>();
            ViewBag.AnalysisCode2 = new List<SelectListItem>();
            ViewBag.AnalysisCode3 = new List<SelectListItem>();
            ViewBag.AnalysisCode4 = new List<SelectListItem>();
            ViewBag.AnalysisCode5 = new List<SelectListItem>();
            ViewBag.ProgramMode = ProgramMode;
            ExpenseIssue model = new ExpenseIssue();
            model.TransactionDate = DateTime.Now;
            return View(model);
        }

        public ActionResult GLSearch()
        {
            return PartialView();
        }

        public JsonResult GLSearchList()
        {
            var result = (from a in wdb.mt_GetGLCodes() select a).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Activity: "ExpenseIssue")]
        public ActionResult CheckWarehouseMultiBin(string details)
        {
            try
            {
                List<MultiBin> myDeserializedObjList = (List<MultiBin>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MultiBin>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse.Trim();
                    var result = (from a in wdb.vw_InvWhControl where a.Warehouse == Warehouse select a).ToList();
                    if (result.Count > 0)
                    {
                        return Json(result.FirstOrDefault().UseMultipleBins, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error. Warehouse : " + Warehouse + " not found in Syspro.", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("Error - No Data. Warehouse not found.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public class MultiBin
        {
            public string Warehouse { get; set; }
            public string Source { get; set; }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "ExpenseIssue")]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
                return Json(ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "ExpenseIssue")]
        public ActionResult ValidateStockCodeDetails(string details)
        {
            try
            {
                return Json(ValidateStockCode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]//run it
        public ActionResult GetAnalysisCode(string details)
        {
            List<ExpenseIssue> myDeserializedObjList = (List<ExpenseIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ExpenseIssue>));
            if (myDeserializedObjList.Count > 0)
            {
                foreach (var item in myDeserializedObjList)
                {
                    var StockCodeCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                    if (StockCodeCheck == null)
                    {
                        return Json("");
                    }

                    string GlCode = "";
                    var settings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                    GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == item.CostCentre && a.WorkCentre == item.WorkCentre && a.ProductClass == StockCodeCheck.ProductClass select a.GlCode).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(GlCode))
                    {
                        var AnalysisRequired = wdb.mt_GetGenAnalysisByGLCode(GlCode).ToList();
                        if (AnalysisRequired.Count > 0)
                        {
                            return Json((from a in AnalysisRequired select new { AnalysisCode = a.AnalysisCode, Description = a.Description, AnalysisType = a.AnalysisType }).ToList());
                        }
                    }
                    else
                    {
                        return Json("");
                    }
                }
            }

            return Json("");
        }


        public string ValidateBarcode(string details)
        {
            try
            {
                List<ExpenseIssue> myDeserializedObjList = (List<ExpenseIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ExpenseIssue>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var StockWarehouseCheck = wdb.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse)).FirstOrDefault();
                        if (StockWarehouseCheck == null)
                        {
                            return "StockCode not stocked in Warehouse " + item.Warehouse + "!.";
                        }
                        if (item.Quantity == 0)
                        {
                            return "Quantity cannot be zero!";
                        }


                        var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == item.CostCentre && a.WorkCentre == item.WorkCentre && a.ProductClass == StockCodeCheck.ProductClass select a.GlCode).FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(GlCode))
                        {
                            return "No GL code found in Matrix for Cost Centre: " + item.CostCentre + " WorkCentre: " + item.WorkCentre + " Product Cass: " + StockCodeCheck.ProductClass;
                        }

                        var TraceableCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(item.StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                        if (TraceableCheck != null)
                        {
                            //StockCode is Traceable -- Lot number required
                            if (string.IsNullOrEmpty(item.Lot))
                            {
                                return "StockCode is Lot Traceable. Lot number required";
                            }
                            else
                            {
                                var LotCheck = wdb.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse) && a.Lot.Equals(item.Lot)).FirstOrDefault();
                                if (LotCheck == null)
                                {
                                    return "Lot " + item.Lot + " not found for StockCode " + item.StockCode + " in Warehouse " + item.Warehouse + "!";
                                }
                                else
                                {
                                    var LotQtyCheck = wdb.LotDetails.Where(a => a.StockCode.Equals(item.StockCode)
                                                                            && a.Warehouse.Equals(item.Warehouse)
                                                                            && a.Lot.Equals(item.Lot)
                                                                          ).Select(a => a.QtyOnHand).Sum();
                                    if (LotQtyCheck < item.Quantity)
                                    {
                                        return "Quantity on hand is less than Quantity to transfer for Lot " + item.Lot + "!";
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(GlCode))
                                        {
                                            var AnalysisRequired = wdb.mt_GetGenAnalysisByGLCode(GlCode).ToList();
                                            var MaxGlAnalysisCodes = (int)AnalysisRequired.Max(a => a.AnalysisType);

                                            if (AnalysisRequired.Count > 0)
                                            {

                                                item.AnalysisRequired = "Analysis Required:" + MaxGlAnalysisCodes;
                                                return item.AnalysisRequired;
                                            }
                                            else
                                            {
                                                return "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //StockCode is not Traceable -- Check Quantity
                            var QtyCheck = wdb.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode)
                                                                          && a.Warehouse.Equals(item.Warehouse)
                                                                          ).Select(a => a.QtyOnHand).Sum();
                            if (QtyCheck < item.Quantity)
                            {
                                return "Quantity on hand is less than Quantity to transfer for StockCode " + item.StockCode + "!";
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(GlCode))
                                {
                                    var AnalysisRequired = wdb.mt_GetGenAnalysisByGLCode(GlCode).ToList();
                                    var MaxGlAnalysisCodes = (int)AnalysisRequired.Max(a => a.AnalysisType);

                                    if (AnalysisRequired.Count > 0)
                                    {
                                        item.AnalysisRequired = "Analysis Required:" + MaxGlAnalysisCodes;
                                        return item.AnalysisRequired;
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string ValidateStockCode(string details)
        {
            try
            {
                List<ExpenseIssue> myDeserializedObjList = (List<ExpenseIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ExpenseIssue>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == item.CostCentre && a.WorkCentre == item.WorkCentre && a.ProductClass == StockCodeCheck.ProductClass select a.GlCode).FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(GlCode))
                        {
                            return "No GL code found in Matrix for Cost Centre: " + item.CostCentre + " WorkCentre: " + item.WorkCentre + " Product Cass: " + StockCodeCheck.ProductClass;
                        }

                        if (!string.IsNullOrWhiteSpace(GlCode))
                        {
                            var AnalysisRequired = wdb.mt_GetGenAnalysisByGLCode(GlCode).ToList();
                            var MaxGlAnalysisCodes = (int)AnalysisRequired.Max(a => a.AnalysisType);

                            if (AnalysisRequired.Count > 0)
                            {
                                item.AnalysisRequired = "Analysis Required:" + MaxGlAnalysisCodes;
                                return item.AnalysisRequired;
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public ActionResult GetWorkCentres(string CostCentre)
        {
            try
            {
                return Json((from a in wdb.BomWorkCentres where a.CostCentre == CostCentre select new { WorkCentre = a.WorkCentre, Description = a.WorkCentre + " - " + a.Description }).ToList());
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [CustomAuthorize(Activity: "ExpenseIssue")]
        public ActionResult PostExpenseIssue(string details, DateTime TransactionDate)
        {
            try
            {
                return Json(PostSysproExpenseIssue(details, TransactionDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        public string PostSysproExpenseIssue(string details, DateTime TransactionDate)
        {
            try
            {
                List<ExpenseIssue> myDeserializedObjList = (List<ExpenseIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ExpenseIssue>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Failed to Log in to Syspro.";
                    }

                    string XmlOut, ErrorMessage;

                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Inventory Expense Issues Business Object");
                    Document.Append("-->");
                    Document.Append("<PostInvExpenseIssues xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMEDOC.XSD\">");

                    foreach (var item in myDeserializedObjList)
                    {
                        var MultiBins = (from a in wdb.vw_InvWhControl where a.Warehouse.Equals(item.Warehouse) select a.UseMultipleBins).FirstOrDefault();
                        var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(item.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();

                        Document.Append("<Item>");
                        Document.Append("<Journal/>");
                        Document.Append("<Warehouse><![CDATA[" + item.Warehouse + "]]></Warehouse>");
                        Document.Append("<StockCode><![CDATA[" + item.StockCode + "]]></StockCode>");
                        Document.Append("<Version/>");
                        Document.Append("<Release/>");
                        Document.Append("<Quantity>" + item.Quantity + "</Quantity>");
                        Document.Append("<UnitOfMeasure/>");
                        Document.Append("<Units/>");
                        Document.Append("<Pieces/>");
                        if (MultiBins == "Y")
                        {
                            Document.Append("<BinLocation><![CDATA[" + item.Bin + "]]></BinLocation>");
                        }

                        Document.Append("<FifoBucket></FifoBucket>");

                        if (TraceableType.TraceableType == "T")
                        {
                            Document.Append("<Lot><![CDATA[" + item.Lot + "]]></Lot>");
                        }
                        else
                        {
                            if (TraceableType.SerialMethod != "N")
                            {
                                Document.Append("<Serials>");
                                Document.Append("<SerialNumber><![CDATA[" + item.Lot + "]]></SerialNumber>");
                                Document.Append("<SerialQuantity>" + item.Quantity + "</SerialQuantity>");
                                //Document.Append("<SerialUnits/>");
                                //Document.Append("<SerialPieces/>");
                                //Document.Append("<SerialFifoBucket></SerialFifoBucket>");
                                //Document.Append("<SerialLocation></SerialLocation>");
                                //Document.Append("<SerialExpiryDate>2006-10-31</SerialExpiryDate>");
                                Document.Append("</Serials>");

                                //Document.Append("<SerialAllocation>");
                                //Document.Append("<FromSerialNumber><![CDATA[" + item.Lot + "]]></FromSerialNumber>");
                                //Document.Append("<ToSerialNumber><![CDATA[" + item.Lot + "]]></ToSerialNumber>");
                                //Document.Append("<SerialQuantity>" + item.Quantity + "</SerialQuantity>");
                                //Document.Append("</SerialAllocation>");
                            }
                        }

                        var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == item.CostCentre && a.WorkCentre == item.WorkCentre && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(GlCode))
                        {
                            return "No GL code found in Matrix for Cost Centre: " + item.CostCentre + " WorkCentre: " + item.WorkCentre + " Product Cass: " + TraceableType.ProductClass;
                        }

                        string Employee = item.Employee;

                        var Reference = item.WorkCentre + "-" + Employee;
                        if (!string.IsNullOrWhiteSpace(Reference))
                        {
                            if (Reference.Length > 30)
                            {
                                Reference = Reference.Substring(0, 30);
                            }
                        }
                        Document.Append("<Reference><![CDATA[" + Reference + "]]></Reference>");
                        Document.Append("<Notation><![CDATA[" + Reference + "]]></Notation>");
                        Document.Append("<LedgerCode><![CDATA[" + GlCode + "]]></LedgerCode>");
                        var UnitCost = (from a in wdb.InvWarehouses where a.StockCode == item.StockCode && a.Warehouse == item.Warehouse select a.UnitCost).FirstOrDefault();
                        decimal EntryAmount = item.Quantity * UnitCost;

                        //Document.Append("		<AnalysisEntry/>");

                        //list of analysiscodes
                        List<string> listAnalysisCodes = new List<string> { item.AnalysisCode1, item.AnalysisCode2, item.AnalysisCode3, item.AnalysisCode4, item.AnalysisCode5 };
                        int lastAnalysisCode = 0;
                        int codeCount = 0;

                        for (int i = 0; i < listAnalysisCodes.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(listAnalysisCodes[i]))
                            {
                                lastAnalysisCode = i;
                                codeCount++;
                            }
                        }

                        //add analysis line entries if codes exist
                        if (codeCount > 0)
                        {
                            if (!string.IsNullOrEmpty(item.AnalysisCode1))
                            {
                                Document.Append("		<AnalysisLineEntry>");
                                Document.Append("			<AnalysisCode>" + item.AnalysisCode1 + "</AnalysisCode>");

                                if (lastAnalysisCode == 0)
                                {
                                    Document.Append("			");
                                    Document.Append("			<EntryAmount>" + EntryAmount.ToString("0.##") + "</EntryAmount>");
                                    Document.Append("			");
                                }

                                Document.Append("		</AnalysisLineEntry>");
                            }

                            if (!string.IsNullOrEmpty(item.AnalysisCode2))
                            {
                                Document.Append("		<AnalysisLineEntry>");
                                Document.Append("			<AnalysisCode>" + item.AnalysisCode2 + "</AnalysisCode>");

                                if (lastAnalysisCode == 1)
                                {
                                    Document.Append("			");
                                    Document.Append("			<EntryAmount>" + EntryAmount.ToString("0.##") + "</EntryAmount>");
                                    Document.Append("			");
                                }

                                Document.Append("		</AnalysisLineEntry>");
                            }

                            if (!string.IsNullOrEmpty(item.AnalysisCode3))
                            {
                                Document.Append("		<AnalysisLineEntry>");
                                Document.Append("			<AnalysisCode>" + item.AnalysisCode3 + "</AnalysisCode>");

                                if (lastAnalysisCode == 2)
                                {
                                    Document.Append("			");
                                    Document.Append("			<EntryAmount>" + EntryAmount.ToString("0.##") + "</EntryAmount>");
                                    Document.Append("			");
                                }

                                Document.Append("		</AnalysisLineEntry>");
                            }

                            if (!string.IsNullOrEmpty(item.AnalysisCode4))
                            {
                                Document.Append("		<AnalysisLineEntry>");
                                Document.Append("			<AnalysisCode>" + item.AnalysisCode4 + "</AnalysisCode>");

                                if (lastAnalysisCode == 3)
                                {
                                    Document.Append("			");
                                    Document.Append("			<EntryAmount>" + EntryAmount.ToString("0.##") + "</EntryAmount>");
                                    Document.Append("			");
                                }

                                Document.Append("		</AnalysisLineEntry>");
                            }

                            if (!string.IsNullOrEmpty(item.AnalysisCode5))
                            {
                                Document.Append("		<AnalysisLineEntry>");
                                Document.Append("			<AnalysisCode>" + item.AnalysisCode5 + "</AnalysisCode>");

                                if (lastAnalysisCode == 4)
                                {
                                    Document.Append("			");
                                    Document.Append("			<EntryAmount>" + EntryAmount.ToString("0.##") + "</EntryAmount>");
                                    Document.Append("			");
                                }

                                Document.Append("		</AnalysisLineEntry>");
                            }
                        }

                        Document.Append("<PasswordForLedgerCode/>");
                        Document.Append("</Item>");
                    }

                    Document.Append("</PostInvExpenseIssues>");

                    //Declaration
                    StringBuilder Parameter = new StringBuilder();

                    //Building Parameter content
                    Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Parameter.Append("<!--");
                    Parameter.Append("Sample XML for the Inventory Expense Issues Business Object");
                    Parameter.Append("-->");
                    Parameter.Append("<PostInvExpenseIssues xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTME.XSD\">");
                    Parameter.Append("<Parameters>");
                    Parameter.Append("<TransactionDate>" + TransactionDate.ToString("yyyy-MM-dd") + "</TransactionDate>");
                    Parameter.Append("<PostingPeriod>C</PostingPeriod>");
                    Parameter.Append("<CreateLotNumber>N</CreateLotNumber>");
                    Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                    Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                    Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                    Parameter.Append("<IgnoreAnalysis>N</IgnoreAnalysis>");
                    Parameter.Append("<AskAnalysis>N</AskAnalysis>");
                    Parameter.Append("<CalledFrom/>");
                    Parameter.Append("</Parameters>");
                    Parameter.Append("</PostInvExpenseIssues>");


                    XmlOut = objSyspro.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "INVTME");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");

                        return "Posting Complete. Jnl : " + Journal;
                    }
                    else
                    {
                        return "Posting failed. Error : " + ErrorMessage;
                    }
                }
                else
                {
                    return "No data found!";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public ActionResult StockCodeSearch()
        {
            return PartialView();
        }

        public JsonResult StockCodeList(string Warehouse)
        {
            var result = (from a in wdb.InvWarehouses.AsNoTracking() join b in wdb.InvMasters on a.StockCode equals b.StockCode where a.Warehouse == Warehouse select new { MStockCode = a.StockCode, MStockDes = b.Description, MStockingUom = b.StockUom, ProductClass = b.ProductClass, WarehouseToUse = b.WarehouseToUse }).Distinct().ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [CustomAuthorize(Activity: "ExpenseIssueMatrix")]
        public ActionResult Matrix()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var result = wdb.sp_GetUserDepartments(Company, Username).ToList();
            return View("Matrix", result);
        }

        public ActionResult CostCentreMatrix(string CostCentre)
        {
            var result = (from a in wdb.sp_GetExpenseIssueMatrix() where a.CostCentre == CostCentre select a).ToList();
            var model = new ExpenseIssue();
            model.MatrixList = result;
            model.CostCentre = CostCentre;
            return View("CostCentreMatrix", model);
        }

        public ActionResult Create(string CostCentre, string WorkCentre, string ProductClass)
        {
            ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == CostCentre select new { WorkCentre = a.WorkCentre, Description = a.WorkCentre + " - " + a.Description }).ToList();
            ViewBag.ProductClassList = (from a in wdb.SalProductClassDes select new { ProductClass = a.ProductClass, Description = a.ProductClass + " - " + a.Description }).ToList();
            var model = new ExpenseIssue();

            model.CostCentre = CostCentre;
            if (string.IsNullOrWhiteSpace(WorkCentre))
            {
                model.WorkCentre = "";
            }
            else
            {
                model.WorkCentre = WorkCentre;
            }

            model.ProductClass = ProductClass;
            var result = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == CostCentre && a.WorkCentre == model.WorkCentre && a.ProductClass == ProductClass select a).FirstOrDefault();
            if (result != null)
            {
                model.GlCode = result.GlCode;
            }
            return View("Create", model);
        }

        [HttpPost]
        public ActionResult Create(ExpenseIssue model)
        {
            ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == model.CostCentre select new { WorkCentre = a.WorkCentre, Description = a.WorkCentre + " - " + a.Description }).ToList();
            ViewBag.ProductClassList = (from a in wdb.SalProductClassDes select new { ProductClass = a.ProductClass, Description = a.ProductClass + " - " + a.Description }).ToList();
            try
            {
                if (string.IsNullOrWhiteSpace(model.WorkCentre))
                {
                    model.WorkCentre = "";
                }
                var result = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == model.CostCentre && a.WorkCentre == model.WorkCentre && a.ProductClass == model.ProductClass select a).FirstOrDefault();
                if (result != null)
                {
                    result.GlCode = model.GlCode;
                    wdb.Entry(result).State = System.Data.EntityState.Modified;
                    wdb.SaveChanges();
                }
                else
                {
                    mtExpenseIssueMatrix obj = new mtExpenseIssueMatrix();
                    obj.CostCentre = model.CostCentre;
                    obj.WorkCentre = model.WorkCentre;
                    obj.ProductClass = model.ProductClass;
                    obj.GlCode = model.GlCode;
                    wdb.Entry(obj).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }
                ModelState.AddModelError("", "Saved.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create", model);
            }
        }

        public ActionResult Delete(string CostCentre, string WorkCentre, string ProductClass)
        {
            if (string.IsNullOrWhiteSpace(WorkCentre))
            {
                WorkCentre = "";
            }
            var check = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == CostCentre && a.WorkCentre == WorkCentre && a.ProductClass == ProductClass select a).FirstOrDefault();
            if (check != null)
            {
                wdb.Entry(check).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
                ModelState.AddModelError("", "Deleted.");
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete.");
            }
            var result = (from a in wdb.sp_GetExpenseIssueMatrix() where a.CostCentre == CostCentre select a).ToList();
            var model = new ExpenseIssue();
            model.MatrixList = result;
            model.CostCentre = CostCentre;
            return View("CostCentreMatrix", model);
        }

        [HttpPost]
        public ActionResult GetUom(string StockCode)
        {
            var result = (from a in wdb.InvMasters where a.StockCode == StockCode select a.StockUom).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}
