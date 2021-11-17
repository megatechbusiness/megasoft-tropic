using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class SysproMaterialIssue
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproCore objSyspro = new SysproCore();
        public string PostMaterialIssue(string details)
        {
            try 
            {
                List<MaterialIssue> myDeserializedObjList = (List<MaterialIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MaterialIssue>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Failed to Log in to Syspro.";
                    }


                    if (!string.IsNullOrEmpty(this.AddMaterialAllocation(myDeserializedObjList, Guid)))
                    {
                        return "Failed to Add Material Allocation. Cannot Issue Material!";
                    }

                    string Parameter, XmlOut, ErrorMessage;
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Post Material Business Object");
                    Document.Append("-->");
                    Document.Append("<PostMaterial xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTMIDOC.XSD\">");
                    foreach (var item in myDeserializedObjList)
                    {
                        Document.Append(this.BuildMaterialIssueDocument(item.Job, item.Warehouse, item.StockCode, item.LotNumber, item.Quantity.ToString(), item.WorkCentre, item.Shift));
                    }
                    Document.Append("</PostMaterial>");
                    Parameter = this.BuildMaterialIssueParameter("N");
                    XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "WIPTMI");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        string Journal = objSyspro.GetXmlValue(XmlOut, "Journal");
                        return "Material Issue posted successfully. Journal : " + Journal;
                    }
                    else
                    {
                        return "Error : " + ErrorMessage;
                    }
                }
                return "No data found to post!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string BuildMaterialIssueDocument(string Job, string Warehouse, string StockCode, string Lot, string Quantity, string WorkCentre, string Shift)
        {
            try
            {
                string JobNo = Job.PadLeft(15, '0');
                var Line = (from a in db.WipJobAllMats where a.Job == Job && a.Warehouse == Warehouse && a.StockCode == StockCode && a.AllocCompleted != "Y" select a.Line).Max();

                string LineNo = "00";

                if (!string.IsNullOrWhiteSpace(Line))
                {
                    LineNo = Line.PadLeft(2, '0');
                }

                //Declaration
                StringBuilder Document = new StringBuilder();


                Document.Append("<Item>");
                Document.Append("<Journal />");
                Document.Append("<Job>" + Job + "</Job>");
                Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Line>" + LineNo + "</Line>");
                Document.Append("<QtyIssued>" + Quantity + "</QtyIssued>");
                Document.Append("<Reference>" + WorkCentre + " / " + Shift + "</Reference>");
                Document.Append("<Notation>" + Lot + "</Notation>");
                //Document.Append("<LedgerCode>00-4530</LedgerCode>");
                //Document.Append("<PasswordForLedgerCode />");
                Document.Append("<ProductClass />");
                Document.Append("<UnitCost />");
                Document.Append("<AllocCompleted>N</AllocCompleted>");
                Document.Append("<FifoBucket />");
                if (Warehouse == "RW")
                {
                    Document.Append("<Bins>");
                    Document.Append("<BinLocation>PROD</BinLocation>");
                    Document.Append("<BinQuantity>" + Math.Round(Convert.ToDecimal(Quantity), 3) + "</BinQuantity>");
                    Document.Append("</Bins>");
                }
                if (!string.IsNullOrEmpty(Lot))
                {
                    Document.Append("<Lot>" + Lot + "</Lot>");
                    Document.Append("<LotConcession />");
                }


                //Document.Append("<MaterialReference />");
                //Document.Append("<CoProductLine />");
                //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
                //Document.Append("<Version>");
                //Document.Append("</Version>");
                //Document.Append("<Release>");
                //Document.Append("</Release>");
                Document.Append("</Item>");


                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string BuildMaterialIssueParameter(string AutoDeplete)
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
                Parameter.Append("<AutoDepleteLotsBins>" + AutoDeplete + "</AutoDepleteLotsBins>");
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
        public string AddMaterialAllocation(List<MaterialIssue> details, string Guid)
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


                bool AddAllocation = false;
                foreach (var item in details)
                {
                    string JobNo = item.Job.PadLeft(15, '0');
                    var Mat = (from a in db.WipJobAllMats where a.Job == JobNo && a.StockCode == item.StockCode && a.Warehouse == item.Warehouse select a).ToList();
                    if (Mat.Count == 0)
                    {
                        //Alloc Exists - Don't Add
                        AddAllocation = true;
                        Document.Append("<Item>");
                        Document.Append("<Job>" + item.Job + "</Job>");
                        if (item.Warehouse == "**")
                        {
                            Document.Append("<NonStocked>Y</NonStocked>");
                        }
                        else
                        {
                            Document.Append("<NonStocked>N</NonStocked>");
                        }
                        Document.Append("<StockCode><![CDATA[" + item.StockCode + "]]></StockCode>");
                        Document.Append("<Warehouse>" + item.Warehouse + "</Warehouse>");
                        Document.Append("<NewWarehouse />");
                        Document.Append("<Line />");
                        Document.Append("<ExplodeIfPhantomPart>N</ExplodeIfPhantomPart>");
                        Document.Append("<ExplodeIfKitPart>N</ExplodeIfKitPart>");
                        Document.Append("<ComponentWhToUse>N</ComponentWhToUse>");
                        //Document.Append("<StockDescription><![CDATA[" + item.MStockDes + "]]></StockDescription>");
                        Document.Append("<QtyReqdType>U</QtyReqdType>");
                        Document.Append("<QtyReqd>0</QtyReqd>");
                        Document.Append("<FixedQtyPerFlag>N</FixedQtyPerFlag>");
                        Document.Append("<FixedQtyPer />");
                        //Document.Append("<UnitCost>" + item.MPrice + "</UnitCost>");
                        Document.Append("<OperationOffset>0001</OperationOffset>");
                        Document.Append("<OpOffsetFlag>O</OpOffsetFlag>");
                        //Document.Append("<Uom>" + item.MStockingUom + "</Uom>");
                        Document.Append("<SequenceNum />");
                        Document.Append("<HierarchyJob>");
                        //Document.Append("<Head><![CDATA[" + item.HierHead + "]]></Head>");
                        Document.Append("<Section1 />");
                        Document.Append("<Section2 />");
                        Document.Append("<Section3 />");
                        Document.Append("<Section4 />");
                        Document.Append("</HierarchyJob>");
                        Document.Append("<Version />");
                        Document.Append("<Release />");
                        Document.Append("<eSignature />");
                        Document.Append("<IncludeinKitIssue>Y</IncludeinKitIssue>");
                        Document.Append("<QuantityToReserve />");
                        Document.Append("<ReserveKitPhantComponents>Y</ReserveKitPhantComponents>");
                        //Document.Append("<ComponentType>");
                        //Document.Append("</ComponentType>");
                        //Document.Append("<EccConsumption>");
                        //Document.Append("</EccConsumption>");
                        //Document.Append("<RefDesignator>");
                        //Document.Append("</RefDesignator>");
                        //Document.Append("<AssemblyPlace>");
                        //Document.Append("</AssemblyPlace>");
                        //Document.Append("<ItemNumber>" + item.Grn + "|" + item.Line.ToString().Trim() + "</ItemNumber>");
                        //Document.Append("<OverEccSpecIssue />");
                        Document.Append("</Item>");
                    }


                }

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

                if (AddAllocation == true)
                {
                    string XmlOut = objSyspro.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "WIPTJM");
                    string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);

                    return ErrorMessage;
                }
                return "";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostTempMaterialIssue()
        {
            try
            {
                var items = (from a in db.mtTmpLotsToReverses where a.Posted == "N" select a).ToList();
                //List<MaterialIssue> myDeserializedObjList = (List<MaterialIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<clsMaterialIssue>));
                if (items.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Failed to Log in to Syspro.";
                    }


                    //if (!string.IsNullOrEmpty(this.AddMaterialAllocation(myDeserializedObjList, Guid)))
                    //{
                    //    return "Failed to Add Material Allocation. Cannot Issue Material!";
                    //}

                    string Parameter, XmlOut, ErrorMessage;
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Post Material Business Object");
                    Document.Append("-->");
                    Document.Append("<PostMaterial xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTMIDOC.XSD\">");
                    //foreach (var item in items)
                    //{
                    //    Document.Append(this.BuildMaterialIssueDocument(item.Job, "FM", "NEWCHI036", item.Lot, item.Quantity.ToString()));
                    //}
                    Document.Append("</PostMaterial>");
                    Parameter = this.BuildMaterialIssueParameter("N");
                    XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "WIPTMI");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        string Journal = objSyspro.GetXmlValue(XmlOut, "Journal");
                        return "Material Issue posted successfully. Journal : " + Journal;
                    }
                    else
                    {
                        return "Error : " + ErrorMessage;
                    }
                }
                return "No data found to post!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}