using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class MasterCard
    {
        SysproCore sys = new SysproCore();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        public string BuildStockCodeDocument(mtMasterCardStockCode model, string Dimensions, string Micron)
        {
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
            Document.Append("<StockCode><![CDATA[" + model.StockCode + "]]></StockCode>");
            Document.Append("</Key>");
            Document.Append("<Description><![CDATA[" + model.Description + "]]></Description>");
            Document.Append("<LongDesc><![CDATA[" + model.LongDesc + "]]></LongDesc>");
            Document.Append("<AlternateKey1><![CDATA[" + Dimensions + "]]></AlternateKey1>");
            Document.Append("<AlternateKey2><![CDATA[" + Micron + "]]></AlternateKey2>");
            //Document.Append("<EccUser/>");<![CDATA[    ]]>
            Document.Append("<StockUom><![CDATA[" + model.StockUom + "]]></StockUom>");
            Document.Append("<AlternateUom>" + model.AlternateUom + "</AlternateUom>");
            Document.Append("<OtherUom>" + model.OtherUom + "</OtherUom>");
            Document.Append("<ConvFactAltUom>" + model.ConvFactAltUom + "</ConvFactAltUom>");
            Document.Append("<ConvMulDiv>" + model.ConvMulDiv + "</ConvMulDiv>");
            Document.Append("<ConvFactOthUom>" + model.ConvFactOthUom + "</ConvFactOthUom>");
            Document.Append("<MulDiv>" + model.MulDiv + "</MulDiv>");
            Document.Append("<Mass>" + model.Mass + "</Mass>");
            //Document.Append("<Volume>0.000000</Volume>");
            Document.Append("<Decimals>" + model.Decimals + "</Decimals>");
            Document.Append("<PriceCategory>A</PriceCategory>");
            Document.Append("<PriceMethod>C</PriceMethod>");
            //Document.Append("<ReturnableItem>N</ReturnableItem>");
            //Document.Append("<Supplier></Supplier>");
            //Document.Append("<CycleCount>0</CycleCount>");
            Document.Append("<ProductClass>" + model.ProductClass + "</ProductClass>");
            Document.Append("<TaxCode>A</TaxCode>");
            //Document.Append("<OtherTaxCode>A</OtherTaxCode>");
            //Document.Append("<AddListPrice>Y</AddListPrice>");
            Document.Append("<ListPriceCode>A</ListPriceCode>");
            //Document.Append("<SellingPrice>" + SellingPrice + "</SellingPrice>");
            Document.Append("<PriceBasis>S</PriceBasis>");
            //Document.Append("<CommissionCode>0</CommissionCode>");
            //Document.Append("<SerialMethod>N</SerialMethod>");
            //Document.Append("<SerialPrefix/>");
            //Document.Append("<SerialSuffix/>");
            //Document.Append("<KitType>N</KitType>");
            //Document.Append("<Buyer/>");
            //Document.Append("<Planner/>");
            Document.Append("<TraceableType>" + model.Traceable + "</TraceableType>");
            Document.Append("<InspectionFlag>N</InspectionFlag>");
            //Document.Append("<MpsFlag>Y</MpsFlag>");
            //Document.Append("<BulkIssueFlag>N</BulkIssueFlag>");
            //Document.Append("<LeadTime>0</LeadTime>");
            //Document.Append("<StockMovementReq>Y</StockMovementReq>");
            //Document.Append("<ClearingFlag>N</ClearingFlag>");
            //Document.Append("<SupercessionDate>2010-10-31</SupercessionDate>");
            //Document.Append("<AbcAnalysisReq>Y</AbcAnalysisReq>");
            //Document.Append("<AbcCostingReq>N</AbcCostingReq>");
            //Document.Append("<ManualCostFlag>N</ManualCostFlag>");
            //Document.Append("<CostUom></CostUom>");
            //Document.Append("<MinPricePct/>");
            //Document.Append("<LabourCost/>");
            //Document.Append("<MaterialCost/>");
            //Document.Append("<FixOverhead/>");
            //Document.Append("<SubContractCost/>");
            //Document.Append("<VariableOverhead/>");
            Document.Append("<PartCategory>" + model.PartCategory + "</PartCategory>");
            //Document.Append("<DrawOfficeNum/>");
            Document.Append("<WarehouseToUse>" + model.WarehouseToUse + "</WarehouseToUse>");
            //Document.Append("<BuyingRule>A</BuyingRule>");
            //Document.Append("<SpecificGravity>0.0000</SpecificGravity>");
            //Document.Append("<Ebq>1.000</Ebq>");
            //Document.Append("<FixTimePeriod>1</FixTimePeriod>");
            //Document.Append("<PanSize>0.000</PanSize>");
            //Document.Append("<DockToStock>0</DockToStock>");
            Document.Append("<OutputMassFlag>F</OutputMassFlag>");
            //Document.Append("<ShelfLife>0</ShelfLife>");
            //Document.Append("<Version/>");
            //Document.Append("<Release/>");
            //Document.Append("<DemandTimeFence>0</DemandTimeFence>");
            Document.Append("<MakeToOrderFlag>N</MakeToOrderFlag>");
            //Document.Append("<ManufLeadTime/>");
            //Document.Append("<GrossReqRule>I</GrossReqRule>");
            //Document.Append("<PercentageYield>100</PercentageYield>");
            //Document.Append("<WipCtlGlCode/>");
            //Document.Append("<ResourceCode/>");
            //Document.Append("<GstTaxCode/>");
            //Document.Append("<PrcInclGst>N</PrcInclGst>");
            //Document.Append("<SerEntryAtSale/>");
            //Document.Append("<UserField1/>");
            //Document.Append("<UserField2/>");
            //Document.Append("<UserField3/>");
            //Document.Append("<UserField4/>");
            //Document.Append("<UserField5/>");
            //Document.Append("<TariffCode/>");
            Document.Append("<SupplementaryUnit>N</SupplementaryUnit>");
            //Document.Append("<EbqPan>E</EbqPan>");
            Document.Append("<LctRequired>N</LctRequired>");
            //Document.Append("<IssMultLotsFlag>Y</IssMultLotsFlag>");
            //Document.Append("<InclInStrValid>Y</InclInStrValid>");
            //Document.Append("<CountryOfOrigin/>");
            //Document.Append("<StockOnHold/>");
            //Document.Append("<StockOnHoldReason/>");
            Document.Append("<EccFlag>N</EccFlag>");
            Document.Append("<StockAndAltUm>N</StockAndAltUm>");
            //Document.Append("<BatchBill>N</BatchBill>");
            //Document.Append("<DistWarehouseToUse></DistWarehouseToUse>");
            Document.Append("<JobClassification>" + model.JobClassification + "</JobClassification>");
            //Document.Append("<ProductGroup/>");
            //Document.Append("<PriceType/>");
            //Document.Append("<Basis/>");
            //Document.Append("<ManufactureUom/>");
            //Document.Append("<ConvFactMuM/>");
            //Document.Append("<ManMulDiv/>");
            //Document.Append("<PhantomIfComp/>");
            //Document.Append("<AltMethodFlag/>");
            //Document.Append("<AltSisoFlag/>");
            //Document.Append("<AltReductionFlag/>");
            //Document.Append("<WithTaxExpenseType>G</WithTaxExpenseType>");
            //Document.Append("<eSignature>{36303032-3330-3031-3038-323434363433}</eSignature>");
            Document.Append("</Item>");
            Document.Append("</SetupInvMaster>");

            return Document.ToString();
        }
        public string BuildStockCodeParameter()
        {
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

            return Parameter.ToString();
        }
        public string BuildStockCodeWarehouseDocument(string StockCode, string Warehouse)
        {
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
            Document.Append("<StockCode>" + StockCode + "</StockCode>");
            Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
            Document.Append("</Key>");
            Document.Append("<CostMultiplier>1.000</CostMultiplier>");
            Document.Append("<MinimumQty/>");
            Document.Append("<MaximumQty/>");
            Document.Append("<UnitCost>0</UnitCost>");
            Document.Append("<DefaultBin>" + Warehouse + "</DefaultBin>");
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

            return Document.ToString();
        }
        public string BuildStockCodeWarehouseParameter()
        {
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

            return Parameter.ToString();
        }
        public string BuildStockCodePricingDocument(string StockCode, string PriceCode, decimal SellingPrice, string PriceBasis)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the Stock code Price Setup Business Object");
            Document.Append("-->");
            Document.Append("<SetupInvPrice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSPRDOC.XSD\">");
            Document.Append("<Item>");
            Document.Append("<Key>");
            Document.Append("<StockCode>" + StockCode + "</StockCode>");
            Document.Append("<PriceCode>" + PriceCode + "</PriceCode>");
            Document.Append("</Key>");
            Document.Append("<SellingPrice>" + SellingPrice + "</SellingPrice>");
            Document.Append("<PriceBasis>" + PriceBasis + "</PriceBasis>");
            Document.Append("<CommissionCode>0</CommissionCode>");
            Document.Append("</Item>");
            Document.Append("</SetupInvPrice>");

            return Document.ToString();
        }
        public string BuildStockCodePricingParamter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate the");
            Parameter.Append("parameters used by the Stock Code Price Setup Business Object");
            Parameter.Append("-->");
            Parameter.Append("<SetupInvPrice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSPR.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</SetupInvPrice>");

            return Parameter.ToString();
        }

        public string BuildMultimediaDocument(int Id)
        {
            //Declaration
            var MasterCardId = (from a in wdb.mtMasterCardHeaders where a.Id == Id select a).FirstOrDefault();

            StringBuilder Document = new StringBuilder();
            ///Building Document Content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2016 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Multimedia Setup Business Object");
            Document.Append("-->");
            Document.Append("<SetupMultimedia xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSMMDOC.XSD\">");
            Document.Append("<Item>");
            Document.Append("<Key>");
            Document.Append("<MultimediaFlag>STK</MultimediaFlag>");
            Document.Append("<KeyField>" + MasterCardId.StockCode + "</KeyField>");
            Document.Append("<ObjectType>BMP</ObjectType>");
            Document.Append(" </Key>");
            Document.Append("<ObjectText><![CDATA[" + MasterCardId.MultiMediaFilePath + "]]></ObjectText>");
            Document.Append("<MultimediaObject><![CDATA[" + MasterCardId.MultiMediaFilePath + "]]></MultimediaObject>");
            Document.Append("</Item>");
            Document.Append("</SetupMultimedia>");

            return Document.ToString();
        }

        public string BuildMultimediaParameter()
        {

            //Declaration
            StringBuilder Parameter = new StringBuilder();
            //
            //Building Parameter Content
            Parameter.Append("<?xml version=\"1.0\"encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2016 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Multimedia Setup Business Object");
            Parameter.Append("-->");
            Parameter.Append("<SetupMultimedia xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSMM.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<OutputErrorsAsXml>N</OutputErrorsAsXml>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</SetupMultimedia>");

            return Parameter.ToString();
        }


        public string PostMasterCardMultimedia(MasterCardViewModel model)
        {
            try
            {
                string Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    return "Failed to login to Syspro.";
                }

                string Document, Parameter, XmlOut, ErrorMessage;
                var StockCodeCheck = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == model.stkobj.StockCode select a).ToList();
                if (StockCodeCheck.Count > 0)
                {
                    Document = BuildMultimediaDocument(model.MasterCardId);
                    Parameter = BuildMultimediaParameter();
                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "COMSMM");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Multimedia Document : " + model.stkobj.StockCode + ". " + ErrorMessage;
                    }
                    else
                    {
                        return "";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string BuildCustomFormDocument(MasterCardViewModel model)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();
            //
            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Custom Form Post Business Object");
            Document.Append("-->");
            Document.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFMDOC.XSD\">");
            Document.Append("<Item>");
            Document.Append("<Key>");
            Document.Append("<FormType>STK</FormType>");
            Document.Append("<KeyFields>");

            Document.Append("<StockCode>" + model.stkobj.StockCode + "</StockCode>");

            Document.Append("</KeyFields>");
            Document.Append("</Key>");
            Document.Append("<Fields>");

            Document.Append(model.CustomForms.InvoiceDim == null ? "" : "<InvoiceDim>" + model.CustomForms.InvoiceDim + "</InvoiceDim>");
            Document.Append(model.CustomForms.BarCode == null ? "" : "<BarCode>" + model.CustomForms.BarCode + "</BarCode>");
            Document.Append(model.CustomForms.GenWidth == null ? "" : "<GenWidth>" + model.CustomForms.GenWidth + "</GenWidth>");
            Document.Append(model.CustomForms.GenLength == null ? "" : "<GenLength>" + model.CustomForms.GenLength + "</GenLength>");
            Document.Append(model.CustomForms.GenLayFlatWidthSiz == null ? "" : "<GenLayFlatWidthSiz>" + model.CustomForms.GenLayFlatWidthSiz + "</GenLayFlatWidthSiz>");
            Document.Append(model.CustomForms.GenMicron == null ? "" : "<GenMicron>" + model.CustomForms.GenMicron + "</GenMicron>");
            Document.Append(model.CustomForms.GenTreatment == null ? "" : "<GenTreatment>" + model.CustomForms.GenTreatment + "</GenTreatment>");
            Document.Append(model.CustomForms.GenDyneValue == null ? "" : "<GenDyneValue>" + model.CustomForms.GenDyneValue + "</GenDyneValue>");
            //Document.Append(model.CustomForms.GenSlit == null ? "" : "<GenSlit>" + model.CustomForms.GenSlit + "</GenSlit>");
            Document.Append(model.CustomForms.GenInkCost1000 == null ? "" : "<GenInkCost1000>" + model.CustomForms.GenInkCost1000 + "</GenInkCost1000>");
            Document.Append(model.CustomForms.GenPalletWrapRoll == null ? "" : "<GenPalletWrapRoll>" + model.CustomForms.GenPalletWrapRoll + "</GenPalletWrapRoll>");
            Document.Append(model.CustomForms.GenPalletRolls == null ? "" : "<GenPalletRolls>" + model.CustomForms.GenPalletRolls + "</GenPalletRolls>");
            Document.Append(model.CustomForms.PrintPrintFront == null ? "" : "<PrintPrintFront>" + model.CustomForms.PrintPrintFront + "</PrintPrintFront>");
            Document.Append(model.CustomForms.PrintPrintBack == null ? "" : "<PrintPrintBack>" + model.CustomForms.PrintPrintBack + "</PrintPrintBack>");
            Document.Append(model.CustomForms.PrintStepSize == null ? "" : "<PrintStepSize>" + model.CustomForms.PrintStepSize + "</PrintStepSize>");
            Document.Append(model.CustomForms.PrintCylinderSize == null ? "" : "<PrintCylinderSize>" + model.CustomForms.PrintCylinderSize + "</PrintCylinderSize>");
            Document.Append(model.CustomForms.PrintAround == null ? "" : "<PrintAround>" + model.CustomForms.PrintAround + "</PrintAround>");
            Document.Append(model.CustomForms.PrintAcross == null ? "" : "<PrintAcross>" + model.CustomForms.PrintAcross + "</PrintAcross>");
            Document.Append(model.CustomForms.PrintCoverageF == null ? "" : "<PrintCoverageF>" + model.CustomForms.PrintCoverageF + "</PrintCoverageF>");
            Document.Append(model.CustomForms.PrintCoverageB == null ? "" : "<PrintCoverageB>" + model.CustomForms.PrintCoverageB + "</PrintCoverageB>");
            Document.Append(model.CustomForms.PrintLinePrint == null ? "" : "<PrintLinePrint>" + model.CustomForms.PrintLinePrint + "</PrintLinePrint>");
            Document.Append(model.CustomForms.BagWidthSize == null ? "" : "<BagWidthSize>" + model.CustomForms.BagWidthSize + "</BagWidthSize>");
            Document.Append(model.CustomForms.BagLengthSize == null ? "" : "<BagLengthSize>" + model.CustomForms.BagLengthSize + "</BagLengthSize>");
            Document.Append(model.CustomForms.BagTopGusset == null ? "" : "<BagTopGusset>" + model.CustomForms.BagTopGusset + "</BagTopGusset>");
            Document.Append(model.CustomForms.BagBottomGusset == null ? "" : "<BagBottomGusset>" + model.CustomForms.BagBottomGusset + "</BagBottomGusset>");
            Document.Append(model.CustomForms.BagRightGusset == null ? "" : "<BagRightGusset>" + model.CustomForms.BagRightGusset + "</BagRightGusset>");
            Document.Append(model.CustomForms.BagLeftGusset == null ? "" : "<BagLeftGusset>" + model.CustomForms.BagLeftGusset + "</BagLeftGusset>");
            Document.Append(model.CustomForms.BagLipSize == null ? "" : "<BagLipSize>" + model.CustomForms.BagLipSize + "</BagLipSize>");
            Document.Append(model.CustomForms.BagHeaderSeal == null ? "" : "<BagHeaderSeal>" + model.CustomForms.BagHeaderSeal + "</BagHeaderSeal>");
            Document.Append(model.CustomForms.BagSealType == null ? "" : "<BagSealType>" + model.CustomForms.BagSealType + "</BagSealType>");
            Document.Append(model.CustomForms.BagPerPack == null ? "" : "<BagPerPack>" + model.CustomForms.BagPerPack + "</BagPerPack>");
            Document.Append(model.CustomForms.BagPerBale == null ? "" : "<BagPerBale>" + model.CustomForms.BagPerBale + "</BagPerBale>");
            Document.Append(model.CustomForms.ExtrRollsUp == null ? "" : "<ExtrRollsUp>" + model.CustomForms.ExtrRollsUp + "</ExtrRollsUp>");
            Document.Append(model.CustomForms.ExtrKgPerRoll == null ? "" : "<ExtrKgPerRoll>" + model.CustomForms.ExtrKgPerRoll + "</ExtrKgPerRoll>");
            Document.Append(model.CustomForms.ExtrMetresPerRoll == null ? "" : "<ExtrMetresPerRoll>" + model.CustomForms.ExtrMetresPerRoll + "</ExtrMetresPerRoll>");
            Document.Append(model.CustomForms.ExtrLFWidthSize == null ? "" : "<ExtrLFWidthSize>" + model.CustomForms.ExtrLFWidthSize + "</ExtrLFWidthSize>");
            Document.Append(model.CustomForms.ExtrDoubleWind == null ? "" : "<ExtrDoubleWind>" + model.CustomForms.ExtrDoubleWind + "</ExtrDoubleWind>");
            Document.Append(model.CustomForms.ExtrCoreWeight == null ? "" : "<ExtrCoreWeight>" + model.CustomForms.ExtrCoreWeight + "</ExtrCoreWeight>");
            Document.Append(model.CustomForms.SlitSheetSlits == null ? "" : "<SlitSheetSlits>" + model.CustomForms.SlitSheetSlits + "</SlitSheetSlits>");
            Document.Append(model.CustomForms.SlitRollsUp == null ? "" : "<SlitRollsUp>" + model.CustomForms.SlitRollsUp + "</SlitRollsUp>");
            Document.Append(model.CustomForms.ExtrCoreWall == null ? "" : "<ExtrCoreWall>" + model.CustomForms.ExtrCoreWall + "</ExtrCoreWall>");
            Document.Append(model.CustomForms.ExtrSlittingCode == null ? "" : "<ExtrSlittingCode>" + model.CustomForms.ExtrSlittingCode + "</ExtrSlittingCode>");
            Document.Append(model.CustomForms.PrintPitch == null ? "" : "<PrintPitch>" + model.CustomForms.PrintPitch + "</PrintPitch>");
            Document.Append(model.CustomForms.PrintMReel == null ? "" : "<PrintMReel>" + model.CustomForms.PrintMReel + "</PrintMReel>");
            Document.Append(model.CustomForms.PrintLayFlatWidth == null ? "" : "<PrintLayFlatWidth>" + model.CustomForms.PrintLayFlatWidth + "</PrintLayFlatWidth>");
            Document.Append(model.CustomForms.PrintFinalUnwind == null ? "" : "<PrintFinalUnwind>" + model.CustomForms.PrintFinalUnwind + "</PrintFinalUnwind>");
            Document.Append(model.CustomForms.SlitWidth == null ? "" : "<SlitWidth>" + model.CustomForms.SlitWidth + "</SlitWidth>");
            Document.Append(model.CustomForms.SlitTolerance == null ? "" : "<SlitTolerance>" + model.CustomForms.SlitTolerance + "</SlitTolerance>");
            Document.Append(model.CustomForms.SlitKgReel == null ? "" : "<SlitKgReel>" + model.CustomForms.SlitKgReel + "</SlitKgReel>");
            Document.Append(model.CustomForms.SlitInterleaved == null ? "" : "<SlitInterleaved>" + model.CustomForms.SlitInterleaved + "</SlitInterleaved>");
            Document.Append(model.CustomForms.SlitCoreID == null ? "" : "<SlitCoreID>" + model.CustomForms.SlitCoreID + "</SlitCoreID>");
            Document.Append(model.CustomForms.SlitReelOdMin == null ? "" : "<SlitReelOdMin>" + model.CustomForms.SlitReelOdMin + "</SlitReelOdMin>");
            Document.Append(model.CustomForms.SlitReelOdMax == null ? "" : "<SlitReelOdMax>" + model.CustomForms.SlitReelOdMax + "</SlitReelOdMax>");
            Document.Append(model.CustomForms.SlitEyemarkSize == null ? "" : "<SlitEyemarkSize>" + model.CustomForms.SlitEyemarkSize + "</SlitEyemarkSize>");
            Document.Append(model.CustomForms.SlitHolePunchSize == null ? "" : "<SlitHolePunchSize>" + model.CustomForms.SlitHolePunchSize + "</SlitHolePunchSize>");
            Document.Append(model.CustomForms.BagUnitsBundle == null ? "" : "<BagUnitsBundle>" + model.CustomForms.BagUnitsBundle + "</BagUnitsBundle>");
            Document.Append(model.CustomForms.BagBundlesParcel == null ? "" : "<BagBundlesParcel>" + model.CustomForms.BagBundlesParcel + "</BagBundlesParcel>");
            Document.Append(model.CustomForms.BagBagUnitsParcel == null ? "" : "<BagBagUnitsParcel>" + model.CustomForms.BagBagUnitsParcel + "</BagBagUnitsParcel>");
            Document.Append(model.CustomForms.MaterialCost == null ? "" : "<MaterialCost>" + model.CustomForms.MaterialCost + "</MaterialCost>");
            Document.Append(model.CustomForms.ActualInkCost == null ? "" : "<ActualInkCost>" + model.CustomForms.ActualInkCost + "</ActualInkCost>");
            Document.Append(model.CustomForms.SpineSeal == null ? "" : "<SpineSeal>" + model.CustomForms.SpineSeal + "</SpineSeal>");
            Document.Append(model.CustomForms.Grams == null ? "" : "<Grams>" + model.CustomForms.Grams + "</Grams>");
            Document.Append("<Inkd>" + DateTime.Now.ToString("yyyy-MM-dd") + "</Inkd>");
            Document.Append(model.CustomForms.PrintRepeat == null ? "" : "<PrintRepeat>" + model.CustomForms.PrintRepeat + "</PrintRepeat>");
            Document.Append(model.CustomForms.BagOpening == null ? "" : "<BagOpening>" + model.CustomForms.BagOpening + "</BagOpening>");
            Document.Append(model.CustomForms.Handle == null ? "" : "<Handle>" + model.CustomForms.Handle + "</Handle>");
            Document.Append(model.CustomForms.PunchHoles == null ? "" : "<PunchHoles>" + model.CustomForms.PunchHoles + "</PunchHoles>");
            Document.Append(model.CustomForms.Wicket == null ? "" : "<Wicket>" + model.CustomForms.Wicket + "</Wicket>");
            Document.Append(model.CustomForms.Perforation == null ? "" : "<Perforation>" + model.CustomForms.Perforation + "</Perforation>");
            Document.Append(model.CustomForms.ReelOd == null ? "" : "<ReelOd>" + model.CustomForms.ReelOd + "</ReelOd>");
            Document.Append(model.CustomForms.BagSeal == null ? "" : "<BagSeal>" + model.CustomForms.BagSeal + "</BagSeal>");
            Document.Append(model.CustomForms.NewManufacturingCo == null ? "" : "<NewManufacturingCo>" + model.CustomForms.NewManufacturingCo + "</NewManufacturingCo>");
            Document.Append(model.CustomForms.OldManufacturingCo == null ? "" : "<OldManufacturingCo>" + model.CustomForms.OldManufacturingCo + "</OldManufacturingCo>");
            Document.Append(model.CustomForms.NewInksSolventsCos == null ? "" : "<NewInksSolventsCos>" + model.CustomForms.NewInksSolventsCos + "</NewInksSolventsCos>");
            Document.Append(model.CustomForms.OldInksSolventsCos == null ? "" : "<OldInksSolventsCos>" + model.CustomForms.OldInksSolventsCos + "</OldInksSolventsCos>");
            Document.Append(model.CustomForms.NewAdhesivesGlueCo == null ? "" : "<NewAdhesivesGlueCo>" + model.CustomForms.NewAdhesivesGlueCo + "</NewAdhesivesGlueCo>");
            Document.Append(model.CustomForms.OldAdhesivesGlueCo == null ? "" : "<OldAdhesivesGlueCo>" + model.CustomForms.OldAdhesivesGlueCo + "</OldAdhesivesGlueCo>");
            Document.Append(model.CustomForms.NewConsumablesCost == null ? "" : "<NewConsumablesCost>" + model.CustomForms.NewConsumablesCost + "</NewConsumablesCost>");
            Document.Append(model.CustomForms.OldConsumablesCost == null ? "" : "<OldConsumablesCost>" + model.CustomForms.OldConsumablesCost + "</OldConsumablesCost>");
            Document.Append(model.CustomForms.NewTransportCost == null ? "" : "<NewTransportCost>" + model.CustomForms.NewTransportCost + "</NewTransportCost>");
            Document.Append(model.CustomForms.OldTransportCost == null ? "" : "<OldTransportCost>" + model.CustomForms.OldTransportCost + "</OldTransportCost>");
            Document.Append(model.CustomForms.NewOtherCost == null ? "" : "<NewOtherCost>" + model.CustomForms.NewOtherCost + "</NewOtherCost>");
            Document.Append(model.CustomForms.OldOtherCost == null ? "" : "<OldOtherCost>" + model.CustomForms.OldOtherCost + "</OldOtherCost>");
            Document.Append(model.CustomForms.InkSystem == null ? "" : "<InkSystem>" + model.CustomForms.InkSystem + "</InkSystem>");
            Document.Append(model.CustomForms.RawMaterialCode == null ? "" : "<RawMaterialCode>" + model.CustomForms.RawMaterialCode + "</RawMaterialCode>");
            Document.Append(model.CustomForms.GenInkCostKg == null ? "" : "<GenInkCostKg>" + model.CustomForms.GenInkCostKg + "</GenInkCostKg>");
            Document.Append(model.CustomForms.LabelMicron == null ? "" : "<LabelMicron>" + model.CustomForms.LabelMicron + "</LabelMicron>");
            Document.Append(model.CustomForms.MD == null ? "" : "<MD>" + model.CustomForms.MD + "</MD>");
            Document.Append(model.CustomForms.TD == null ? "" : "<TD>" + model.CustomForms.TD + "</TD>");
            Document.Append(model.CustomForms.CofFF == null ? "" : "<CofFF>" + model.CustomForms.CofFF + "</CofFF>");
            Document.Append(model.CustomForms.CofFS == null ? "" : "<CofFS>" + model.CustomForms.CofFS + "</CofFS>");
            Document.Append(model.CustomForms.ExtrSlitWidth == null ? "" : "<ExtrSlitWidth>" + model.CustomForms.ExtrSlitWidth + "</ExtrSlitWidth>");
            Document.Append(model.CustomForms.ApprovedInkCode == null ? "" : "<ApprovedInkCode>" + model.CustomForms.ApprovedInkCode + "</ApprovedInkCode>");
            Document.Append(model.CustomForms.SuppCurrInkCost == null ? "" : "<SuppCurrInkCost>" + model.CustomForms.SuppCurrInkCost + "</SuppCurrInkCost>");
            Document.Append(model.CustomForms.PressReturn == null ? "" : "<PressReturn>" + model.CustomForms.PressReturn + "</PressReturn>");
            Document.Append(model.CustomForms.BoxColour == null ? "" : "<BoxColour>" + model.CustomForms.BoxColour + "</BoxColour>");
            Document.Append(model.CustomForms.NoCalc == null ? "" : "<NoCalc>" + model.CustomForms.NoCalc + "</NoCalc>");
            Document.Append(model.CustomForms.ProductType == null ? "" : "<ProductType>" + model.CustomForms.ProductType + "</ProductType>");
            Document.Append(model.CustomForms.ProductSubType1 == null ? "" : "<ProductSubType1>" + model.CustomForms.ProductSubType1 + "</ProductSubType1>");
            Document.Append(model.CustomForms.Industry == null ? "" : "<Industry>" + model.CustomForms.Industry + "</Industry>");
            Document.Append(model.CustomForms.AltName == null ? "" : "<AltName>" + model.CustomForms.AltName + "</AltName>");

            Document.Append("</Fields>");
            Document.Append("</Item>");
            Document.Append("</PostCustomForm>");

            return Document.ToString();
        }
        public string BuildCustomFormParameter(string FunctionType)
        {
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
            Parameter.Append("<Function>" + FunctionType + "</Function>");//"A" = Add, "U" = Update, "D" = Delete
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostCustomForm>");

            return Parameter.ToString();
        }



        public string PostStockCodeCreation(MasterCardViewModel model)
        {
            try
            {
                string Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    return "Failed to login to Syspro.";
                }

                string Document, Parameter, XmlOut, ErrorMessage;
                var StockCodeCheck = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == model.stkobj.StockCode select a).ToList();
                if (StockCodeCheck.Count == 0)
                {
                    Document = BuildStockCodeDocument(model.stkobj, model.CustomForms.InvoiceDim, model.CustomForms.GenMicron.ToString());
                    Parameter = BuildStockCodeParameter();
                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "INVSST");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add StockCode : " + model.stkobj.StockCode + ". " + ErrorMessage;
                    }
                    else
                    {


                        //Log StockCode Created
                        LogStockCodeCreated(model.MasterCardId, model.stkobj);

                    }
                }
                string MultimediaError = "";
                //Call Multimedia Setup Function
                //string Document, Parameter, XmlOut, ErrorMessage;
                var MultiCheck = (from a in wdb.mtMasterCardHeaders where a.Id == model.MasterCardId select a).FirstOrDefault();
                if (MultiCheck != null)
                {
                    if (!string.IsNullOrWhiteSpace(MultiCheck.MultiMediaFilePath))
                    {
                        if (model.stkobj.StockCode == MultiCheck.StockCode)
                        {
                            var sysCheck = (from a in wdb.AdmMultimedias where a.MultimediaFlag == "STK" && a.KeyField == model.stkobj.StockCode select a).ToList();
                            if (sysCheck.Count == 0)
                            {
                                try
                                {
                                    Document = BuildMultimediaDocument(model.MasterCardId);
                                    Parameter = BuildMultimediaParameter();
                                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "COMSMM");
                                    ErrorMessage = sys.GetXmlErrors(XmlOut);

                                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                                    {
                                        MultimediaError = "Failed to Add Multimedia for StockCode : " + model.stkobj.StockCode + ". " + ErrorMessage;
                                    }
                                }
                                catch (Exception)
                                {
                                    MultimediaError= "Failed to Add Multimedia for StockCode : " + model.stkobj.StockCode + ". ";
                                }
                                
                            }
                        }
                    }



                }

                var StockCodeNarrations = (from a in wdb.InvNarrationHdrs.AsNoTracking() where a.StockCode == model.stkobj.StockCode select a).ToList();
                if (StockCodeNarrations.Count == 0)
                {
                    Document = BuildInvNarrationsDoc(model.stkobj.StockCode, model.SalesOrderAddText, model.JobNarrations);
                    Parameter = BuildInvNarrationsParameter();
                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "INVSNA");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add StockCode Narrations for StockCode : " + model.stkobj.StockCode + ". " + ErrorMessage;
                    }
                }








                //Warehouses
                bool PostWarehouse = false;
                //Declaration
                StringBuilder WhDocument = new StringBuilder();

                //Building Document content
                WhDocument.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                WhDocument.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                WhDocument.Append("<!--");
                WhDocument.Append("Sample XML for the Stock Code Warehouse Setup Business Object");
                WhDocument.Append("-->");
                WhDocument.Append("<SetupInvWarehouse xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSWSDOC.XSD\">");
                var WarhouseCheck = (from a in wdb.InvWarehouses.AsNoTracking() where a.StockCode == model.stkobj.StockCode && a.Warehouse == model.stkobj.WarehouseToUse select a).ToList();
                if (WarhouseCheck.Count == 0)
                {
                    PostWarehouse = true;
                    WhDocument.Append("<Item>");
                    WhDocument.Append("<Key>");
                    WhDocument.Append("<StockCode>" + model.stkobj.StockCode + "</StockCode>");
                    WhDocument.Append("<Warehouse>" + model.stkobj.WarehouseToUse + "</Warehouse>");
                    WhDocument.Append("</Key>");
                    WhDocument.Append("<CostMultiplier>1.000</CostMultiplier>");
                    WhDocument.Append("<UnitCost>0</UnitCost>");
                    WhDocument.Append("<DefaultBin>" + model.stkobj.WarehouseToUse + "</DefaultBin>");
                    WhDocument.Append("</Item>");
                }

                if (model.AddWarehouse != null)
                {
                    if (model.AddWarehouse.Count > 0)
                    {
                        foreach (var wh in model.AddWarehouse)
                        {
                            if (wh != model.stkobj.WarehouseToUse)
                            {
                                WarhouseCheck = (from a in wdb.InvWarehouses.AsNoTracking() where a.StockCode == model.stkobj.StockCode && a.Warehouse == wh select a).ToList();
                                if (WarhouseCheck.Count == 0)
                                {
                                    PostWarehouse = true;
                                    WhDocument.Append("<Item>");
                                    WhDocument.Append("<Key>");
                                    WhDocument.Append("<StockCode>" + model.stkobj.StockCode + "</StockCode>");
                                    WhDocument.Append("<Warehouse>" + wh + "</Warehouse>");
                                    WhDocument.Append("</Key>");
                                    WhDocument.Append("<CostMultiplier>1.000</CostMultiplier>");
                                    WhDocument.Append("<UnitCost>0</UnitCost>");
                                    WhDocument.Append("<DefaultBin>" + wh + "</DefaultBin>");
                                    WhDocument.Append("</Item>");
                                }
                            }
                        }
                    }
                }


                WhDocument.Append("</SetupInvWarehouse>");
                if (PostWarehouse == true)
                {
                    XmlOut = sys.SysproSetupAdd(Guid, BuildStockCodeWarehouseParameter(), WhDocument.ToString(), "INVSWS");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Warehouse for StockCode :" + model.stkobj.StockCode + ". " + ErrorMessage;
                    }
                }


                var PriceCheck = (from a in wdb.InvPrices.AsNoTracking() where a.StockCode == model.stkobj.StockCode && a.PriceCode == "A" select a).ToList();
                if (PriceCheck.Count == 0)
                {
                    Document = BuildStockCodePricingDocument(model.stkobj.StockCode, "A", 0, "S");
                    Parameter = BuildStockCodePricingParamter();
                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "INVSPR");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Price Code A for StockCode :" + model.stkobj.StockCode + ". " + ErrorMessage;
                    }
                }

                var CustomFormCheck = wdb.sp_MasterCardGetStockCodeCustomFormData(model.stkobj.StockCode).ToList();
                if (CustomFormCheck.Count == 0)
                {
                    Parameter = BuildCustomFormParameter("A");
                }
                else
                {
                    Parameter = BuildCustomFormParameter("U");
                }
                Document = BuildCustomFormDocument(model);
                XmlOut = sys.SysproPost(Guid, Parameter, Document, "COMTFM");
                ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    return "Failed to Add Custom Form Data for StockCode :" + model.stkobj.StockCode + ". " + ErrorMessage;
                }
                //Log custom form entry
                model.CustomForms.StockCode = model.stkobj.StockCode;
                model.CustomForms.Username = HttpContext.Current.User.Identity.Name.ToUpper();
                model.CustomForms.DatePosted = DateTime.Now;
                using (var cdb = new WarehouseManagementEntities(""))
                {
                    cdb.Entry(model.CustomForms).State = System.Data.EntityState.Added;
                    cdb.SaveChanges();
                }


                return "StockCode " + model.stkobj.StockCode + " created successfully. "+ MultimediaError ;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void LogStockCodeCreated(int KeyId, mtMasterCardStockCode stkobj)
        {
            try
            {
                using (var ldb = new WarehouseManagementEntities(""))
                {
                    var stk = (from a in ldb.mtMasterCardStockCodes where a.Id == KeyId && a.StockCode == stkobj.StockCode select a).FirstOrDefault();
                    if (stk == null)
                    {
                        stkobj.Id = KeyId;
                        stkobj.DateUpdated = DateTime.Now;
                        stkobj.Username = HttpContext.Current.User.Identity.Name.ToUpper();
                        ldb.Entry(stkobj).State = System.Data.EntityState.Added;
                        ldb.SaveChanges();
                    }
                    else
                    {
                        stk.Id = KeyId;
                        stk.DateUpdated = DateTime.Now;
                        stk.Username = HttpContext.Current.User.Identity.Name.ToUpper();
                        ldb.Entry(stk).State = System.Data.EntityState.Modified;
                        ldb.SaveChanges();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostBom(int KeyId)
        {
            try
            {

                string Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    return "Failed to login to Syspro.";
                }
                var bom = wdb.sp_MasterCardGetBomStructureForPosting(KeyId).ToList();
                if (bom.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the BOM Structure Setup Business Object");
                    Document.Append("-->");
                    Document.Append("<SetupBomStructure xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSSTDOC.XSD\">");



                    foreach (var co in bom)
                    {
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
                        Document.Append("<ComponentType />");
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
                    }

                    Document.Append("</SetupBomStructure>");

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

                    string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "BOMSST");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to create BOM Structure : " + ErrorMessage;
                    }
                }

                var ops = wdb.sp_MasterCardGetBomOperationsForPosting(KeyId).ToList();
                if (ops.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the BOM Routing Business Object");
                    Document.Append("-->");
                    Document.Append("<SetupBomRouting xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSRODOC.XSD\">");

                    foreach (var op in ops)
                    {

                        var ETCalcMethod = wdb.sp_MasterCardGetWorkCentreDetails(op.WorkCentre).ToList().FirstOrDefault().EtCalcMeth;


                        Document.Append("<Item>");
                        Document.Append("<Key>");
                        Document.Append("<StockCode><![CDATA[" + op.StockCode + "]]></StockCode>");
                        Document.Append("<Version />");
                        Document.Append("<Release />");
                        Document.Append("<Route><![CDATA[" + op.Route + "]]></Route>");
                        //Document.Append("<Operation>1</Operation>");
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

                        //Document.Append("<IRunTimeEnt>0</IRunTimeEnt>");
                        //Document.Append("<IStartupTime />");
                        //Document.Append("<ITeardownTime />");
                        //Document.Append("<IWaitTime />");
                        //Document.Append("<IStartupQty />");
                        //Document.Append("<IStartupQtyEnt />");
                        //Document.Append("<IMachine />");
                        //Document.Append("<IUnitCapacity />");
                        //Document.Append("<IMaxWorkOpertrs>1</IMaxWorkOpertrs>");
                        //Document.Append("<IMaxProdUnits>1</IMaxProdUnits>");
                        //Document.Append("<ITimeTaken />");
                        //Document.Append("<ITimeTakenEnt />");
                        //Document.Append("<IQuantity>1</IQuantity>");
                        //Document.Append("<IQuantityEnt />");
                        //Document.Append("<SubSupplier />");
                        //Document.Append("<SubPoStockCode />");
                        //Document.Append("<SubQtyPer />");
                        //Document.Append("<SubOrderUom />");
                        //Document.Append("<SubOpUnitValue />");
                        //Document.Append("<SubPlanner />");
                        //Document.Append("<SubBuyer />");
                        //Document.Append("<SubLeadTime />");
                        //Document.Append("<SubDockToStock />");
                        //Document.Append("<SubOffsiteDays />");
                        Document.Append("<Milestone>N</Milestone>");
                        //Document.Append("<ElapsedTime />");
                        //Document.Append("<MovementTime />");
                        //Document.Append("<NarrationCode />");
                        //Document.Append("<NumOfPieces />");
                        //Document.Append("<InspectionFlag>N</InspectionFlag>");
                        //Document.Append("<MinorSetup />");
                        //Document.Append("<MinorSetupCode />");
                        //Document.Append("<ToolSet />");
                        //Document.Append("<ToolSetQty />");
                        //Document.Append("<ToolConsumption />");
                        //Document.Append("<TransferQtyOrPct />");
                        //Document.Append("<TransferQtyPct />");
                        //Document.Append("<TransferQtyPctEnt />");
                        //Document.Append("<OperYieldPct />");
                        //Document.Append("<OperYieldQty />");
                        //Document.Append("<OperYieldQtyEnt />");
                        //Document.Append("<TimeCalcFlag>Y</TimeCalcFlag>");
                        Document.Append("</Item>");
                    }
                    Document.Append("</SetupBomRouting>");

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

                    string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "BOMSRO");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to create BOM Operations : " + ErrorMessage;
                    }

                }


                var narr = wdb.sp_MasterCardGetBomNarrationsForPosting(KeyId).ToList();
                if (narr.Count > 0)
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

                    foreach (var n in narr)
                    {

                        Document.Append("<Item>");
                        Document.Append("<Key>");
                        Document.Append("<Source>BOMOPERATION</Source>");
                        Document.Append("<JobNumber></JobNumber>");
                        Document.Append("<ParentPart><![CDATA[" + n.StockCode + "]]></ParentPart>");
                        Document.Append("<Version></Version>");
                        Document.Append("<Release></Release>");
                        Document.Append("<Component></Component>");
                        Document.Append("<Route>0</Route>");
                        Document.Append("<Operation>" + n.Operation + "</Operation>");
                        Document.Append("<SequenceNum></SequenceNum>");
                        Document.Append("<Warehouse></Warehouse>");
                        Document.Append("<Line></Line>");
                        Document.Append("</Key>");
                        Document.Append("<Narration><![CDATA[" + n.Narration + "]]></Narration>");
                        Document.Append("</Item>");

                    }

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
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to add BOM Narrations : " + ErrorMessage;
                    }
                }

                return "BOM created successfully in Syspro.";


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string BuildInvNarrationsDoc(string StockCode, string SalesOrderText, string JobNarrations)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2012 SYSPRO Ltd.-->");
                Document.Append("<!--This is an example XML instance to demonstrate use of the Inventory narration setup Setup Business Object-->");
                Document.Append("<SetupInvNarrations xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSNADOC.XSD\">");


                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<TextType>W</TextType>");
                Document.Append("<LanguageCode />");
                Document.Append("</Key>");
                Document.Append("<Text><![CDATA[" + JobNarrations + "]]></Text>");
                Document.Append("</Item>");

                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                Document.Append("<TextType>S</TextType>");
                Document.Append("<LanguageCode />");
                Document.Append("</Key>");
                Document.Append("<Text><![CDATA[" + SalesOrderText + "]]></Text>");
                Document.Append("</Item>");

                Document.Append("</SetupInvNarrations>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildInvNarrationsParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2012 SYSPRO Ltd.-->");
            Parameter.Append("<!--This is an example XML instance to demonstrate use of the Inventory narration setup Setup Business Object-->");
            Parameter.Append("<SetupInvNarrations xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSNA.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</SetupInvNarrations>");

            return Parameter.ToString();
        }
    }
}