using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using MegasoftDelayedPosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MegasoftDelayedPostingService.MegasoftDelayedPosting;

namespace MegasoftDelayedPostingService
{
    class SysproBusinessLogic
    {
        SysproCore objSyspro = new SysproCore();
        SysproEntities db = new SysproEntities();
        MegasoftEntities mdb = new MegasoftEntities();
        public void PostBinTransfer(sp_GetDelayedPostingData_Result model, string Guid)
        {
            try
            {
                string Parameter, XmlOut, ErrorMessage;

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Inventory Bin Transfers Business Object");
                Document.Append("-->");
                Document.Append("<PostInvBinTransfers xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMBDOC.XSD\">");

                Document.Append(this.BuildBinTransferDocument(model.FromWarehouse, model.FromBin, model.StockCode, model.Lot, model.Quantity.ToString(), model.ToBin));
                
                Document.Append("</PostInvBinTransfers>");

                Parameter = this.BuildBinTransferParameter();
                XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "INVTMB");                
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    db.sp_SaveInvTransfer("Bin", model.FromWarehouse, model.FromBin, model.ToWarehouse, model.ToBin, model.StockCode, model.Lot, model.Quantity, Journal, "", model.Username);
                    using(var udb = new SysproEntities())
                    {
                        var result = (from a in udb.mtInvDelayedPostings where a.TrnId == model.TrnId select a).FirstOrDefault();
                        result.Journal = Journal;
                        result.Status = 2;
                        result.ErrorMessage = "";
                        udb.Entry(result).State = System.Data.Entity.EntityState.Modified;
                        udb.SaveChanges();
                    }

                    ErrorEventLog.WriteErrorLog("I", "Delayed Posting Id :" + model.TrnId + " - Posting Complete. Jnl : " + Journal);
                }
                else
                {
                    using (var udb = new SysproEntities())
                    {
                        var result = (from a in udb.mtInvDelayedPostings where a.TrnId == model.TrnId select a).FirstOrDefault();
                        result.Status = 3;
                        result.ErrorMessage = ErrorMessage;
                        udb.Entry(result).State = System.Data.Entity.EntityState.Modified;
                        udb.SaveChanges();
                    }
                    ErrorEventLog.WriteErrorLog("E", "Bin Transfer Error : Delayed Posting Id :" + model.TrnId + " - " + ErrorMessage);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildBinTransferDocument(string Warehouse, string SourceBin, string StockCode, string Lot, string Quantity, string DestinationBin)
        {
            try
            {
                // var MultiBins = (from a in db.InvWhControls where a.Warehouse.Equals(SourceWarehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in db.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();

                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Version/>");
                Document.Append("<Release/>");
                Document.Append("<Quantity>" + Quantity + "</Quantity>");
                Document.Append("<UnitOfMeasure/>");
                Document.Append("<Units/>");
                Document.Append("<Pieces/>");
                Document.Append("<FromBin>" + SourceBin + "</FromBin>");
                Document.Append("<ToBin>" + DestinationBin + "</ToBin>");
                if (TraceableType.TraceableType == "T")
                {
                    Document.Append("<Lot>" + Lot + "</Lot>");
                }

                Document.Append("<UpdateZeroLotDate/>");
                Document.Append("<Reference>" + Lot + "</Reference>");
                Document.Append("<Notation>" + Lot + "</Notation>");
                Document.Append("<eSignature/>");
                Document.Append("</Item>");


                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildBinTransferParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory Bin Transfers Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvBinTransfers xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMB.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<PostingPeriod>C</PostingPeriod>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<WarnIfLotDatesDiffer>N</WarnIfLotDatesDiffer>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvBinTransfers>");

            return Parameter.ToString();

        }

        public void PostWarehouseTransfer(sp_GetDelayedPostingData_Result model, string Guid)
        {
            try
            {
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

                Document.Append(this.BuildWarehouseTransferDocument(model.FromWarehouse, model.FromBin, model.StockCode, model.Lot, model.Quantity.ToString(), model.ToWarehouse, model.ToBin));                
                Document.Append("</PostInvWhTransferOut>");

                Parameter = this.BuildWarehouseTransferParameter();
                XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "INVTMO");
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    db.sp_SaveInvTransfer("Immediate", model.FromWarehouse, model.FromBin, model.ToWarehouse, model.ToBin, model.StockCode, model.Lot, model.Quantity, Journal, "", model.Username);
                    using (var udb = new SysproEntities())
                    {
                        var result = (from a in udb.mtInvDelayedPostings where a.TrnId == model.TrnId select a).FirstOrDefault();
                        result.Journal = Journal;
                        result.Status = 2;
                        result.ErrorMessage = "";
                        udb.Entry(result).State = System.Data.Entity.EntityState.Modified;
                        udb.SaveChanges();
                    }

                    ErrorEventLog.WriteErrorLog("I", "Delayed Posting Id :" + model.TrnId + " - Posting Complete. Jnl : " + Journal);
                }
                else
                {
                    using (var udb = new SysproEntities())
                    {
                        var result = (from a in udb.mtInvDelayedPostings where a.TrnId == model.TrnId select a).FirstOrDefault();
                        result.Status = 3;
                        result.ErrorMessage = ErrorMessage;
                        udb.Entry(result).State = System.Data.Entity.EntityState.Modified;
                        udb.SaveChanges();
                    }

                    ErrorEventLog.WriteErrorLog("E", "Immediate Transfer Error : Delayed Posting Id :" + model.TrnId + " - " + ErrorMessage);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildWarehouseTransferDocument(string SourceWarehouse, string SourceBin, string StockCode, string Lot, string Quantity, string DestinationWarehouse, string DestinationBin)
        {
            try
            {
                var MultiBins = (from a in db.vw_InvWhControl where a.Warehouse.Equals(SourceWarehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in db.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();

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


        public void PostJobReceiptByBatch(string PalletNo, string Guid)
        {
           
            try
            {
                ErrorEventLog.WriteErrorLog("I", "0");
                //string PalletNo = detail.FirstOrDefault().Lot;
                //user = current user
                //string User = HttpContext.Current.User.Identity.Name.ToUpper();
                var JobsToPost = (from a in db.mtProductionLabels where a.PalletNo == PalletNo && (a.LabelReceipted == "D") && a.Scanned == "Y" select a).ToList();
                if (JobsToPost.Count == 0)
                {
                    return;// "No unposted data found.";
                }

                

                List<WhseManJobReceipt> result = JobsToPost.GroupBy(l => l.Job).Select(cl => new WhseManJobReceipt { Job = cl.First().Job, Quantity = Convert.ToInt32(cl.Sum(c => c.NetQty)), Lot = cl.First().PalletNo }).ToList();
                //string Journal = "Pallet: " + PalletNo + ", Job Receipt Completed Successfully. Journal : ";


                ErrorEventLog.WriteErrorLog("I", "1");
                foreach (var item in result)
                {
                    var setting = (from a in db.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                    string ErrorMessage = "";
                    string XmlOut;
                    if (setting.PostMaterialIssue == true)
                    {
                        ErrorMessage = this.PostMaterialIssue(Guid, item.Job, item.Lot, (decimal)item.Quantity);

                        if (!string.IsNullOrEmpty(ErrorMessage))
                        {
                            foreach (var muitem in result)
                            {
                                using (var mfdb = new SysproEntities())
                                {
                                    var _tbl = (from a in mfdb.mtProductionLabels where a.Job == muitem.Job && a.BatchId == muitem.Lot select a).FirstOrDefault();
                                    _tbl.LabelReceipted = "E";
                                    _tbl.ErrorMessage = "Material Issue Error: " + ErrorMessage;
                                    mfdb.Entry(_tbl).State = System.Data.Entity.EntityState.Modified;
                                    mfdb.SaveChanges();
                                }
                            }
                            return;// "Material Issue Error: " + ErrorMessage;
                        }
                    }
                    if (setting.PostLabour == true)
                    {
                        ErrorMessage = this.PostLabourIssue(Guid, item.Job, (decimal)item.Quantity);

                        if (!string.IsNullOrEmpty(ErrorMessage))
                        {
                            foreach (var muitem in result)
                            {
                                using (var lfdb = new SysproEntities())
                                {
                                    var _tbl = (from a in lfdb.mtProductionLabels where a.Job == muitem.Job && a.BatchId == muitem.Lot select a).FirstOrDefault();
                                    _tbl.LabelReceipted = "E";
                                    _tbl.ErrorMessage = "Labour Issue Error: " + ErrorMessage;
                                    lfdb.Entry(_tbl).State = System.Data.Entity.EntityState.Modified;
                                    lfdb.SaveChanges();
                                }
                            }
                            return;// "Labour Issue Error: " + ErrorMessage;
                        }
                    }

                    ErrorEventLog.WriteErrorLog("I", "2");
                    var BatchList = (from a in JobsToPost where a.PalletNo == item.Lot && a.Job == item.Job select new WhseManJobReceipt { Job = a.Job, Lot = a.BatchId, Quantity = (decimal)a.NetQty }).OrderBy(x => x.Lot).ToList();
                    XmlOut = objSyspro.SysproPost(Guid, this.BuildJobReceiptParameter(), this.BuildJobReceiptDocument(BatchList), "WIPTJR");
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    string JobJournal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                    ErrorEventLog.WriteErrorLog("I", "3");
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        foreach (var a in BatchList)
                        {
                            string Job = a.Job.PadLeft(15, '0');
                            db.sp_UpdateLabelReceipted(Job, a.Lot, "Y", JobJournal, "DELAYEDPOSTING");

                            var Traceable = (from Z in db.WipMasters where Z.Job == Job && Z.TraceableType == "T" select Z).ToList();
                            if (Traceable.Count > 0)
                            {
                                var check = db.sp_BaggingCheckCustomForm(a.Lot, Job).ToList().Count();
                                if (check > 0)
                                {
                                    db.sp_BaggingUpdateCustomForm(a.Lot, PalletNo, Job);
                                }
                                else
                                {
                                    db.sp_BaggingSaveCustomForm(a.Lot, PalletNo, Job);
                                }
                            }
                        }
                        mtPalletControl close = new mtPalletControl();
                        close = db.mtPalletControls.Find(PalletNo);
                        close.Status = "C";
                        db.Entry(close).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        //Journal += JobJournal;
                    }
                    else
                    {
                        foreach (var uitem in BatchList)
                        {
                            using (var fdb = new SysproEntities())
                            {
                                var _tbl = (from a in fdb.mtProductionLabels where a.Job == uitem.Job && a.BatchId == uitem.Lot select a).FirstOrDefault();
                                _tbl.LabelReceipted = "E";
                                _tbl.ErrorMessage = ErrorMessage;
                                fdb.Entry(_tbl).State = System.Data.Entity.EntityState.Modified;
                                fdb.SaveChanges();
                            }
                        }
                        return;// "Job Receipt Error: " + ErrorMessage;
                    }
                }
                //objSyspro.SysproLogoff(Guid);
                return;// Journal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

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

                var items = db.sp_GetKitMaterialToIssue(Job.PadLeft(15, '0'), Quantity).ToList();
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
                            var lots = db.sp_GetAutoDepleteLots(item.StockCode, item.Warehouse, item.MaterialToPost).ToList();
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
                                Document.Append("<MaterialReference>DELAYEDPOSTING</MaterialReference>");
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
                            Document.Append("<QtyIssued>" + string.Format("{0:##,###,##0.00}", item.MaterialToPost) + "</QtyIssued>");
                            Document.Append("<Reference>" + Job + "</Reference>");
                            Document.Append("<MaterialReference>DELAYEDPOSTING</MaterialReference>");
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

                var items = db.sp_GetKitLabourToIssue(Job.PadLeft(15, '0'), Quantity).ToList();

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
                        Document.Append("<AdditionalReference>DELAYEDPOSTING</AdditionalReference>");
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
        public string BuildJobReceiptDocument(List<WhseManJobReceipt> detail)
        {
            ErrorEventLog.WriteErrorLog("I", "2.1");
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
                ErrorEventLog.WriteErrorLog("I", "2.2");
                ErrorEventLog.WriteErrorLog("I", item.Job);
                var StocKCode = (from a in db.WipMasters.AsNoTracking() where a.Job == item.Job select a.StockCode).FirstOrDefault();
                var result = (from a in db.InvMasters.AsNoTracking() where a.StockCode == StocKCode select a).FirstOrDefault();
                ErrorEventLog.WriteErrorLog("I", "2.3");
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
                Document.Append("<CostBasis>E</CostBasis>");
                //Document.Append("<ReceiptCost>441.73</ReceiptCost>");
                Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
                Document.Append("<MaterialDistributionValue />");
                Document.Append("<LaborDistributionValue />");
                Document.Append("<JobComplete></JobComplete>");
                Document.Append("<CoProductComplete>N</CoProductComplete>");
                Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                Document.Append("<SalesOrderComplete>N</SalesOrderComplete>");

                string Job = item.Job.PadLeft(15, '0');
                var Traceable = (from a in db.WipMasters where a.Job == Job && a.TraceableType == "T" select a).ToList();
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
            ErrorEventLog.WriteErrorLog("I", "2.4");
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

        public void SendEmail(Mail mail, List<string> Files)
        {
            //function to send email
            MailMessage email = new MailMessage();
            foreach (var address in mail.To.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                email.To.Add(address);
            }

            if (!string.IsNullOrEmpty(mail.CC))
            {
                email.CC.Add(mail.CC);
            }

            email.From = new MailAddress(mail.From);
            email.Subject = mail.Subject;
            string Body = mail.Body;
            email.Body = Body;
            email.IsBodyHtml = true;

            var emailSettings = (from a in mdb.mtSystemSettings select a).FirstOrDefault();

            SmtpClient smtp = new SmtpClient();
            smtp.Host = emailSettings.SmtpHost;
            smtp.Port = (int)emailSettings.SmtpPort;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(emailSettings.FromAddress, emailSettings.FromAddressPassword);
            smtp.EnableSsl = true;
            //smtp.UseDefaultCredentials = true;
            //smtp.EnableSsl = false;


            foreach (var path in Files)
            {
                // Create the file attachment for this e-mail message.
                Attachment data = new Attachment(path, MediaTypeNames.Application.Pdf);
                // Add time stamp information for the file.
                ContentDisposition disposition = data.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(path);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(path);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(path);

                // Add the file attachment to this e-mail message.
                email.Attachments.Add(data);
            }
            smtp.Send(email);
            smtp.Dispose();

        }

        public string ExportPdf(string Report)
        {
            try
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePathRelativeToAssembly = Path.Combine(assemblyPath, @"..\MegasoftDelayedPostingService\ReportAutomation\PDF\");
                string normalizedPath = Path.GetFullPath(filePathRelativeToAssembly);
                ErrorEventLog.WriteErrorLog("I", normalizedPath);
                string[] files = Directory.GetFiles(normalizedPath);

                foreach (string delFile in files)
                {
                    FileInfo fi = new FileInfo(delFile);
                    if (fi.LastWriteTime < DateTime.Now.AddDays(-7))
                        fi.Delete();
                }
            }
            catch (Exception err)
            {
                ErrorEventLog.WriteErrorLog("I", "Error Cleaning" + err.Message);
            }

            try

            {

                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePathRelativeToAssembly = Path.Combine(assemblyPath, @"..\MegasoftDelayedPostingService\ReportAutomation\PDF\");
                string FilePath = Path.GetFullPath(filePathRelativeToAssembly);
                var ReportDet = (from a in db.mtReportAutomations where a.Report == Report select a).FirstOrDefault();

                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportDet.ReportPath);
                ErrorEventLog.WriteErrorLog("I", "Loaded");
                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["SysproEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'SysproEntities' in web.config file");
                }
                string sysproConnectionString = sysproSettings.ConnectionString;
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                string password = sqlConnectionStringBuilder.Password;
                string userId = sqlConnectionStringBuilder.UserID;

                rpt.SetDatabaseLogon(userId, password);
                ErrorEventLog.WriteErrorLog("I", "Logged in");

                string FileName = ReportDet.Report + "-" + DateTime.Now.ToString("_MMddyyyy_HHmmss") + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);
                ErrorEventLog.WriteErrorLog("I", OutputPath);

                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                ErrorEventLog.WriteErrorLog("I", "Exported");
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                return OutputPath;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class ExportFile
        {
            public string Requistion { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
        }

    }



}
