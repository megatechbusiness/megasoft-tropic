using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class InvoiceMatching
    {
        public List<sp_GetGrnLinesForInvoicing_Result> PoLines { get; set; }
        public string PurchaseOrder { get; set; }
        public string Grn { get; set; }
        public string DeliveryNote { get; set; }
        public DateTime? DeliveryNoteDate { get; set; }
        public string Invoice { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string Requisition { get; set; }
        public string Branch { get; set; }
        public string Site { get; set; }
        public bool PoAttached  { get; set; }
        public bool GrnAttached  { get; set; }
        public bool PodAttached  { get; set; }
        public bool CastAndExtensions  { get; set; }
        public bool GlCodeChecked  { get; set; }
        public bool SupplierCodeChecked  { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
        public string  Supplier { get; set; }
        public string ReceivedBy { get; set; }
        public string Currency { get; set; }
    }
}