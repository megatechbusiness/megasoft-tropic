using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class WhseManProductionReceipt
    {
        private SysproCore objSyspro = new SysproCore();
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();

        public string PostJobReceipt(List<WhseManJobReceipt> detail)
        {
            string Guid = "";
            try
            {
                string JobNo = detail.FirstOrDefault().Job.PadLeft(15, '0');
                string PalletNo = detail.FirstOrDefault().Lot;
                var palletCheck = (from a in wdb.mtProductionLabels where a.Job == JobNo && a.BatchId == PalletNo select a).ToList();
                if (palletCheck.Count == 0)
                {
                    return "Pallet : " + PalletNo + " not found for Job : " + JobNo + ".";
                }

                if (palletCheck.FirstOrDefault().LabelReceipted == "Y")
                {
                    return "Item already receipted!";
                }

                Guid = objSyspro.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to login to Syspro.";
                }
                else
                {
                    var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();

                    string ErrorMessage = "";
                    string XmlOut;

                    //Posting set for 1 Job at a time.
                    ErrorMessage = this.PostMaterialIssue(Guid, detail.FirstOrDefault().Job, detail.FirstOrDefault().Lot, (decimal)detail.FirstOrDefault().Quantity);

                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        return "Material Issue Error: " + ErrorMessage;
                    }

                    ErrorMessage = this.PostLabourIssue(Guid, detail.FirstOrDefault().Job, (decimal)detail.FirstOrDefault().Quantity);

                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        return "Labour Issue Error: " + ErrorMessage;
                    }

                    XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), this.BuildJobReceiptDocument(detail), "WIPTJR");
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    string Journal = objSyspro.GetXmlValue(XmlOut, "Journal");
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        foreach (var item in detail)
                        {
                            string Job = item.Job.PadLeft(15, '0');
                            wdb.sp_UpdateLabelReceipted(Job, item.Lot, "Y", Journal, HttpContext.Current.User.Identity.Name.ToUpper());
                        }

                        mtPalletControl pallet = new mtPalletControl();
                        pallet = (from a in wdb.mtPalletControls where a.PalletNo == PalletNo select a).ToList().FirstOrDefault();
                        pallet.Status = "C";
                        wdb.Entry(pallet).State = System.Data.EntityState.Modified;
                        wdb.SaveChanges();

                        return "Job Receipt Completed Successfully. Journal : " + Journal;
                    }
                    else
                    {
                        return "Job Receipt Error: " + ErrorMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                objSyspro.SysproLogoff(Guid);
            }
        }

        public string BuildJobReceiptDocument(List<WhseManJobReceipt> detail)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Job Receipt Posting Business Object");
            Document.Append("-->");
            Document.Append("<PostJobReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJRDOC.XSD\">");
            foreach (var item in detail)
            {
                //var result = wdb.sp_GetProductionJobDetails(item.Job).FirstOrDefault();
                var StocKCode = (from a in wdb.WipMasters.AsNoTracking() where a.Job == item.Job select a.StockCode).FirstOrDefault();
                var result = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == StocKCode select a).FirstOrDefault();
                Document.Append("<Item>");
                Document.Append("<Journal></Journal>");
                Document.Append("<Job>" + item.Job + "</Job>");
                Document.Append("<CoProductLine />");
                Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                if (result.StockUom == "TH")
                {
                    decimal Qty;
                    Qty = item.Quantity / 1000;
                    Document.Append("<Quantity>" + Qty.ToString() + "</Quantity>");
                }
                else
                {
                    Document.Append("<Quantity>" + item.Quantity.ToString() + "</Quantity>");
                }

                Document.Append("<InspectionFlag>N</InspectionFlag>");
                var CostBasis = wdb.sp_GetProductionCostBasis(item.Job).FirstOrDefault();
                Document.Append("<CostBasis>" + CostBasis + "</CostBasis>");
                //Document.Append("<ReceiptCost>441.73</ReceiptCost>");
                Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
                Document.Append("<MaterialDistributionValue />");
                Document.Append("<LaborDistributionValue />");
                Document.Append("<JobComplete></JobComplete>");
                Document.Append("<CoProductComplete>N</CoProductComplete>");
                Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                Document.Append("<SalesOrderComplete>N</SalesOrderComplete>");

                string Job = item.Job.PadLeft(15, '0');
                var Traceable = (from a in wdb.WipMasters.AsNoTracking() where a.Job == Job && a.TraceableType == "T" select a).ToList();
                if (Traceable.Count > 0)
                {
                    if (!string.IsNullOrEmpty(item.Lot))
                    {
                        Document.Append("<Lot>" + item.Lot + "</Lot>");
                        Document.Append("<LotConcession>" + 1 + "</LotConcession>");
                        Document.Append("<LotExpiryDate></LotExpiryDate>");
                    }
                }

                Document.Append("<BinLocation></BinLocation>");
                Document.Append("<BinOnHold />");
                Document.Append("<BinOnHoldReason />");
                Document.Append("<BinUpdateWhDefault />");
                Document.Append("<FifoBucket />");
                //Document.Append("<Serials>");
                //Document.Append("<SerialNumber>8875</SerialNumber>");
                //Document.Append("<SerialQuantity>12</SerialQuantity>");
                //Document.Append("<SerialExpiryDate>2011-12-30</SerialExpiryDate>");
                //Document.Append("<SerialLocation />");
                //Document.Append("<SerialFifoBucket />");
                //Document.Append("</Serials>");
                Document.Append("<WipInspectionReference />");
                Document.Append("<WipInspectionNarration />");
                Document.Append("<HierarchyJob>");
                Document.Append("<Head />");
                Document.Append("<Section1 />");
                Document.Append("<Section2 />");
                Document.Append("<Section3 />");
                Document.Append("<Section4 />");
                Document.Append("<CostOfSalesAmount />");
                Document.Append("</HierarchyJob>");
                Document.Append("<AddReference>" + item.Lot + "</AddReference>");
                Document.Append("<MaterialReference />");
                Document.Append("<QuantityFromStock />");
                //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
                Document.Append("</Item>");
            }

            Document.Append("</PostJobReceipts>");


            return Document.ToString();
        }

        public string BuildJobReceiptParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Job Receipt Posting Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostJobReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJR.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<SetJobToCompleteIfCoProductsComplete>N</SetJobToCompleteIfCoProductsComplete>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostJobReceipts>");

            return Parameter.ToString();
        }

        public class MaterialItems
        {
            public string Job { get; set; }
            public string Warehouse { get; set; }
            public string StockCode { get; set; }
            public decimal Quantity { get; set; }
            public string ErrorMessage { get; set; }
        }

        public string PostMaterialIssue(string Guid, string Job, string Pallet, decimal Quantity)
        {
            try
            {
                //string BuildXml = this.GetMaterialBuild(Guid, Job, Quantity);

                //if (!BuildXml.StartsWith("<"))
                //{
                //    return BuildXml;
                //}

                ////string BuildErrorMessage = objSyspro.GetXmlErrors(BuildXml);

                ////if(!string.IsNullOrEmpty(BuildErrorMessage))
                ////{
                ////    return BuildErrorMessage;
                ////}

                //var XDoc = XDocument.Parse(BuildXml);
                //var items = (from x in XDoc.Descendants("Item")
                //             select new MaterialItems
                //             {
                //                 Job = x.Element("Job").Value,
                //                 Warehouse = x.Element("Warehouse").Value,
                //                 StockCode = x.Element("StockCode").Value,
                //                 Quantity = Convert.ToDecimal(x.Element("QuantityToIssue").Value),

                //             }).ToList();

                var items = wdb.sp_GetKitMaterialToIssue(Job.PadLeft(15, '0'), Quantity).ToList();
                var procError = (from a in items where a.ErrorMessage != "" select a).ToList();
                if (procError.Count > 0)
                {
                    //Concatenate All Errors
                    var returnError = procError.Select(i => i.ErrorMessage).Aggregate((i, j) => i + "," + j);
                    return returnError.ToString();
                }

                items = (from a in items where a.MaterialToPost > 0 select a).ToList();

                if (items.Count > 0)
                {
                    StringBuilder Document = new StringBuilder();

                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Post Material Business Object");
                    Document.Append("-->");
                    Document.Append("<PostMaterial xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTMIDOC.XSD\">");

                    foreach (var item in items)
                    {
                        if (item.TraceableType == "T")
                        {
                            var lots = wdb.sp_GetAutoDepleteLots(item.StockCode, item.Warehouse, item.MaterialToPost).ToList();
                            foreach (var lot in lots)
                            {
                                Document.Append("<Item>");
                                Document.Append("<Journal />");
                                Document.Append("<Job>" + Job + "</Job>");
                                Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                                Document.Append("<Warehouse>" + item.Warehouse + "</Warehouse>");
                                Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                                Document.Append("<Line>" + item.Line + "</Line>");
                                Document.Append("<QtyIssued>" + string.Format("{0:##,###,##0.00}", lot.Allocated) + "</QtyIssued>");
                                Document.Append("<Reference>" + Job + "</Reference>");
                                Document.Append("<MaterialReference>E:" + HttpContext.Current.User.Identity.Name.ToUpper() + "</MaterialReference>");
                                Document.Append("<Notation>" + Pallet + "</Notation>");
                                Document.Append("<ProductClass />");
                                Document.Append("<UnitCost />");
                                Document.Append("<AllocCompleted>N</AllocCompleted>");
                                Document.Append("<FifoBucket />");
                                Document.Append("<Lot>" + lot.Lot + "</Lot>");
                                //Document.Append("<LotConcession />");
                                Document.Append("</Item>");
                            }
                        }
                        else
                        {
                            Document.Append("<Item>");
                            Document.Append("<Journal />");
                            Document.Append("<Job>" + Job + "</Job>");
                            Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                            Document.Append("<Warehouse>" + item.Warehouse + "</Warehouse>");
                            Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                            Document.Append("<Line>" + item.Line + "</Line>");
                            Document.Append("<QtyIssued>" + item.MaterialToPost + "</QtyIssued>");
                            Document.Append("<Reference>" + Job + "</Reference>");
                            Document.Append("<MaterialReference>E:" + HttpContext.Current.User.Identity.Name.ToUpper() + "</MaterialReference>");
                            Document.Append("<Notation>" + Pallet + "</Notation>");
                            Document.Append("<ProductClass />");
                            Document.Append("<UnitCost />");
                            Document.Append("<AllocCompleted>N</AllocCompleted>");
                            Document.Append("<FifoBucket />");
                            //Document.Append("<Lot></Lot>");
                            //Document.Append("<LotConcession />");
                            Document.Append("</Item>");
                        }
                    }
                    Document.Append("</PostMaterial>");

                    string XmlOut, ErrorMessage;

                    XmlOut = objSyspro.SysproPost(Guid, this.BuildMaterialIssueParameter(), Document.ToString(), "WIPTMI");
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        return "";
                    }
                    else
                    {
                        return (ErrorMessage);
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetMaterialBuild(string Guid, string Job, decimal Quantity)
        {
            try
            {
                string XmlOut, ErrorMessage;
                XmlOut = objSyspro.SysproBuild(Guid, this.BuildMaterialDocument(Job, Quantity), "WIPRMI");
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    return XmlOut;
                }
                else
                {
                    return "Material Build Error:" + ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildMaterialDocument(string Job, decimal Quantity)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Build Material Issue Business Object");
            Document.Append("-->");
            Document.Append("<Build xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPRMI.XSD\">");
            Document.Append("<Parameters>");
            Document.Append("<Job>" + Job + "</Job>");
            Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
            Document.Append("<KitQuantity>" + Quantity.ToString() + "</KitQuantity>");
            Document.Append("<IssueNonStockedMaterial>Y</IssueNonStockedMaterial>");
            Document.Append("<IssueNegativeAllocations>N</IssueNegativeAllocations>");
            Document.Append("<IssueCompletedAllocations>Y</IssueCompletedAllocations>");
            Document.Append("<IssueToMaxOutstanding>N</IssueToMaxOutstanding>");
            Document.Append("<ReturnValidMaterialsOnly>N</ReturnValidMaterialsOnly>");
            Document.Append("<ReturnWhenQtyIssueZero>N</ReturnWhenQtyIssueZero>");
            Document.Append("<ReturnEccConsumption>N</ReturnEccConsumption>");
            Document.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Document.Append("<IncludeFloorstock>N</IncludeFloorstock>");
            Document.Append("</Parameters>");
            Document.Append("<Filter>");
            Document.Append("<Operation FilterType=\"A\" FilterValue=\"\" />");
            Document.Append("</Filter>");
            Document.Append("</Build>");

            return Document.ToString();
        }

        public string BuildMaterialIssueParameter()
        {
            try
            {
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
                Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<AutoDepleteLotsBins>Y</AutoDepleteLotsBins>");
                Parameter.Append("<PostFloorstock>N</PostFloorstock>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostMaterial>");

                return Parameter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildLabourDocument(string Job, decimal Quantity)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Build Labor Issue Business Object");
            Document.Append("-->");
            Document.Append("<Build xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPRLI.XSD\">");
            Document.Append("<Parameters>");
            Document.Append("<Job>" + Job + "</Job>");
            Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
            Document.Append("<KitQuantity>" + Quantity.ToString() + "</KitQuantity>");
            Document.Append("<IssueSubcontractLabor>Y</IssueSubcontractLabor>");
            Document.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Document.Append("</Parameters>");
            Document.Append("<Filter>");
            Document.Append("<Operation FilterType=\"A\" />");
            Document.Append("</Filter>");
            Document.Append("</Build>");

            return Document.ToString();
        }

        public string PostLabourIssue(string Guid, string Job, decimal Quantity)
        {
            try
            {
                //string BuildXml = this.GetLabourBuild(Guid, Job, Quantity);

                //if (!BuildXml.StartsWith("<"))
                //{
                //    return BuildXml;
                //}

                //var XDoc = XDocument.Parse(BuildXml);
                //var items = (from x in XDoc.Descendants("Item")
                //             select new LabourItems
                //             {
                //                 Job = x.Element("Job").Value,
                //                 Operation = x.Element("LOperation").Value,
                //                 WorkCentre = x.Element("LWorkCentre").Value,
                //                 RunTime = x.Element("LRunTimeHours").Value,
                //                 QtyCompleted = x.Element("LQtyComplete").Value
                //             }).ToList();

                var items = wdb.sp_GetKitLabourToIssue(Job.PadLeft(15, '0'), Quantity).ToList();

                items = (from a in items where a.RunTimeToPost > 0 || a.RequiredSetupTime > 0 select a).ToList();

                if (items.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("This is an example XML instance to demonstrate");
                    Document.Append("use of the WIP Labor Posting Business Object");
                    Document.Append("-->");
                    Document.Append("<PostLabour xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTLPDOC.XSD\">");

                    foreach (var item in items)
                    {
                        Document.Append("<Item>");
                        Document.Append("<Journal />");
                        Document.Append("<Job>" + Job + "</Job>");
                        Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                        Document.Append("<LOperation>" + item.Operation + "</LOperation>");
                        Document.Append("<LWorkCentre>" + item.WorkCentre + "</LWorkCentre>");
                        Document.Append("<LWcRateInd>1</LWcRateInd>"); //FROM SQL
                        //Document.Append("<LEmployee>000001</LEmployee>");
                        //Document.Append("<LMachine>1</LMachine>");
                        Document.Append("<LRunTimeHours>" + item.RunTimeToPost + "</LRunTimeHours>");
                        Document.Append("<LSetUpHours>" + item.RequiredSetupTime + "</LSetUpHours>");
                        //Document.Append("<LStartupHours>1</LStartupHours>");
                        //Document.Append("<LTeardownHours />");
                        Document.Append("<ManualWorkCenterRates>N</ManualWorkCenterRates>");
                        //Document.Append("<ManualRates>");
                        //Document.Append("<SetupRate>0.0000</SetupRate>");
                        //Document.Append("<RunRate>0.0000</RunRate>");
                        //Document.Append("<FixedOverheadRate>0.0000</FixedOverheadRate>");
                        //Document.Append("<VariableOverheadRate>0.0000</VariableOverheadRate>");
                        //Document.Append("<StartupRate>0.0000</StartupRate>");
                        //Document.Append("<TeardownRate>0.0000</TeardownRate>");
                        //Document.Append("</ManualRates>");
                        Document.Append("<NonProductiveCode />");
                        Document.Append("<Reference>" + Job + "</Reference>");
                        Document.Append("<AdditionalReference>E:" + HttpContext.Current.User.Identity.Name.ToUpper() + "</AdditionalReference>");
                        Document.Append("<MultipleScrapEntries>N</MultipleScrapEntries>");
                        Document.Append("<ScrapCode />");
                        Document.Append("<MultipleScrap>");
                        Document.Append("<MultipleScrapCode />");
                        Document.Append("<MultipleScrapQty />");
                        Document.Append("</MultipleScrap>");
                        Document.Append("<CoProductScrap>");
                        Document.Append("<CoProductLineNumber />");
                        Document.Append("<CoProductScrapCode />");
                        Document.Append("<CoProductScrapQty />");
                        Document.Append("<CoProductReservedLot />");
                        Document.Append("<CoProductReservedSerials>");
                        Document.Append("<SerialNumber />");
                        Document.Append("<SerialQuantity />");
                        Document.Append("</CoProductReservedSerials>");
                        Document.Append("</CoProductScrap>");
                        Document.Append("<LQtyComplete>" + item.ProdQty + "</LQtyComplete>");
                        Document.Append("<LQtyScrapped />");
                        //Document.Append("<PiecesCompleted>1</PiecesCompleted>");
                        Document.Append("<OperCompleted>N</OperCompleted>");
                        Document.Append("<LEmployeeRatInd />");
                        Document.Append("<SubcontractValue />");
                        Document.Append("<Esignature />");
                        Document.Append("<ReservedLot />");
                        Document.Append("<ReservedSerials>");
                        Document.Append("<SerialNumber />");
                        Document.Append("<SerialQuantity />");
                        Document.Append("</ReservedSerials>");
                        Document.Append("</Item>");
                    }

                    Document.Append("</PostLabour>");

                    string XmlOut, ErrorMessage;

                    XmlOut = objSyspro.SysproPost(Guid, this.BuildLabourIssueParamater(), Document.ToString(), "WIPTLP");
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        return "";
                    }
                    else
                    {
                        return "Kit Labour Issue Failed. " + ErrorMessage;
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class LabourItems
        {
            public string Job { get; set; }
            public string Operation { get; set; }
            public string WorkCentre { get; set; }
            public string RunTime { get; set; }
            public string QtyCompleted { get; set; }
        }

        public string BuildLabourIssueParamater()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the parameters for the WIP Labor Posting Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostLabour xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTLP.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<PostingPeriod>C</PostingPeriod>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<UpdateQtyToMakeWithScrap>N</UpdateQtyToMakeWithScrap>");
            Parameter.Append("<UncompleteNonMile>Y</UncompleteNonMile>");
            Parameter.Append("<UseWCRateIfEmpRateZero>Y</UseWCRateIfEmpRateZero>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostLabour>");

            return Parameter.ToString();
        }

        public string GetLabourBuild(string Guid, string Job, decimal Quantity)
        {
            try
            {
                string XmlOut, ErrorMessage;
                XmlOut = objSyspro.SysproBuild(Guid, this.BuildLabourDocument(Job, Quantity), "WIPRLI");
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    return XmlOut;
                }
                else
                {
                    return "Labour Build Error: " + ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool CheckDelayedPosting(string Warehouse)
        {
            try
            {
                HttpCookie database = HttpContext.Current.Request.Cookies.Get("SysproDatabase");
                var company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var result = (from a in mdb.mtDelayedPostingWarehouses where a.Company == company && a.Warehouse == Warehouse select a).ToList();
                if (result.Count > 0)
                {
                    return true;
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
        public string PostJobReceiptByBatch(string PalletNo)
        {
            string Guid = "";
            try
            {
                //string PalletNo = detail.FirstOrDefault().Lot;
                //user = current user
                string User = HttpContext.Current.User.Identity.Name.ToUpper();
                var JobsToPost = (from a in wdb.mtProductionLabels where a.PalletNo == PalletNo && (a.LabelReceipted == "N" || a.LabelReceipted == null) && a.Scanned == "Y" select a).ToList();
                if (JobsToPost.Count == 0)
                {
                    return "No unposted data found.";
                }
                var ExpectedCost = wdb.sp_CheckJobCardExpectedCost(JobsToPost.FirstOrDefault().Job).ToList();
                if (ExpectedCost.Count == 0)
                {
                    return "Job expected cost cannot be zero.";
                }
                //Delayed Posting Check
                //Get Job Warehouse
                //Check if delayed posting actived for warehouse
                //if activated --> update posted flag to D
                //return message items queued for posting.


                string Department = JobsToPost.FirstOrDefault().Department;

                var jobdet = wdb.sp_GetJobDetails(JobsToPost.FirstOrDefault().Job.PadLeft(15, '0')).ToList();
                var warehouse = jobdet.FirstOrDefault().Warehouse;

                //Check if delayed posting has been turned on for warehouse.                    
                if (this.CheckDelayedPosting(warehouse) == true)
                {
                    using (var delayDb = new WarehouseManagementEntities(""))
                    {
                        foreach (var updItem in JobsToPost)
                        {
                            updItem.LabelReceipted = "D";
                            delayDb.Entry(updItem).State = System.Data.EntityState.Modified;
                            delayDb.SaveChanges();
                        }
                    }


                    mtPalletControl close = new mtPalletControl();
                    close = wdb.mtPalletControls.Find(PalletNo);
                    close.Status = "C";
                    wdb.Entry(close).State = System.Data.EntityState.Modified;
                    wdb.SaveChanges();

                    return "Delayed posting activated. Items queued for posting.";
                }

                List<WhseManJobReceipt> result = JobsToPost.GroupBy(l => l.Job).Select(cl => new WhseManJobReceipt { Job = cl.First().Job, Quantity = Convert.ToInt32(cl.Sum(c => c.NetQty)), Lot = cl.First().PalletNo }).ToList();
                string Journal = "Pallet: " + PalletNo + ", Job Receipt Completed Successfully. Journal : ";


                Guid = objSyspro.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to login to Syspro.";
                }

                foreach (var item in result)
                {
                    var StocKCode = (from a in wdb.WipMasters.AsNoTracking() where a.Job == item.Job select a.StockCode).FirstOrDefault();
                    var LotExists = (from a in wdb.LotDetails where a.Lot == item.Lot && a.StockCode == StocKCode select a).ToList();
                    if (LotExists.Count > 0)
                    {
                        return "Validation Error: Lot number : " + item.Lot + " already exists in Syspro for Stock Code : " + StocKCode;
                    }

                    var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                    string ErrorMessage = "";
                    string XmlOut;
                    if (setting.PostMaterialIssue == true)
                    {
                        ErrorMessage = this.PostMaterialIssue(Guid, item.Job, item.Lot, (decimal)item.Quantity);

                        if (!string.IsNullOrEmpty(ErrorMessage))
                        {
                            return "Material Issue Error: " + ErrorMessage;
                        }
                    }
                    if (setting.PostLabour == true)
                    {
                        ErrorMessage = this.PostLabourIssue(Guid, item.Job, (decimal)item.Quantity);

                        if (!string.IsNullOrEmpty(ErrorMessage))
                        {
                            return "Labour Issue Error: " + ErrorMessage;
                        }
                    }
                    var BatchList = (from a in JobsToPost where a.PalletNo == item.Lot && a.Job == item.Job select new WhseManJobReceipt { Job = a.Job, Lot = a.BatchId, Quantity = (decimal)a.NetQty }).OrderBy(x => x.Lot).ToList();
                    XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), this.BuildJobReceiptDocument(BatchList), "WIPTJR");
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    string JobJournal = objSyspro.GetXmlValue(XmlOut, "Journal");

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        foreach (var a in BatchList)
                        {
                            string Job = a.Job.PadLeft(15, '0');
                            wdb.sp_UpdateLabelReceipted(Job, a.Lot, "Y", JobJournal, User);

                            var Traceable = (from Z in wdb.WipMasters where Z.Job == Job && Z.TraceableType == "T" select Z).ToList();
                            if (Traceable.Count > 0)
                            {
                                wdb.sp_ProductionLotCustomForm(a.Lot, Job, PalletNo);
                            }
                        }
                        mtPalletControl close = new mtPalletControl();
                        close = wdb.mtPalletControls.Find(PalletNo);
                        close.Status = "C";
                        wdb.Entry(close).State = System.Data.EntityState.Modified;
                        wdb.SaveChanges();

                        Journal += JobJournal;
                    }
                    else
                    {
                        return "Job Receipt Error: " + ErrorMessage;
                    }
                }
                objSyspro.SysproLogoff(Guid);
                return Journal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public string ValidateBarcode(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        //Check if bail No Exists
                        var BailNoCheck = (from a in wdb.mtProductionLabels where a.BatchId == item.BailNo select a).ToList().FirstOrDefault();

                        if (BailNoCheck == null)
                        {
                            return "Error: Bail does not exist. " + item.BailNo;
                        }
                        if (BailNoCheck.LabelReceipted == "Y")
                        {
                            return "Error: Bail is already receipted. " + item.BailNo;
                        }
                        if (BailNoCheck.Scanned == "Y" && !string.IsNullOrEmpty(item.PalletNo))
                        {
                            return "Error: Bail is already scanned. " + item.BailNo;
                        }
                        if (BailNoCheck.Scanned == "Y" && string.IsNullOrEmpty(item.PalletNo))
                        {
                            return BailNoCheck.PalletNo;
                        }


                        //Flag Bail as Scanned
                        mtProductionLabel flag = new mtProductionLabel();
                        flag = wdb.mtProductionLabels.Find(item.Job.PadLeft(15, '0'), item.BailNo);
                        flag.Scanned = "Y";
                        flag.ScannedBy = HttpContext.Current.User.Identity.Name.ToUpper();
                        flag.ScanDate = DateTime.Now;
                        wdb.Entry(flag).State = System.Data.EntityState.Modified;
                        wdb.SaveChanges();

                        //If No Pallet No. then CreateOne
                        if (string.IsNullOrEmpty(item.PalletNo))
                        {
                            string Job = item.Job.PadLeft(15, '0');
                            int palletcount = (from a in wdb.mtPalletControls where a.Job == Job select a.PalletNo).ToList().Count();
                            int palletno;
                            if (palletcount > 0)
                            {
                                palletno = palletcount + 1;
                            }
                            else
                            {
                                palletno = 1;
                            }
                            var DefaultPalletNo = wdb.mtWhseManSettings.Where(a => a.SettingId.Equals(1)).FirstOrDefault();
                            mtPalletControl control = new mtPalletControl();
                            control.PalletNo = item.Job.Trim() + "-" + palletno.ToString().PadLeft(DefaultPalletNo.DefaultPalletNo.Length, '0');
                            control.Job = item.Job.Trim().PadLeft(15, '0');
                            control.PalletSeq = palletno;
                            control.Status = "O";
                            wdb.Entry(control).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();

                            //Update PalletNo against Bail
                            using (var udb = new WarehouseManagementEntities(""))
                            {
                                mtProductionLabel mpl = new mtProductionLabel();
                                mpl = udb.mtProductionLabels.Find(item.Job.PadLeft(15, '0'), item.BailNo);
                                mpl.PalletNo = item.Job.Trim() + "-" + palletno.ToString().PadLeft(DefaultPalletNo.DefaultPalletNo.Length, '0');
                                udb.Entry(mpl).State = System.Data.EntityState.Modified;
                                udb.SaveChanges();
                            }


                            return item.Job.Trim() + "-" + palletno.ToString().PadLeft(DefaultPalletNo.DefaultPalletNo.Length, '0'); ;
                        }
                        else
                        {
                            //Update PalletNo against Bail
                            mtProductionLabel mpl = new mtProductionLabel();
                            mpl = wdb.mtProductionLabels.Find(item.Job.PadLeft(15, '0'), item.BailNo);
                            mpl.PalletNo = item.PalletNo.Trim();
                            wdb.Entry(mpl).State = System.Data.EntityState.Modified;
                            wdb.SaveChanges();

                            return item.PalletNo;
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }

        public string SearchPalletNo(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string PalletNo = myDeserializedObjList.FirstOrDefault().PalletNo;
                    var Pallet = wdb.mtPalletControls.Where(a => a.PalletNo.Equals(PalletNo)).FirstOrDefault();
                    if (Pallet != null)
                    {
                        if (Pallet.Status != "C")
                        {
                            return PalletNo;
                        }
                        else
                        {
                            return "Error: Pallet " + PalletNo + " is closed. Please open pallet to continue.";
                        }
                    }
                    else
                    {
                        return "Error: Pallet not found";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }

        public string SearchBailNo(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string BailNo = myDeserializedObjList.FirstOrDefault().BailNo.Trim();
                    var Bail = wdb.mtProductionLabels.Where(a => a.BatchId.Equals(BailNo) && a.PalletNo != "").FirstOrDefault();
                    if (Bail != null)
                    {
                        var Pallet = wdb.mtPalletControls.Where(a => a.PalletNo.Equals(Bail.PalletNo) && a.Status != "C").FirstOrDefault();
                        if (Pallet != null)
                        {
                            return Bail.PalletNo;
                        }
                        else
                        {
                            return "Error: Pallet " + Bail.PalletNo + " is closed. Please open pallet to continue";
                        }

                    }
                    else
                    {
                        return "Error: Pallet not found for this bail";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }

        public string PostBatchReversal()
        {
            try
            {
                string Guid = objSyspro.SysproLogin();
                var rev = (from a in wdb.mtTmpLotsToReverses where a.Posted == "N" select a).ToList();
                if (rev.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Job Receipt Posting Business Object");
                    Document.Append("-->");
                    Document.Append("<PostJobReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJRDOC.XSD\">");
                    foreach (var item in rev)
                    {
                        var result = wdb.sp_GetProductionJobDetails(item.Job).FirstOrDefault();
                        Document.Append("<Item>");
                        Document.Append("<Journal></Journal>");
                        Document.Append("<Job>" + item.Job + "</Job>");
                        Document.Append("<CoProductLine />");
                        Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                        //if (result.StockUom == "TH")
                        //{
                        //    decimal Qty;
                        //    Qty = (decimal)item.Quantity / 1000;
                        //    Document.Append("<Quantity>" + Qty.ToString() + "</Quantity>");
                        //}
                        //else
                        //{
                        //    Document.Append("<Quantity>" + item.Quantity.ToString() + "</Quantity>");
                        //}

                        Document.Append("<Quantity>" + item.Quantity.ToString() + "</Quantity>");
                        Document.Append("<QuantityFromStock>" + item.Quantity.ToString() + "</QuantityFromStock >");
                        Document.Append("<InspectionFlag>N</InspectionFlag>");
                        var CostBasis = wdb.sp_GetProductionCostBasis(item.Job).FirstOrDefault();
                        Document.Append("<CostBasis>" + CostBasis + "</CostBasis>");
                        //Document.Append("<ReceiptCost>441.73</ReceiptCost>");
                        Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
                        Document.Append("<MaterialDistributionValue />");
                        Document.Append("<LaborDistributionValue />");
                        Document.Append("<JobComplete> </JobComplete>");
                        Document.Append("<CoProductComplete>N</CoProductComplete>");
                        Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                        Document.Append("<SalesOrderComplete>N</SalesOrderComplete>");

                        string Job = item.Job.PadLeft(15, '0');
                        var Traceable = (from a in wdb.WipMasters where a.Job == Job && a.TraceableType == "T" select a).ToList();
                        if (Traceable.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(item.Lot))
                            {
                                Document.Append("<Lot>" + item.Lot + "</Lot>");
                                Document.Append("<LotConcession>" + 1 + "</LotConcession>");
                                Document.Append("<LotExpiryDate></LotExpiryDate>");
                            }
                        }

                        Document.Append("<BinLocation></BinLocation>");
                        Document.Append("<BinOnHold />");
                        Document.Append("<BinOnHoldReason />");
                        Document.Append("<BinUpdateWhDefault />");
                        Document.Append("<FifoBucket />");
                        //Document.Append("<Serials>");
                        //Document.Append("<SerialNumber>8875</SerialNumber>");
                        //Document.Append("<SerialQuantity>12</SerialQuantity>");
                        //Document.Append("<SerialExpiryDate>2011-12-30</SerialExpiryDate>");
                        //Document.Append("<SerialLocation />");
                        //Document.Append("<SerialFifoBucket />");
                        //Document.Append("</Serials>");
                        Document.Append("<WipInspectionReference />");
                        Document.Append("<WipInspectionNarration />");
                        Document.Append("<HierarchyJob>");
                        Document.Append("<Head />");
                        Document.Append("<Section1 />");
                        Document.Append("<Section2 />");
                        Document.Append("<Section3 />");
                        Document.Append("<Section4 />");
                        Document.Append("<CostOfSalesAmount />");
                        Document.Append("</HierarchyJob>");
                        Document.Append("<AddReference>" + item.Lot + "</AddReference>");
                        Document.Append("<MaterialReference />");
                        // Document.Append("<QuantityFromStock />");
                        //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
                        Document.Append("</Item>");
                    }

                    Document.Append("</PostJobReceipts>");


                    string XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), Document.ToString(), "WIPTJR");
                    string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        using (var udb = new WarehouseManagementEntities(""))
                        {
                            foreach (var upd in rev)
                            {
                                var check = (from a in wdb.mtTmpLotsToReverses where a.Job == upd.Job && a.Lot == upd.Lot select a).FirstOrDefault();
                                check.Posted = "Y";
                                udb.Entry(check).State = System.Data.EntityState.Modified;
                                udb.SaveChanges();
                            }
                        }

                    }
                    return ErrorMessage;
                }
                return "No data found.";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostProductionReturnJobReceipt(List<WhseManJobReceipt> detail)
        {
            try
            {



                string Guid = objSyspro.SysproLogin();

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Job Receipt Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostJobReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJRDOC.XSD\">");
                foreach (var item in detail)
                {
                    string Job = item.Job.PadLeft(15, '0');

                    Document.Append("<Item>");
                    Document.Append("<Journal></Journal>");
                    Document.Append("<Job>" + Job + "</Job>");
                    Document.Append("<CoProductLine />");
                    Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");

                    var JobDetails = (from a in wdb.WipMasters where a.Job == Job select a).FirstOrDefault();

                    if (item.Quantity == 0)
                    {
                        var LotQty = (from a in wdb.LotDetails.AsNoTracking() where a.Lot == item.Lot select a.QtyOnHand).FirstOrDefault();
                        if (LotQty == 0)
                        {
                            return "Lot: " + item.Lot + " quantity cannot be zero";
                        }

                        decimal NegativeQty = LotQty * -1;
                        Document.Append("<Quantity>" + NegativeQty.ToString() + "</Quantity>");

                        if (!string.IsNullOrEmpty(JobDetails.SalesOrder))
                        {
                            Document.Append("<QuantityFromStock>" + LotQty + "</QuantityFromStock>");
                        }

                    }
                    else
                    {
                        decimal NegativeQty = item.Quantity * -1;
                        Document.Append("<Quantity>" + NegativeQty.ToString() + "</Quantity>");

                        if (!string.IsNullOrEmpty(JobDetails.SalesOrder))
                        {
                            Document.Append("<QuantityFromStock>" + item.Quantity + "</QuantityFromStock>");
                        }
                    }


                    Document.Append("<InspectionFlag>N</InspectionFlag>");

                    var CostBasis = wdb.sp_GetProductionCostBasis(Job).FirstOrDefault();
                    Document.Append("<CostBasis>" + CostBasis + "</CostBasis>");

                    //Document.Append("<ReceiptCost>441.73</ReceiptCost>");
                    Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
                    Document.Append("<MaterialDistributionValue />");
                    Document.Append("<LaborDistributionValue />");
                    Document.Append("<JobComplete> </JobComplete>");
                    Document.Append("<CoProductComplete>N</CoProductComplete>");
                    Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                    Document.Append("<SalesOrderComplete>N</SalesOrderComplete>");

                    if (JobDetails.TraceableType == "T")
                    {
                        if (!string.IsNullOrEmpty(item.Lot))
                        {
                            Document.Append("<Lot>" + item.Lot + "</Lot>");
                            Document.Append("<LotConcession>" + 1 + "</LotConcession>");
                            Document.Append("<LotExpiryDate></LotExpiryDate>");
                        }
                    }

                    Document.Append("<BinLocation></BinLocation>");
                    Document.Append("<BinOnHold />");
                    Document.Append("<BinOnHoldReason />");
                    Document.Append("<BinUpdateWhDefault />");
                    Document.Append("<FifoBucket />");
                    //Document.Append("<Serials>");
                    //Document.Append("<SerialNumber>8875</SerialNumber>");
                    //Document.Append("<SerialQuantity>12</SerialQuantity>");
                    //Document.Append("<SerialExpiryDate>2011-12-30</SerialExpiryDate>");
                    //Document.Append("<SerialLocation />");
                    //Document.Append("<SerialFifoBucket />");
                    //Document.Append("</Serials>");
                    Document.Append("<WipInspectionReference />");
                    Document.Append("<WipInspectionNarration />");
                    Document.Append("<HierarchyJob>");
                    Document.Append("<Head />");
                    Document.Append("<Section1 />");
                    Document.Append("<Section2 />");
                    Document.Append("<Section3 />");
                    Document.Append("<Section4 />");
                    Document.Append("<CostOfSalesAmount />");
                    Document.Append("</HierarchyJob>");
                    Document.Append("<AddReference>" + item.Lot + "</AddReference>");
                    Document.Append("<MaterialReference />");

                    //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
                    Document.Append("</Item>");
                }

                Document.Append("</PostJobReceipts>");

                string XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), Document.ToString(), "WIPTJR");
                string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    string Username = HttpContext.Current.User.Identity.Name.ToUpper();
                    using (var udb = new WarehouseManagementEntities(""))
                    {
                        foreach (var upd in detail)
                        {
                            string SaveJob = upd.Job;
                            wdb.sp_SaveReturnedProductionBatch(SaveJob, upd.Lot, upd.Quantity, Username);
                        }
                    }

                }
                return ErrorMessage;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string PostWarehouseTransfer(string Pallet, string TransferWarehouse)
        {
            string Guid = "";
            try
            {

                Guid = objSyspro.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to login to Syspro.";
                }

                var PalletItems = (from a in wdb.mtProductionLabels where a.PalletNo == Pallet && a.LabelReceipted == "Y" select a).ToList();
                if (PalletItems.Count > 0)
                {

                    var jobdet = wdb.sp_GetJobDetails(PalletItems.FirstOrDefault().Job.PadLeft(15, '0')).ToList();

                    if (jobdet.FirstOrDefault().Warehouse == TransferWarehouse)
                    {
                        return "";
                    }

                    string Parameter, XmlOut, ErrorMessage;

                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Inventory Warehouse Transfer Out Business Object");
                    Document.Append("-->");
                    Document.Append("<PostInvWhTransferOut xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMODOC.XSD\">");

                    foreach (var item in PalletItems)
                    {
                        Document.Append(this.BuildWarehouseTransferDocument(jobdet.FirstOrDefault().Warehouse, jobdet.FirstOrDefault().Warehouse, jobdet.FirstOrDefault().StockCode, item.BatchId, item.NetQty.ToString(), TransferWarehouse, TransferWarehouse));
                    }
                    Document.Append("</PostInvWhTransferOut>");

                    Parameter = this.BuildWarehouseTransferParameter();
                    XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "INVTMO");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        foreach (var item in PalletItems)
                        {
                            wdb.sp_SaveInvTransfer("Immediate", jobdet.FirstOrDefault().Warehouse, jobdet.FirstOrDefault().Warehouse, TransferWarehouse, TransferWarehouse, jobdet.FirstOrDefault().StockCode, item.BatchId, item.NetQty, Journal, "", HttpContext.Current.User.Identity.Name.ToUpper());
                        }
                        return "Pallet transferred to Warehouse : " + TransferWarehouse + ". Jnl : " + Journal;
                    }
                    else
                    {
                        return "Error : " + ErrorMessage;
                    }
                }
                else
                {
                    return "No items found for warehouse transfer of pallet.";
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(Guid))
                {
                    objSyspro.SysproLogoff(Guid);
                }
            }
        }


        public string BuildWarehouseTransferDocument(string SourceWarehouse, string SourceBin, string StockCode, string Lot, string Quantity, string DestinationWarehouse, string DestinationBin)
        {
            try
            {
                var MultiBins = (from a in wdb.vw_InvWhControl where a.Warehouse.Equals(SourceWarehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();

                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<Immediate>Y</Immediate>");
                Document.Append("<NoDestination>N</NoDestination>");
                Document.Append("<FromWarehouse>" + SourceWarehouse + "</FromWarehouse>");
                if (MultiBins == "Y")
                {
                    Document.Append("<FromBin>" + SourceBin + "</FromBin>");
                }
                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Version/>");
                Document.Append("<Release/>");
                Document.Append("<Quantity>" + Quantity + "</Quantity>");
                Document.Append("<UnitOfMeasure/>");
                Document.Append("<Units/>");
                Document.Append("<Pieces/>");
                Document.Append("<ToWarehouse>" + DestinationWarehouse + "</ToWarehouse>");
                if (MultiBins == "Y")
                {
                    Document.Append("<ToBin>" + DestinationBin + "</ToBin>");
                }
                Document.Append("<ToWhJournal></ToWhJournal>");
                if (TraceableType.TraceableType == "T")
                {
                    Document.Append("<Lot>" + Lot + "</Lot>");
                }

                Document.Append("<Reference>" + Lot + "</Reference>");
                Document.Append("<Notation>" + Lot + "</Notation>");
                Document.Append("<LedgerCode/>");
                //Document.Append("<PasswordForLedgerCode/>");
                //if(TraceableType.SerialMethod != "N")
                //{
                //    Document.Append("<Serials>");
                //    Document.Append("<SerialNumber>BCS11495</SerialNumber>");
                //    Document.Append("<SerialQuantity>1</SerialQuantity>");
                //    Document.Append("<SerialUnits/>");
                //    Document.Append("<SerialPieces/>");
                //    Document.Append("<SerialLocation/>");
                //    Document.Append("</Serials>");
                //    Document.Append("<SerialAllocation>");
                //    Document.Append("<FromSerialNumber>BCS11497</FromSerialNumber>");
                //    Document.Append("<ToSerialNumber>BCS11499</ToSerialNumber>");
                //    Document.Append("<SerialQuantity>3</SerialQuantity>");
                //    Document.Append("</SerialAllocation>");
                //}

                Document.Append("<eSignature/>");
                Document.Append("</Item>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildWarehouseTransferParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory Warehouse Transfer Out Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvWhTransferOut xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMO.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<CreateDestinationWarehouse>N</CreateDestinationWarehouse>");
            Parameter.Append("<TransferCostIfReceivingWhCostZero>N</TransferCostIfReceivingWhCostZero>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvWhTransferOut>");

            return Parameter.ToString();

        }

    }
}