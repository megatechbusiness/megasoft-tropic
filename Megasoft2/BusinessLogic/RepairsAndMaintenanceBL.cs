using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class RepairsAndMaintenanceBL
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        Email _email = new Email();
        public string PostJobCreation(string Guid, string ScannedStockCode, decimal Quantity, string Warehouse, string Bin, string Lot, string Operator, string WorkCentre, string Notes, string CostCentre, string Company)
        {
            try
            {
                var settings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();

                string Branch = "TROPIC";
                decimal ExchangeRate = 0;
                string ActionType = "A";

                if (Warehouse == "**")
                {
                    string Requisition = "";
                    string ReqError = PostRequisition(Guid, ScannedStockCode, Quantity, Warehouse, " ", Notes, ref Requisition, Company);
                    if (string.IsNullOrWhiteSpace(ReqError))
                    {
                        string CustomFormError = PostReqCustomForm(Guid, CostCentre, Requisition, Branch, ExchangeRate, ActionType);
                        if (!string.IsNullOrWhiteSpace(CustomFormError))
                        {
                            return "Created. Job linked to Requisition :" + Requisition + ". Failed to update requisition Custom Forms.";
                        }
                        return "Created. Job linked to Requisition :" + Requisition;
                    }
                    else
                    {
                        return "Failed to create requisition. " + ReqError + ". Create requisition manually.";
                    }
                }

                string StockCode = "";
                string StockCodeMessage = PostStockCodeCreation(Guid, ScannedStockCode, ref StockCode, Warehouse);
                if (!string.IsNullOrWhiteSpace(StockCodeMessage))
                {
                    return "Failed to setup return Stock Code : " + StockCodeMessage;
                }

                string XmlOut = sys.SysproPost(Guid, this.BuildJobCreationParameter(), this.BuildJobCreationDocument(settings.RepairsAndMaintenanceJobClass, StockCode, Quantity, Warehouse), "WIPTJB");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    string JobNumber = sys.GetFirstXmlValue(XmlOut, "Job");

                    try
                    {
                        PostJobNotes(Guid, JobNumber, Notes);
                    }
                    catch (Exception)
                    {

                    }





                    //Add material allocation to Job
                    string MaterialError = PostMaterialAllocation(Guid, JobNumber, StockCode, Warehouse, Quantity);
                    if (string.IsNullOrWhiteSpace(MaterialError))
                    {
                        decimal? QuantityToExpense = 0;

                        //Changed below to do inventory receipt.

                        ////Check if we have stock
                        //if (CheckQtyOnHand(StockCode, Warehouse, Quantity, Lot, ref QuantityToExpense) == false)
                        //{

                        //}
                        //Do Inventory Receipt to bring stock in
                        string PostInventoryReceipt = PostiInventoryReceipt(Guid, Warehouse, StockCode, Quantity, Lot);
                        //Change to negative expense issue. If item traceable and qty on hand < repair qty then -expense the difference, this will bring the item into stock.
                        //if not traceable then still do the negative issue.
                        //string ExpenseIssueError = PostSysproExpenseIssue(Guid, Warehouse, StockCode, QuantityToExpense, Bin, Lot, Operator, WorkCentre, CostCentre);
                        if (!string.IsNullOrWhiteSpace(PostInventoryReceipt))
                        {
                            return "Job : " + JobNumber + " Created. Material Allocation added to Job. Failed to post Inventory Receipt. " + PostInventoryReceipt;
                        }

                        //Do Material Issue
                        string MaterialIssueError = PostMaterialIssue(Guid, JobNumber, StockCode, Warehouse, Bin, Lot, Operator, WorkCentre, Quantity);
                        if (!string.IsNullOrWhiteSpace(MaterialIssueError))
                        {
                            return "Job : " + JobNumber + " Created. Material Allocation added to Job. Failed to post Material Issue. " + MaterialIssueError;
                        }
                        else
                        {
                            string Requisition = "";
                            string ReqError = PostRequisition(Guid, StockCode, Quantity, Warehouse, JobNumber, Notes, ref Requisition, Company);
                            if (string.IsNullOrWhiteSpace(ReqError))
                            {

                                string CustomFormError = PostReqCustomForm(Guid, CostCentre, Requisition, Branch, ExchangeRate, ActionType);
                                if (!string.IsNullOrWhiteSpace(CustomFormError))
                                {
                                    return "Posted successfully. Job : " + JobNumber + " Created. Job linked to Requisition :" + Requisition + ". Failed to update requisition Custom Forms.";
                                }
                                return "Posted successfully. Job : " + JobNumber + " Created. Job linked to Requisition :" + Requisition;
                            }
                            else
                            {
                                return "Posted successfully. Job : " + JobNumber + " Created. Failed to create requisition. " + ReqError + ". Create requisition manually and link to Job.";
                            }

                        }
                    }
                    else
                    {

                        return "Job : " + JobNumber + " Created. Failed to add Material Allocation. " + MaterialError;
                    }


                }
                else
                {
                    return "Failed to create Job. " + ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                CostCentre = "MAIN";
                Branch = "TROPIC";
                ExchangeRate = 0;
                ActionType = "A";

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

        public string BuildJobCreationDocument(string JobClass, string StockCode, decimal Quantity, string Warehouse)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the WIP Job Header Maintenance Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostJob xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJBDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Job />");
                //Document.Append("<JobDescription></JobDescription>");
                Document.Append("<JobClassification><![CDATA[" + JobClass + "]]></JobClassification>");
                Document.Append("<Priority></Priority>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Version />");
                Document.Append("<Release />");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                Document.Append("<QtyToMake>" + Quantity + "</QtyToMake>");
                Document.Append("<GrossQuantityFlag>N</GrossQuantityFlag>");
                Document.Append("<Customer></Customer>");
                Document.Append("<CustomerName />");
                Document.Append("<JobTenderDate></JobTenderDate>");
                Document.Append("<DateCalcMethod>M</DateCalcMethod>");
                Document.Append("<IncludeOvertimePct>N</IncludeOvertimePct>");
                Document.Append("<SetMethodToManual>N</SetMethodToManual>");
                Document.Append("<JobStartDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</JobStartDate>");
                Document.Append("<JobDeliveryDate>" + DateTime.Now.AddDays(7).ToString("yyyy-MM-dd") + "</JobDeliveryDate>");
                Document.Append("<SeqCheckReq>N</SeqCheckReq>");
                Document.Append("<ConfirmedFlag>Y</ConfirmedFlag>");
                Document.Append("<HoldFlag>N</HoldFlag>");
                Document.Append("<Route>0</Route>");
                //Document.Append("<RoutePassword />");
                //Document.Append("<WipCtlGlCode />");
                //Document.Append("<NonStocked>N</NonStocked>");
                //Document.Append("<StockDescription />");
                //Document.Append("<ExpLabour />");
                //Document.Append("<ExpMaterial />");
                //Document.Append("<SellingPrice />");
                //Document.Append("<AddLabPct />");
                //Document.Append("<AddMatPct />");
                //Document.Append("<ProfitPct />");
                //Document.Append("<TraceableType>N</TraceableType>");
                //Document.Append("<HierarchyFlag>N</HierarchyFlag>");
                //Document.Append("<HierarchyContract />");
                //Document.Append("<HierarchyCode />");
                //Document.Append("<SalesOrder />");
                //Document.Append("<SalesOrderLine />");
                //Document.Append("<IncludeAllocations>Y</IncludeAllocations>");
                //Document.Append("<ComponentWarehouse />");
                //Document.Append("<SubJobOptions>");
                //Document.Append("<CreateSubJobs>N</CreateSubJobs>");
                //Document.Append("<SubJobPrefix />");
                //Document.Append("<SubJobSuffix />");
                //Document.Append("<UseGlForAllJobs>N</UseGlForAllJobs>");
                //Document.Append("</SubJobOptions>");
                //Document.Append("<CheckLeadTime>Y</CheckLeadTime>");
                //Document.Append("<ReCalculateJobDates>Y</ReCalculateJobDates>");
                //Document.Append("<ChangeSalesOrderWarehouse />");
                //Document.Append("<CopyMaterial>N</CopyMaterial>");
                //Document.Append("<CopyLabor>N</CopyLabor>");
                //Document.Append("<CopyStockCode />");
                //Document.Append("<CopyVer />");
                //Document.Append("<CopyRel />");
                //Document.Append("<CopyFromJob>");
                //Document.Append("<ArchivedJob>N</ArchivedJob>");
                //Document.Append("<ArchiveNumber />");
                //Document.Append("<FromJob></FromJob>");
                //Document.Append("<PlannedOrBuilt>P</PlannedOrBuilt>");
                //Document.Append("<IncludeMaterialAllocs>N</IncludeMaterialAllocs>");
                //Document.Append("<IncludeLaborAllocs>N</IncludeLaborAllocs>");
                //Document.Append("<IncludeNotepad>N</IncludeNotepad>");
                //Document.Append("<IncludeCustomForms>N</IncludeCustomForms>");
                //Document.Append("<IncludeNarrations>N</IncludeNarrations>");
                //Document.Append("<IncludeMultimedia>N</IncludeMultimedia>");
                //Document.Append("</CopyFromJob>");
                //Document.Append("<CoProducts>");
                //Document.Append("<CoProduct>");
                //Document.Append("<CoProductLine>1</CoProductLine>");
                //Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                //Document.Append("<QtyToMake />");
                //Document.Append("<QtyPerNotional />");
                //Document.Append("<ExcludeCoProduct>N</ExcludeCoProduct>");
                //Document.Append("</CoProduct>");
                //Document.Append("</CoProducts>");
                //Document.Append("<AutomaticallyReserveAllocs>Y</AutomaticallyReserveAllocs>");
                //Document.Append("<eSignature>{36303032-3330-3031-3038-323434363433}</eSignature>");
                Document.Append("</Item>");
                Document.Append("</PostJob>");

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildJobCreationParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the WIP Job Header Maintenance Posting Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostJob xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJB.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<ActionType>A</ActionType>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<Snapshot>N</Snapshot>");
            Parameter.Append("<TriggerAps>Y</TriggerAps>");
            Parameter.Append("<UseParentWhRoute>N</UseParentWhRoute>");
            Parameter.Append("<UseParentCompWh>N</UseParentCompWh>");
            Parameter.Append("<StatusInProcess>N</StatusInProcess>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostJob>");

            return Parameter.ToString();
        }



        public string PostMaterialAllocation(string Guid, string Job, string StockCode, string Warehouse, decimal Quantity)
        {

            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the WIP Material Allocations Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostMaterialAllocations xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJMDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Job><![CDATA[" + Job + "]]></Job>");
                Document.Append("<NonStocked>N</NonStocked>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("<QtyReqdType>U</QtyReqdType>");
                Document.Append("<QtyReqd><![CDATA[0]]></QtyReqd>");
                Document.Append("<FixedQtyPerFlag>N</FixedQtyPerFlag>");
                //Document.Append("<FixedQtyPer><![CDATA[" + Quantity + "]]></FixedQtyPer>");
                //Document.Append("<UnitCost>         1.00000</UnitCost>");   
                Document.Append("<OperationOffset>0001</OperationOffset>");
                Document.Append("<OpOffsetFlag>O</OpOffsetFlag>");
                Document.Append("</Item>");
                Document.Append("</PostMaterialAllocations>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the WIP Material Allocations Posting Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostMaterialAllocations xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJM.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<Snapshot>N</Snapshot>");
                Parameter.Append("<ActionType>A</ActionType>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostMaterialAllocations>");

                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "WIPTJM");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);

                return ErrorMessage;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }



        public bool CheckQtyOnHand(string StockCode, string Warehouse, decimal? Quantity, string Lot, ref decimal? QtyToExpense)
        {
            try
            {
                var TraceableCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                if (TraceableCheck != null)
                {
                    using (WarehouseManagementEntities ndb = new WarehouseManagementEntities(""))
                    {
                        var LotQtyCheck = ndb.LotDetails.Where(a => a.StockCode.Equals(StockCode)
                                                                            && a.Warehouse.Equals(Warehouse)
                                                                            && a.Lot.Equals(Lot)
                                                                          ).Select(a => (decimal?)a.QtyOnHand).Sum();

                        if (LotQtyCheck < Quantity)
                        {
                            QtyToExpense = (Quantity - LotQtyCheck) * -1;
                            return false;
                        }
                        else
                        {
                            QtyToExpense = Quantity * -1;
                            return true;
                        }
                    }
                }
                else
                {
                    //var QtyCheck = wdb.InvWarehouses.Where(a => a.StockCode.Equals(StockCode)
                    //                                                      && a.Warehouse.Equals(Warehouse)
                    //                                                      ).Select(a => a.QtyOnHand).Sum();
                    //if (QtyCheck < Quantity)
                    //{
                    //    return false;
                    //}
                    //else
                    //{
                    //    return true;
                    //}
                    QtyToExpense = Quantity * -1;
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string PostInventoryReceipt(string Guid, string Job, string StockCode, string Warehouse, string Bin, string Operator, string WorkCentre, decimal Quantity)
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
                Document.Append("<Item>");
                Document.Append("<Journal />");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Quantity><![CDATA[" + Quantity + "]]></Quantity>");

                var TraceableCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                if (TraceableCheck != null)
                {
                    Document.Append("<Lot></Lot>");
                }

                if (!string.IsNullOrWhiteSpace(Bin))
                {
                    Document.Append("<Bins>");
                    Document.Append("<BinLocation><![CDATA[" + Bin + "]]></BinLocation>");
                    Document.Append("<BinQuantity><![CDATA[" + Quantity + "]]></BinQuantity>");
                    Document.Append("<BinUnits />");
                    Document.Append("<BinPieces />");
                    Document.Append("</Bins>");
                }

                Document.Append("<Reference><![CDATA[" + Job + "-" + WorkCentre + "]]></Reference>");
                Document.Append("<Notation><![CDATA[" + Job + " - " + WorkCentre + "]]></Notation>");
                Document.Append("</Item>");
                Document.Append("</PostInvReceipts>");


                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Inventory Receipts Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostInvReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMR.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<ManualSerialTransfersAllowed>N</ManualSerialTransfersAllowed>");
                Parameter.Append("<ReturnDetailedReceipt>N</ReturnDetailedReceipt>");
                Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostInvReceipts>");

                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "INVTMR");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);

                return ErrorMessage;


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string PostMaterialIssue(string Guid, string Job, string StockCode, string Warehouse, string Bin, string Lot, string Operator, string WorkCentre, decimal Quantity)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                var newName = Operator.Split('-');
                string prefix = newName[1].Substring(0, 1);

                var splitBySpace = newName[1].Split(' ');

                List<string> x = new List<string>();

                foreach (var item in splitBySpace)
                {
                    if (!item.Contains('('))
                    {
                        x.Add(item);
                    }
                }

                var newOperator = "";

                if (x.Count == 1)
                {
                    newOperator = x[0];
                }

                else
                {
                    newOperator = prefix + ". " + x[x.Count - 1];
                }


                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Post Material Business Object");
                Document.Append("-->");
                Document.Append("<PostMaterial xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTMIDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Journal />");
                Document.Append("<Job><![CDATA[" + Job + "]]></Job>");
                Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Line>00</Line>");
                Document.Append("<QtyIssued><![CDATA[" + Quantity + "]]></QtyIssued>");
                Document.Append("<Reference><![CDATA[" + WorkCentre + "-" + newOperator + "]]></Reference>");
                Document.Append("<Notation><![CDATA[" + WorkCentre + "-" + Operator + "]]></Notation>");
                //Document.Append("<LedgerCode>00-4530</LedgerCode>");
                //Document.Append("<PasswordForLedgerCode />");
                //Document.Append("<ProductClass />");
                //Document.Append("<UnitCost />");
                //Document.Append("<AllocCompleted>N</AllocCompleted>");
                //Document.Append("<FifoBucket />");
                var MultiBins = (from a in wdb.vw_InvWhControl where a.Warehouse.Equals(Warehouse) select a.UseMultipleBins).FirstOrDefault();

                if (MultiBins == "Y")
                {
                    Document.Append("<Bins>");
                    Document.Append("<BinLocation><![CDATA[" + Bin + "]]></BinLocation>");
                    Document.Append("<BinQuantity><![CDATA[" + Quantity + "]]></BinQuantity>");
                    Document.Append("</Bins>");
                }


                var TraceableCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                if (TraceableCheck != null)
                {
                    Document.Append("<Lot><![CDATA[" + Lot + "]]></Lot>");
                    Document.Append("<LotConcession />");
                }
                //Document.Append("<Serials>");
                //Document.Append("<SerialNumber>0205</SerialNumber>");
                //Document.Append("<SerialQuantity>1</SerialQuantity>");
                //Document.Append("<SerialFifoBucket />");
                //Document.Append("</Serials>");
                //Document.Append("<SerialAllocation>");
                //Document.Append("<FromSerialNumber>0206</FromSerialNumber>");
                //Document.Append("<ToSerialNumber>0214</ToSerialNumber>");
                //Document.Append("<SerialQuantity>9</SerialQuantity>");
                //Document.Append("</SerialAllocation>");
                //Document.Append("<MaterialReference />");
                //Document.Append("<CoProductLine />");
                //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
                //Document.Append("<Version>");
                //Document.Append("</Version>");
                //Document.Append("<Release>");
                //Document.Append("</Release>");
                Document.Append("</Item>");
                Document.Append("</PostMaterial>");


                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the parameters for the Post Material Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostMaterial xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTMI.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
                Parameter.Append("<PostingPeriod>C</PostingPeriod>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<AutoDepleteLotsBins>N</AutoDepleteLotsBins>");
                Parameter.Append("<PostFloorstock>N</PostFloorstock>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostMaterial>");

                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "WIPTMI");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);

                return ErrorMessage;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string PostRequisition(string Guid, string StockCode, decimal Quantity, string Warehouse, string Job, string Notes, ref string Requisition, string Company)
        {
            try
            {
                string Username = HttpContext.Current.User.Identity.Name.ToUpper();
                var Requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var StockDesc = (from a in wdb.InvMasters where a.StockCode == StockCode select a.Description).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(StockDesc))
                {
                    StockDesc = StockCode;
                }
                else
                {
                    StockDesc = StockCode + "-" + StockDesc;
                }


                if (StockDesc.Length > 50)
                {
                    StockDesc = StockDesc.Substring(0, 49);
                }

                //Declaration Xml 
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

                Document.Append("<RequisitionLine>0</RequisitionLine>");
                Document.Append("<StockCode><![CDATA[Repair]]></StockCode>");
                Document.Append("<Description><![CDATA[" + StockDesc + "]]></Description>");
                Document.Append("<Quantity><![CDATA[" + Quantity + "]]></Quantity>");
                Document.Append("<Reason><![CDATA[" + Notes + "]]></Reason>");
                Document.Append("<UnitOfMeasure>EA</UnitOfMeasure>");
                Document.Append("<Warehouse><![CDATA[**]]></Warehouse>");
                Document.Append("<Price><![CDATA[0]]></Price>");
                Document.Append("<Job><![CDATA[" + Job + "]]></Job>");
                Document.Append("<TaxCode>J</TaxCode>");
                Document.Append("<ProductClass>0007</ProductClass>");
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
                Parameter.Append("<ActionType>A</ActionType>");
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
                    ReqRouting(Guid, Requisition, Company, Username);
                }

                return ErrorMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string ReqRouting(string sysGuid, string Requisition, string Company, string Username)
        {
            var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.Company == Company && a.GuidActive == "Y" select a).ToList();
            var reqheader = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
            var code = (from a in wdb.sp_mtReqGetRouteOnUsers(Username, Company, reqheader.PurchaseCategory, reqheader.CostCentre)
                        where a.Username == Username
                        select a).ToList();
            if (reqheader != null)
            {


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
                Document.Append("<RequisitionNumber><![CDATA[" + Requisition + "]]></RequisitionNumber>");
                Document.Append("<RequisitionLine>0</RequisitionLine>");
                Document.Append("<RouteToUser><![CDATA[" + code.FirstOrDefault().UserCode + "]]></RouteToUser>");
                Document.Append("<RouteNotation><![CDATA[Please Approve]]></RouteNotation>");
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

                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {

                    ClearActiveTracking(Requisition, Company);
                    using (var edb = new MegasoftEntities())
                    {
                        List<sp_mtReqGetRouteOnUsers_Result> RoutOn = new List<sp_mtReqGetRouteOnUsers_Result>();
                        RoutOn = code;
                        foreach (var item in RoutOn)
                        {

                            Guid eGuid = Guid.NewGuid();
                            mtReqRoutingTracking obj = new mtReqRoutingTracking();
                            obj.MegasoftGuid = eGuid;
                            obj.Company = Company;
                            obj.Requisition = Requisition;
                            obj.Originator = reqheader.OriginatorCode.Trim();
                            obj.RoutedTo = item.UserCode.Trim();
                            obj.DateRouted = DateTime.Now;
                            obj.Username = Username;
                            obj.RouteNote = "Please Approve";
                            obj.GuidActive = "Y";
                            obj.NoOfApprovals = item.NoOfApprovals;
                            obj.Approved = "N";
                            obj.ProcessApiRequest = "N";
                            edb.Entry(obj).State = System.Data.EntityState.Added;
                            edb.SaveChanges();

                            SendEmail(Requisition, reqheader.OriginatorCode.Trim(), item.UserCode.Trim(), eGuid, Company);

                        }
                    }


                    return "Requistion routed.";
                }
                else
                {
                    string GuidActive = Tracking.FirstOrDefault().GuidActive;
                    //Approval failed so we need last tracking to become active again
                    using (var ldb = new MegasoftEntities())
                    {
                        var LastTracking = (from a in ldb.mtReqRoutingTrackings where a.Requisition == Requisition && a.Company == Company && a.GuidActive == "N" select a).OrderByDescending(a => a.Id).FirstOrDefault();
                        LastTracking.GuidActive = "Y";
                        LastTracking.Approved = "N";
                        LastTracking.DateApproved = null;
                        ldb.Entry(LastTracking).State = System.Data.EntityState.Modified;
                        ldb.SaveChanges();
                    }

                    return ErrorMessage;
                }


            }
            else
            {
                return "Failed to get requisition details.";
            }
        }

        public void SendEmail(string Requisition, string RoutedBy, string RoutedTo, Guid RouteGuid, string Company)
        {
            try
            {
                //HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                //var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.FriendlyName).FirstOrDefault();
                var ToUser = (from a in wdb.sp_mtReqGetRequisitionUsers() where a.UserCode == RoutedTo select a).FirstOrDefault();
                //var FromAddress = (from a in mdb.mtSystemSettings where a.Id == 1 select a.FromAddress).FirstOrDefault();
                var FromAddress = (from a in mdb.mtEmailSettings where a.EmailProgram == "RequisitionSystem" select a.FromAddress).FirstOrDefault();
                Mail objMail = new Mail();
                objMail.From = FromAddress;
                objMail.To = ToUser.Email;
                objMail.Subject = "Requisition for " + Company;
                objMail.Body = GetEmailTemplate(Requisition, RoutedBy, RoutedTo, RouteGuid, Company);

                List<string> attachments = new List<string>();
                _email.SendEmail(objMail, attachments, "RequisitionSystem");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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

        public string PostJobNotes(string Guid, string Job, string Notes)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the BOM Narration Setup Business Object");
                Document.Append("-->");
                Document.Append("<SetupBomNarration xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSSNDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<Source>JOB</Source>");
                Document.Append("<JobNumber><![CDATA[" + Job + "]]></JobNumber>");
                Document.Append("</Key>");
                Document.Append("<Narration><![CDATA[" + Notes + "]]></Narration>");
                Document.Append("</Item>");
                Document.Append("</SetupBomNarration>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the BOM Narration Setup Business Object");
                Parameter.Append("-->");
                Parameter.Append("<SetupBomNarration xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSSN.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</SetupBomNarration>");

                string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "BOMSSN");

                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                return ErrorMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }




        public string PostSysproExpenseIssue(string Guid, string Warehouse, string StockCode, decimal Quantity, string Bin, string Lot, string Employee, string WorkCentre, string CostCentre)
        {
            try
            {

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


                var MultiBins = (from a in wdb.vw_InvWhControl where a.Warehouse.Equals(Warehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();

                //var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == CostCentre && a.WorkCentre == WorkCentre && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();


                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Version/>");
                Document.Append("<Release/>");
                Document.Append("<Quantity>" + Quantity + "</Quantity>");
                Document.Append("<UnitOfMeasure/>");
                Document.Append("<Units/>");
                Document.Append("<Pieces/>");
                if (MultiBins == "Y")
                {
                    Document.Append("<BinLocation><![CDATA[" + Bin + "]]></BinLocation>");
                }

                Document.Append("<FifoBucket></FifoBucket>");

                if (TraceableType.TraceableType == "T")
                {
                    Document.Append("<Lot><![CDATA[" + Lot + "]]></Lot>");
                }
                else
                {
                    if (TraceableType.SerialMethod != "N")
                    {
                        Document.Append("<Serials>");
                        Document.Append("<SerialNumber><![CDATA[" + Lot + "]]></SerialNumber>");
                        Document.Append("<SerialQuantity>" + Quantity + "</SerialQuantity>");
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




                //string Employee = Employee;
                //string EmployeeText = item.Employee;
                //int index = EmployeeText.IndexOf(" -- ");
                //if (index == 0)
                //{
                //    Employee = item.Employee.Substring(0, 30);
                //}
                //else
                //{
                //    Employee = item.Employee.Substring(0, index);
                //}


                var Reference = WorkCentre + "-" + Employee;
                if (!string.IsNullOrWhiteSpace(Reference))
                {
                    if (Reference.Length > 30)
                    {
                        Reference = Reference.Substring(0, 30);
                    }
                }
                Document.Append("<Reference><![CDATA[" + Reference + "]]></Reference>");
                Document.Append("<Notation><![CDATA[" + Reference + "]]></Notation>");
                //Document.Append("<LedgerCode><![CDATA[" + GlCode + "]]></LedgerCode>");
                Document.Append("<PasswordForLedgerCode/>");
                Document.Append("</Item>");


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
                Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
                Parameter.Append("<PostingPeriod>C</PostingPeriod>");
                Parameter.Append("<CreateLotNumber>N</CreateLotNumber>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
                Parameter.Append("<AskAnalysis>N</AskAnalysis>");
                Parameter.Append("<CalledFrom/>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostInvExpenseIssues>");


                XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "INVTME");
                ErrorMessage = sys.GetXmlErrors(XmlOut);
                return ErrorMessage;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string PostStockCodeCreation(string Guid, string FromStockCode, ref string StockCode, string Warehouse)
        {
            try
            {
                var SuffixCheck = FromStockCode.Substring(FromStockCode.Length - 3);
                if (SuffixCheck == "-SR")
                {
                    StockCode = FromStockCode;

                    string InvWhseMsg = PostStockCodeWarehouse(Guid, StockCode, Warehouse);
                    return InvWhseMsg;
                }
                else
                {
                    //check if stockcode exists
                    StockCode = FromStockCode + "-SR";
                    string nStockCode = StockCode;
                    var StockCodeCheck = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == nStockCode select a).FirstOrDefault();
                    if (StockCodeCheck != null)
                    {
                        string InvWhseMsg = PostStockCodeWarehouse(Guid, StockCode, Warehouse);
                        return InvWhseMsg;
                    }
                    else
                    {
                        StockCodeCheck = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == FromStockCode select a).FirstOrDefault();
                        string Desc = StockCodeCheck.Description + " (Repair)";
                        if (Desc.Length > 50)
                        {
                            Desc = Desc.Substring(0, 50);
                        }

                        string LongDesc = StockCodeCheck.LongDesc + " (Repair)";
                        if (LongDesc.Length > 100)
                        {
                            LongDesc = LongDesc.Substring(0, 100);
                        }


                        //Create StockCode
                        //Declaration
                        StringBuilder Document = new StringBuilder();
                        //Building Document content
                        Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("Sample XML for the Stock Code Setup Business Object");
                        Document.Append("-->");
                        Document.Append("<SetupInvMaster xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSSTDOC.XSD\">");
                        Document.Append("<Item>");
                        Document.Append("<Key>");
                        Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");//
                        Document.Append("</Key>");
                        Document.Append("<Description><![CDATA[" + Desc + "]]></Description>");//
                        Document.Append("<LongDesc><![CDATA[" + LongDesc + "]]></LongDesc>");
                        Document.Append("<AlternateKey1><![CDATA[" + StockCodeCheck.AlternateKey1.ToString().Trim() + "]]></AlternateKey1>");
                        Document.Append("<AlternateKey2><![CDATA[" + StockCodeCheck.AlternateKey2.ToString().Trim() + "]]></AlternateKey2>");
                        Document.Append("<EccUser/>");
                        Document.Append("<StockUom><![CDATA[" + StockCodeCheck.StockUom.ToString().Trim() + "]]></StockUom>");//
                        Document.Append("<AlternateUom><![CDATA[" + StockCodeCheck.AlternateUom.ToString().Trim() + "]]></AlternateUom>");//
                        Document.Append("<OtherUom><![CDATA[" + StockCodeCheck.OtherUom.ToString().Trim() + "]]></OtherUom>");//
                        Document.Append("<ConvFactAltUom><![CDATA[" + StockCodeCheck.ConvFactAltUom.ToString().Trim() + "]]></ConvFactAltUom>");//
                        Document.Append("<ConvMulDiv><![CDATA[" + StockCodeCheck.ConvMulDiv.ToString().Trim() + "]]></ConvMulDiv>");//
                        Document.Append("<ConvFactOthUom><![CDATA[" + StockCodeCheck.ConvFactOthUom.ToString().Trim() + "]]></ConvFactOthUom>");//
                        Document.Append("<MulDiv><![CDATA[" + StockCodeCheck.MulDiv.ToString().Trim() + "]]></MulDiv>");//
                        Document.Append("<Mass><![CDATA[" + StockCodeCheck.Mass.ToString().Trim() + "]]></Mass>");//
                        Document.Append("<Volume><![CDATA[" + StockCodeCheck.Volume.ToString().Trim() + "]]></Volume>");//
                        Document.Append("<Decimals><![CDATA[" + StockCodeCheck.Decimals.ToString().Trim() + "]]></Decimals>");//
                        Document.Append("<PriceCategory><![CDATA[" + StockCodeCheck.PriceCategory.ToString().Trim() + "]]></PriceCategory>");//
                        Document.Append("<PriceMethod><![CDATA[" + StockCodeCheck.PriceMethod.ToString().Trim() + "]]></PriceMethod>");//
                        //Document.Append("<ReturnableItem>N</ReturnableItem>");
                        Document.Append("<Supplier><![CDATA[" + StockCodeCheck.Supplier.ToString().Trim() + "]]></Supplier>");//
                        Document.Append("<CycleCount>0</CycleCount>");
                        Document.Append("<ProductClass><![CDATA[" + StockCodeCheck.ProductClass.ToString().Trim() + "]]></ProductClass>");//
                        Document.Append("<TaxCode><![CDATA[" + StockCodeCheck.TaxCode.ToString().Trim() + "]]></TaxCode>");//
                        Document.Append("<OtherTaxCode><![CDATA[" + StockCodeCheck.OtherTaxCode.ToString().Trim() + "]]></OtherTaxCode>");//
                        Document.Append("<AddListPrice>Y</AddListPrice>");
                        Document.Append("<ListPriceCode><![CDATA[" + StockCodeCheck.ListPriceCode.ToString().Trim() + "]]></ListPriceCode>");//
                        //Document.Append("<SellingPrice>" + StockCodeCheck.ToString().Trim() + "</SellingPrice>");//INVPRICE JOIN ON LIST PRICE
                        //Document.Append("<PriceBasis>" + StockCodeCheck.PriceBasis.ToString().Trim() + "</PriceBasis>");//INVPRICE
                        Document.Append("<CommissionCode>0</CommissionCode>");
                        Document.Append("<SerialMethod>N</SerialMethod>");
                        Document.Append("<SerialPrefix/>");
                        Document.Append("<SerialSuffix/>");
                        Document.Append("<KitType>N</KitType>");
                        Document.Append("<Buyer><![CDATA[" + StockCodeCheck.Buyer.ToString().Trim() + "]]></Buyer>");//
                        Document.Append("<Planner><![CDATA[" + StockCodeCheck.Planner.ToString().Trim() + "]]></Planner>");//
                        Document.Append("<TraceableType><![CDATA[" + StockCodeCheck.TraceableType.ToString().Trim() + "]]></TraceableType>");//
                        Document.Append("<InspectionFlag><![CDATA[" + StockCodeCheck.InspectionFlag.ToString().Trim() + "]]></InspectionFlag>");//
                        Document.Append("<MpsFlag><![CDATA[" + StockCodeCheck.MpsFlag.ToString().Trim() + "]]></MpsFlag>");//
                        Document.Append("<BulkIssueFlag>N</BulkIssueFlag>");
                        Document.Append("<LeadTime><![CDATA[" + StockCodeCheck.LeadTime.ToString().Trim() + "]]></LeadTime>");//
                        Document.Append("<StockMovementReq>Y</StockMovementReq>");
                        Document.Append("<ClearingFlag>N</ClearingFlag>");
                        Document.Append("<SupercessionDate></SupercessionDate>");
                        Document.Append("<AbcAnalysisReq>Y</AbcAnalysisReq>");
                        Document.Append("<AbcCostingReq>N</AbcCostingReq>");
                        Document.Append("<ManualCostFlag>N</ManualCostFlag>");
                        Document.Append("<CostUom><![CDATA[" + StockCodeCheck.CostUom.ToString().Trim() + "]]></CostUom>");//
                        Document.Append("<MinPricePct/>");
                        Document.Append("<LabourCost/>");
                        Document.Append("<MaterialCost/>");
                        Document.Append("<FixOverhead/>");
                        Document.Append("<SubContractCost/>");
                        Document.Append("<VariableOverhead/>");
                        Document.Append("<PartCategory><![CDATA[" + StockCodeCheck.PartCategory.ToString().Trim() + "]]></PartCategory>");//
                        Document.Append("<DrawOfficeNum/>");
                        Document.Append("<WarehouseToUse><![CDATA[" + StockCodeCheck.WarehouseToUse.ToString().Trim() + "]]></WarehouseToUse>");//CHECK PRESS RETURN
                        Document.Append("<BuyingRule><![CDATA[" + StockCodeCheck.BuyingRule.ToString().Trim() + "]]></BuyingRule>");//
                        Document.Append("<SpecificGravity><![CDATA[" + StockCodeCheck.SpecificGravity.ToString().Trim() + "]]></SpecificGravity>");//
                        Document.Append("<Ebq><![CDATA[" + StockCodeCheck.Ebq.ToString().Trim() + "]]></Ebq>");//
                        Document.Append("<FixTimePeriod>1</FixTimePeriod>");
                        Document.Append("<PanSize>0.000</PanSize>");
                        Document.Append("<DockToStock>0</DockToStock>");
                        Document.Append("<OutputMassFlag>F</OutputMassFlag>");
                        Document.Append("<ShelfLife>0</ShelfLife>");
                        Document.Append("<Version/>");
                        Document.Append("<Release/>");
                        Document.Append("<DemandTimeFence>0</DemandTimeFence>");
                        Document.Append("<MakeToOrderFlag>N</MakeToOrderFlag>");
                        Document.Append("<ManufLeadTime/>");
                        Document.Append("<GrossReqRule>I</GrossReqRule>");
                        Document.Append("<PercentageYield>100</PercentageYield>");
                        Document.Append("<WipCtlGlCode/>");
                        Document.Append("<ResourceCode/>");
                        Document.Append("<GstTaxCode/>");
                        Document.Append("<PrcInclGst>N</PrcInclGst>");
                        Document.Append("<SerEntryAtSale/>");
                        Document.Append("<UserField1/>");
                        Document.Append("<UserField2/>");
                        Document.Append("<UserField3/>");
                        Document.Append("<UserField4/>");
                        Document.Append("<UserField5/>");
                        Document.Append("<TariffCode/>");
                        Document.Append("<SupplementaryUnit>N</SupplementaryUnit>");
                        Document.Append("<EbqPan>E</EbqPan>");
                        Document.Append("<LctRequired>N</LctRequired>");
                        Document.Append("<IssMultLotsFlag>Y</IssMultLotsFlag>");
                        Document.Append("<InclInStrValid>Y</InclInStrValid>");
                        Document.Append("<CountryOfOrigin/>");
                        Document.Append("<StockOnHold/>");
                        Document.Append("<StockOnHoldReason/>");
                        Document.Append("<EccFlag>N</EccFlag>");
                        Document.Append("<StockAndAltUm>N</StockAndAltUm>");
                        Document.Append("<BatchBill>N</BatchBill>");
                        Document.Append("<DistWarehouseToUse></DistWarehouseToUse>");
                        Document.Append("<JobClassification/>");
                        Document.Append("<ProductGroup/>");
                        Document.Append("<PriceType/>");
                        Document.Append("<Basis/>");
                        Document.Append("<ManufactureUom><![CDATA[" + StockCodeCheck.ManufactureUom.ToString().Trim() + "]]></ManufactureUom>");//
                        Document.Append("<ConvFactMuM><![CDATA[" + StockCodeCheck.ConvFactMuM.ToString().Trim() + "]]></ConvFactMuM>");//
                        Document.Append("<ManMulDiv><![CDATA[" + StockCodeCheck.ManMulDiv.ToString().Trim() + "]]></ManMulDiv>");//
                        //Document.Append("<PhantomIfComp/>");
                        //Document.Append("<AltMethodFlag/>");
                        //Document.Append("<AltSisoFlag/>");
                        //Document.Append("<AltReductionFlag/>");
                        //Document.Append("<WithTaxExpenseType>G</WithTaxExpenseType>");
                        //Document.Append("<eSignature>{36303032-3330-3031-3038-323434363433}</eSignature>");
                        Document.Append("</Item>");
                        Document.Append("</SetupInvMaster>");


                        //Declaration
                        StringBuilder Parameter = new StringBuilder();

                        //Building Parameter content
                        Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                        Parameter.Append("<!--");
                        Parameter.Append("Sample XML for the Stock Code Setup Business Object");
                        Parameter.Append("-->");
                        Parameter.Append("<SetupInvMaster xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSST.XSD\">");
                        Parameter.Append("<Parameters>");
                        Parameter.Append("<ApplyProductClassDefault></ApplyProductClassDefault>");
                        Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                        Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                        Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                        Parameter.Append("<ValidateAgainstConfiguration>Y</ValidateAgainstConfiguration>");
                        Parameter.Append("</Parameters>");
                        Parameter.Append("</SetupInvMaster>");


                        string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "INVSST");
                        string ErrorMessage = sys.GetXmlErrors(XmlOut);
                        if (string.IsNullOrWhiteSpace(ErrorMessage))
                        {
                            string InvWhseMsg = PostStockCodeWarehouse(Guid, StockCode, Warehouse);
                            return InvWhseMsg;
                        }
                        else
                        {
                            return ErrorMessage;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostStockCodeWarehouse(string Guid, string StockCode, string Warehouse)
        {
            try
            {

                var WarehouseCheck = (from a in wdb.InvWarehouses where a.StockCode == StockCode && a.Warehouse == Warehouse select a).FirstOrDefault();
                if (WarehouseCheck != null)
                {
                    return "";
                }

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Stock Code Warehouse Setup Business Object");
                Document.Append("-->");
                Document.Append("<SetupInvWarehouse xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSWSDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Warehouse><![CDATA[" + Warehouse + "]]></Warehouse>");
                Document.Append("</Key>");
                Document.Append("<CostMultiplier>1.000</CostMultiplier>");
                Document.Append("<MinimumQty/>");
                Document.Append("<MaximumQty/>");
                Document.Append("<UnitCost>0</UnitCost>");
                Document.Append("<DefaultBin><![CDATA[" + Warehouse + "]]></DefaultBin>");
                Document.Append("<SafetyStockQty/>");
                Document.Append("<ReOrderQty/>");
                Document.Append("<PalletQty/>");
                Document.Append("<UserField1/>");
                Document.Append("<UserField2/>");
                Document.Append("<UserField3/>");
                Document.Append("<OrderPolicy>C</OrderPolicy>");
                Document.Append("<MajorOrderMult/>");
                Document.Append("<MinorOrderMult/>");
                Document.Append("<OrderMinimum/>");
                Document.Append("<OrderMaximum/>");
                Document.Append("<OrderFixPeriod>01</OrderFixPeriod>");
                Document.Append("<TrfSuppliedItem>N</TrfSuppliedItem>");
                Document.Append("<DefaultSourceWh/>");
                Document.Append("<TrfLeadTime>0</TrfLeadTime>");
                Document.Append("<TrfCostGlCode/>");
                Document.Append("<TrfCostMultiply/>");
                Document.Append("<TrfReplenishWh>0</TrfReplenishWh>");
                Document.Append("<TrfBuyingRule>A</TrfBuyingRule>");
                Document.Append("<TrfDockToStock/>");
                Document.Append("<TrfFixTimePeriod/>");
                Document.Append("<LabourCost/>");
                Document.Append("<MaterialCost/>");
                Document.Append("<FixedOverhead/>");
                Document.Append("<VariableOverhead/>");
                Document.Append("<SubContractCost/>");
                Document.Append("<ManualCostFlag>N</ManualCostFlag>");
                Document.Append("<BoughtOutWhsLvl>N</BoughtOutWhsLvl>");
                Document.Append("<Supplier/>");
                Document.Append("<LeadTime/>");
                Document.Append("<DockToStock/>");
                Document.Append("<eSignature>{36303032-3330-3031-3038-323434363433}</eSignature>");
                Document.Append("</Item>");
                Document.Append("</SetupInvWarehouse>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Stock Code Warehouse Setup Business Object");
                Parameter.Append("-->");
                Parameter.Append("<SetupInvWarehouse xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSWS.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ApplyProductClassDefault>BA</ApplyProductClassDefault>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</SetupInvWarehouse>");

                string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "INVSWS");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                return ErrorMessage;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildInventoryReceiptDocument(string Warehouse, string StockCode, decimal NetMass, string Lot)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Inventory Receipts Business Object");
            Document.Append("-->");
            Document.Append("<PostInvReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMRDOC.XSD\">");
            var Traceable = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == StockCode && a.TraceableType == "T" select a).ToList();
            Document.Append("<Item>");
            Document.Append("<Journal/>");
            Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
            Document.Append("<StockCode>" + StockCode + "</StockCode>");
            Document.Append("<Version/>");
            Document.Append("<Release/>");
            Document.Append("<Quantity>" + NetMass + "</Quantity>");
            Document.Append("<UnitOfMeasure/>");
            Document.Append("<Units/>");
            Document.Append("<Pieces/>");
            //Document.Append("<UnitCost>115.00000</UnitCost>");
            Document.Append("<TotalCost/>");
            Document.Append("<FifoBucket/>");

            if (Traceable.Count > 0)
            {
                if (!string.IsNullOrEmpty(Lot))
                {
                    Document.Append("<Lot>" + Lot + "</Lot>");
                    Document.Append("<LotConcession>1</LotConcession>");
                    Document.Append("<LotExpiryDate/>");
                }
            }


            Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
            //Document.Append("<Bins>");
            //Document.Append("<BinLocation>A1</BinLocation>");
            //Document.Append("<BinQuantity>9.000</BinQuantity>");
            //Document.Append("<BinUnits/>");
            //Document.Append("<BinPieces/>");
            //Document.Append("</Bins>");
            //Document.Append("<Serials>");
            //Document.Append("<SerialNumber>0206</SerialNumber>");
            //Document.Append("<SerialQuantity>1</SerialQuantity>");
            //Document.Append("<SerialUnits/>");
            //Document.Append("<SerialPieces/>");
            //Document.Append("<SerialExpiryDate/>");
            //Document.Append("<SerialLocation/>");
            //Document.Append("</Serials>");
            //Document.Append("<SerialRange>");
            //Document.Append("<SerialPrefix>BCT</SerialPrefix>");
            //Document.Append("<SerialSuffix>1</SerialSuffix>");
            //Document.Append("<SerialQuantity>8</SerialQuantity>");
            //Document.Append("<SerialExpiryDate/>");
            //Document.Append("<SerialLocation/>");
            //Document.Append("</SerialRange>");
            //Document.Append("<SerialAllocation>");
            //Document.Append("<FromSerialNumber>0205</FromSerialNumber>");
            //Document.Append("<ToSerialNumber>0209</ToSerialNumber>");
            //Document.Append("<SerialQuantity>10.000</SerialQuantity>");
            //Document.Append("</SerialAllocation>");
            //Document.Append("<ApplyCostMultiplier>Y</ApplyCostMultiplier>");
            //Document.Append("<CostMultiplier/>");
            //Document.Append("<NonMerchandiseCost>150.00</NonMerchandiseCost>");
            Document.Append("<NonMerchandiseDistribution>");
            //Document.Append("<NmReference>Cost Ref</NmReference>");
            //Document.Append("<NmLedgerCode>30-1120</NmLedgerCode>");
            //Document.Append("<NmAmount>150.00</NmAmount>");
            Document.Append("</NonMerchandiseDistribution>");
            Document.Append("<Reference>Receipt</Reference>");
            Document.Append("<Notation>Receipt note</Notation>");
            Document.Append("<LedgerCode/>");
            Document.Append("<PasswordForLedgerCode/>");
            Document.Append("<eSignature/>");
            Document.Append("<AnalysisEntry/>");
            Document.Append("<AnalysisLineEntry>");
            //Document.Append("<AnalysisCode1>Air</AnalysisCode1>");
            //Document.Append("<AnalysisCode2>Conf</AnalysisCode2>");
            //Document.Append("<AnalysisCode3>East</AnalysisCode3>");
            Document.Append("<AnalysisCode4/>");
            Document.Append("<AnalysisCode5/>");
            Document.Append("<StartDate/>");
            Document.Append("<EndDate/>");
            //Document.Append("<EntryAmount>100</EntryAmount>");
            //Document.Append("<Comment>Analysis entry details</Comment>");
            Document.Append("</AnalysisLineEntry>");
            Document.Append("</Item>");

            Document.Append("</PostInvReceipts>");


            return Document.ToString();
        }

        public string BuildInventoryReceiptParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();


            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory Receipts Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMR.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ManualSerialTransfersAllowed>N</ManualSerialTransfersAllowed>");
            Parameter.Append("<ReturnDetailedReceipt>N</ReturnDetailedReceipt>");
            Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvReceipts>");

            return Parameter.ToString();
        }

        public string PostiInventoryReceipt(string Guid, string Warehouse, string StockCode, decimal NetMass, string Lot)
        {
            //string Guid = "";
            try
            {

                string User = HttpContext.Current.User.Identity.Name.ToUpper();
                string ErrorMessage = "";
                string XmlOut;

                //if (string.IsNullOrEmpty(Guid))
                //{
                //    //Guid = sys.SysproLogin(null, null);
                //    if (string.IsNullOrEmpty(Guid))
                //    {
                //        return "Failed to login to Syspro.";
                //    }
                //}
                XmlOut = sys.SysproPost(Guid, this.BuildInventoryReceiptParameter(), this.BuildInventoryReceiptDocument(Warehouse, StockCode, NetMass, Lot), "INVTMR");
                ErrorMessage = sys.GetXmlErrors(XmlOut);
                string StockJournal = sys.GetXmlValue(XmlOut, "Journal");
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    //if (!string.IsNullOrEmpty(Guid))
                    //{
                    //    sys.SysproLogoff(Guid);
                    //}
                    return "";

                }
                else
                {
                    if (!string.IsNullOrEmpty(Guid))
                    {
                        sys.SysproLogoff(Guid);
                    }
                    return "Inventory Receipt Error: " + ErrorMessage;
                }

            }
            catch (Exception ex)
            {

                if (!string.IsNullOrEmpty(Guid))
                {
                    sys.SysproLogoff(Guid);
                }
                throw new Exception(ex.Message);
            }

        }

        public string GetEmailTemplate(string Requisition, string RoutedBy, string RoutedTo, Guid RouteGuid, string Company)
        {

            var Header = wdb.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
            var detail = wdb.sp_mtReqGetRequisitionLines(Requisition, RoutedTo, HttpContext.Current.User.Identity.Name.ToUpper(), Company).ToList();

            // bool CanApprove = RoutedToCanApprove(Requisition, RoutedTo);
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
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">Hi there,</p>");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">The below requisition has been routed for your attention.</p>");
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
            Document.Append("							  <td style=\"font-weight:bold;\"></td>");
            Document.Append("							  <td></td>");
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
            Document.Append("							  <td style=\"font-weight:bold;\">Routed By</td>");
            Document.Append("							  <td>" + RoutedBy + " - " + RoutedByName + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            //Document.Append("							  <td style=\"font-weight:bold;\">Route Note</td>");
            //Document.Append("							  <td colspan=\"3\">" + RouteNote + "</td>");
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
            //Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">or click the \"Approve\" button.</p>");
            //Document.Append("						");
            Document.Append("						<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; box-sizing: border-box;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");
            Document.Append("<td align=\"left\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");


            //string HostUrl = Request.Url.Host;
            //if (HostUrl == "localhost")
            //{
            //    HostUrl = "localhost:52696";
            //}
            //string ViewUrl = @"http://" + HostUrl + "//Requisition/Create?Requisition=" + Requisition;
            //Document.Append("									<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #3498db; border-radius: 5px; text-align: center;\"> <a href=\"" + ViewUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #3498db; border: solid 1px #3498db; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #3498db;\">View</a> </td>");
            Document.Append("									<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>");
            //if (CanApprove)
            //{
            //    string ApproveUrl = @"http://" + HostUrl + "/api/RequisitionApi/" + RouteGuid;
            //    Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #5cb85c; border-radius: 5px; text-align: center;\"> <a href=\"" + ApproveUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #5cb85c; border: solid 1px #5cb85c; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #5cb85c;\">Approve</a> </td>");

            //}
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
    }
}