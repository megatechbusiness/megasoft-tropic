using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class clsMaterialIssue
    {
        clsData Data = new clsData();
        SysproCore objSys = new SysproCore();

        public DataTable GetJobs(string FilterText)
        {
            try
            {
                return Data.SelectData("SELECT * FROM WipMaster WITH(NOLOCK) WHERE Complete != 'Y' AND HoldFlag != 'Y' AND ConfirmedFlag = 'Y'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostMaterialIssue(string details, string Username)
        {
            try
            {
                List<MaterialIssue> myDeserializedObjList = (List<MaterialIssue>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MaterialIssue>));
                if (myDeserializedObjList.Count > 0)
                {
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
                        Document.Append(this.BuildMaterialIssueDocument(item.Line, item.Job, item.Warehouse, item.StockCode, item.Quantity.ToString(), item.Shift, item.WorkCentre, item.Reference, item.Lot));

                    }
                    Document.Append("</PostMaterial>");

                    string Guid = objSys.SysproLogin(Username);
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Error : Failed to Log in to Syspro.";
                    }

                    string Parameter, XmlOut, ErrorMessage;

                    Parameter = this.BuildMaterialIssueParameter(myDeserializedObjList.FirstOrDefault().TrnDate);
                    XmlOut = objSys.SysproPost(Guid, Parameter, Document.ToString(), "WIPTMI");
                    objSys.SysproLogoff(Guid);
                    ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        string Journal = objSys.GetXmlValue(XmlOut, "Journal");
                        return "Material Issue posted successfully. Journal : " + Journal;
                    }
                    else
                    {
                        return "Error : " + ErrorMessage;
                    }
                }
                return "Error : No data found to post!";
            }
            catch (Exception ex)
            {
                throw new Exception("Error : " + ex.Message);
            }
        }


        public DataTable GetMaterialAllocationList(string Job)
        {
            try
            {
                return Data.SelectData("EXEC sp_GetMaterialAllocationList '" + Job.PadLeft(15, '0') + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildMaterialIssueDocument(string Line, string Job, string Warehouse, string StockCode, string Quantity, string Shift, string WorkCentre, string Reference, string Lot)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();


                Document.Append("<Item>");
                Document.Append("<Journal />");
                Document.Append("<Job>" + Job + "</Job>");
                Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Line>" + Line + "</Line>");
                Document.Append("<QtyIssued>" + Math.Round(Convert.ToDecimal(Quantity), 3) + "</QtyIssued>");
                Document.Append("<Reference>" + WorkCentre.Trim() + " / " + Shift.Trim() + "</Reference>");
                Document.Append("<Notation>" + Reference + "</Notation>");
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


        public string BuildMaterialIssueParameter(DateTime trnDate)
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
                Parameter.Append("<TransactionDate>" + trnDate.ToString("yyyy-MM-dd") + "</TransactionDate>");
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

        public class MaterialIssue
        {
            public string Job { get; set; }
            public string StockCode { get; set; }
            public string Description { get; set; }
            public string Line { get; set; }
            public decimal QtyRequired { get; set; }
            public decimal QtyIssued { get; set; }
            public decimal Quantity { get; set; }
            public string Shift { get; set; }
            public string WorkCentre { get; set; }
            public string Warehouse { get; set; }
            public DateTime TrnDate { get; set; }
            public string Reference { get; set; }
            public string Lot { get; set; }
            public int NumberofLabels { get; set; }

        }

        public DataTable GetWorkCentre()
        {
            try
            {
                return Data.SelectData("SELECT A.WorkCentre, Description = A.WorkCentre + ' - ' + Description FROM BomWorkCentre A WITH(NOLOCK) LEFT JOIN  [BomWorkCentre+] B WITH (NOLOCK) ON A.WorkCentre = B.WorkCentre WHERE ISNULL(B.ActiveWorkCentre,'Y') != 'N'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetWarehouse()
        {
            try
            {
                //return Data.SelectData("SELECT Warehouse, Description = Warehouse + ' - ' + Description FROM InvWhControl WITH(NOLOCK)");
                return Data.SelectData("SELECT Warehouse, Description = Warehouse + ' - ' + Description FROM InvWhControl WITH(NOLOCK) WHERE Warehouse IN ('RW','MB','PW')");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetShifts()
        {
            try
            {
                return Data.SelectData("SELECT Shift = Reference,Description = Shift FROM tpShifts WITH (NOLOCK)");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetStockCodesByWarehouse(string Warehouse, string FilterText)
        {
            try
            {
                return Data.SelectData("sp_GetStockCodeByWarehouse '" + Warehouse + "','" + FilterText + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetStockCodeDescription(string StockCode)
        {
            try
            {
                return Data.SelectData("SELECT Description FROM InvMaster WITH(NOLOCK) WHERE StockCode = '" + StockCode + "'").Rows[0]["Description"].ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable AddMaterialAllocation(string Job, string StockCode, string Warehouse, string Username)
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
                Document.Append("<Job>" + Job + "</Job>");
                Document.Append("<NonStocked>N</NonStocked>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
                Document.Append("<NewWarehouse />");
                Document.Append("<Line />");
                Document.Append("<ExplodeIfPhantomPart>N</ExplodeIfPhantomPart>");
                Document.Append("<ExplodeIfKitPart>N</ExplodeIfKitPart>");
                Document.Append("<ComponentWhToUse>N</ComponentWhToUse>");
                Document.Append("<QtyReqdType>U</QtyReqdType>");
                Document.Append("<QtyReqd>0</QtyReqd>");
                Document.Append("<FixedQtyPerFlag>N</FixedQtyPerFlag>");
                Document.Append("<FixedQtyPer />");
                Document.Append("<OperationOffset>0001</OperationOffset>");
                Document.Append("<OpOffsetFlag>O</OpOffsetFlag>");
                Document.Append("<SequenceNum />");
                Document.Append("<HierarchyJob>");
                Document.Append("<Section1 />");
                Document.Append("<Section2 />");
                Document.Append("<Section3 />");
                Document.Append("<Section4 />");
                Document.Append("</HierarchyJob>");
                Document.Append("<Version />");
                Document.Append("<Release />");
                Document.Append("<eSignature />");
                Document.Append("<IncludeinKitIssue>N</IncludeinKitIssue>");
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
                //Document.Append("<OverEccSpecIssue />");
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

                string Guid = objSys.SysproLogin(Username);
                if (string.IsNullOrEmpty(Guid))
                {
                    throw new Exception("Error : Failed to Log in to Syspro.");
                }

                string XmlOut = objSys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "WIPTJM");
                string ErrorMessage = objSys.GetXmlErrors(XmlOut);
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    string Line = objSys.GetFirstXmlValue(XmlOut, "Line").Replace(';', ' ').Trim();
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Job");
                    dt.Columns.Add("StockCode");
                    dt.Columns.Add("Warehouse");
                    dt.Columns.Add("Line");

                    DataRow dr = dt.NewRow();
                    dr["Job"] = Job;
                    dr["StockCode"] = StockCode;
                    dr["Warehouse"] = Warehouse;
                    dr["Line"] = Line.Trim();

                    dt.Rows.Add(dr);
                    return dt;
                }
                else
                {
                    throw new Exception(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public decimal GetQtyOnHand(string StockCode, string Warehouse)
        {
            try
            {
                DataTable dt = Data.SelectData("SELECT QtyOnHand FROM InvWarehouse WITH(NOLOCK) WHERE StockCode = '" + StockCode + "' AND Warehouse = '" + Warehouse + "'");
                if (dt.Rows.Count > 0)
                {
                    return (decimal)dt.Rows[0]["QtyOnHand"];

                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string GetTraceable(string StockCode)
        {
            try
            {
                return Data.SelectData("SELECT  CASE WHEN TraceableType = 'T' THEN 'Yes' ELSE 'No' END AS Traceable FROM InvMaster WITH(NOLOCK) WHERE StockCode = '" + StockCode + "'").Rows[0]["Traceable"].ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public DataTable GetLots(string Warehouse, string StockCode)
        {
            try
            {
                return Data.SelectData("sp_GetMaterialIssueLots '" + StockCode + "','" + Warehouse + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}