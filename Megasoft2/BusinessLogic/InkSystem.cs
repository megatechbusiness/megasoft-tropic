using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class InkSystem
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproCore sys = new SysproCore();

        public string BuildXmlStructure(InkComponets co)
        {
            //List<BomStructure> check = new List<BomStructure>();
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the BOM Structure Setup Business Object");
            Document.Append("-->");
            Document.Append("<SetupBomStructure xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSSTDOC.XSD\">");
                       
            Document.Append("<Item>");
            Document.Append("<Key>");
            Document.Append("<ParentPart><![CDATA[" + co.ParentPart + "]]></ParentPart>");
            Document.Append("<Version />");
            Document.Append("<Release />");
            Document.Append("<Route><![CDATA[" + co.Route + "]]></Route>");
            Document.Append("<SequenceNum><![CDATA[" + co.SequenceNum + "]]></SequenceNum>");
            Document.Append("<Component><![CDATA[" + co.Component + "]]></Component>");
            Document.Append("</Key>");
            Document.Append("<EccConsumption />");
            Document.Append("<ComVersion />");
            Document.Append("<ComRelease />");
            //Document.Append("<StructureOnDate>2001-10-29</StructureOnDate>");
            //Document.Append("<StructureOffDate>2007-01-01</StructureOffDate>");
            Document.Append("<OpOffsetFlag>O</OpOffsetFlag>");
            Document.Append("<OperationOffset>1</OperationOffset>");
            Document.Append("<UomFlag>S</UomFlag>");
            Document.Append("<QtyPer><![CDATA[" + co.QtyPer + "]]></QtyPer>");
            //Document.Append("<QtyPerEnt>1</QtyPerEnt>");
            Document.Append("<ScrapPercentage><![CDATA[" + co.ScrapPercentage + "]]></ScrapPercentage>");
            Document.Append("<ScrapQuantity><![CDATA[" + co.ScrapQuantity + "]]></ScrapQuantity>");
            //Document.Append("<ScrapQuantityEnt />");
            Document.Append("<SoOptionFlag>N</SoOptionFlag>");
            Document.Append("<SoPrintFlag>Y</SoPrintFlag>");
            Document.Append("<InclScrapFlag>Y</InclScrapFlag>");
            Document.Append("<ReasonForChange />");
            Document.Append("<InclKitIssues>Y</InclKitIssues>");
            Document.Append("<CreateSubJob>N</CreateSubJob>");
            Document.Append("<WetWeightPercent />");
            Document.Append("<IncludeBatch />");
            Document.Append("<IncludeFromJob />");
            Document.Append("<IncludeToJob />");
            Document.Append("<FixedQtyPer />");
            Document.Append("<ComponentType>"+ co.Analox+"</ComponentType>");
            Document.Append("<RefDesignator />");
            Document.Append("<AssemblyPlace />");
            Document.Append("<ItemNumber />");
            Document.Append("<FixedQtyPerEnt />");
            Document.Append("<FixedQtyPerFlag>N</FixedQtyPerFlag>");
            Document.Append("<Warehouse />");
            Document.Append("<IgnoreFloorFlag>N</IgnoreFloorFlag>");
            //Document.Append("<eSignature>{36303032-3330-3031-3038-323434363433}</eSignature>");
            Document.Append("<OvrEccSpecIss />");
            Document.Append("</Item>");
            

            Document.Append("</SetupBomStructure>");


            return Document.ToString();
        }

        public string BuildXmlOperations(InkBomOperation op, string ActionType)
        {
            StringBuilder Document = new StringBuilder();
            //string ActionType = "";
            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the BOM Routing Business Object");
            Document.Append("-->");
            Document.Append("<SetupBomRouting xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSRODOC.XSD\">");

           
            var ETCalcMethod = db.sp_mtInkSystemGetWorkCentreDetails(op.WorkCentre).ToList().FirstOrDefault().EtCalcMeth;
            
            Document.Append("<Item>");
            Document.Append("<Key>");
            Document.Append("<StockCode><![CDATA[" + op.StockCode + "]]></StockCode>");
            Document.Append("<Version />");
            Document.Append("<Release />");
            Document.Append("<Route><![CDATA[" + op.Route + "]]></Route>");
            
            if (ActionType!="A")
            {
                Document.Append("<Operation>" + op.Operation + "</Operation>");
            }

            Document.Append("</Key>");
            Document.Append("<WorkCentre><![CDATA[" + op.WorkCentre + "]]></WorkCentre>");
            Document.Append("<WcRateInd>1</WcRateInd>");
            Document.Append("<ISetUpTime />");
            if (ETCalcMethod == "U")
            {
                Document.Append("<IRunTime><![CDATA[" + op.UnitRunTime + "]]></IRunTime>");
            }
            else
            {
                Document.Append("<IQuantity><![CDATA[" + op.Quantity + "]]></IQuantity>");
                Document.Append("<ITimeTaken><![CDATA[" + op.TimeTaken + "]]></ITimeTaken>");
            }

            Document.Append("<Milestone>N</Milestone>");
            Document.Append("</Item>");
            
            Document.Append("</SetupBomRouting>");



            return Document.ToString();
        }

        public string BuildBomOpsParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the parameters used in BOM Routing Setup Business Object");
            Parameter.Append("-->");
            Parameter.Append("<SetupBomRouting xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSRO.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<UseStockingQuantities>Y</UseStockingQuantities>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</SetupBomRouting>");

            return Parameter.ToString();
        }

        public string BuildBomNarration(InkBomOperation narr)
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
            Document.Append("<Source>BOMOPERATION</Source>");
            Document.Append("<JobNumber></JobNumber>");
            Document.Append("<ParentPart><![CDATA[" + narr.StockCode + "]]></ParentPart>");
            Document.Append("<Version></Version>");
            Document.Append("<Release></Release>");
            Document.Append("<Component></Component>");
            Document.Append("<Route>"+narr.Route+"</Route>");

            Document.Append("<Operation>" + narr.Operation + "</Operation>");
            Document.Append("<SequenceNum></SequenceNum>");
            Document.Append("<Warehouse></Warehouse>");
            Document.Append("<Line></Line>");
            Document.Append("</Key>");
            Document.Append("<Narration><![CDATA[" + narr.Narrations + "]]></Narration>");
            Document.Append("</Item>");

            Document.Append("</SetupBomNarration>");

            return Document.ToString();
        }

        public string BuildBomNarrationParameter()
        {
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
            return Parameter.ToString();
        }
        
        public string BuildBomStructureParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the BOM Structure Setup Business Object");
            Parameter.Append("-->");
            Parameter.Append("<SetupBomStructure xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSST.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<UseStockingQuantities>Y</UseStockingQuantities>");
            Parameter.Append("<AllowRevertStkUom>N</AllowRevertStkUom>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</SetupBomStructure>");

            return Parameter.ToString();
        }

        public string PostBomOperation(InkBomOperation bom, string ActionType)
        {
            try
            {
                string PostMessage = "";
                string Guid = sys.SysproLogin();
                //Declaration
                //BuildXmlOperations(bom, ActionType);
                string XmlOut;
                if (ActionType == "A")
                {
                    XmlOut = sys.SysproSetupAdd(Guid, BuildBomOpsParameter(), BuildXmlOperations(bom,ActionType), "BOMSRO");
                }
                else if (ActionType == "C")
                {
                    XmlOut = sys.SysproSetupUpdate(Guid, BuildBomOpsParameter(), BuildXmlOperations(bom, ActionType), "BOMSRO");
                }
                else
                {
                    XmlOut = sys.SysproSetupDelete(Guid, BuildBomOpsParameter(), BuildXmlOperations(bom, ActionType), "BOMSRO");
                }
              
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    return "Failed to create BOM Operations : " + ErrorMessage;
                }
                if (ActionType=="A")
                {
                    PostMessage = "Bom Operation Added Successfully!";
                }
                else if(ActionType=="C")
                {
                    PostMessage = "Bom Operation Changes Posted Successfully!";
                }
                else
                {
                    PostMessage = "Bom Operation Deleted Successfully!";
                }

                //else
                //{
                //    if (bom.Narrations!=null)
                //    {
                //        XmlOut = sys.SysproSetupDelete(Guid, BuildBomNarrationParameter(), BuildBomNarration(bom), "BOMSSN");
                //    }
                //}
                sys.SysproLogoff(Guid);
                return PostMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string PostAltBomNarration(InkBomOperation bom, string ActionType)
        {
            string PostMessage = "";
            string Guid = sys.SysproLogin();
            string XmlOut;

            if (ActionType == "A")
            {
                XmlOut = sys.SysproSetupAdd(Guid, BuildBomNarrationParameter(), BuildBomNarration(bom), "BOMSSN");
            }
            else if (ActionType == "C")
            {
                XmlOut = sys.SysproSetupUpdate(Guid, BuildBomNarrationParameter(), BuildBomNarration(bom), "BOMSSN");
            }
            else
            {
                XmlOut = sys.SysproSetupDelete(Guid, BuildBomNarrationParameter(), BuildBomNarration(bom), "BOMSSN");
            }
           string ErrorMessage = sys.GetXmlErrors(XmlOut);
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                return "Failed to add BOM Narrations : " + ErrorMessage;
            }
            if (ActionType == "A")
            {
                PostMessage = "Bom Narration Added Successfully!";
            }
            else if (ActionType == "C")
            {
                PostMessage = "Bom Narration Changes Posted Successfully!";
            }
            else
            {
                PostMessage = "Bom Narration Deleted Successfully!";
            }
            sys.SysproLogoff(Guid);
            return PostMessage;

        }

        public string PostBomStructure(InkComponets bom, string ActionType)
        {
            try
            {
                string PostMessage = "";
                string Guid = sys.SysproLogin();
                //Declaration
                BuildXmlStructure(bom);

                string XmlOut;
                if (ActionType == "A")
                {
                    XmlOut = sys.SysproSetupAdd(Guid, BuildBomStructureParameter(), BuildXmlStructure(bom), "BOMSST");
                }
                else if (ActionType == "C")
                {
                    XmlOut = sys.SysproSetupUpdate(Guid, BuildBomStructureParameter(), BuildXmlStructure(bom), "BOMSST");
                }
                else
                {
                    XmlOut = sys.SysproSetupDelete(Guid, BuildBomStructureParameter(), BuildXmlStructure(bom), "BOMSST");
                }
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    return "Failed to create BOM Structure : " + ErrorMessage;
                }

                if (ActionType == "A")
                {
                    PostMessage = "Bom Component Added Successfully!";
                }
                else if (ActionType == "C")
                {
                    PostMessage = "Bom Component Changes Posted Successfully!";
                }
                else
                {
                    PostMessage = "Bom Component Deleted Successfully!";
                }

                sys.SysproLogoff(Guid);
                return PostMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}