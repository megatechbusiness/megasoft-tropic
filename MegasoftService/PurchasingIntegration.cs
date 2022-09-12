using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegasoftService
{
    class PurchasingIntegration
    {
        SysproEntities sdb = new SysproEntities();
        SysproCore sys = new SysproCore();
        public void PostGrn()
        {
            try
            {

                var Grn = (from a in sdb.mtGrnDetails.AsNoTracking() where a.PostStatus == 1 select a.Grn).Distinct().ToList();
                if (Grn.Count > 0)
                {

                    string Guid = sys.SysproLogin(Properties.Settings.Default.SysproUser);
                    foreach (var grnItem in Grn)
                    {
                        var det = (from a in sdb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();


                        bool ToggleRequired = this.CheckGrnMatchingToggleRequired(det.FirstOrDefault().Supplier);
                        if (ToggleRequired == true)
                        {
                            this.ToggleGrnMatching(det.FirstOrDefault().Supplier, "Y");
                        }


                        //Declaration
                        StringBuilder Document = new StringBuilder();

                        //Building Document content
                        Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("This is an example XML instance to demonstrate");
                        Document.Append("use of the Purchase Order Receipts Business Object");
                        Document.Append("-->");
                        Document.Append("<PostPurchaseOrderReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"portordoc.xsd\">");

                        //string Username = "";
                        //if(!string.IsNullOrEmpty(det.FirstOrDefault().Level2AuthorizedBy))
                        //{
                        //    Username = det.FirstOrDefault().Level2AuthorizedBy;
                        //}
                        //else
                        //{
                        //    Username = det.FirstOrDefault().Level1AuthorizedBy;
                        //}

                        //if multi tax line dont do grn
                        var TaxCodeCount = (from a in det select a.TaxCode).Distinct().ToList();
                        if (TaxCodeCount.Count > 1)
                        {
                            //check if any stock items exist
                            var Stocked = (from a in det where a.Warehouse != "**" select a).ToList();
                            if (Stocked.Count > 0)
                            {
                                foreach (var line in Stocked)
                                {
                                    Document.Append(this.BuildGrnLines(line.PurchaseOrder, (int)line.PurchaseOrderLin, line.QtyReceived, line.DeliveryNote, line.GlCode, line.SuspenseAccount, line.AnalysisEntry, (decimal)line.Price, line.StockDescription.ToString(), line.Warehouse, line.Supplier, line.Requisition));
                                }
                                Document.Append("</PostPurchaseOrderReceipts>");
                                //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                string XmlOut = "";


                                //Syspro Post
                                try
                                {
                                    XmlOut = sys.SysproPost(Guid, this.BuildGrnParameter(), Document.ToString(), "PORTOR");
                                }
                                catch (Exception ex)
                                {
                                    ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                    ErrorEventLog.WriteErrorLog("E", ex.Message);
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var re in det)
                                        {
                                            re.GrnPostDate = DateTime.Now;
                                            re.GrnError = "Grn Error : " + ex.Message;
                                            re.PostStatus = 3;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                        }
                                    }
                                    sys.SysproLogoff(Guid);
                                    if (ToggleRequired == true)
                                    {
                                        this.ToggleGrnMatching(det.FirstOrDefault().Supplier, "N");
                                    }
                                    return;
                                }


                                //ErrorEventLog.WriteErrorLog("E", XmlOut);
                                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                                if (!string.IsNullOrEmpty(ErrorMessage))
                                {
                                    ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                    ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                                    //Error Occured
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var re in det)
                                        {
                                            re.GrnPostDate = DateTime.Now;
                                            re.GrnError = "Grn Error : " + ErrorMessage;
                                            re.PostStatus = 3;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                        }
                                    }
                                }
                                else
                                {
                                    string SysGrn = sys.GetFirstXmlValue(XmlOut, "Grn");
                                    string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");
                                    string JournalYear = sys.GetFirstXmlValue(XmlOut, "JnlYear");
                                    string JournalMonth = sys.GetFirstXmlValue(XmlOut, "JnlMonth");
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var re in det)
                                        {
                                            re.GrnPostDate = DateTime.Now;
                                            re.SysproGrn = SysGrn;
                                            re.Journal = Journal;
                                            re.GrnJournalYear = Convert.ToDecimal(JournalYear);
                                            re.GrnJournalMonth = Convert.ToDecimal(JournalMonth);
                                            re.GrnError = "";
                                            re.PostStatus = 2;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                            //ErrorEventLog.WriteErrorLog("I", "Purchase Order Receipt completed successfully : Syspro Grn : " + SysGrn);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //All items non Stocked. Dont do Grn
                                using (var edb = new SysproEntities())
                                {
                                    var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                    foreach (var re in result)
                                    {
                                        re.GrnPostDate = DateTime.Now;
                                        re.GrnError = "";
                                        re.PostStatus = 2;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();
                                        //ErrorEventLog.WriteErrorLog("I", "Purchase Order Receipt completed successfully : Syspro Grn : " + SysGrn);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var line in det)
                            {
                                Document.Append(this.BuildGrnLines(line.PurchaseOrder, (int)line.PurchaseOrderLin, line.QtyReceived, line.DeliveryNote, line.GlCode, line.SuspenseAccount, line.AnalysisEntry, (decimal)line.Price, line.StockDescription.ToString(), line.Warehouse, line.Supplier, line.Requisition));
                            }
                            Document.Append("</PostPurchaseOrderReceipts>");
                            //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                            string XmlOut = "";


                            //Syspro Post
                            try
                            {
                                XmlOut = sys.SysproPost(Guid, this.BuildGrnParameter(), Document.ToString(), "PORTOR");
                            }
                            catch (Exception ex)
                            {
                                ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                ErrorEventLog.WriteErrorLog("E", ex.Message);
                                using (var edb = new SysproEntities())
                                {

                                    foreach (var re in det)
                                    {
                                        re.GrnPostDate = DateTime.Now;
                                        re.GrnError = "Grn Error : " + ex.Message;
                                        re.PostStatus = 3;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();


                                    }
                                }
                                sys.SysproLogoff(Guid);
                                if (ToggleRequired == true)
                                {
                                    this.ToggleGrnMatching(det.FirstOrDefault().Supplier, "N");
                                }
                                return;
                            }


                            //ErrorEventLog.WriteErrorLog("E", XmlOut);
                            string ErrorMessage = sys.GetXmlErrors(XmlOut);
                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {
                                ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                                //Error Occured
                                using (var edb = new SysproEntities())
                                {

                                    foreach (var re in det)
                                    {
                                        re.GrnPostDate = DateTime.Now;
                                        re.GrnError = "Grn Error : " + ErrorMessage;
                                        re.PostStatus = 3;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();

                                    }
                                }
                            }
                            else
                            {
                                string SysGrn = sys.GetFirstXmlValue(XmlOut, "Grn");
                                string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");
                                string JournalYear = sys.GetFirstXmlValue(XmlOut, "JnlYear");
                                string JournalMonth = sys.GetFirstXmlValue(XmlOut, "JnlMonth");
                                using (var edb = new SysproEntities())
                                {

                                    foreach (var re in det)
                                    {
                                        re.GrnPostDate = DateTime.Now;
                                        re.SysproGrn = SysGrn;
                                        re.Journal = Journal;
                                        re.GrnJournalYear = Convert.ToDecimal(JournalYear);
                                        re.GrnJournalMonth = Convert.ToDecimal(JournalMonth);
                                        re.GrnError = "";
                                        re.PostStatus = 2;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();
                                        //ErrorEventLog.WriteErrorLog("I", "Purchase Order Receipt completed successfully : Syspro Grn : " + SysGrn);
                                    }
                                }
                            }
                        }


                        if (ToggleRequired == true)
                        {
                            this.ToggleGrnMatching(det.FirstOrDefault().Supplier, "N");
                        }
                    }
                    sys.SysproLogoff(Guid);



                }
            }
            catch (Exception ex)
            {
                throw new Exception("Grn Post Error: " + ex.Message);
            }
        }


        public void DoMaterialIssuePost()
        {
            try
            {
                //Post Material Issue

                var Grn = (from a in sdb.mtGrnDetails.AsNoTracking() where a.PostStatus == 4 select a.Grn).Distinct().ToList();
                if (Grn.Count > 0)
                {

                    string Guid = sys.SysproLogin(Properties.Settings.Default.SysproUser);
                    foreach (var grnItem in Grn)
                    {

                        var det = (from a in sdb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                        string Username = "";
                        if (!string.IsNullOrEmpty(det.FirstOrDefault().Level2AuthorizedBy))
                        {
                            Username = det.FirstOrDefault().Level2AuthorizedBy;
                        }
                        else
                        {
                            Username = det.FirstOrDefault().Level1AuthorizedBy;
                        }



                        //Syspro Post
                        string MatError = "";
                        try
                        {
                            MatError = this.PostMaterialIssue(Guid, grnItem);
                        }
                        catch (Exception ex)
                        {
                            using (var edb = new SysproEntities())
                            {
                                var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                foreach (var re in result)
                                {
                                    re.MaterialIssueError = ex.Message;
                                    re.PostStatus = 7;
                                    edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                    edb.SaveChanges();
                                }

                            }
                            sys.SysproLogoff(Guid);
                            return;
                        }

                        if (!string.IsNullOrEmpty(MatError))
                        {
                            using (var edb = new SysproEntities())
                            {
                                var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                foreach (var re in result)
                                {
                                    re.MaterialIssueError = MatError;
                                    re.PostStatus = 7;
                                    edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                    edb.SaveChanges();
                                }

                            }
                        }
                        else
                        {
                            using (var edb = new SysproEntities())
                            {
                                var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                foreach (var re in result)
                                {
                                    re.MaterialIssueError = "";
                                    re.PostStatus = 6;
                                    edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                    edb.SaveChanges();
                                }
                            }
                        }

                    }
                    sys.SysproLogoff(Guid);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Material Issue Error: " + ex.Message);
            }
        }

        public void PostMaterialAllocation()
        {
            try
            {
                var Grn = (from a in sdb.mtGrnDetails.AsNoTracking() where a.PostStatus == 2 select a.Grn).Distinct().ToList();
                if (Grn.Count > 0)
                {
                    string Guid = sys.SysproLogin(Properties.Settings.Default.SysproUser);
                    foreach (var grnItem in Grn)
                    {
                        var det = (from a in sdb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                        string Username = "";
                        if (!string.IsNullOrEmpty(det.FirstOrDefault().Level2AuthorizedBy))
                        {
                            Username = det.FirstOrDefault().Level2AuthorizedBy;
                        }
                        else
                        {
                            Username = det.FirstOrDefault().Level1AuthorizedBy;
                        }



                        string MatAllocError = "";
                        //Syspro Post
                        try
                        {
                            MatAllocError = this.AddMaterialAllocation(Guid, grnItem);
                        }
                        catch (Exception ex)
                        {
                            using (var edb = new SysproEntities())
                            {
                                var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                foreach (var re in result)
                                {
                                    re.PostStatus = 5;
                                    re.MaterialAllocationError = "Failed to add Material Allocation. " + ex.Message + ".";
                                    edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                    edb.SaveChanges();
                                }

                            }
                            ErrorEventLog.WriteErrorLog("E", "Failed to add Material Allocation. " + ex.Message + ".");
                            sys.SysproLogoff(Guid);
                            return;
                        }

                        if (!string.IsNullOrEmpty(MatAllocError))
                        {
                            using (var edb = new SysproEntities())
                            {
                                var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                foreach (var re in result)
                                {
                                    re.PostStatus = 5;
                                    re.MaterialAllocationError = "Failed to add Material Allocation. " + MatAllocError + ".";
                                    edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                    edb.SaveChanges();
                                }

                            }
                            ErrorEventLog.WriteErrorLog("E", MatAllocError);
                        }
                        else
                        {
                            using (var edb = new SysproEntities())
                            {
                                var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                foreach (var re in result)
                                {
                                    re.PostStatus = 4;
                                    re.MaterialAllocationError = "";
                                    edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                    edb.SaveChanges();
                                }
                            }

                            //Do Material Issue
                            string MatError = "";
                            try
                            {
                                MatError = this.PostMaterialIssue(Guid, grnItem);
                            }
                            catch (Exception ex)
                            {
                                using (var edb = new SysproEntities())
                                {
                                    var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                    foreach (var re in result)
                                    {
                                        re.MaterialIssueError = ex.Message;
                                        re.PostStatus = 7;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();
                                    }

                                }
                                sys.SysproLogoff(Guid);
                                return;
                            }

                            if (!string.IsNullOrEmpty(MatError))
                            {
                                using (var edb = new SysproEntities())
                                {
                                    var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                    foreach (var re in result)
                                    {
                                        re.MaterialIssueError = MatError;
                                        re.PostStatus = 7;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();
                                    }

                                }
                            }
                            else
                            {
                                using (var edb = new SysproEntities())
                                {
                                    var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                    foreach (var re in result)
                                    {
                                        re.MaterialIssueError = "";
                                        re.PostStatus = 6;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();
                                    }
                                }
                            }



                        }

                    }
                    sys.SysproLogoff(Guid);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Material Allocation Error: " + ex.Message);
            }
        }


        public void PostInvoiceMatch()
        {
            try
            {

                var Grn = (from a in sdb.mtGrnDetails.AsNoTracking() where a.PostStatus == 6 select a.Grn).Distinct().ToList();
                if (Grn.Count > 0)
                {
                    string Guid = sys.SysproLogin(Properties.Settings.Default.SysproUser);
                    foreach (var grnItem in Grn)
                    {
                        var detUser = (from a in sdb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();

                        bool ToggleRequired = this.CheckGrnMatchingToggleRequired(detUser.FirstOrDefault().Supplier);
                        if (ToggleRequired == true)
                        {
                            this.ToggleGrnMatching(detUser.FirstOrDefault().Supplier, "Y");
                        }

                        string Branch = detUser.FirstOrDefault().Branch;
                        string Site = detUser.FirstOrDefault().Site;
                        string ApBranch = (from a in sdb.mtBranchSites.AsNoTracking() where a.Branch == Branch && a.Site == Site select a.ApBranch).FirstOrDefault();


                        var det = sdb.sp_GetInvoiceDetailsByGrn(grnItem).Where(a => a.PostStatus == 6).ToList();

                        var TaxCodeCount = (from a in det select a.TaxCode).Distinct().ToList();

                        if (TaxCodeCount.Count > 1)
                        {
                            var Warehouses = (from a in det where a.Warehouse != "**" && a.PostStatus == 6 select a).ToList();
                            var InvoiceAmount = (from a in Warehouses select a.LineTotal + a.LineVAT).Sum();
                            var VatAmount = (from a in Warehouses select a.LineVAT).Sum();
                            if (Warehouses.Count > 0)
                            {
                                //Do stocked items Stocked First
                                StringBuilder Document = new StringBuilder();



                                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                                Document.Append("<!--");
                                Document.Append("Sample XML for the Post AP Invoice Business Object");
                                Document.Append("-->");
                                Document.Append("<PostApInvoice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSTINDOC.XSD\">");
                                Document.Append("<Item>");
                                Document.Append("<Supplier><![CDATA[" + Warehouses.FirstOrDefault().Supplier + "]]></Supplier>");
                                Document.Append("<TransactionCode>I</TransactionCode>");
                                Document.Append("<Branch>" + ApBranch + "</Branch>");
                                Document.Append("<Invoice><![CDATA[" + Warehouses.FirstOrDefault().Invoice + "/1" + "]]></Invoice>");
                                Document.Append("<DiscountBasis>T</DiscountBasis>");
                                Document.Append("<DiscountableValue>" + string.Format("{0:.##}", InvoiceAmount) + "</DiscountableValue>");
                                Document.Append("<EntryNumber />");
                                Document.Append("<TransactionValue>" + string.Format("{0:.##}", InvoiceAmount) + "</TransactionValue>"); //Total Including Tax. Invoice Amount = Tax Value + Total Grn Value
                                Document.Append("<TaxCode>" + Warehouses.FirstOrDefault().TaxCode + "</TaxCode>");
                                Document.Append("<TaxValue>" + string.Format("{0:.##}", VatAmount) + "</TaxValue>");
                                Document.Append("<MiscellaneousCharge />");
                                Document.Append("<Notation><![CDATA[" + Warehouses.FirstOrDefault().Requisition + "]]></Notation>");
                                Document.Append("<TransactionReference><![CDATA[" + Warehouses.FirstOrDefault().Requisition + "]]></TransactionReference>");
                                Document.Append("<JournalNotation><![CDATA[" + Warehouses.FirstOrDefault().Requisition + "]]></JournalNotation>");
                                Document.Append("<InvoiceDate>" + Convert.ToDateTime(Warehouses.FirstOrDefault().InvoiceDate).ToString("yyyy-MM-dd") + "</InvoiceDate>");
                                Document.Append("<DueDate />");
                                Document.Append("<DiscountDate />");
                                Document.Append("<ExchRateAtEntry />");
                                Document.Append("<FixedExchRate />");
                                Document.Append("<TaxBasis>I</TaxBasis>");//I
                                Document.Append("<CalculateQstOnTax>Y</CalculateQstOnTax>");
                                Document.Append("<AnalysisEntry />");
                                foreach (var line in Warehouses)
                                {
                                    //if (line.SysproGrn == null) { line.SysproGrn = ""; };
                                    //if (line.Journal == null) { line.Journal = 0; };
                                    Document.Append(BuildGrnMatchDocument(line.SysproGrn, line.Supplier, (decimal)line.Journal, (decimal)line.JournalEntry, line.GlCode, line.TaxCode, (decimal)line.LineTotal, line.AnalysisRequired, line.AnalysisEntry, line.SuspenseAccount, false, (decimal)line.LineVAT, line.StockDescription, (decimal)line.QtyReceived, line.Job, line.GrnLine));

                                }
                                Document.Append("</Item>");
                                Document.Append("</PostApInvoice>");

                                string XmlOut = "";
                                //Syspro Post
                                try
                                {
                                    XmlOut = sys.SysproPost(Guid, this.BuildGrnMatchingParameter((int)Warehouses.FirstOrDefault().ReqGrnYear, (int)Warehouses.FirstOrDefault().ReqGrnMonth), Document.ToString(), "APSTIN");
                                }
                                catch (Exception ex)
                                {
                                    //Error
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var line in Warehouses)
                                        {
                                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                            foreach (var re in result)
                                            {
                                                re.InvoicePostDate = DateTime.Now;
                                                re.PostStatus = 9;
                                                re.InvoiceError = ex.Message;
                                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                                edb.SaveChanges();
                                                ErrorEventLog.WriteErrorLog("E", ex.Message);
                                            }
                                        }

                                    }
                                    if (ToggleRequired == true)
                                    {
                                        this.ToggleGrnMatching(detUser.FirstOrDefault().Supplier, "N");
                                    }
                                    sys.SysproLogoff(Guid);
                                    return;
                                }


                                //ErrorEventLog.WriteErrorLog("E", XmlOut.ToString());
                                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                                if (!string.IsNullOrEmpty(ErrorMessage))
                                {
                                    //Error
                                    //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                    ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var line in Warehouses)
                                        {
                                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                            foreach (var re in result)
                                            {
                                                re.InvoicePostDate = DateTime.Now;
                                                re.PostStatus = 9;
                                                re.InvoiceError = ErrorMessage;
                                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                                edb.SaveChanges();

                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");
                                    string JournalYear = sys.GetFirstXmlValue(XmlOut, "TrnYear");
                                    string JournalMonth = sys.GetFirstXmlValue(XmlOut, "TrnMonth");
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var line in Warehouses)
                                        {
                                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                            foreach (var re in result)
                                            {
                                                re.ApJournal = Journal;
                                                re.ApJournalYear = Convert.ToDecimal(JournalYear);
                                                re.ApJournalMonth = Convert.ToDecimal(JournalMonth);
                                                re.InvoicePostDate = DateTime.Now;
                                                re.InvoiceError = "";
                                                re.PostStatus = 8;
                                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                                edb.SaveChanges();
                                                //ErrorEventLog.WriteErrorLog("I", "Invoice Matching completed successfully.");
                                            }
                                        }
                                    }


                                }


                            }
                            else //Process as Normal
                            {
                                bool MultiTax = false;

                                StringBuilder Document = new StringBuilder();


                                InvoiceAmount = (from a in det select a.LineTotal + a.LineVAT).Sum();
                                VatAmount = (from a in det select a.LineVAT).Sum();
                                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                                Document.Append("<!--");
                                Document.Append("Sample XML for the Post AP Invoice Business Object");
                                Document.Append("-->");
                                Document.Append("<PostApInvoice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSTINDOC.XSD\">");
                                Document.Append("<Item>");
                                Document.Append("<Supplier><![CDATA[" + det.FirstOrDefault().Supplier + "]]></Supplier>");
                                Document.Append("<TransactionCode>I</TransactionCode>");
                                Document.Append("<Branch>" + ApBranch + "</Branch>");
                                Document.Append("<Invoice><![CDATA[" + det.FirstOrDefault().Invoice + "]]></Invoice>");
                                Document.Append("<DiscountBasis>T</DiscountBasis>");
                                Document.Append("<DiscountableValue>" + string.Format("{0:.##}", InvoiceAmount) + "</DiscountableValue>");
                                Document.Append("<EntryNumber />");
                                Document.Append("<TransactionValue>" + string.Format("{0:.##}", InvoiceAmount) + "</TransactionValue>"); //Total Including Tax. Invoice Amount = Tax Value + Total Grn Value
                                if (TaxCodeCount.Count() > 1)
                                {
                                    MultiTax = true;
                                    Document.Append("<TaxCode>D</TaxCode>");
                                    Document.Append("<TaxValue>" + string.Format("{0:.##}", VatAmount) + "</TaxValue>");
                                }
                                else
                                {
                                    MultiTax = false;
                                    Document.Append("<TaxCode>" + det.FirstOrDefault().TaxCode + "</TaxCode>");
                                    Document.Append("<TaxValue>" + string.Format("{0:.##}", VatAmount) + "</TaxValue>");
                                }

                                Document.Append("<MiscellaneousCharge />");
                                Document.Append("<Notation><![CDATA[" + det.FirstOrDefault().Requisition + "]]></Notation>");
                                Document.Append("<TransactionReference><![CDATA[" + det.FirstOrDefault().Requisition + "]]></TransactionReference>");
                                Document.Append("<JournalNotation><![CDATA[" + det.FirstOrDefault().Requisition + "]]></JournalNotation>");
                                Document.Append("<InvoiceDate>" + Convert.ToDateTime(det.FirstOrDefault().InvoiceDate).ToString("yyyy-MM-dd") + "</InvoiceDate>");
                                Document.Append("<DueDate />");
                                Document.Append("<DiscountDate />");
                                Document.Append("<ExchRateAtEntry />");
                                Document.Append("<FixedExchRate />");
                                Document.Append("<TaxBasis>I</TaxBasis>");//I
                                Document.Append("<CalculateQstOnTax>Y</CalculateQstOnTax>");
                                Document.Append("<AnalysisEntry />");
                                foreach (var line in det)
                                {
                                    //if (line.SysproGrn == null) { line.SysproGrn = ""; };
                                    //if (line.Journal == null) { line.Journal = 0; };
                                    Document.Append(BuildGrnMatchDocument(line.SysproGrn, line.Supplier, (decimal)line.Journal, (decimal)line.JournalEntry, line.GlCode, line.TaxCode, (decimal)line.LineTotal, line.AnalysisRequired, line.AnalysisEntry, line.SuspenseAccount, MultiTax, (decimal)line.LineVAT, line.StockDescription, (decimal)line.QtyReceived, line.Job, line.GrnLine));

                                }
                                Document.Append("</Item>");
                                Document.Append("</PostApInvoice>");
                                //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                //ErrorEventLog.WriteErrorLog("E", this.BuildGrnMatchingParameter((int)det.FirstOrDefault().ReqGrnYear, (int)det.FirstOrDefault().ReqGrnMonth));


                                string XmlOut = "";
                                //Syspro Post
                                try
                                {
                                    XmlOut = sys.SysproPost(Guid, this.BuildGrnMatchingParameter((int)det.FirstOrDefault().ReqGrnYear, (int)det.FirstOrDefault().ReqGrnMonth), Document.ToString(), "APSTIN");
                                }
                                catch (Exception ex)
                                {
                                    //Error
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var line in det)
                                        {
                                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                            foreach (var re in result)
                                            {
                                                re.InvoicePostDate = DateTime.Now;
                                                re.PostStatus = 9;
                                                re.InvoiceError = ex.Message;
                                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                                edb.SaveChanges();
                                                ErrorEventLog.WriteErrorLog("E", ex.Message);
                                            }
                                        }

                                    }
                                    sys.SysproLogoff(Guid);
                                    if (ToggleRequired == true)
                                    {
                                        this.ToggleGrnMatching(detUser.FirstOrDefault().Supplier, "N");
                                    }
                                    return;
                                }


                                //ErrorEventLog.WriteErrorLog("E", XmlOut.ToString());
                                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                                if (!string.IsNullOrEmpty(ErrorMessage))
                                {
                                    //Error
                                    //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                    ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var line in det)
                                        {
                                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                            foreach (var re in result)
                                            {
                                                re.InvoicePostDate = DateTime.Now;
                                                re.PostStatus = 9;
                                                re.InvoiceError = ErrorMessage;
                                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                                edb.SaveChanges();

                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");
                                    string JournalYear = sys.GetFirstXmlValue(XmlOut, "TrnYear");
                                    string JournalMonth = sys.GetFirstXmlValue(XmlOut, "TrnMonth");
                                    using (var edb = new SysproEntities())
                                    {
                                        foreach (var line in det)
                                        {
                                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                            foreach (var re in result)
                                            {
                                                re.ApJournal = Journal;
                                                re.ApJournalYear = Convert.ToDecimal(JournalYear);
                                                re.ApJournalMonth = Convert.ToDecimal(JournalMonth);
                                                re.InvoicePostDate = DateTime.Now;
                                                re.InvoiceError = "";
                                                re.PostStatus = 8;

                                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                                edb.SaveChanges();
                                                //ErrorEventLog.WriteErrorLog("I", "Invoice Matching completed successfully.");
                                            }
                                        }
                                    }

                                    //Update Po Receipt Qty
                                    if (MultiTax == true)
                                    {
                                        sdb.sp_UpdatePurchaseOrderReceiptQty(grnItem);
                                    }
                                }
                            }
                        }
                        else //Process as Normal
                        {
                            bool MultiTax = false;

                            StringBuilder Document = new StringBuilder();

                            var InvoiceAmount = (from a in det select a.LineTotal + a.LineVAT).Sum();
                            var VatAmount = (from a in det select a.LineVAT).Sum();

                            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                            Document.Append("<!--");
                            Document.Append("Sample XML for the Post AP Invoice Business Object");
                            Document.Append("-->");
                            Document.Append("<PostApInvoice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSTINDOC.XSD\">");
                            Document.Append("<Item>");
                            Document.Append("<Supplier><![CDATA[" + det.FirstOrDefault().Supplier + "]]></Supplier>");
                            Document.Append("<TransactionCode>I</TransactionCode>");
                            Document.Append("<Branch>" + ApBranch + "</Branch>");
                            Document.Append("<Invoice><![CDATA[" + det.FirstOrDefault().Invoice + "]]></Invoice>");
                            Document.Append("<DiscountBasis>T</DiscountBasis>");
                            Document.Append("<DiscountableValue>" + string.Format("{0:.##}", InvoiceAmount) + "</DiscountableValue>");
                            Document.Append("<EntryNumber />");
                            Document.Append("<TransactionValue>" + string.Format("{0:.##}", InvoiceAmount) + "</TransactionValue>"); //Total Including Tax. Invoice Amount = Tax Value + Total Grn Value
                            if (TaxCodeCount.Count() > 1)
                            {
                                MultiTax = true;
                                Document.Append("<TaxCode>" + det.FirstOrDefault().TaxCode + "</TaxCode>");
                                Document.Append("<TaxValue>" + string.Format("{0:.##}", VatAmount) + "</TaxValue>");
                            }
                            else
                            {
                                MultiTax = false;
                                Document.Append("<TaxCode>" + det.FirstOrDefault().TaxCode + "</TaxCode>");
                                Document.Append("<TaxValue>" + string.Format("{0:.##}", VatAmount) + "</TaxValue>");
                            }

                            Document.Append("<MiscellaneousCharge />");
                            Document.Append("<Notation><![CDATA[" + det.FirstOrDefault().Requisition + "]]></Notation>");
                            Document.Append("<TransactionReference><![CDATA[" + det.FirstOrDefault().Requisition + "]]></TransactionReference>");
                            Document.Append("<JournalNotation><![CDATA[" + det.FirstOrDefault().Requisition + "]]></JournalNotation>");
                            Document.Append("<InvoiceDate>" + Convert.ToDateTime(det.FirstOrDefault().InvoiceDate).ToString("yyyy-MM-dd") + "</InvoiceDate>");
                            Document.Append("<DueDate />");
                            Document.Append("<DiscountDate />");
                            Document.Append("<ExchRateAtEntry />");
                            Document.Append("<FixedExchRate />");
                            Document.Append("<TaxBasis>I</TaxBasis>");//I
                            Document.Append("<CalculateQstOnTax>Y</CalculateQstOnTax>");
                            Document.Append("<AnalysisEntry />");
                            foreach (var line in det)
                            {
                                //if (line.SysproGrn == null) { line.SysproGrn = ""; };
                                //if (line.Journal == null) { line.Journal = 0; };
                                Document.Append(BuildGrnMatchDocument(line.SysproGrn, line.Supplier, (decimal)line.Journal, (decimal)line.JournalEntry, line.GlCode, line.TaxCode, (decimal)line.LineTotal, line.AnalysisRequired, line.AnalysisEntry, line.SuspenseAccount, MultiTax, (decimal)line.LineVAT, line.StockDescription, (decimal)line.QtyReceived, line.Job, line.GrnLine));

                            }
                            Document.Append("</Item>");
                            Document.Append("</PostApInvoice>");
                            ErrorEventLog.WriteErrorLog("E", Document.ToString());
                            ErrorEventLog.WriteErrorLog("E", this.BuildGrnMatchingParameter((int)det.FirstOrDefault().ReqGrnYear, (int)det.FirstOrDefault().ReqGrnMonth));

                            string XmlOut = "";
                            //Syspro Post
                            try
                            {
                                XmlOut = sys.SysproPost(Guid, this.BuildGrnMatchingParameter((int)det.FirstOrDefault().ReqGrnYear, (int)det.FirstOrDefault().ReqGrnMonth), Document.ToString(), "APSTIN");
                            }
                            catch (Exception ex)
                            {
                                //Error
                                using (var edb = new SysproEntities())
                                {
                                    foreach (var line in det)
                                    {
                                        var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                        foreach (var re in result)
                                        {
                                            re.InvoicePostDate = DateTime.Now;
                                            re.PostStatus = 9;
                                            re.InvoiceError = ex.Message;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                            ErrorEventLog.WriteErrorLog("E", ex.Message);
                                        }
                                    }

                                }
                                sys.SysproLogoff(Guid);
                                if (ToggleRequired == true)
                                {
                                    this.ToggleGrnMatching(detUser.FirstOrDefault().Supplier, "N");
                                }
                                return;
                            }


                            //ErrorEventLog.WriteErrorLog("E", XmlOut.ToString());
                            string ErrorMessage = sys.GetXmlErrors(XmlOut);
                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {
                                //Error
                                //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                                ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                                using (var edb = new SysproEntities())
                                {
                                    foreach (var line in det)
                                    {
                                        var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                        foreach (var re in result)
                                        {
                                            re.InvoicePostDate = DateTime.Now;
                                            re.PostStatus = 9;
                                            re.InvoiceError = ErrorMessage;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();

                                        }
                                    }
                                }
                            }
                            else
                            {

                                string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");
                                string JournalYear = sys.GetFirstXmlValue(XmlOut, "TrnYear");
                                string JournalMonth = sys.GetFirstXmlValue(XmlOut, "TrnMonth");
                                using (var edb = new SysproEntities())
                                {
                                    foreach (var line in det)
                                    {
                                        var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem && a.GrnLine == line.GrnLine select a).ToList();
                                        foreach (var re in result)
                                        {
                                            re.ApJournal = Journal;
                                            re.ApJournalYear = Convert.ToDecimal(JournalYear);
                                            re.ApJournalMonth = Convert.ToDecimal(JournalMonth);
                                            re.InvoicePostDate = DateTime.Now;
                                            re.InvoiceError = "";
                                            re.PostStatus = 8;

                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                            //ErrorEventLog.WriteErrorLog("I", "Invoice Matching completed successfully.");
                                        }
                                    }
                                }

                                //Update Po Receipt Qty
                                if (MultiTax == true)
                                {
                                    sdb.sp_UpdatePurchaseOrderReceiptQty(grnItem);
                                }
                            }
                        }
                        if (ToggleRequired == true)
                        {
                            this.ToggleGrnMatching(detUser.FirstOrDefault().Supplier, "N");
                        }
                    }
                    sys.SysproLogoff(Guid);


                }
            }
            catch (Exception ex)
            {
                throw new Exception("Grn Matching Error: " + ex.Message + " - " + ex.InnerException.Message);
            }
        }

        public string BuildGrnLines(string Po, int Line, decimal Qty, string DelNote, string GlCode, string SuspenseAccount, string AnalysisEntry, decimal Price, string StockDesc, string Warehouse, string Supplier, string Requisition)
        {
            try
            {
                string Notation = Supplier + " " + Qty + " X " + StockDesc;
                StringBuilder Document = new StringBuilder();
                if (Notation.Length > 50)
                {
                    Notation = Notation.Substring(0, 50);
                }
                Document.Append("<Item>");
                Document.Append("<Receipt>");
                Document.Append("<Journal />");
                Document.Append("<PurchaseOrder>" + Po + "</PurchaseOrder>");
                Document.Append("<PurchaseOrderLine>" + Line + "</PurchaseOrderLine>");
                Document.Append("<Warehouse />");
                //Document.Append("<StockCode>" + StockDesc + "</StockCode>");
                Document.Append("<Quantity>" + Qty + "</Quantity>");
                Document.Append("<UnitOfMeasure />");
                Document.Append("<Units />");
                Document.Append("<Pieces />");
                Document.Append("<DeliveryNote><![CDATA[" + DelNote + "]]></DeliveryNote>");
                Document.Append("<Cost />");
                Document.Append("<CostBasis>P</CostBasis>");
                Document.Append("<SwitchOnGRNMatching>N</SwitchOnGRNMatching>");
                //Document.Append("<GRNNumber></GRNNumber>");

                Document.Append("<Reference>" + Requisition + "</Reference>");
                Document.Append("<GRNSource>1</GRNSource>");
                Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
                //Document.Append("<Lot />");
                //Document.Append("<LotExpiryDate />");
                //Document.Append("<Certificate />");
                //Document.Append("<Concession />");
                //Document.Append("<Bins>");
                //Document.Append("<BinLocation>A1</BinLocation>");
                //Document.Append("<BinQuantity>750.000</BinQuantity>");
                //Document.Append("<BinUnits />");
                //Document.Append("<BinPieces />");
                //Document.Append("</Bins>");
                //Document.Append("<Serials>");
                //Document.Append("<SerialNumber>0205</SerialNumber>");
                //Document.Append("<SerialQuantity>1</SerialQuantity>");
                //Document.Append("<SerialUnits />");
                //Document.Append("<SerialPieces />");
                //Document.Append("<SerialExpiryDate />");
                //Document.Append("<SerialLocation />");
                //Document.Append("</Serials>");
                //Document.Append("<SerialRange>");
                //Document.Append("<SerialPrefix>999</SerialPrefix>");
                //Document.Append("<SerialSuffix>1</SerialSuffix>");
                //Document.Append("<StartSerialNumber />");
                //Document.Append("<SerialQuantity>8</SerialQuantity>");
                //Document.Append("<SerialExpiryDate />");
                //Document.Append("<SerialLocation />");
                //Document.Append("</SerialRange>");
                Document.Append("<PurchaseOrderLineComplete>N</PurchaseOrderLineComplete>");
                Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                Document.Append("<ChangeSalesOrderStatus>N</ChangeSalesOrderStatus>");
                Document.Append("<ApplyCostMultiplier>N</ApplyCostMultiplier>");
                Document.Append("<CostMultiplier />");
                Document.Append("<Notation><![CDATA[" + Notation + "]]></Notation>");
                if (Warehouse == "**")
                {
                    var JobList = (from a in sdb.CusGenMaster_.AsNoTracking() where a.GlCode == GlCode && a.PurchasingCategory == "WIP" select a).ToList();
                    if (JobList.Count > 0)
                    {
                        var SettingId = (from a in sdb.mtRequisitionSettings.AsNoTracking() where a.SettingId == 1 select a).FirstOrDefault();
                        Document.Append("<LedgerCode><![CDATA[" + SuspenseAccount + "]]></LedgerCode>");
                        Document.Append("<PasswordForLedgerCode />");
                        Document.Append("<DebitLedgerCode><![CDATA[" + SettingId.NonStockedControlAccount + "]]></DebitLedgerCode>");
                        Document.Append("<PasswordForDebitLedgerCode />");
                    }
                    else
                    {
                        Document.Append("<LedgerCode><![CDATA[" + SuspenseAccount + "]]></LedgerCode>");
                        Document.Append("<PasswordForLedgerCode />");
                        Document.Append("<DebitLedgerCode><![CDATA[" + GlCode + "]]></DebitLedgerCode>");
                        Document.Append("<PasswordForDebitLedgerCode />");

                    }
                }


                //Document.Append("<CountryOfOrigin />");
                //Document.Append("<DeliveryTerms />");
                //Document.Append("<ShippingLocation>");
                //Document.Append("</ShippingLocation>");
                //Document.Append("<NatureOfTransaction />");
                //Document.Append("<ModeOfTransport />");
                //Document.Append("<TradersReference />");
                //Document.Append("<TariffCode />");
                //Document.Append("<UnitMass />");
                //Document.Append("<SupplementaryUnits>N</SupplementaryUnits>");
                //Document.Append("<SupplementaryUnitsFactor>");
                //Document.Append("</SupplementaryUnitsFactor>");
                //Document.Append("<AnalysisEntry />");
                //Document.Append("<AnalysisLineEntry>");
                //Document.Append("<AnalysisCode1>Air</AnalysisCode1>");
                //Document.Append("<AnalysisCode2>Conf</AnalysisCode2>");
                //Document.Append("<AnalysisCode3>East</AnalysisCode3>");
                //Document.Append("<AnalysisCode4 />");
                //Document.Append("<AnalysisCode5 />");
                //Document.Append("<StartDate />");
                //Document.Append("<EndDate />");
                //Document.Append("<EntryAmount>100</EntryAmount>");
                //Document.Append("<Comment>Analysis entry details</Comment>");
                //Document.Append("</AnalysisLineEntry>");
                //Document.Append("<DebitAnalysisEntry />");

                var AnalysisRequired = (from a in sdb.GenMasters.AsNoTracking() where a.GlCode == GlCode select a).ToList();

                if (AnalysisRequired.Count > 0)
                {
                    if (AnalysisRequired.FirstOrDefault().AnalysisRequired == "Y")
                    {
                        decimal EntryAmount = Math.Round(Convert.ToDecimal(Price * Qty), 2);
                        Document.Append("<DebitAnalysisLineEntry>");
                        Document.Append("<AnalysisCode1><![CDATA[" + AnalysisEntry + "]]></AnalysisCode1>");
                        Document.Append("<StartDate />");
                        Document.Append("<EndDate />");
                        Document.Append("<EntryAmount>" + EntryAmount + "</EntryAmount>");
                        Document.Append("<Comment></Comment>");
                        Document.Append("</DebitAnalysisLineEntry>");


                        //Document.Append("<AnalysisLineEntry>");
                        //Document.Append("<AnalysisCode1>" + AnalysisEntry + "</AnalysisCode1>");
                        //Document.Append("<StartDate />");
                        //Document.Append("<EndDate />");
                        //Document.Append("<EntryAmount>" + EntryAmount + "</EntryAmount>");
                        //Document.Append("<Comment></Comment>");
                        //Document.Append("</AnalysisLineEntry>");
                    }
                }

                //Document.Append("<eSignature />");
                Document.Append("</Receipt>");
                Document.Append("</Item>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildGrnParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the Purchase Order Receipts Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostPurchaseOrderReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"portor.xsd\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<NonStockedWhToUse>");
            Parameter.Append("</NonStockedWhToUse>");
            Parameter.Append("<GRNMatchingAction>A</GRNMatchingAction>");
            Parameter.Append("<AllowBlankSupplier>N</AllowBlankSupplier>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ManualSerialTransfersAllowed>N</ManualSerialTransfersAllowed>");
            Parameter.Append("<IgnoreAnalysis>N</IgnoreAnalysis>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostPurchaseOrderReceipts>");

            return Parameter.ToString();

        }


        public string BuildGrnMatchDocument(string Grn, string Supplier, decimal Journal, decimal JournalEntry, string GlCode, string TaxCode, decimal LineTotal, string AnalysisRequired, string AnalysisEntry, string SuspenseAccount, bool MultiTax, decimal VatAmount, string Description, decimal Qty, string Job, int GrnLine)
        {
            try
            {

                var Tax = (from a in sdb.AdmTaxes.AsNoTracking() where a.TaxCode == TaxCode select a).FirstOrDefault();
                var settings = (from a in sdb.mtRequisitionSettings.AsNoTracking() where a.SettingId == 1 select a).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();



                if (MultiTax == true)
                {
                    string LedgerNotation = GrnLine.ToString();//Qty.ToString() + " X " + Description;
                    if (LedgerNotation.Length > 30)
                    {
                        LedgerNotation = LedgerNotation.Substring(0, 30);
                    }

                    Document.Append("<LedgerDistribution>");
                    if (string.IsNullOrEmpty(Job.Trim()))
                    {
                        Document.Append("<LedgerCode><![CDATA[" + GlCode + "]]></LedgerCode>");//glcode from req
                    }
                    else
                    {
                        Document.Append("<LedgerCode>UMHCONADM831999</LedgerCode>");//Projects and contracts
                    }
                    Document.Append("<LedgerTaxCode>E</LedgerTaxCode>");
                    Document.Append("<LedgerNotation><![CDATA[" + LedgerNotation + "]]></LedgerNotation>");
                    Document.Append("<LedgerValue>" + string.Format("{0:.##}", LineTotal) + "</LedgerValue>");
                    Document.Append("<TaxOnlyLine>N</TaxOnlyLine>");

                    if (AnalysisRequired == "Y")
                    {

                        Document.Append("<AnalysisLineEntry>");
                        Document.Append("<AnalysisCode1>" + AnalysisEntry + "</AnalysisCode1>");
                        Document.Append("<EntryAmount>" + string.Format("{0:.##}", LineTotal) + "</EntryAmount>");
                        Document.Append("</AnalysisLineEntry>");
                    }

                    Document.Append("</LedgerDistribution>");


                    if (VatAmount != 0)
                    {
                        Document.Append("<LedgerDistribution>");
                        Document.Append("<LedgerCode><![CDATA[" + Tax.ApTaxGlCode + "]]></LedgerCode>");
                        Document.Append("<LedgerTaxCode>" + TaxCode + "</LedgerTaxCode>");
                        Document.Append("<LedgerNotation><![CDATA[" + LedgerNotation + "]]></LedgerNotation>");
                        Document.Append("<LedgerValue>" + string.Format("{0:.##}", VatAmount) + "</LedgerValue>");
                        Document.Append("<TaxOnlyLine>Y</TaxOnlyLine>");
                        Document.Append("</LedgerDistribution>");
                    }



                }
                else
                {
                    Document.Append("<GrnDetails>");
                    Document.Append("<GrnMatchType>O</GrnMatchType>");
                    //Document.Append("<GrnMatchValue>" + SubTotal + "</GrnMatchValue>");//Total per line excl Tax
                    Document.Append("<GrnSupplier><![CDATA[" + Supplier + "]]></GrnSupplier>");
                    Document.Append("<GrnGrn>" + Grn + "</GrnGrn>");
                    Document.Append("<GrnFlag>I</GrnFlag>");
                    Document.Append("<GrnJournal>" + Journal + "</GrnJournal>");
                    Document.Append("<GrnEntry>" + JournalEntry + "</GrnEntry>");
                    Document.Append("</GrnDetails>");
                }





                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildGrnMatchingParameter(int GrnYear, int GrnMonth)
        {
            try
            {

                string PostingPeriod = "C";
                var PostPeriod = sdb.sp_GetAPPostingPeriod(GrnYear, GrnMonth).ToList();
                if (PostPeriod.Count > 0)
                {
                    PostingPeriod = PostPeriod.FirstOrDefault().PostingPeriod;
                }


                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Post AP Invoice Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostApInvoice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSTIN.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<PostingPeriod>" + PostingPeriod + "</PostingPeriod>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<PostZeroAdjustments>N</PostZeroAdjustments>");
                Parameter.Append("<DefaultDiscountDate />");
                Parameter.Append("<DefaultDueDate />");
                Parameter.Append("<LedgerDistributionCurrency>L</LedgerDistributionCurrency>");
                Parameter.Append("<AutomaticTaxCalculation>N</AutomaticTaxCalculation>");
                //if(MultiTax == "Y")
                //{
                Parameter.Append("<PermissibleTaxVariance>1.00</PermissibleTaxVariance>");
                //}

                Parameter.Append("<ApArContraTrx>N</ApArContraTrx>");
                Parameter.Append("<IgnoreAnalysis>N</IgnoreAnalysis>");
                Parameter.Append("<ApTaxReliefAsOfDate>");
                Parameter.Append("</ApTaxReliefAsOfDate>");
                Parameter.Append("<ApTaxReliefNoOfDays>");
                Parameter.Append("</ApTaxReliefNoOfDays>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostApInvoice>");

                return Parameter.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string GetDefaultTaxCode(string Guid)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the AP Setup Query Business Object");
                Document.Append("-->");
                Document.Append("<Query xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"IMPQCR.XSD\">");
                Document.Append("<Option>");
                Document.Append("<XslStyleSheet />");
                Document.Append("</Option>");
                Document.Append("</Query>");

                string XmlOut = sys.SysproQuery(Guid, Document.ToString(), "IMPQCR");

                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    throw new Exception(ErrorMessage);
                }
                else
                {
                    string TaxCode = sys.GetXmlValue(XmlOut, "DefaultTaxCode");
                    TaxCode = TaxCode.Replace(";", "");
                    return TaxCode.Trim();
                }



            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostMaterialIssue(string Guid, string Grn)
        {
            try
            {
                var MatIssue = (from a in sdb.mtGrnDetails.AsNoTracking() where a.Grn == Grn select a).ToList();
                if (MatIssue.Count > 0)
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
                    bool hasLines = false;
                    foreach (var item in MatIssue)
                    {
                        if (!string.IsNullOrEmpty(item.Job.Trim()))
                        {
                            //ErrorEventLog.WriteErrorLog("E", item.Job);
                            hasLines = true;
                            var WipMaster = (from a in sdb.WipMasters.AsNoTracking() where a.Job == item.Job select a).FirstOrDefault();
                            string GrnLine = item.Grn + "|" + item.GrnLine.ToString().Trim();
                            string WipMat = (from a in sdb.WipJobAllMats.AsNoTracking() where a.Job == item.Job && a.StockCode == item.StockCode && a.Warehouse == item.Warehouse && a.ItemNumber == GrnLine select a.Line).FirstOrDefault().ToString();



                            //ErrorEventLog.WriteErrorLog("E", WipMat.ToString());

                            Document.Append("<Item>");
                            Document.Append("<Journal />");
                            Document.Append("<Job>" + item.Job + "</Job>");
                            if (item.Warehouse == "**")
                            {
                                Document.Append("<NonStockedFlag>Y</NonStockedFlag>");
                            }
                            else
                            {
                                Document.Append("<NonStockedFlag>N</NonStockedFlag>");
                            }

                            Document.Append("<Warehouse>" + item.Warehouse + "</Warehouse>");
                            Document.Append("<StockCode><![CDATA[" + item.StockCode.Trim() + "]]></StockCode>");
                            Document.Append("<Line>" + WipMat.ToString().PadLeft(2, '0') + "</Line>");
                            Document.Append("<QtyIssued>" + string.Format("{0:##,###,##0.000}", item.QtyReceived) + "</QtyIssued>");
                            if (string.IsNullOrEmpty(item.SysproGrn))
                            {
                                Document.Append("<Reference></Reference>");
                            }
                            else
                            {
                                Document.Append("<Reference>" + item.SysproGrn.PadLeft(15, '0') + "</Reference>");
                            }
                            Document.Append("<Notation>" + item.Requisition + "</Notation>");
                            Document.Append("<LedgerCode><![CDATA[" + WipMaster.WipCtlGlCode + "]]></LedgerCode>");
                            //Document.Append("<PasswordForLedgerCode />");
                            Document.Append("<ProductClass>_OTH</ProductClass>");
                            Document.Append("<UnitCost>" + string.Format("{0:##,###,##0.000}", item.Price) + "</UnitCost>");
                            Document.Append("<AllocCompleted>Y</AllocCompleted>");
                            Document.Append("<MaterialReference><![CDATA[" + item.Supplier + " - " + item.Requisition + "]]></MaterialReference>");
                            Document.Append("</Item>");
                        }

                    }

                    if (hasLines == false)
                    {
                        return "";
                    }

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
                    //ErrorEventLog.WriteErrorLog("E", Document.ToString());
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");
                        string JournalYear = sys.GetFirstXmlValue(XmlOut, "JnlYear");
                        string JournalMonth = sys.GetFirstXmlValue(XmlOut, "JnlMonth");
                        using (var edb = new SysproEntities())
                        {
                            var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == Grn select a).ToList();
                            foreach (var re in result)
                            {
                                re.IssueJournal = Journal;
                                re.IssueJournalYear = Convert.ToDecimal(JournalYear);
                                re.IssueJournalMonth = Convert.ToDecimal(JournalMonth);
                                edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                edb.SaveChanges();
                            }

                        }
                    }

                    return ErrorMessage;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string AddMaterialAllocation(string Guid, string Grn)
        {
            try
            {
                var MatALloc = sdb.sp_GetPoLinesForMaterialAllocation(Grn).ToList();
                if (MatALloc.Count > 0)
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

                    foreach (var item in MatALloc)
                    {
                        Document.Append("<Item>");
                        Document.Append("<Job>" + item.Job + "</Job>");
                        if (item.MWarehouse == "**")
                        {
                            Document.Append("<NonStocked>Y</NonStocked>");
                        }
                        else
                        {
                            Document.Append("<NonStocked>N</NonStocked>");
                        }
                        Document.Append("<StockCode><![CDATA[" + item.MStockCode + "]]></StockCode>");
                        Document.Append("<Warehouse>" + item.MWarehouse + "</Warehouse>");
                        Document.Append("<NewWarehouse />");
                        Document.Append("<Line />");
                        Document.Append("<ExplodeIfPhantomPart>N</ExplodeIfPhantomPart>");
                        Document.Append("<ExplodeIfKitPart>N</ExplodeIfKitPart>");
                        Document.Append("<ComponentWhToUse>N</ComponentWhToUse>");
                        Document.Append("<StockDescription><![CDATA[" + item.MStockDes + "]]></StockDescription>");
                        Document.Append("<QtyReqdType>U</QtyReqdType>");
                        Document.Append("<QtyReqd>" + item.MOrderQty + "</QtyReqd>");
                        Document.Append("<FixedQtyPerFlag>N</FixedQtyPerFlag>");
                        Document.Append("<FixedQtyPer />");
                        Document.Append("<UnitCost>" + item.MPrice + "</UnitCost>");
                        Document.Append("<OperationOffset>0001</OperationOffset>");
                        Document.Append("<OpOffsetFlag>O</OpOffsetFlag>");
                        Document.Append("<Uom>" + item.MStockingUom + "</Uom>");
                        Document.Append("<SequenceNum />");
                        Document.Append("<HierarchyJob>");
                        Document.Append("<Head><![CDATA[" + item.HierHead + "]]></Head>");
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
                        Document.Append("<ItemNumber>" + item.Grn + "|" + item.Line.ToString().Trim() + "</ItemNumber>");
                        //Document.Append("<OverEccSpecIssue />");
                        Document.Append("</Item>");
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

                    string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "WIPTJM");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        ErrorEventLog.WriteErrorLog("E", Document.ToString());
                    }
                    return ErrorMessage;
                }
                return "";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void PostGrnAdjustment(string Guid)
        {
            try
            {
                var Grn = (from a in sdb.mtGrnDetails.AsNoTracking() where a.PostStatus == 8 select a.Grn).Distinct().ToList();
                if (Grn.Count > 0)
                {

                    foreach (var grnItem in Grn)
                    {
                        var detUser = (from a in sdb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                        var TaxCodeCount = (from a in detUser select a.TaxCode).Distinct().ToList();
                        if (TaxCodeCount.Count > 1)
                        {
                            string Username = "";
                            if (!string.IsNullOrEmpty(detUser.FirstOrDefault().Level2AuthorizedBy))
                            {
                                Username = detUser.FirstOrDefault().Level2AuthorizedBy;
                            }
                            else
                            {
                                Username = detUser.FirstOrDefault().Level1AuthorizedBy;
                            }



                            var det = sdb.sp_GetInvoiceDetailsByGrn(grnItem).ToList();
                            if (det.Count > 0)
                            {
                                var settings = (from a in sdb.mtRequisitionSettings.AsNoTracking() where a.SettingId == 1 select a).FirstOrDefault();
                                //Declaration
                                StringBuilder Document = new StringBuilder();

                                //Building Document content
                                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                                Document.Append("<!-- Copyright 1994-2009 SYSPRO Ltd.-->");
                                Document.Append("<!--");
                                Document.Append("This is an example XML instance to demonstrate");
                                Document.Append("use of the P/O Post GRN Adjustments Business Object");
                                Document.Append("-->");
                                Document.Append("<PostGrnAdjustments xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTGNDOC.XSD\">");

                                foreach (var line in det)
                                {
                                    Document.Append("<Item>");
                                    Document.Append("<GrnDetails>");
                                    Document.Append("<Supplier>" + line.Supplier + "</Supplier>");
                                    Document.Append("<GrnNumber>" + line.SysproGrn + "</GrnNumber>");
                                    Document.Append("<GrnSource>I</GrnSource>");
                                    Document.Append("<GrnJournal>" + line.Journal + "</GrnJournal>");
                                    Document.Append("<GrnJournalEntry>" + line.JournalEntry + "</GrnJournalEntry>");
                                    Document.Append("<Unmatch>N</Unmatch>");
                                    Document.Append("<AdjustmentValue>" + string.Format("{0:.##}", line.LineTotal * -1) + "</AdjustmentValue>");
                                    Document.Append("<Reference />");
                                    //Document.Append("<DeliveryNote></DeliveryNote>");
                                    //Document.Append("<JournalNotation>A100</JournalNotation>");
                                    Document.Append("<LedgerCode>" + settings.GrnClearingAccount + "</LedgerCode>");
                                    Document.Append("<eSignature />");
                                    Document.Append("</GrnDetails>");
                                    Document.Append("</Item>");
                                }
                                Document.Append("</PostGrnAdjustments>");

                                string XmlOut = "";
                                //Syspro Post
                                try
                                {
                                    XmlOut = sys.SysproPost(Guid, this.BuildGrnAdjustmentParameter(), Document.ToString(), "PORTGN");
                                }
                                catch (Exception ex)
                                {
                                    using (var edb = new SysproEntities())
                                    {
                                        var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                        foreach (var re in result)
                                        {
                                            re.InvoicePostDate = DateTime.Now;
                                            re.PostStatus = 11;
                                            re.GrnAdjustmentError = ex.Message;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                            ErrorEventLog.WriteErrorLog("E", ex.Message);
                                        }
                                    }
                                    return;
                                }

                                //ErrorEventLog.WriteErrorLog("E", XmlOut.ToString());
                                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                                if (!string.IsNullOrEmpty(ErrorMessage))
                                {
                                    //Error
                                    using (var edb = new SysproEntities())
                                    {
                                        var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                        foreach (var re in result)
                                        {
                                            re.InvoicePostDate = DateTime.Now;
                                            re.PostStatus = 11;
                                            re.GrnAdjustmentError = ErrorMessage;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                            ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                                        }
                                    }
                                }
                                else
                                {

                                    using (var edb = new SysproEntities())
                                    {
                                        var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                        foreach (var re in result)
                                        {
                                            re.GrnAdjustmentError = "";
                                            re.PostStatus = 10;
                                            edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                            edb.SaveChanges();
                                            //ErrorEventLog.WriteErrorLog("I", "Invoice Matching completed successfully.");
                                        }
                                    }
                                }

                            }
                            else
                            {
                                using (var edb = new SysproEntities())
                                {
                                    var result = (from a in edb.mtGrnDetails.AsNoTracking() where a.Grn == grnItem select a).ToList();
                                    foreach (var re in result)
                                    {
                                        re.GrnAdjustmentError = "";
                                        re.PostStatus = 10;
                                        edb.Entry(re).State = System.Data.Entity.EntityState.Modified;
                                        edb.SaveChanges();
                                        //ErrorEventLog.WriteErrorLog("I", "Invoice Matching completed successfully.");
                                    }
                                }
                            }

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Grn Adjustment Error: " + ex.Message);
            }
        }

        public string BuildGrnAdjustmentParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the Purchase Order Receipts Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostGrnAdjustments xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTGN.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<ActionType>C</ActionType>");
            Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
            Parameter.Append("<PostingPeriod>1</PostingPeriod>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostGrnAdjustments>");

            return Parameter.ToString();

        }


        public void CreatePurchaseOrder()
        {
            try
            {
                var result = (from a in sdb.mtRequisitionHeaders.AsNoTracking() where a.Status == 99 select a.Requisition).Distinct().ToList();
                if (result.Count > 0)
                {
                    foreach (var req in result)
                    {
                        this.PostPurchaseOrder(req);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void PostPurchaseOrder(string Requisition)
        {
            try
            {
                var SupplierList = sdb.sp_GetSuppliersForPo(Requisition).ToList();
                if (SupplierList.Count > 0)
                {
                    string Guid = sys.SysproLogin("");
                    string SupplierError = "";
                    foreach (var Supplier in SupplierList)
                    {

                        SupplierError += this.CreateSupplier(Guid, Supplier.Supplier);

                    }
                    if (!string.IsNullOrEmpty(SupplierError))
                    {
                        ErrorEventLog.WriteErrorLog("E", "Failed to create Supplier(s). " + SupplierError);
                    }

                    string PurchaseOrders = "";
                    string ReturnError = "";
                    var Header = (from a in sdb.mtRequisitionHeaders.AsNoTracking() where a.Requisition == Requisition select a).FirstOrDefault();
                    foreach (var Supplier in SupplierList)
                    {



                        var Detail = (from a in sdb.mtRequisitionDetails.AsNoTracking() where a.Requisition == Requisition && a.Supplier == Supplier.Supplier select a).ToList();

                        StringBuilder Document = new StringBuilder();
                        Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("This is an example XML instance to demonstrate");
                        Document.Append("use of the Purchase Order Transaction Posting Business Object");
                        Document.Append("-->");
                        Document.Append("<PostPurchaseOrders xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTOIDOC.XSD\">");
                        Document.Append("<Orders>");
                        Document.Append(this.BuildPoHeader(Header, Supplier.Supplier, "A"));
                        Document.Append("<OrderDetails>");
                        foreach (var line in Detail)
                        {
                            Document.Append(this.BuildPoDetail(line.StockCode, line.Description, line.Uom, (decimal)line.Quantity, (decimal)line.SupplierValue, line.Warehouse, line.ProductClass, line.TaxCode, Supplier.Supplier, line.Job, line.HierarchyCategory, line.GlCode, line.Requisition, line.Line, "A"));
                        }
                        Document.Append("</OrderDetails>");
                        Document.Append("</Orders>");
                        Document.Append("</PostPurchaseOrders>");


                        if (Guid != "")
                        {
                            string XmlOut = sys.SysproPost(Guid, this.BuildPoParameter("N"), Document.ToString(), "PORTOI");
                            string ErrorMessage = sys.GetXmlErrors(XmlOut);
                            if (!string.IsNullOrEmpty(ErrorMessage))
                            {

                                ReturnError += ErrorMessage;
                            }
                            else
                            {
                                string PO = sys.GetXmlValue(XmlOut, "PurchaseOrder");
                                using (var db = new SysproEntities())
                                {
                                    var req = (from a in db.mtRequisitionDetails.AsNoTracking() where a.Requisition == Requisition && a.Supplier == Supplier.Supplier select a).ToList();
                                    foreach (var line in req)
                                    {
                                        line.PurchaseOrder = PO.Remove(PO.Length - 1, 1);
                                        db.Entry(line).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }


                                }
                                sdb.sp_UpdatePoLineInReq(PO.Remove(PO.Length - 1, 1));
                                PurchaseOrders += PO;
                                sdb.sp_SavePoAnalysisCategory(PO.Remove(PO.Length - 1, 1));

                            }
                        }
                        else
                        {
                            ErrorEventLog.WriteErrorLog("E", "Error : Failed to log in to Syspro");
                        }



                    }

                    sys.SysproLogoff(Guid);
                    if (ReturnError != "")
                    {
                        if (PurchaseOrders != "")
                        {
                            this.UpdatePoStatus(Requisition);

                            ErrorEventLog.WriteErrorLog("I", ReturnError + ". The following Purchase Order(s) were created : " + PurchaseOrders);
                        }
                        else
                            this.UpdatePoStatus(Requisition);
                        ErrorEventLog.WriteErrorLog("E", ReturnError);
                    }
                    else
                    {
                        this.UpdatePoStatus(Requisition);
                        ErrorEventLog.WriteErrorLog("I", "The following Purchase Order(s) were created : " + PurchaseOrders);
                    }


                }
                else
                {
                    ErrorEventLog.WriteErrorLog("E", "Error : No unposted Supplier(s) found for requisition : " + Requisition);
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdatePoStatus(string Requisition)
        {
            try
            {
                int PoStatus;
                var PoComplete = (from a in sdb.mtRequisitionDetails.AsNoTracking() where a.Requisition == Requisition && (a.PurchaseOrder == "" || a.PurchaseOrder == null) select a).ToList();
                if (PoComplete.Count > 0)
                {
                    PoStatus = 8;
                }
                else
                {
                    PoStatus = 7;
                }

                using (var db = new SysproEntities())
                {
                    var Header = (from a in db.mtRequisitionHeaders.AsNoTracking() where a.Requisition == Requisition select a).FirstOrDefault();
                    Header.Status = PoStatus;
                    db.Entry(Header).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string CreateSupplier(string Guid, string Supplier)
        {
            try
            {
                var Sup = (from a in sdb.ApSuppliers.AsNoTracking() where a.Supplier == Supplier select a).ToList();
                if (Sup.Count > 0)
                {
                    return "";
                }

                var mtSup = (from a in sdb.mtApSuppliers.AsNoTracking() where a.Supplier == Supplier select a).FirstOrDefault();
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Supplier Maintenance Business Object");
                Document.Append("-->");
                Document.Append("<SetupApSupplier xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSSSPDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<Supplier><![CDATA[" + Supplier + "]]></Supplier>");
                Document.Append("</Key>");
                Document.Append("<SupplierName><![CDATA[" + mtSup.SupplierName + "]]></SupplierName>");
                string SupplierName = "";
                if (mtSup.SupplierName.Length > 20)
                {
                    SupplierName = SupplierName.Substring(0, 20);
                }
                else
                {
                    SupplierName = mtSup.SupplierName;
                }
                Document.Append("<SupShortName><![CDATA[" + SupplierName + "]]></SupShortName>");
                Document.Append("<SupplierChName></SupplierChName>");
                Document.Append("<LanguageCode />");
                Document.Append("<Telephone></Telephone>");
                Document.Append("<Fax></Fax>");
                Document.Append("<Contact></Contact>");
                Document.Append("<Branch>" + mtSup.Branch + "</Branch>");
                Document.Append("<TermsCode>" + mtSup.TermsCode + "</TermsCode>");
                Document.Append("<Bank>A</Bank>");
                Document.Append("<Currency>R</Currency>");
                //Document.Append("<DateSuppAdded>2006-03-01</DateSuppAdded>");
                Document.Append("<WithTaxId />");
                Document.Append("<WithTaxCode />");
                Document.Append("<SupAddr1><![CDATA[" + mtSup.Address1 + "]]></SupAddr1>");
                Document.Append("<SupAddr2><![CDATA[" + mtSup.Address2 + "]]></SupAddr2>");
                Document.Append("<SupAddr3><![CDATA[" + mtSup.Address3 + "]]></SupAddr3>");
                Document.Append("<SupAddr3Loc />");
                Document.Append("<SupAddr4><![CDATA[" + mtSup.Address4 + "]]></SupAddr4>");
                Document.Append("<SupAddr5><![CDATA[" + mtSup.Address5 + "]]></SupAddr5>");
                //Document.Append("<SupPostalCode>L4B 1B4</SupPostalCode>");
                Document.Append("<SupAddGpsLat />");
                Document.Append("<SupAddGpsLong />");
                Document.Append("<LanguageCodeSup />");
                //Document.Append("<RemitName>Bicycles Unlimited</RemitName>");
                //Document.Append("<RemitAddr1>P O Box 3469</RemitAddr1>");
                //Document.Append("<RemitAddr2>Northwest</RemitAddr2>");
                //Document.Append("<RemitAddr3>Richmond</RemitAddr3>");
                //Document.Append("<RemitAddr3Loc />");
                //Document.Append("<RemitAddr4>Ontario</RemitAddr4>");
                //Document.Append("<RemitAddr5>Canada</RemitAddr5>");
                //Document.Append("<RemitPostalCode>L4B 1B4</RemitPostalCode>");
                //Document.Append("<RemitGpsLat />");
                //Document.Append("<RemitGpsLong />");
                //Document.Append("<LanguageCodeRem />");
                Document.Append("<SupplierClass>SU</SupplierClass>");
                Document.Append("<LineDiscCode />");
                Document.Append("<InvDiscCode />");
                Document.Append("<Area>N</Area>");
                Document.Append("<UserField1>Local</UserField1>");
                Document.Append("<UserField2 />");
                Document.Append("<DefTaxCode />");
                Document.Append("<TaxRegnNum><![CDATA[" + mtSup.TaxRegnNum + "]]></TaxRegnNum>");
                Document.Append("<DefaultChequeDay>00</DefaultChequeDay>");
                Document.Append("<PoDefaultDoc>1</PoDefaultDoc>");
                Document.Append("<OnHold>N</OnHold>");
                Document.Append("<FaxRemitFlag />");
                Document.Append("<FaxRemitNum />");
                Document.Append("<FaxRemitContact />");
                Document.Append("<RemitEmail>");
                Document.Append("</RemitEmail>");
                Document.Append("<SupplierType>T</SupplierType>");
                //Document.Append("<NumMthsBeforDel>99</NumMthsBeforDel>");
                //Document.Append("<MerchGlCode />");
                //Document.Append("<FreightGlCode>10-5500</FreightGlCode>");
                Document.Append("<PayByEft>Y</PayByEft>");
                Document.Append("<BankBranch>1</BankBranch>");
                Document.Append("<BankAccount>1</BankAccount>");
                Document.Append("<EftUserField1 />");
                Document.Append("<EftUserField2 />");
                Document.Append("<EftStatementRef />");
                Document.Append("<EftBankAccType>1</EftBankAccType>");
                //Document.Append("<ChLanguage>05</ChLanguage>");
                Document.Append("<Required1099 />");
                Document.Append("<DefaultFmt1099 />");
                Document.Append("<AskYtdValue1099 />");
                Document.Append("<MinPay1099 />");
                Document.Append("<PayeRef1099 />");
                Document.Append("<YtdAmount1099 />");
                Document.Append("<PurchOrdAllowed>Y</PurchOrdAllowed>");
                Document.Append("<MinPoVal>0</MinPoVal>");
                Document.Append("<MinPoMass>0</MinPoMass>");
                Document.Append("<MinPoVolume>0</MinPoVolume>");
                Document.Append("<Contracts>N</Contracts>");
                Document.Append("<GrnMatchReqd>Y</GrnMatchReqd>");
                Document.Append("<LctRequired>N</LctRequired>");
                Document.Append("<EdiTypeFlag>N</EdiTypeFlag>");
                Document.Append("<EdiReceiverCode></EdiReceiverCode>");
                Document.Append("<EdiFax />");
                Document.Append("<FaxContact />");
                Document.Append("<Email></Email>");
                Document.Append("<Nationality />");
                Document.Append("<TransactionNature>0</TransactionNature>");
                Document.Append("<DeliveryTerms />");
                Document.Append("<ShippingLocation />");
                Document.Append("<AutoVoucherReqd>N</AutoVoucherReqd>");
                Document.Append("<FaxVoucherFlag>P</FaxVoucherFlag>");
                Document.Append("<FaxVoucherNum />");
                Document.Append("<VoucherEmail />");
                Document.Append("<CollectTax>V</CollectTax>");
                Document.Append("<State></State>");
                Document.Append("<CountyZip></CountyZip>");
                Document.Append("<City></City>");
                Document.Append("<WithTaxExpenseType>G</WithTaxExpenseType>");
                Document.Append("<WithTaxSupplierInd>F</WithTaxSupplierInd>");
                //Document.Append("<eSignature>{36303032-3330-3031-3038-323434363433}</eSignature>");
                Document.Append("</Item>");
                Document.Append("</SetupApSupplier>");


                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Supplier Maintenance Business Object");
                Parameter.Append("-->");
                Parameter.Append("<SetupApSupplier xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSSSP.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</SetupApSupplier>");


                string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "APSSSP");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                return ErrorMessage;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildPoParameter(string IgnoreWarnings)
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
                Parameter.Append("<IgnoreWarnings>" + IgnoreWarnings + "</IgnoreWarnings>");
                Parameter.Append("<AllowNonStockItems>Y</AllowNonStockItems>");
                Parameter.Append("<AllowZeroPrice>N</AllowZeroPrice>");
                Parameter.Append("<ValidateWorkingDays>N</ValidateWorkingDays>");
                Parameter.Append("<AllowPoWhenBlanketPo>N</AllowPoWhenBlanketPo>");
                Parameter.Append("<DefaultMemoCode>S</DefaultMemoCode>");
                Parameter.Append("<FixedExchangeRate>N</FixedExchangeRate>");
                Parameter.Append("<DefaultMemoDays>12</DefaultMemoDays>");
                Parameter.Append("<AllowBlankLedgerCode>Y</AllowBlankLedgerCode>");
                Parameter.Append("<DefaultDeliveryAddress />");
                Parameter.Append("<CalcDueDate>N</CalcDueDate>");
                Parameter.Append("<InsertDangerousGoodsText>N</InsertDangerousGoodsText>");
                Parameter.Append("<InsertAdditionalPOText>N</InsertAdditionalPOText>");
                Parameter.Append("<OutputItemforDetailLines>N</OutputItemforDetailLines>");
                Parameter.Append("<Status>1</Status>");
                Parameter.Append("<StatusInProcess />");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostPurchaseOrders>");

                return Parameter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildPoHeader(mtRequisitionHeader Header, string Supplier, string ActionType, string PurchaseOrder = "")
        {
            try
            {

                var ApSupplier = (from a in sdb.ApSuppliers.AsNoTracking() where a.Supplier == Supplier select a).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();

                Document.Append("<OrderHeader>");
                Document.Append("<OrderActionType>" + ActionType.ToString() + "</OrderActionType>");
                Document.Append("<Supplier><![CDATA[" + Supplier + "]]></Supplier>");
                if (ActionType == "C")
                {
                    Document.Append("<PurchaseOrder>" + PurchaseOrder.ToString() + "</PurchaseOrder>");
                }
                Document.Append("<ExchRateFixed />");
                Document.Append("<ExchangeRate />");
                Document.Append("<OrderType>L</OrderType>");
                Document.Append("<Customer></Customer>");
                if (string.IsNullOrWhiteSpace(ApSupplier.TaxRegnNum))
                {
                    Document.Append("<TaxStatus>E</TaxStatus>");
                }
                else
                {
                    Document.Append("<TaxStatus>N</TaxStatus>");
                }

                Document.Append("<PaymentTerms />");
                Document.Append("<InvoiceTerms></InvoiceTerms>");
                Document.Append("<CustomerPoNumber>" + Header.Requisition + "</CustomerPoNumber>");
                Document.Append("<ShippingInstrs></ShippingInstrs>");
                Document.Append("<OrderDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</OrderDate>");
                Document.Append("<DueDate>" + Convert.ToDateTime(Header.DueDate).ToString("yyyy-MM-dd") + "</DueDate>");
                Document.Append("<MemoDate></MemoDate>");
                Document.Append("<ApplyDueDateToLines>A</ApplyDueDateToLines>");
                Document.Append("<MemoCode />");
                Document.Append("<Buyer />");
                Document.Append("<DeliveryName></DeliveryName>");
                Document.Append("<DeliveryAddr1><![CDATA[" + Header.ToAddress1 + "]]></DeliveryAddr1>");
                Document.Append("<DeliveryAddr2><![CDATA[" + Header.ToAddress2 + "]]></DeliveryAddr2>");
                Document.Append("<DeliveryAddr3><![CDATA[" + Header.ToAddress3 + "]]></DeliveryAddr3>");
                Document.Append("<DeliveryAddrLoc></DeliveryAddrLoc>");
                Document.Append("<DeliveryAddr4><![CDATA[" + Header.ToAddress4 + "]]></DeliveryAddr4>");
                Document.Append("<DeliveryAddr5><![CDATA[" + Header.ToAddress5 + "]]></DeliveryAddr5>");
                //Document.Append("<DeliveryGpsLat>00.000000</DeliveryGpsLat>");
                //Document.Append("<DeliveryGpsLon>000.000000</DeliveryGpsLon>");
                Document.Append("<PostalCode></PostalCode>");
                Document.Append("<DeliveryTerms>");
                Document.Append("</DeliveryTerms>");
                Document.Append("<ShippingLocation>");
                Document.Append("</ShippingLocation>");
                Document.Append("<AutoVoucher>");
                Document.Append("</AutoVoucher>");
                Document.Append("<LanguageCode>");
                Document.Append("</LanguageCode>");
                Document.Append("<Warehouse />");
                Document.Append("<DiscountLessPlus />");
                //Document.Append("<DiscPercent1>2.50</DiscPercent1>");
                //Document.Append("<DiscPercent2>1.50</DiscPercent2>");
                //Document.Append("<DiscPercent3>1.00</DiscPercent3>");
                //Document.Append("<PurchaseOrder>221124</PurchaseOrder>");
                //Document.Append("<ChgPOStatToReadyToPrint />");
                //Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
                //Document.Append("<eSignature />");
                Document.Append("</OrderHeader>");


                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildPoDetail(string StockCode, string Description, string Uom, decimal Quantity, decimal Price, string Warehouse, string ProductClass, string TaxCode, string Supplier, string Job, string Category, string GlCode, string Requisition, int Line, string ActionType)
        {
            try
            {

                var ApSupplier = (from a in sdb.ApSuppliers.AsNoTracking() where a.Supplier == Supplier select a).FirstOrDefault();

                StringBuilder Document = new StringBuilder();

                Document.Append("<StockLine>");
                if (ActionType == "C")
                {
                    Document.Append("<PurchaseOrderLine>" + Line.ToString() + "</PurchaseOrderLine>");
                }
                //
                Document.Append("<LineActionType>" + ActionType.ToString() + "</LineActionType>");
                Document.Append("<StockCode><![CDATA[" + StockCode + "]]></StockCode>");

                if (Description.Length > 50)
                {
                    Description = Description.Substring(0, 50);
                }


                //ErrorEventLog.WriteErrorLog("E","Warehouse : " + Warehouse);
                Document.Append("<StockDescription><![CDATA[" + Description + "]]></StockDescription>");
                Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");//??
                Document.Append("<SupCatalogue>" + Line.ToString() + "</SupCatalogue>");
                Document.Append("<OrderQty>" + Quantity + "</OrderQty>");

                if (Warehouse == "**")
                {
                    Document.Append("<OrderUom>" + Uom + "</OrderUom>");
                }

                Document.Append("<Units />");
                Document.Append("<Pieces />");
                Document.Append("<PriceMethod>M</PriceMethod>");
                Document.Append("<SupplierContract />");
                Document.Append("<SupplierContractReference />");
                Document.Append("<Price>" + string.Format("{0:##,###,##0.00000}", Price) + "</Price>");

                if (Warehouse == "**")
                {
                    Document.Append("<PriceUom>" + Uom + "</PriceUom>");
                }


                //Document.Append("<LineDiscType>P</LineDiscType>");
                //Document.Append("<LineDiscLessPlus>L</LineDiscLessPlus>");
                //Document.Append("<LineDiscPercent1>0.5</LineDiscPercent1>");
                //Document.Append("<LineDiscPercent2>0</LineDiscPercent2>");
                //Document.Append("<LineDiscPercent3>0</LineDiscPercent3>");
                //Document.Append("<LineDiscValue>0</LineDiscValue>");
                if (ApSupplier.TaxRegnNum != "")
                {
                    Document.Append("<Taxable>Y</Taxable>");
                    Document.Append("<TaxCode>" + TaxCode + "</TaxCode>");
                }

                if (!string.IsNullOrEmpty(Job))
                {
                    Document.Append("<Job>" + Job + "</Job>");
                    if (!string.IsNullOrEmpty(Category))
                    {
                        Document.Append("<HierHead><![CDATA[" + Category + "]]></HierHead>");
                    }
                }
                Document.Append("<Section1 />");
                Document.Append("<Section2 />");
                Document.Append("<Section3 />");
                Document.Append("<Section4 />");
                Document.Append("<Version />");
                Document.Append("<Release></Release>");
                Document.Append("<LatestDueDate />");
                Document.Append("<OriginalDueDate />");
                Document.Append("<RescheduleDueDate />");
                Document.Append("<LedgerCode><![CDATA[" + GlCode + "]]></LedgerCode>");
                Document.Append("<PasswordForLedgerCode />");
                Document.Append("<SubcontractOp />");
                Document.Append("<InspectionReqd />");
                Document.Append("<ProductClass>" + ProductClass + "</ProductClass>");
                Document.Append("<NonsUnitMass />");
                Document.Append("<NonsUnitVol />");
                Document.Append("<BlanketPurchaseOrder></BlanketPurchaseOrder>");
                Document.Append("<AttachOrderToBPO />");
                Document.Append("<WithholdingTaxExpenseType>G</WithholdingTaxExpenseType>");

                Document.Append("<NonStockedLine />");
                //Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
                Document.Append("</StockLine>");

                var NTextLines = (from a in sdb.mtRequisitionTextLines.AsNoTracking() where a.Requisition == Requisition && a.RequisitionLine == Line select a).ToList();
                foreach (var Ntext in NTextLines)
                {
                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment><![CDATA[" + Ntext.NText + "]]></Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines />");
                    Document.Append("<ChangeSingleCommentLine />");
                    Document.Append("</CommentLine>");
                }

                var LastLine = (from a in sdb.mtRequisitionDetails.AsNoTracking() where a.Requisition == Requisition && a.Supplier == Supplier select a.Line).Max();
                if (Line == LastLine)
                {
                    NTextLines = (from a in sdb.mtRequisitionTextLines.AsNoTracking() where a.Requisition == Requisition && a.RequisitionLine == 0 select a).ToList();
                    foreach (var Ntext in NTextLines)
                    {
                        Document.Append("<CommentLine>");
                        Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<Comment><![CDATA[" + Ntext.NText + "]]></Comment>");
                        Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                        Document.Append("<DeleteAttachedCommentLines />");
                        Document.Append("<ChangeSingleCommentLine />");
                        Document.Append("</CommentLine>");
                    }
                }

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CheckGrnMatchingToggleRequired(string Supplier)
        {
            try
            {
                using (var edb = new SysproEntities())
                {
                    var result = (from a in edb.ApSuppliers.AsNoTracking() where a.Supplier == Supplier select a.GrnMatchReqd).ToList();
                    if (result.Count > 0)
                    {
                        if (result.FirstOrDefault() == "Y")
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("Supplier : " + Supplier + " not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ToggleGrnMatching(string Supplier, string Toggle)
        {
            try
            {
                using (var edb = new SysproEntities())
                {
                    var result = (from a in edb.ApSuppliers.AsNoTracking() where a.Supplier == Supplier select a).FirstOrDefault();
                    result.GrnMatchReqd = Toggle;
                    edb.Entry(result).State = EntityState.Modified;
                    edb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public void PostBulkCancel()
        {
            try
            {
                var result = (from a in sdb.mtTmpPoToCancels where a.Status == 1 select a).ToList();
                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        this.CancelPurchaseOrder(item.PurchaseOrder, item.Requisition);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public void CancelPurchaseOrder(string PurchaseOrder, string Requisition)
        {
            try
            {





                var result = sdb.sp_GetPurchaseOrderToCancel(PurchaseOrder).ToList();





                var sum = (from a in result select a.QtyReceived).Sum();

                if (sum > 0)
                {
                    string Guid = sys.SysproLogin("");
                    //Update Order Qty to Grn Qty
                    //Declaration
                    StringBuilder Document = new StringBuilder();

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
                    Document.Append("<OrderActionType>C</OrderActionType>");
                    Document.Append("<PurchaseOrder>" + PurchaseOrder + "</PurchaseOrder>");
                    Document.Append("</OrderHeader>");
                    Document.Append("<OrderDetails>");
                    foreach (var line in result)
                    {

                        if (line.QtyReceived > 0)
                        {
                            Document.Append("<StockLine>");
                            Document.Append("<PurchaseOrderLine>" + line.Line + "</PurchaseOrderLine>");
                            Document.Append("<LineActionType>C</LineActionType>");
                            Document.Append("<OrderQty>" + line.QtyReceived + "</OrderQty>");
                            Document.Append("<NonStockedLine />");
                            Document.Append("</StockLine>");
                        }
                        else
                        {
                            Document.Append("<StockLine>");
                            Document.Append("<PurchaseOrderLine>" + line.Line + "</PurchaseOrderLine>");
                            Document.Append("<LineActionType>D</LineActionType>");
                            Document.Append("<NonStockedLine />");
                            Document.Append("</StockLine>");
                        }

                    }


                    Document.Append("</OrderDetails>");
                    Document.Append("</Orders>");
                    Document.Append("</PostPurchaseOrders>");

                    string XmlOut = sys.SysproPost(Guid, this.BuildPoParameter("Y"), Document.ToString(), "PORTOI");
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                        using (var qdb = new SysproEntities())
                        {
                            var re = (from a in qdb.mtTmpPoToCancels where a.Requisition == Requisition && a.PurchaseOrder == PurchaseOrder select a).FirstOrDefault();
                            re.Status = 2;
                            qdb.Entry(re).State = EntityState.Modified;
                            qdb.SaveChanges();
                        }
                    }
                    else
                    {
                        if (this.CheckIfSplitOrder(Requisition) == false)
                        {
                            using (var cdb = new SysproEntities())
                            {

                                var re = (from a in cdb.mtRequisitionHeaders where a.Requisition == Requisition select a).FirstOrDefault();
                                re.Status = 12;
                                cdb.Entry(re).State = EntityState.Modified;
                                cdb.SaveChanges();
                            }
                        }

                        //AU.AuditPurchaseOrderManual(PurchaseOrder, 0, "D", "PurchaseOrderCancelled", "Header", PurchaseOrder, "");
                        ErrorEventLog.WriteErrorLog("I", "Purchase Order : " + PurchaseOrder + " has been Grn'd. Order Quantity has been updated to receipted quantity.");

                        using (var qdb = new SysproEntities())
                        {
                            var re = (from a in qdb.mtTmpPoToCancels where a.Requisition == Requisition && a.PurchaseOrder == PurchaseOrder select a).FirstOrDefault();
                            re.Status = 3;
                            qdb.Entry(re).State = EntityState.Modified;
                            qdb.SaveChanges();
                        }
                    }


                }
                else
                {
                    //Cancel Po

                    string Guid = sys.SysproLogin("");
                    //Update Order Qty to Grn Qty
                    //Declaration
                    StringBuilder Document = new StringBuilder();

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
                    Document.Append("<OrderActionType>D</OrderActionType>");
                    Document.Append("<PurchaseOrder>" + PurchaseOrder + "</PurchaseOrder>");
                    Document.Append("</OrderHeader>");
                    Document.Append("</Orders>");
                    Document.Append("</PostPurchaseOrders>");

                    string XmlOut = sys.SysproPost(Guid, this.BuildPoParameter("Y"), Document.ToString(), "PORTOI");
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {

                        ErrorEventLog.WriteErrorLog("E", ErrorMessage);
                        using (var qdb = new SysproEntities())
                        {
                            var re = (from a in qdb.mtTmpPoToCancels where a.Requisition == Requisition && a.PurchaseOrder == PurchaseOrder select a).FirstOrDefault();
                            re.Status = 2;
                            qdb.Entry(re).State = EntityState.Modified;
                            qdb.SaveChanges();
                        }
                    }
                    else
                    {
                        if (this.CheckIfSplitOrder(Requisition) == false)
                        {
                            using (var cdb = new SysproEntities())
                            {
                                var re = (from a in cdb.mtRequisitionHeaders where a.Requisition == Requisition select a).FirstOrDefault();
                                re.Status = 12;
                                cdb.Entry(re).State = EntityState.Modified;
                                cdb.SaveChanges();
                            }
                        }
                        //AU.AuditPurchaseOrderManual(PurchaseOrder, 0, "D", "PurchaseOrderCancelled", "Header", PurchaseOrder, "");
                        ErrorEventLog.WriteErrorLog("I", "Purchase Order :" + PurchaseOrder + " has been cancelled.");
                        using (var qdb = new SysproEntities())
                        {
                            var re = (from a in qdb.mtTmpPoToCancels where a.Requisition == Requisition && a.PurchaseOrder == PurchaseOrder select a).FirstOrDefault();
                            re.Status = 3;
                            qdb.Entry(re).State = EntityState.Modified;
                            qdb.SaveChanges();
                        }
                    }


                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckIfSplitOrder(string requisition)
        {
            try
            {
                using (var cdb = new SysproEntities())
                {

                    var re = (from a in cdb.mtRequisitionDetails where a.Requisition == requisition select a.PurchaseOrder).Distinct().ToList();
                    if (re.Count > 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }



}
