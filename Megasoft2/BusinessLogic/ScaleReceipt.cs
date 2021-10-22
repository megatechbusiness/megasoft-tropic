using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{

    public class ScaleReceipt
    {


        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private SysproCore objSyspro = new SysproCore();


        //public string PostJobReceiptByBatch(string PalletNo)
        //{
        //    string Guid = "";
        //    try
        //    {               
        //        //user = current user
        //        string User = HttpContext.Current.User.Identity.Name.ToUpper();
        //        var JobsToPost = (from a in wdb.mtProductionLabels where a.PalletNo == PalletNo && (a.LabelReceipted == "N" || a.LabelReceipted == null)  select a).ToList();
        //        if (JobsToPost.Count == 0)
        //        {
        //            return "No unposted data found.";
        //        }

        //        List<WhseManJobReceipt> result = JobsToPost.GroupBy(l => l.Job).Select(cl => new WhseManJobReceipt { Job = cl.First().Job, Quantity = Convert.ToInt32(cl.Sum(c => c.ProductionQty)), Lot = cl.First().PalletNo}).ToList();
        //        string Journal = "Job Receipt Completed Successfully. Journal : ";
        //        Guid = objSyspro.SysproLogin();
        //        if (string.IsNullOrEmpty(Guid))
        //        {
        //            return "Failed to login to Syspro.";
        //        }

        //        foreach (var item in result)
        //        {                  
        //            string ErrorMessage = "";
        //            string XmlOut;

        //            var BatchList = (from a in JobsToPost where a.PalletNo == item.Lot && a.Job == item.Job select new WhseManJobReceipt { Job = a.Job, Lot = a.BatchId, Quantity = (decimal)a.ProductionQty }).ToList();
        //            XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), this.BuildJobReceiptDocument(BatchList), "WIPTJR");
        //            ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
        //            string JobJournal = objSyspro.GetXmlValue(XmlOut, "Journal");

        //            if (string.IsNullOrEmpty(ErrorMessage))
        //            {
        //                foreach (var a in BatchList)
        //                {
        //                    string CurrJob = a.Job.PadLeft(15, '0');
        //                    var Traceable = (from b in wdb.WipMasters where b.Job == CurrJob && b.TraceableType == "T" select b).ToList();
        //                    if (Traceable.Count > 0)
        //                    {
        //                        wdb.sp_UpdateLabelReceipted(a.Job.PadLeft(15, '0'), a.Lot, "Y", JobJournal, User);
        //                        var check = wdb.sp_BaggingCheckCustomForm(a.Lot, a.Job.PadLeft(15, '0')).ToList().Count();
        //                        if (check > 0)
        //                        {
        //                            wdb.sp_BaggingUpdateCustomForm(a.Lot, PalletNo, a.Job.PadLeft(15, '0'));
        //                        }
        //                        else
        //                        {
        //                            wdb.sp_BaggingSaveCustomForm(a.Lot, PalletNo, a.Job.PadLeft(15, '0'));
        //                        }
        //                    } 
        //                }
        //                mtPalletControl close = new mtPalletControl();
        //                close = wdb.mtPalletControls.Find(PalletNo);
        //                close.Status = "C";
        //                wdb.Entry(close).State = System.Data.EntityState.Modified;
        //                wdb.SaveChanges();

        //                Journal += JobJournal;
        //            }
        //            else
        //            {
        //                return "Job Receipt Error: " + ErrorMessage;
        //            }
        //        }
        //        objSyspro.SysproLogoff(Guid);
        //        return Journal;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //}

        //public string BuildJobReceiptDocument(List<WhseManJobReceipt> detail)
        //{
        //    //Declaration
        //    StringBuilder Document = new StringBuilder();

        //    //Building Document content
        //    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
        //    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
        //    Document.Append("<!--");
        //    Document.Append("Sample XML for the Job Receipt Posting Business Object");
        //    Document.Append("-->");
        //    Document.Append("<PostJobReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJRDOC.XSD\">");
        //    foreach (var item in detail)
        //    {
        //     //   var result = wdb.sp_GetProductionJobDetails(item.Job).FirstOrDefault();
        //        Document.Append("<Item>");
        //        Document.Append("<Journal></Journal>");
        //        Document.Append("<Job>" + item.Job + "</Job>");
        //        Document.Append("<CoProductLine />");
        //        Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
        //        Document.Append("<Quantity>" + item.Quantity.ToString() + "</Quantity>");
        //        Document.Append("<InspectionFlag>N</InspectionFlag>");
        //        Document.Append("<CostBasis>E</CostBasis>");
        //        //Document.Append("<ReceiptCost>441.73</ReceiptCost>");
        //        Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
        //        Document.Append("<MaterialDistributionValue />");
        //        Document.Append("<LaborDistributionValue />");
        //        Document.Append("<JobComplete> </JobComplete>");
        //        Document.Append("<CoProductComplete>N</CoProductComplete>");
        //        Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
        //        Document.Append("<SalesOrderComplete>N</SalesOrderComplete>");

        //        string Job = item.Job.PadLeft(15, '0');
        //        var Traceable = (from a in wdb.WipMasters where a.Job == Job && a.TraceableType == "T" select a).ToList();
        //        if (Traceable.Count > 0)
        //        {
        //            if (!string.IsNullOrEmpty(item.Lot))
        //            {
        //                Document.Append("<Lot>" + item.Lot + "</Lot>");
        //                Document.Append("<LotConcession>" + 1 + "</LotConcession>");
        //                Document.Append("<LotExpiryDate></LotExpiryDate>");
        //            }
        //        }

        //        Document.Append("<BinLocation></BinLocation>");
        //        Document.Append("<BinOnHold />");
        //        Document.Append("<BinOnHoldReason />");
        //        Document.Append("<BinUpdateWhDefault />");
        //        Document.Append("<FifoBucket />");
        //        //Document.Append("<Serials>");
        //        //Document.Append("<SerialNumber>8875</SerialNumber>");
        //        //Document.Append("<SerialQuantity>12</SerialQuantity>");
        //        //Document.Append("<SerialExpiryDate>2011-12-30</SerialExpiryDate>");
        //        //Document.Append("<SerialLocation />");
        //        //Document.Append("<SerialFifoBucket />");
        //        //Document.Append("</Serials>");
        //        Document.Append("<WipInspectionReference />");
        //        Document.Append("<WipInspectionNarration />");
        //        Document.Append("<HierarchyJob>");
        //        Document.Append("<Head />");
        //        Document.Append("<Section1 />");
        //        Document.Append("<Section2 />");
        //        Document.Append("<Section3 />");
        //        Document.Append("<Section4 />");
        //        Document.Append("<CostOfSalesAmount />");
        //        Document.Append("</HierarchyJob>");
        //        Document.Append("<AddReference>" + item.Lot + "</AddReference>");
        //        Document.Append("<MaterialReference />");
        //        Document.Append("<QuantityFromStock />");
        //        //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
        //        Document.Append("</Item>");
        //    }

        //    Document.Append("</PostJobReceipts>");

        //    return Document.ToString();
        //}

        //public string BuildJobReceiptParameter()
        //{
        //    //Declaration
        //    StringBuilder Parameter = new StringBuilder();

        //    //Building Parameter content
        //    Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
        //    Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
        //    Parameter.Append("<!--");
        //    Parameter.Append("Sample XML for the Job Receipt Posting Business Object");
        //    Parameter.Append("-->");
        //    Parameter.Append("<PostJobReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTJR.XSD\">");
        //    Parameter.Append("<Parameters>");
        //    Parameter.Append("<ValidateOnly>N</ValidateOnly>");
        //    Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
        //    Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
        //    Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
        //    Parameter.Append("<SetJobToCompleteIfCoProductsComplete>N</SetJobToCompleteIfCoProductsComplete>");
        //    Parameter.Append("</Parameters>");
        //    Parameter.Append("</PostJobReceipts>");

        //    return Parameter.ToString();
        //}
    }
}