using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class RequisitionAudit
    {
        SysproEntities sdb = new SysproEntities("");

        public void AuditNewRequisition(string Requisition)
        {
            try
            {
                mtRequisitionAudit au = new mtRequisitionAudit();
                au.Requisition = Requisition;
                au.Line = 0;
                au.TrnType = "A";
                au.Program = "Requisition Header";
                au.KeyField = "Requisition";
                au.OldValue = "";
                au.NewValue = Requisition;
                au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                au.TrnDate = DateTime.Now;
                sdb.mtRequisitionAudits.Add(au);
                sdb.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AuditNewLine(string Requisition, int Line)
        {
            try
            {
                mtRequisitionAudit au = new mtRequisitionAudit();
                au.Requisition = Requisition;
                au.Line = Line;
                au.TrnType = "A";
                au.Program = "Requisition Line";
                au.KeyField = "Line";
                au.OldValue = "";
                au.NewValue = Line.ToString();
                au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                au.TrnDate = DateTime.Now;
                sdb.mtRequisitionAudits.Add(au);
                sdb.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AuditModifiedLine(string Requisition, int Line, List<EntityChanges> changes)
        {
            try
            {
                foreach(var change in changes)
                {
                    mtRequisitionAudit au = new mtRequisitionAudit();
                    au.Requisition = Requisition;
                    au.Line = Line;
                    au.TrnType = "C";
                    au.Program = "Requisition Line";
                    au.KeyField = change.KeyField;
                    au.OldValue = change.OldValue;
                    au.NewValue = change.NewValue;
                    au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                    au.TrnDate = DateTime.Now;
                    sdb.mtRequisitionAudits.Add(au);
                    sdb.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AuditModifiedRequisition(string Requisition, List<EntityChanges> changes)
        {
            try
            {
                foreach (var change in changes)
                {
                    mtRequisitionAudit au = new mtRequisitionAudit();
                    au.Requisition = Requisition;
                    au.Line = 0;
                    au.TrnType = "C";
                    au.Program = "Requisition Header";
                    au.KeyField = change.KeyField;
                    au.OldValue = change.OldValue;
                    au.NewValue = change.NewValue;
                    au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                    au.TrnDate = DateTime.Now;
                    sdb.mtRequisitionAudits.Add(au);
                    sdb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void AuditNewRequisitionQuote(string Requisition, int Line, string KeyField, string Value)
        {
            try
            {
                if((!string.IsNullOrEmpty(Value)) && Value != null && Value != "0" && Value != "0.00")
                {
                    mtRequisitionAudit au = new mtRequisitionAudit();
                    au.Requisition = Requisition;
                    au.Line = Line;
                    au.TrnType = "A";
                    au.Program = "Requisition Quote";
                    au.KeyField = KeyField;
                    au.OldValue = "";
                    au.NewValue = Value.ToString();
                    au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                    au.TrnDate = DateTime.Now;
                    sdb.mtRequisitionAudits.Add(au);
                    sdb.SaveChanges();
                }
                
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AuditModifiedQuoteLine(string Requisition, int Line, List<EntityChanges> changes)
        {
            try
            {
                foreach (var change in changes)
                {
                    mtRequisitionAudit au = new mtRequisitionAudit();
                    au.Requisition = Requisition;
                    au.Line = Line;
                    au.TrnType = "C";
                    au.Program = "Requisition Quote";
                    au.KeyField = change.KeyField;
                    au.OldValue = change.OldValue;
                    au.NewValue = change.NewValue;
                    au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                    au.TrnDate = DateTime.Now;
                    sdb.mtRequisitionAudits.Add(au);
                    sdb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AuditRequisitionManual(string Requisition, int Line, string TrnType, string Program, string KeyField,  string OldValue, string NewValue)
        {
            try
            {
                mtRequisitionAudit au = new mtRequisitionAudit();
                au.Requisition = Requisition;
                au.Line = Line;
                au.TrnType = TrnType;
                au.Program = Program;
                au.KeyField = KeyField;
                au.OldValue = OldValue;
                au.NewValue = NewValue;
                au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                au.TrnDate = DateTime.Now;
                sdb.mtRequisitionAudits.Add(au);
                sdb.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void AuditPurchaseOrderManual(string PurchaseOrder, int Line, string TrnType, string Program, string KeyField, string OldValue, string NewValue)
        {
            try
            {
                mtRequisitionAudit au = new mtRequisitionAudit();
                au.PurchaseOrder = PurchaseOrder;
                au.Line = Line;
                au.TrnType = TrnType;
                au.Program = Program;
                au.KeyField = KeyField;
                au.OldValue = OldValue;
                au.NewValue = NewValue;
                au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                au.TrnDate = DateTime.Now;
                sdb.mtRequisitionAudits.Add(au);
                sdb.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AuditGrnManual(string Grn, int Line, string TrnType, string Program, string KeyField, string OldValue, string NewValue)
        {
            try
            {
                mtGrnInvoiceAudit au = new mtGrnInvoiceAudit();
                au.Grn = Grn;
                au.GrnLine = Line;
                au.TrnType = TrnType;
                au.Program = Program;
                au.KeyField = KeyField;
                au.OldValue = OldValue;
                au.NewValue = NewValue;
                au.Username = HttpContext.Current.User.Identity.Name.ToUpper().ToString();
                au.TrnDate = DateTime.Now;
                sdb.mtGrnInvoiceAudits.Add(au);
                sdb.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        

    }
}