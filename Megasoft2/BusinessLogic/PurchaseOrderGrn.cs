using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    
    public class PurchaseOrderGrn
    {
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        RequisitionAudit AU = new RequisitionAudit();

        public string SaveGrn(PoGrn model)
        {
            try
            {
                //if(model.Grn != null)
                //{
                //    using(var db = new SysproEntities(""))
                //    {
                //        var result = (from a in db.mtGrnDetails where a.Grn == model.Grn select a).ToList();
                //        foreach(var item in result)
                //        {
                //            db.Entry(item).State = System.Data.EntityState.Deleted;
                //            db.SaveChanges();
                //        }
                //    }

                    
                //}

                string Grn = this.GetNextGrn();
                string Branch = model.GrnLines.FirstOrDefault().Branch;
                string Site = model.GrnLines.FirstOrDefault().Site;
                int Line = 1;
                foreach(var line in model.GrnLines)
                {
                    if(line.GrnQty != null && line.GrnQty != 0)
                    {
                        var PostDates = sdb.sp_GetPostingPeriod(model.DeliveryNoteDate).FirstOrDefault();
                        mtGrnDetail det = new mtGrnDetail();
                        det.Grn = Grn;
                        det.GrnLine = Line;
                        det.PurchaseOrder = model.PurchaseOrder;
                        det.PurchaseOrderLin = (decimal)line.Line;
                        det.Supplier = line.Supplier;
                        det.OrigReceiptDate = DateTime.Now;
                        det.ReqGrnMonth = (decimal)PostDates.PostMonth;
                        det.ReqGrnYear = (decimal)PostDates.PostYear;
                        det.StockCode = line.StockCode;
                        det.StockDescription = line.Description;
                        det.Warehouse = line.Warehouse;
                        det.QtyReceived = (decimal)line.GrnQty;
                        det.QtyUom = line.Uom;
                        det.Price = line.Price;
                        if (line.Job == null)
                        {
                            det.Job = "";
                        }
                        else
                        {
                            det.Job = line.Job;
                        }
                        
                        det.DeliveryNote = model.DeliveryNote;
                        det.DeliveryNoteDate = model.DeliveryNoteDate;                       
                        det.ProductClass = line.ProductClass;
                        det.TaxCode = line.TaxCode;
                        det.GlCode = line.GlCode;
                        if(line.HierachyCategory == null)
                        {
                            det.AnalysisEntry = "";
                        }
                        else
                        {
                            det.AnalysisEntry = line.HierachyCategory;
                        }                        
                        det.SuspenseAccount = (from a in sdb.mtBranchSites where a.Branch == Branch && a.Site == Site select a).FirstOrDefault().AccrualSuspenseAcc;
                        det.Requisition = line.Requisition;
                        det.Branch = line.Branch;
                        det.Site = line.Site;
                        det.ReceivedBy = model.ReceivedBy;
                        det.Currency = line.Currency;
                        det.GrnDoneBy = HttpContext.Current.User.Identity.Name.ToUpper();
                        sdb.mtGrnDetails.Add(det);
                        sdb.SaveChanges();
                        Line++;
                        AU.AuditGrnManual(Grn, Line, "A", "Purchase Order Grn", "QtyReceived", "", line.GrnQty.ToString());
                    }
                    


                }
                using (var setdb = new SysproEntities(""))
                {
                    var result = (from a in setdb.mtRequisitionSettings where a.SettingId == 1 select a).FirstOrDefault();
                    result.LastGrn = Grn;
                    setdb.Entry(result).State = System.Data.EntityState.Modified;
                    setdb.SaveChanges();
                }

                AU.AuditGrnManual(Grn, Line, "A", "Purchase Order Grn", "DeliveryNote", "", model.DeliveryNote);
                AU.AuditGrnManual(Grn, Line, "A", "Purchase Order Grn", "DeliveryDate", "", model.DeliveryNoteDate.ToString());
                AU.AuditGrnManual(Grn, Line, "A", "Purchase Order Grn", "ReceivedBy", "", model.ReceivedBy);
                return Grn;

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string GetNextGrn()
        {
            try
            {
                return sdb.sp_GetNextGrnNumber().FirstOrDefault().NewGrn;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}