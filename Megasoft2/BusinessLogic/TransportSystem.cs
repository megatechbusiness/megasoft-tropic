using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.BusinessLogic
{
    public class TransportSystem
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private SysproCore objSyspro = new SysproCore();

        public string BuildPurchaseOrderDocument(string Supplier, string Waybill, string Notes, string PoPrice, string PoQty, string Town, string Customer, decimal Weight, string TaxCode, string LineType, string Comment, string LedgerCode, string TrackId, string DispatchNote, string DispatchNoteLine)
        {
            try
            {

                //Declaration
                StringBuilder Document = new StringBuilder();

                if(LineType == "S")
                {
                   

                    string Description = "";
                    if (Weight == 0)
                    {
                        Description = Customer + '-' + Town;
                    }
                    else
                    {
                        Description = Customer + '-' + Town + "-" + Weight.ToString() + "kg";
                    }

                    //Building Document content

                    Document.Append("<StockLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<StockCode><![CDATA[" + Waybill.Trim() + "]]></StockCode>");
                    Document.Append("<StockDescription><![CDATA[" + Description.Trim() + "]]></StockDescription>");
                    Document.Append("<Warehouse>**</Warehouse>");

                    if(TrackId != "")
                    {
                        mtTransportWaybillDetail det = new mtTransportWaybillDetail();
                        det = wdb.mtTransportWaybillDetails.Find(Convert.ToInt32(TrackId), Waybill.ToString().Trim(), DispatchNote.ToString().Trim(), Convert.ToInt32(DispatchNoteLine));
                        Document.Append("<SupCatalogue>" + det.SeqNo + "</SupCatalogue>");
                    }
                   
                    Document.Append("<OrderQty>" + Convert.ToDecimal(PoQty) + "</OrderQty>");
                    Document.Append("<OrderUom>EA</OrderUom>");
                    Document.Append("<Units/>");
                    Document.Append("<Pieces/>");
                    Document.Append("<PriceMethod>M</PriceMethod>");
                    Document.Append("<SupplierContract/>");
                    Document.Append("<SupplierContractReference/>");
                    Document.Append("<Price>" + Convert.ToDecimal(PoPrice) + "</Price>");
                    Document.Append("<PriceUom>EA</PriceUom>");

                    //Document.Append("<LineDiscType>P</LineDiscType>");
                    //Document.Append("<LineDiscLessPlus>L</LineDiscLessPlus>");
                    //Document.Append("<LineDiscPercent1>0.5</LineDiscPercent1>");
                    //Document.Append("<LineDiscPercent2>0</LineDiscPercent2>");
                    //Document.Append("<LineDiscPercent3>0</LineDiscPercent3>");
                    //Document.Append("<LineDiscValue>0</LineDiscValue>");

                    Document.Append("<Taxable>Y</Taxable>");
                    Document.Append("<TaxCode>" + TaxCode + "</TaxCode>");
                    Document.Append("<Job/>");
                    Document.Append("<HierHead/>");
                    Document.Append("<Section1/>");
                    Document.Append("<Section2/>");
                    Document.Append("<Section3/>");
                    Document.Append("<Section4/>");
                    Document.Append("<Version/>");
                    Document.Append("<Release/>");
                    Document.Append("<LatestDueDate/>");
                    Document.Append("<OriginalDueDate/>");
                    Document.Append("<RescheduleDueDate/>");
                    Document.Append("<LedgerCode><![CDATA[" + LedgerCode.ToString() + "]]></LedgerCode>");
                    Document.Append("<PasswordForLedgerCode/>");
                    Document.Append("<SubcontractOp/>");
                    Document.Append("<InspectionReqd/>");
                    Document.Append("<ProductClass>0017</ProductClass>");
                    Document.Append("<NonsUnitMass/>");
                    Document.Append("<NonsUnitVol/>");
                    Document.Append("<BlanketPurchaseOrder/>");
                    Document.Append("<AttachOrderToBPO/>");
                    Document.Append("<WithholdingTaxExpenseType>G</WithholdingTaxExpenseType>");
                    Document.Append("<NonStockedLine>Y</NonStockedLine>");
                    Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
                    Document.Append("</StockLine>");

                }
                else
                {
                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment><![CDATA[" + Comment + "]]></Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");
                }





                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildPurchaseOrderParameter()
        {
            try
            {
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

                return Parameter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostPurchaseOrder(string details)
        {
            try
            {
                List<TransportSystemModel> myDeserializedObjList = (List<TransportSystemModel>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<TransportSystemModel>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Failed to Log in to Syspro.";
                    }
                    string Parameter, XmlOut, ErrorMessage;
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    string Supplier = myDeserializedObjList.FirstOrDefault().Supplier;

                    var Supp = (from a in wdb.mtTransporters where a.Transporter == Supplier select a).FirstOrDefault();



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
                    Document.Append("<OrderActionType>A</OrderActionType>");
                    Document.Append("<Supplier><![CDATA[" + myDeserializedObjList.FirstOrDefault().Supplier + "]]></Supplier>");
                    Document.Append("<ExchRateFixed/>");
                    Document.Append("<ExchangeRate/>");
                    Document.Append("<OrderType>L</OrderType>");

                    if(myDeserializedObjList.FirstOrDefault().Taxable == true)
                    {
                        Document.Append("<TaxStatus>N</TaxStatus>");
                    }
                    else
                    {
                        Document.Append("<TaxStatus>E</TaxStatus>");
                    }

                    Document.Append("<PaymentTerms/>");
                    Document.Append("<InvoiceTerms>1</InvoiceTerms>");
                    Document.Append("<CustomerPoNumber></CustomerPoNumber>");

                    Document.Append("<OrderDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</OrderDate>");
                    Document.Append("<DueDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</DueDate>");
                    Document.Append("<MemoDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</MemoDate>");
                    Document.Append("<ApplyDueDateToLines>A</ApplyDueDateToLines>");
                    Document.Append("<MemoCode/>");
                    Document.Append("<Buyer>IS</Buyer>");

                    Document.Append("<AutoVoucher></AutoVoucher>");
                    Document.Append("<LanguageCode></LanguageCode>");
                    Document.Append("<Warehouse>**</Warehouse>");
                    Document.Append("<DiscountLessPlus/>");

                    Document.Append("<ChgPOStatToReadyToPrint/>");
                    Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
                    Document.Append("<eSignature/>");
                    Document.Append("</OrderHeader>");
                    Document.Append("<OrderDetails>");

                    var LedgerCode = (from a in wdb.mtTransporters where a.Transporter == Supplier.Trim() select a.GLCode).FirstOrDefault();
                    var TermsCode = wdb.sp_GetSupplierTermsCode(Supplier).FirstOrDefault();

                    foreach (var item in myDeserializedObjList)
                    {
                        if(item.Weight == "")
                        {
                            item.Weight = "0";
                        }
                        Document.Append(this.BuildPurchaseOrderDocument(item.Supplier, item.Waybill, item.Notes, item.PoPrice, item.PoQty, item.Town, item.Customer, Convert.ToDecimal(item.Weight), myDeserializedObjList.FirstOrDefault().TaxCode, item.LineType, item.Comment, LedgerCode,item.TrackId,item.DispatchNote,item.DispatchNoteLine));
                    }

                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment><![CDATA[.]]></Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");

                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment><![CDATA[DEPT : DELIVERY COSTS     " + LedgerCode + "]]></Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");

                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment>TERMS : " + TermsCode.Description + " FROM DATE OF STATEMENT</Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");

                    Document.Append("</OrderDetails>");
                    Document.Append("</Orders>");
                    Document.Append("</PostPurchaseOrders>");
                    Parameter = this.BuildPurchaseOrderParameter();
                    XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "PORTOI");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        string PurchaseOrder = objSyspro.GetFirstXmlValue(XmlOut, "PurchaseOrder");
                         var Username = HttpContext.Current.User.Identity.Name.ToUpper();

                        foreach (var item in myDeserializedObjList)
                        {
                            if(!string.IsNullOrEmpty(item.DispatchNote))
                            {
                                mtTransportWaybillDetail det = new mtTransportWaybillDetail();
                                det = wdb.mtTransportWaybillDetails.Find(Convert.ToInt32(item.TrackId), item.Waybill.ToString().Trim(), item.DispatchNote.ToString().Trim(), Convert.ToInt32(item.DispatchNoteLine));
                                wdb.sp_TransUpdateWaybill(Convert.ToInt32(item.TrackId),item.Waybill, item.DispatchNote, Convert.ToInt32(item.DispatchNoteLine), PurchaseOrder.PadLeft(15, '0'), 0, Username);
                                wdb.sp_TransUpdatePurchaseOrderLine(PurchaseOrder.PadLeft(15, '0'), det.SeqNo);
                            }

                        }


                        this.SaveCustomForms(PurchaseOrder);

                        return "Waybills posted successfully. Purchase Order : " + PurchaseOrder;
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



        public void SaveCustomForms(string PurchaseOrder)
        {
            try
            {
                wdb.sp_TransSaveCustomForms(PurchaseOrder.PadLeft(15, '0'));
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}