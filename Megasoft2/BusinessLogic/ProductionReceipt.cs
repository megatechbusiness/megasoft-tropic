using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Megasoft2.BusinessLogic
{
    public class ProductionReceipt
    {

        SysproCore objSyspro = new SysproCore();
        SysproMaterialIssue objMat = new SysproMaterialIssue();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private SysproCore sys = new SysproCore();
        public string PostProductionReceipt(string details)
        {
            string Guid="";
            try
            {
                Guid = objSyspro.SysproLogin();
                if(string.IsNullOrEmpty(Guid))
                {
                    return "Failed to login to Syspro.";
                }
                else
                {
                    List<Models.ProductionReceipt> myDeserializedObjList = (List<Models.ProductionReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<Models.ProductionReceipt>));
                    if (myDeserializedObjList.Count > 0)
                    {
                        string ReturnMessage = "";
                        foreach (var item in myDeserializedObjList)
                        {
                            ReturnMessage = this.PostMaterialIssue(Guid, item.Job, item.Quantity); //Any errors will be thrown as exception;
                            ReturnMessage += this.PostLabourIssue(Guid, item.Job, item.Quantity); // Will be empty string if Posted Successfully;
                            ReturnMessage += this.PostJobReceipt(Guid, item.Job, item.Quantity, item.LotNumber);                            
                        }
                        return ReturnMessage;
                    }
                    return "No data found to post.";
                }
                
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                objSyspro.SysproLogoff(Guid);
            }
        }

        public string GetMaterialBuild(string Guid, string Job, decimal Quantity)
        {
            try
            {
                string XmlOut, ErrorMessage;
                XmlOut = objSyspro.SysproBuild(Guid, this.BuildMaterialDocument(Job, Quantity), "WIPRMI");
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if(string.IsNullOrEmpty(ErrorMessage))
                {
                    return XmlOut;
                }
                else
                {
                    return ErrorMessage;
                }
            }
            catch(Exception ex)
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
            Document.Append("<IssueNegativeAllocations>Y</IssueNegativeAllocations>");
            Document.Append("<IssueCompletedAllocations>Y</IssueCompletedAllocations>");
            Document.Append("<IssueToMaxOutstanding>N</IssueToMaxOutstanding>");
            Document.Append("<ReturnValidMaterialsOnly>Y</ReturnValidMaterialsOnly>");
            Document.Append("<ReturnWhenQtyIssueZero>Y</ReturnWhenQtyIssueZero>");
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

        public string PostMaterialIssue(string Guid, string Job, decimal Quantity)
        {
            try
            {
                string BuildXml = this.GetMaterialBuild(Guid, Job, Quantity);

                if (BuildXml.StartsWith("There is no material to issue for job"))
                {
                    return "";
                }

                var XDoc = XDocument.Parse(BuildXml);
                var items = (from x in XDoc.Descendants("Item")
                             select new MaterialItems
                             {
                                 Job = x.Element("Job").Value,
                                 Warehouse = x.Element("Warehouse").Value,
                                 StockCode = x.Element("StockCode").Value,
                                 Quantity = Convert.ToDecimal(x.Element("QuantityToIssue").Value)
                             }).ToList();

                StringBuilder Document = new StringBuilder();

                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Post Material Business Object");
                Document.Append("-->");
                Document.Append("<PostMaterial xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTMIDOC.XSD\">");

                foreach(var item in items)
                {
                    Document.Append("<Item>");
                    Document.Append("<Journal />");
                    Document.Append("<Job>" + Job + "</Job>");
                    Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                    Document.Append("<Warehouse>" + item.Warehouse + "</Warehouse>");
                    Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                    Document.Append("<Line>00</Line>");
                    Document.Append("<QtyIssued>" + item.Quantity + "</QtyIssued>");
                    Document.Append("<Reference>Megasoft</Reference>");
                    Document.Append("<Notation>Test INV issue</Notation>");
                    Document.Append("<ProductClass />");
                    Document.Append("<UnitCost />");
                    Document.Append("<AllocCompleted>N</AllocCompleted>");
                    Document.Append("<FifoBucket />");
                    //Document.Append("<Lot>" + Lot + "</Lot>");
                    //Document.Append("<LotConcession />");                    
                    Document.Append("</Item>");
                }
                Document.Append("</PostMaterial>");

                string XmlOut, ErrorMessage;

                XmlOut = objSyspro.SysproPost(Guid, objMat.BuildMaterialIssueParameter("Y"), Document.ToString(), "WIPTMI");
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if(string.IsNullOrEmpty(ErrorMessage))
                {
                    return "";
                }
                else
                {
                    throw new Exception(ErrorMessage);
                }

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class MaterialItems
        {
            public string Job { get; set; }
            public string  Warehouse { get; set; }
            public string StockCode { get; set; }
            public decimal Quantity { get; set; }

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
            catch(Exception ex)
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
            Document.Append("<IgnoreWarnings>N</IgnoreWarnings>");
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
                string BuildXml = this.GetLabourBuild(Guid, Job, Quantity);

                if(BuildXml.StartsWith("Labour Build Error: "))
                {
                    return BuildXml;
                }

                var XDoc = XDocument.Parse(BuildXml);
                var items = (from x in XDoc.Descendants("Item")
                             select new LabourItems
                             {
                                 Job = x.Element("Job").Value,
                                 Operation = x.Element("LOperation").Value,
                                 WorkCentre = x.Element("LWorkCentre").Value,
                                 RunTime = x.Element("LRunTimeHours").Value,
                                 QtyCompleted = x.Element("LQtyComplete").Value
                             }).ToList();

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

                foreach(var item in items)
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
                    Document.Append("<LRunTimeHours>" + item.RunTime + "</LRunTimeHours>");
                    //Document.Append("<LSetUpHours>1</LSetUpHours>");
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
                    Document.Append("<Reference />");
                    Document.Append("<AdditionalReference />");
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
                    Document.Append("<LQtyComplete>" + item.QtyCompleted + "</LQtyComplete>");
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
            catch(Exception ex)
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
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<PostingPeriod>C</PostingPeriod>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<UpdateQtyToMakeWithScrap>N</UpdateQtyToMakeWithScrap>");
            Parameter.Append("<UncompleteNonMile>N</UncompleteNonMile>");
            Parameter.Append("<UseWCRateIfEmpRateZero>Y</UseWCRateIfEmpRateZero>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostLabour>");

            return Parameter.ToString();
        }


        public string BuildJobReceiptDocument(string Job, decimal Quantity, string Lot)
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
            Document.Append("<Item>");
            Document.Append("<Journal></Journal>");
            Document.Append("<Job>" + Job + "</Job>");
            Document.Append("<CoProductLine />");
            Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
            Document.Append("<Quantity>" + Quantity.ToString() + "</Quantity>");
            Document.Append("<InspectionFlag>N</InspectionFlag>");
            Document.Append("<CostBasis>C</CostBasis>");
            //Document.Append("<ReceiptCost>441.73</ReceiptCost>");
            Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
            Document.Append("<MaterialDistributionValue />");
            Document.Append("<LaborDistributionValue />");
            Document.Append("<JobComplete>N</JobComplete>");
            Document.Append("<CoProductComplete>N</CoProductComplete>");
            Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
            Document.Append("<SalesOrderComplete>N</SalesOrderComplete>");
            if(!string.IsNullOrEmpty(Lot))
            {
                Document.Append("<Lot>" + Lot + "</Lot>");
                Document.Append("<LotConcession>" + Job + "</LotConcession>");
                Document.Append("<LotExpiryDate></LotExpiryDate>");
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
            Document.Append("<AddReference />");
            Document.Append("<MaterialReference />");
            Document.Append("<QuantityFromStock />");
            //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
            Document.Append("</Item>");
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
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<SetJobToCompleteIfCoProductsComplete>N</SetJobToCompleteIfCoProductsComplete>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostJobReceipts>");

            return Parameter.ToString();
        }


        public string PostJobReceipt(string Guid, string Job, decimal Quantity, string Lot)
        {
            try
            {
                string XmlOut, ErrorMessage;
                XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), this.BuildJobReceiptDocument(Job, Quantity, Lot), "WIPTJR");
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    return "Job Receipt Completed Successfully";
                }
                else
                {
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