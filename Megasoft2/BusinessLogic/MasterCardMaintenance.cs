using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class MasterCardMaintenance
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproCore sys = new SysproCore();
        public string BuildStockCodeDocument(mtMasterCardStockCodeUpdate model, string Dimensions, string Micron)
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

        public void LogStockCodeCreated(int KeyId, mtMasterCardStockCodeUpdate stkobj)
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

        public string BuildMultimediaDocument(int Id)
        {
            //Declaration
            var MasterCardId = (from a in db.mtMasterCardHeaders where a.Id == Id select a).FirstOrDefault();

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

        public string BuildInvNarrationsDoc(string StockCode, string SalesOrderText, string JobNarrations, string TextType)
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

                if (TextType == "W")
                {
                    Document.Append("<Item>");
                    Document.Append("<Key>");
                    Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                    Document.Append("<TextType>W</TextType>");
                    Document.Append("<LanguageCode />");
                    Document.Append("</Key>");
                    Document.Append("<Text><![CDATA[" + JobNarrations + "]]></Text>");
                    Document.Append("</Item>");
                }
                else
                {
                    Document.Append("<Item>");
                    Document.Append("<Key>");
                    Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");
                    Document.Append("<TextType>S</TextType>");
                    Document.Append("<LanguageCode />");
                    Document.Append("</Key>");
                    Document.Append("<Text><![CDATA[" + SalesOrderText + "]]></Text>");
                    Document.Append("</Item>");
                }


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

        public string PostStockCodeCreation(int Id, string Guid)
        {

            try
            {


                var StockCodeModel = (from a in db.mtMasterCardStockCodeUpdates where a.Id == Id select a).FirstOrDefault();
                var CustomFormModel = (from a in db.mtMasterCardStockCodeCustomFormUpdates where a.Id == Id select a).FirstOrDefault();
                var WarehouseList = db.sp_MasterCardGetWarehouseUpdate(Id, StockCodeModel.StockCode).ToList();
                string Document, Parameter, XmlOut, ErrorMessage;
                var StockCodeCheck = (from a in db.InvMasters.AsNoTracking() where a.StockCode == StockCodeModel.StockCode select a).ToList();
                if (StockCodeCheck.Count == 1)
                {
                    //check if posted flag in mtMasterCardStockCodeUpdates != true
                    Document = BuildStockCodeDocument(StockCodeModel, CustomFormModel.InvoiceDim, CustomFormModel.GenMicron.ToString());
                    Parameter = BuildStockCodeParameter();
                    XmlOut = sys.SysproSetupUpdate(Guid, Parameter, Document, "INVSST");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to update StockCode : " + StockCodeModel.StockCode + ". " + ErrorMessage;
                    }
                    else
                    {
                        //Update mtMasterCardStockCodeUpdates posted flag
                        StockCodeModel.Posted = true;
                        StockCodeModel.DatePosted = DateTime.Now;
                        db.Entry(StockCodeModel).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else
                {
                    return "StockCode not found in Syspro";
                }




                var narrationcheck = (from x in db.InvNarrations
                                      where x.StockCode == StockCodeModel.StockCode && x.TextType == "W"
                                      select x).ToList();
                if (narrationcheck.Count == 0)
                {
                    Document = BuildInvNarrationsDoc(StockCodeModel.StockCode, StockCodeModel.SalesOrderAddText, StockCodeModel.JobNarrations, "W");
                    Parameter = BuildInvNarrationsParameter();
                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "INVSNA");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Job Narrations for StockCode : " + StockCodeModel.StockCode + ". " + ErrorMessage;
                    }
                }
                else
                {
                    Document = BuildInvNarrationsDoc(StockCodeModel.StockCode, StockCodeModel.SalesOrderAddText, StockCodeModel.JobNarrations, "W");
                    Parameter = BuildInvNarrationsParameter();
                    XmlOut = sys.SysproSetupUpdate(Guid, Parameter, Document, "INVSNA");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Update Job Narrations for StockCode : " + StockCodeModel.StockCode + ". " + ErrorMessage;
                    }
                }

                narrationcheck = (from x in db.InvNarrations
                                  where x.StockCode == StockCodeModel.StockCode && x.TextType == "S"
                                  select x).ToList();
                if (narrationcheck.Count == 0)
                {
                    Document = BuildInvNarrationsDoc(StockCodeModel.StockCode, StockCodeModel.SalesOrderAddText, StockCodeModel.JobNarrations, "S");
                    Parameter = BuildInvNarrationsParameter();
                    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "INVSNA");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Sales Order Narrations for StockCode : " + StockCodeModel.StockCode + ". " + ErrorMessage;
                    }
                }
                else
                {
                    Document = BuildInvNarrationsDoc(StockCodeModel.StockCode, StockCodeModel.SalesOrderAddText, StockCodeModel.JobNarrations, "S");
                    Parameter = BuildInvNarrationsParameter();
                    XmlOut = sys.SysproSetupUpdate(Guid, Parameter, Document, "INVSNA");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Sales Order Narrations for StockCode : " + StockCodeModel.StockCode + ". " + ErrorMessage;
                    }
                }



                //Warehouses
                bool PostWarehouse = false;
                ////Declaration
                StringBuilder WhDocument = new StringBuilder();

                //Building Document content
                WhDocument.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                WhDocument.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                WhDocument.Append("<!--");
                WhDocument.Append("Sample XML for the Stock Code Warehouse Setup Business Object");
                WhDocument.Append("-->");
                WhDocument.Append("<SetupInvWarehouse xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVSWSDOC.XSD\">");
                var WarhouseCheck = (from a in db.InvWarehouses.AsNoTracking() where a.StockCode == StockCodeModel.StockCode && a.Warehouse == StockCodeModel.WarehouseToUse select a).ToList();
                if (WarhouseCheck.Count == 0)
                {
                    PostWarehouse = true;
                    WhDocument.Append("<Item>");
                    WhDocument.Append("<Key>");
                    WhDocument.Append("<StockCode>" + StockCodeModel.StockCode + "</StockCode>");
                    WhDocument.Append("<Warehouse>" + StockCodeModel.WarehouseToUse + "</Warehouse>");
                    WhDocument.Append("</Key>");
                    WhDocument.Append("<CostMultiplier>1.000</CostMultiplier>");
                    WhDocument.Append("<UnitCost>0</UnitCost>");
                    WhDocument.Append("<DefaultBin>" + StockCodeModel.WarehouseToUse + "</DefaultBin>");
                    WhDocument.Append("</Item>");
                }

                //if (model.warehouseUpdate_Result != null)
                //{
                if (WarehouseList.Count() > 0)
                {
                    foreach (var wh in WarehouseList)
                    {
                        if (wh.Allowed == true)
                        {
                            WarhouseCheck = (from a in db.InvWarehouses.AsNoTracking() where a.StockCode == StockCodeModel.StockCode && a.Warehouse == wh.Warehouse select a).ToList();
                            if (WarhouseCheck.Count == 0)
                            {
                                PostWarehouse = true;
                                WhDocument.Append("<Item>");
                                WhDocument.Append("<Key>");
                                WhDocument.Append("<StockCode>" + StockCodeModel.StockCode + "</StockCode>");
                                WhDocument.Append("<Warehouse>" + wh.Warehouse + "</Warehouse>");
                                WhDocument.Append("</Key>");
                                WhDocument.Append("<CostMultiplier>1.000</CostMultiplier>");
                                WhDocument.Append("<UnitCost>0</UnitCost>");
                                WhDocument.Append("<DefaultBin>" + wh.Warehouse + "</DefaultBin>");
                                WhDocument.Append("</Item>");
                            }
                        }
                    }
                }
                //}

                WhDocument.Append("</SetupInvWarehouse>");
                if (PostWarehouse == true)
                {
                    XmlOut = sys.SysproSetupAdd(Guid, BuildStockCodeWarehouseParameter(), WhDocument.ToString(), "INVSWS");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Add Warehouse for StockCode :" + StockCodeModel.StockCode + ". " + ErrorMessage;
                    }
                }
                //var PriceCheck = (from a in db.InvPrices.AsNoTracking() where a.StockCode == model.stkobj.StockCode && a.PriceCode == "A" select a).ToList();
                //if (PriceCheck.Count == 0)
                //{
                //    Document = BuildStockCodePricingDocument(model.stkobj.StockCode, "A", 0, "S");
                //    Parameter = BuildStockCodePricingParamter();
                //    XmlOut = sys.SysproSetupAdd(Guid, Parameter, Document, "INVSPR");
                //    ErrorMessage = sys.GetXmlErrors(XmlOut);
                //    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                //    {
                //        return "Failed to Add Price Code A for StockCode :" + model.stkobj.StockCode + ". " + ErrorMessage;
                //    }
                //}

                var CustomFormCheck = db.sp_MasterCardGetStockCodeCustomFormData(StockCodeModel.StockCode).ToList();
                if (CustomFormCheck.Count == 0)
                {
                    Parameter = BuildCustomFormParameter("A");
                }
                else
                {
                    Parameter = BuildCustomFormParameter("U");
                }
                Document = BuildCustomFormDocument(CustomFormModel, StockCodeModel.StockCode);
                XmlOut = sys.SysproPost(Guid, Parameter, Document, "COMTFM");
                ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (!string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    return "Failed to post Custom Form Data for StockCode :" + StockCodeModel.StockCode + ". " + ErrorMessage;
                }



                return "StockCode " + StockCodeModel.StockCode + " updated successfully.";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string BuildCustomFormDocument(mtMasterCardStockCodeCustomFormUpdate model, string StockCode)
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

            Document.Append("<StockCode>" + StockCode + "</StockCode>");

            Document.Append("</KeyFields>");
            Document.Append("</Key>");
            Document.Append("<Fields>");

            Document.Append(model.InvoiceDim == null ? "" : "<InvoiceDim>" + model.InvoiceDim + "</InvoiceDim>");
            Document.Append(model.BarCode == null ? "" : "<BarCode>" + model.BarCode + "</BarCode>");
            Document.Append(model.GenWidth == null ? "" : "<GenWidth>" + model.GenWidth + "</GenWidth>");
            Document.Append(model.GenLength == null ? "" : "<GenLength>" + model.GenLength + "</GenLength>");
            Document.Append(model.GenLayFlatWidthSiz == null ? "" : "<GenLayFlatWidthSiz>" + model.GenLayFlatWidthSiz + "</GenLayFlatWidthSiz>");
            Document.Append(model.GenMicron == null ? "" : "<GenMicron>" + model.GenMicron + "</GenMicron>");
            Document.Append(model.GenTreatment == null ? "" : "<GenTreatment>" + model.GenTreatment + "</GenTreatment>");
            Document.Append(model.GenDyneValue == null ? "" : "<GenDyneValue>" + model.GenDyneValue + "</GenDyneValue>");
            Document.Append(model.GenSlit == null ? "" : "<GenSlit>" + model.GenSlit + "</GenSlit>");
            Document.Append(model.GenInkCost1000 == null ? "" : "<GenInkCost1000>" + model.GenInkCost1000 + "</GenInkCost1000>");
            Document.Append(model.GenPalletWrapRoll == null ? "" : "<GenPalletWrapRoll>" + model.GenPalletWrapRoll + "</GenPalletWrapRoll>");
            Document.Append(model.GenPalletRolls == null ? "" : "<GenPalletRolls>" + model.GenPalletRolls + "</GenPalletRolls>");
            Document.Append(model.PrintPrintFront == null ? "" : "<PrintPrintFront>" + model.PrintPrintFront + "</PrintPrintFront>");
            Document.Append(model.PrintPrintBack == null ? "" : "<PrintPrintBack>" + model.PrintPrintBack + "</PrintPrintBack>");
            Document.Append(model.PrintStepSize == null ? "" : "<PrintStepSize>" + model.PrintStepSize + "</PrintStepSize>");
            Document.Append(model.PrintCylinderSize == null ? "" : "<PrintCylinderSize>" + model.PrintCylinderSize + "</PrintCylinderSize>");
            Document.Append(model.PrintAround == null ? "" : "<PrintAround>" + model.PrintAround + "</PrintAround>");
            Document.Append(model.PrintAcross == null ? "" : "<PrintAcross>" + model.PrintAcross + "</PrintAcross>");
            Document.Append(model.PrintCoverageF == null ? "" : "<PrintCoverageF>" + model.PrintCoverageF + "</PrintCoverageF>");
            Document.Append(model.PrintCoverageB == null ? "" : "<PrintCoverageB>" + model.PrintCoverageB + "</PrintCoverageB>");
            Document.Append(model.PrintLinePrint == null ? "" : "<PrintLinePrint>" + model.PrintLinePrint + "</PrintLinePrint>");
            Document.Append(model.BagWidthSize == null ? "" : "<BagWidthSize>" + model.BagWidthSize + "</BagWidthSize>");
            Document.Append(model.BagLengthSize == null ? "" : "<BagLengthSize>" + model.BagLengthSize + "</BagLengthSize>");
            Document.Append(model.BagTopGusset == null ? "" : "<BagTopGusset>" + model.BagTopGusset + "</BagTopGusset>");
            Document.Append(model.BagBottomGusset == null ? "" : "<BagBottomGusset>" + model.BagBottomGusset + "</BagBottomGusset>");
            Document.Append(model.BagRightGusset == null ? "" : "<BagRightGusset>" + model.BagRightGusset + "</BagRightGusset>");
            Document.Append(model.BagLeftGusset == null ? "" : "<BagLeftGusset>" + model.BagLeftGusset + "</BagLeftGusset>");
            Document.Append(model.BagLipSize == null ? "" : "<BagLipSize>" + model.BagLipSize + "</BagLipSize>");
            Document.Append(model.BagHeaderSeal == null ? "" : "<BagHeaderSeal>" + model.BagHeaderSeal + "</BagHeaderSeal>");
            Document.Append(model.BagSealType == null ? "" : "<BagSealType>" + model.BagSealType + "</BagSealType>");
            Document.Append(model.BagPerPack == null ? "" : "<BagPerPack>" + model.BagPerPack + "</BagPerPack>");
            Document.Append(model.BagPerBale == null ? "" : "<BagPerBale>" + model.BagPerBale + "</BagPerBale>");
            Document.Append(model.ExtrRollsUp == null ? "" : "<ExtrRollsUp>" + model.ExtrRollsUp + "</ExtrRollsUp>");
            Document.Append(model.ExtrKgPerRoll == null ? "" : "<ExtrKgPerRoll>" + model.ExtrKgPerRoll + "</ExtrKgPerRoll>");
            Document.Append(model.ExtrMetresPerRoll == null ? "" : "<ExtrMetresPerRoll>" + model.ExtrMetresPerRoll + "</ExtrMetresPerRoll>");
            Document.Append(model.ExtrLFWidthSize == null ? "" : "<ExtrLFWidthSize>" + model.ExtrLFWidthSize + "</ExtrLFWidthSize>");
            Document.Append(model.ExtrDoubleWind == null ? "" : "<ExtrDoubleWind>" + model.ExtrDoubleWind + "</ExtrDoubleWind>");
            Document.Append(model.ExtrCoreWeight == null ? "" : "<ExtrCoreWeight>" + model.ExtrCoreWeight + "</ExtrCoreWeight>");
            Document.Append(model.SlitSheetSlits == null ? "" : "<SlitSheetSlits>" + model.SlitSheetSlits + "</SlitSheetSlits>");
            Document.Append(model.SlitRollsUp == null ? "" : "<SlitRollsUp>" + model.SlitRollsUp + "</SlitRollsUp>");
            Document.Append(model.ExtrCoreWall == null ? "" : "<ExtrCoreWall>" + model.ExtrCoreWall + "</ExtrCoreWall>");
            Document.Append(model.ExtrSlittingCode == null ? "" : "<ExtrSlittingCode>" + model.ExtrSlittingCode + "</ExtrSlittingCode>");
            Document.Append(model.PrintPitch == null ? "" : "<PrintPitch>" + model.PrintPitch + "</PrintPitch>");
            Document.Append(model.PrintMReel == null ? "" : "<PrintMReel>" + model.PrintMReel + "</PrintMReel>");
            Document.Append(model.PrintLayFlatWidth == null ? "" : "<PrintLayFlatWidth>" + model.PrintLayFlatWidth + "</PrintLayFlatWidth>");
            Document.Append(model.PrintFinalUnwind == null ? "" : "<PrintFinalUnwind>" + model.PrintFinalUnwind + "</PrintFinalUnwind>");
            Document.Append(model.SlitWidth == null ? "" : "<SlitWidth>" + model.SlitWidth + "</SlitWidth>");
            Document.Append(model.SlitTolerance == null ? "" : "<SlitTolerance>" + model.SlitTolerance + "</SlitTolerance>");
            Document.Append(model.SlitKgReel == null ? "" : "<SlitKgReel>" + model.SlitKgReel + "</SlitKgReel>");
            Document.Append(model.SlitInterleaved == null ? "" : "<SlitInterleaved>" + model.SlitInterleaved + "</SlitInterleaved>");
            Document.Append(model.SlitCoreID == null ? "" : "<SlitCoreID>" + model.SlitCoreID + "</SlitCoreID>");
            Document.Append(model.SlitReelOdMin == null ? "" : "<SlitReelOdMin>" + model.SlitReelOdMin + "</SlitReelOdMin>");
            Document.Append(model.SlitReelOdMax == null ? "" : "<SlitReelOdMax>" + model.SlitReelOdMax + "</SlitReelOdMax>");
            Document.Append(model.SlitEyemarkSize == null ? "" : "<SlitEyemarkSize>" + model.SlitEyemarkSize + "</SlitEyemarkSize>");
            Document.Append(model.SlitHolePunchSize == null ? "" : "<SlitHolePunchSize>" + model.SlitHolePunchSize + "</SlitHolePunchSize>");
            Document.Append(model.BagUnitsBundle == null ? "" : "<BagUnitsBundle>" + model.BagUnitsBundle + "</BagUnitsBundle>");
            Document.Append(model.BagBundlesParcel == null ? "" : "<BagBundlesParcel>" + model.BagBundlesParcel + "</BagBundlesParcel>");
            Document.Append(model.BagBagUnitsParcel == null ? "" : "<BagBagUnitsParcel>" + model.BagBagUnitsParcel + "</BagBagUnitsParcel>");
            Document.Append(model.MaterialCost == null ? "" : "<MaterialCost>" + model.MaterialCost + "</MaterialCost>");
            Document.Append(model.ActualInkCost == null ? "" : "<ActualInkCost>" + model.ActualInkCost + "</ActualInkCost>");
            Document.Append(model.SpineSeal == null ? "" : "<SpineSeal>" + model.SpineSeal + "</SpineSeal>");
            Document.Append(model.Grams == null ? "" : "<Grams>" + model.Grams + "</Grams>");
            Document.Append("<Inkd>" + DateTime.Now.ToString("yyyy-MM-dd") + "</Inkd>");
            Document.Append(model.PrintRepeat == null ? "" : "<PrintRepeat>" + model.PrintRepeat + "</PrintRepeat>");
            Document.Append(model.BagOpening == null ? "" : "<BagOpening>" + model.BagOpening + "</BagOpening>");
            Document.Append(model.Handle == null ? "" : "<Handle>" + model.Handle + "</Handle>");
            Document.Append(model.PunchHoles == null ? "" : "<PunchHoles>" + model.PunchHoles + "</PunchHoles>");
            Document.Append(model.Wicket == null ? "" : "<Wicket>" + model.Wicket + "</Wicket>");
            Document.Append(model.Perforation == null ? "" : "<Perforation>" + model.Perforation + "</Perforation>");
            Document.Append(model.ReelOd == null ? "" : "<ReelOd>" + model.ReelOd + "</ReelOd>");
            Document.Append(model.BagSeal == null ? "" : "<BagSeal>" + model.BagSeal + "</BagSeal>");
            Document.Append(model.NewManufacturingCo == null ? "" : "<NewManufacturingCo>" + model.NewManufacturingCo + "</NewManufacturingCo>");
            Document.Append(model.OldManufacturingCo == null ? "" : "<OldManufacturingCo>" + model.OldManufacturingCo + "</OldManufacturingCo>");
            Document.Append(model.NewInksSolventsCos == null ? "" : "<NewInksSolventsCos>" + model.NewInksSolventsCos + "</NewInksSolventsCos>");
            Document.Append(model.OldInksSolventsCos == null ? "" : "<OldInksSolventsCos>" + model.OldInksSolventsCos + "</OldInksSolventsCos>");
            Document.Append(model.NewAdhesivesGlueCo == null ? "" : "<NewAdhesivesGlueCo>" + model.NewAdhesivesGlueCo + "</NewAdhesivesGlueCo>");
            Document.Append(model.OldAdhesivesGlueCo == null ? "" : "<OldAdhesivesGlueCo>" + model.OldAdhesivesGlueCo + "</OldAdhesivesGlueCo>");
            Document.Append(model.NewConsumablesCost == null ? "" : "<NewConsumablesCost>" + model.NewConsumablesCost + "</NewConsumablesCost>");
            Document.Append(model.OldConsumablesCost == null ? "" : "<OldConsumablesCost>" + model.OldConsumablesCost + "</OldConsumablesCost>");
            Document.Append(model.NewTransportCost == null ? "" : "<NewTransportCost>" + model.NewTransportCost + "</NewTransportCost>");
            Document.Append(model.OldTransportCost == null ? "" : "<OldTransportCost>" + model.OldTransportCost + "</OldTransportCost>");
            Document.Append(model.NewOtherCost == null ? "" : "<NewOtherCost>" + model.NewOtherCost + "</NewOtherCost>");
            Document.Append(model.OldOtherCost == null ? "" : "<OldOtherCost>" + model.OldOtherCost + "</OldOtherCost>");
            Document.Append(model.InkSystem == null ? "" : "<InkSystem>" + model.InkSystem + "</InkSystem>");
            Document.Append(model.RawMaterialCode == null ? "" : "<RawMaterialCode>" + model.RawMaterialCode + "</RawMaterialCode>");
            Document.Append(model.GenInkCostKg == null ? "" : "<GenInkCostKg>" + model.GenInkCostKg + "</GenInkCostKg>");
            Document.Append(model.LabelMicron == null ? "" : "<LabelMicron>" + model.LabelMicron + "</LabelMicron>");
            Document.Append(model.MD == null ? "" : "<MD>" + model.MD + "</MD>");
            Document.Append(model.TD == null ? "" : "<TD>" + model.TD + "</TD>");
            Document.Append(model.CofFF == null ? "" : "<CofFF>" + model.CofFF + "</CofFF>");
            Document.Append(model.CofFS == null ? "" : "<CofFS>" + model.CofFS + "</CofFS>");
            Document.Append(model.ExtrSlitWidth == null ? "" : "<ExtrSlitWidth>" + model.ExtrSlitWidth + "</ExtrSlitWidth>");
            Document.Append(model.ApprovedInkCode == null ? "" : "<ApprovedInkCode>" + model.ApprovedInkCode + "</ApprovedInkCode>");
            Document.Append(model.SuppCurrInkCost == null ? "" : "<SuppCurrInkCost>" + model.SuppCurrInkCost + "</SuppCurrInkCost>");
            Document.Append(model.PressReturn == null ? "" : "<PressReturn>" + model.PressReturn + "</PressReturn>");
            Document.Append(model.BoxColour == null ? "" : "<BoxColour>" + model.BoxColour + "</BoxColour>");
            Document.Append(model.NoCalc == null ? "" : "<NoCalc>" + model.NoCalc + "</NoCalc>");
            Document.Append(model.ProductType == null ? "" : "<ProductType>" + model.ProductType + "</ProductType>");
            Document.Append(model.ProductSubType1 == null ? "" : "<ProductSubType1>" + model.ProductSubType1 + "</ProductSubType1>");
            Document.Append(model.Industry == null ? "" : "<Industry>" + model.Industry + "</Industry>");
            Document.Append(model.AltName == null ? "" : "<AltName>" + model.AltName + "</AltName>");

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

        public string BuildXmlStructure(List<mtMasterCardBomStructureUpdate> check)
        {
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the BOM Structure Setup Business Object");
            Document.Append("-->");
            Document.Append("<SetupBomStructure xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"BOMSSTDOC.XSD\">");

            foreach (var co in check)
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


            return Document.ToString();
        }

        public string BuildXmlOperations(List<mtMasterCardBomOperationsUpdate> ops)
        {
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
                var ETCalcMethod = db.sp_MasterCardGetWorkCentreDetails(op.WorkCentre).ToList().FirstOrDefault().EtCalcMeth;

                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<StockCode><![CDATA[" + op.StockCode + "]]></StockCode>");
                Document.Append("<Version />");
                Document.Append("<Release />");
                Document.Append("<Route><![CDATA[" + op.Route + "]]></Route>");
                if (op.ActionType != "A")
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
            }
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

        public string BuildBomNarration(sp_MasterCardGetBomNarrationsForPostingUpdate_Result narr)
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
            Document.Append("<Route>0</Route>");
            Document.Append("<Operation>" + narr.Operation + "</Operation>");
            Document.Append("<SequenceNum></SequenceNum>");
            Document.Append("<Warehouse></Warehouse>");
            Document.Append("<Line></Line>");
            Document.Append("</Key>");
            Document.Append("<Narration><![CDATA[" + narr.Narration + "]]></Narration>");
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

        public string PostBom(int KeyId, string ActionType, string Guid)
        {
            try
            {
                string PostMessage = "";


                var check = (from a in db.mtMasterCardBomStructureUpdates
                             where a.Id == KeyId && a.ActionType == ActionType && a.Posted == null
                             select a).ToList();
                if (check.Count > 0)
                {
                    //Declaration
                    BuildXmlStructure(check);

                    string XmlOut;
                    if (ActionType == "A")
                    {
                        XmlOut = sys.SysproSetupAdd(Guid, BuildBomStructureParameter(), BuildXmlStructure(check), "BOMSST");
                    }
                    else if (ActionType == "C")
                    {
                        XmlOut = sys.SysproSetupUpdate(Guid, BuildBomStructureParameter(), BuildXmlStructure(check), "BOMSST");
                    }
                    else
                    {
                        XmlOut = sys.SysproSetupDelete(Guid, BuildBomStructureParameter(), BuildXmlStructure(check), "BOMSST");
                    }
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to create BOM Structure : " + ErrorMessage;
                    }
                    else
                    {
                        foreach (var item in check)
                        {
                            item.Posted = true;
                            item.DatePosted = DateTime.Now;
                            db.Entry(item).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                        }
                        PostMessage = "Bom changes posted successfully.";
                    }
                }

                //}

                var ops = (from a in db.mtMasterCardBomOperationsUpdates
                           where a.Id == KeyId && a.ActionType == ActionType && a.Posted == null
                           select a).ToList();
                if (ops.Count > 0)
                {
                    //Declaration
                    BuildXmlOperations(ops);
                    string XmlOut;
                    if (ActionType == "A")
                    {
                        XmlOut = sys.SysproSetupAdd(Guid, BuildBomOpsParameter(), BuildXmlOperations(ops), "BOMSRO");
                    }
                    else if (ActionType == "C")
                    {
                        XmlOut = sys.SysproSetupUpdate(Guid, BuildBomOpsParameter(), BuildXmlOperations(ops), "BOMSRO");
                    }
                    else
                    {
                        XmlOut = sys.SysproSetupDelete(Guid, BuildBomOpsParameter(), BuildXmlOperations(ops), "BOMSRO");
                    }

                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to create BOM Operations : " + ErrorMessage;
                    }

                    else
                    {
                        foreach (var item in ops)
                        {
                            item.Posted = true;
                            item.DatePosted = DateTime.Now;
                            db.Entry(item).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                        }
                        PostMessage = "Bom changes posted successfully.";
                    }
                }


                //MASTER CARD MAINTENACE BUSINESS
                var result = db.sp_MasterCardGetBomNarrationsForPostingUpdate(KeyId).Where(a => a.ActionType == ActionType).ToList();
                foreach (var item in result)
                {
                    string XmlOut;
                    if (item.ActionType == "A")
                    {
                        XmlOut = sys.SysproSetupAdd(Guid, BuildBomNarrationParameter(), BuildBomNarration(item), "BOMSSN");
                    }
                    else if (item.ActionType == "C")
                    {
                        XmlOut = sys.SysproSetupUpdate(Guid, BuildBomNarrationParameter(), BuildBomNarration(item), "BOMSSN");
                    }
                    else
                    {
                        XmlOut = sys.SysproSetupDelete(Guid, BuildBomNarrationParameter(), BuildBomNarration(item), "BOMSSN");
                    }
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to add BOM Narrations : " + ErrorMessage;
                    }
                }


                return PostMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string PostBomUpdate(int KeyId)
        {
            try
            {
                string PostMessage = "";
                string Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    return "Failed to login to Syspro.";
                }
                //Check BomStructure
                var check = (from a in db.mtMasterCardBomStructureUpdates
                             where a.Id == KeyId && a.ActionType == "C" && a.Posted == null
                             select a).ToList();
                if (check.Count > 0)
                {
                    //Declaration
                    BuildXmlStructure(check);
                    string XmlOut = sys.SysproSetupUpdate(Guid, BuildBomStructureParameter(), BuildXmlStructure(check), "BOMSST");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Update BOM Structure : " + ErrorMessage;
                    }

                    else
                    {
                        foreach (var item in check)
                        {
                            item.Posted = true;
                            item.DatePosted = DateTime.Now;
                            db.Entry(item).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                        }
                        PostMessage = "Bom changes posted successfully.";
                    }
                }
                //Check BomOperations
                var ops = (from a in db.mtMasterCardBomOperationsUpdates
                           where a.Id == KeyId && a.ActionType == "C" && a.Posted == null
                           select a).ToList();
                if (ops.Count > 0)
                {
                    //Declaration
                    BuildXmlOperations(ops);

                    string XmlOut = sys.SysproSetupUpdate(Guid, BuildBomOpsParameter(), BuildXmlOperations(ops), "BOMSRO");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Update BOM Operations : " + ErrorMessage;
                    }

                    else
                    {
                        foreach (var item in ops)
                        {
                            item.Posted = true;
                            item.DatePosted = DateTime.Now;
                            db.Entry(item).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                        }
                        PostMessage = "Bom changes posted successfully.";
                    }
                }
                //Check BomNarrations
                //var narr = db.sp_MasterCardGetBomNarrationsForPosting(KeyId).ToList();
                //if (narr.Count > 0)
                //{
                //    //Declaration
                //    BuildXmlNarration(narr);

                //    string XmlOut = sys.SysproSetupUpdate(Guid, BuildXmlNarration(narr), BuildXmlNarration(narr), "BOMSSN");
                //    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                //    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                //    {
                //        return "Failed to Update" +
                //            " BOM Narrations : " + ErrorMessage;
                //    }
                //}

                return PostMessage;


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string PostBomDelete(int KeyId)
        {
            try
            {
                string PostMessage = "";
                string Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    return "Failed to login to Syspro.";
                }
                //Check BomStructure
                var check = (from a in db.mtMasterCardBomStructureUpdates
                             where a.Id == KeyId && a.ActionType == "D" && a.Posted == null
                             select a).ToList();
                if (check.Count > 0)
                {
                    //Declaration
                    BuildXmlStructure(check);
                    string XmlOut = sys.SysproSetupDelete(Guid, BuildBomStructureParameter(), BuildXmlStructure(check), "BOMSST");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Delete BOM Structure : " + ErrorMessage;
                    }

                    else
                    {
                        foreach (var item in check)
                        {
                            item.Posted = true;
                            item.DatePosted = DateTime.Now;
                            db.Entry(item).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                        }
                        PostMessage = "Bom changes posted successfully.";
                    }
                }
                //Check BomOperations
                var ops = (from a in db.mtMasterCardBomOperationsUpdates
                           where a.Id == KeyId && a.ActionType == "D" && a.Posted == null
                           select a).ToList();
                if (ops.Count > 0)
                {
                    //Declaration
                    BuildXmlOperations(ops);
                    string XmlOut = sys.SysproSetupDelete(Guid, BuildBomOpsParameter(), BuildXmlOperations(ops), "BOMSRO");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        return "Failed to Delete BOM Operations : " + ErrorMessage;
                    }

                    else
                    {
                        foreach (var item in ops)
                        {
                            item.Posted = true;
                            item.DatePosted = DateTime.Now;
                            db.Entry(item).State = System.Data.EntityState.Modified;
                            db.SaveChanges();
                        }
                        PostMessage = "Bom changes posted successfully.";
                    }
                }

                ////Check BomNarration
                //var narr = db.sp_MasterCardGetBomNarrationsForPosting(KeyId).ToList();
                //if (narr.Count > 0)
                //{
                //    //Declaration
                //    BuildXmlNarration(narr);
                //    string XmlOut = sys.SysproSetupDelete(Guid, BuildXmlNarration(narr), BuildXmlNarration(narr), "BOMSSN");
                //    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                //    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                //    {
                //        return "Failed to Delete BOM Narrations : " + ErrorMessage;
                //    }
                //}

                return PostMessage;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}