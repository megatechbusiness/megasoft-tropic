//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Megasoft2
{
    using System;
    using System.Collections.Generic;
    
    public partial class mtGrnDetail
    {
        public string Grn { get; set; }
        public int GrnLine { get; set; }
        public string PurchaseOrder { get; set; }
        public decimal PurchaseOrderLin { get; set; }
        public string Supplier { get; set; }
        public System.DateTime OrigReceiptDate { get; set; }
        public decimal ReqGrnYear { get; set; }
        public decimal ReqGrnMonth { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public string Warehouse { get; set; }
        public decimal QtyReceived { get; set; }
        public string QtyUom { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Job { get; set; }
        public string DeliveryNote { get; set; }
        public Nullable<System.DateTime> DeliveryNoteDate { get; set; }
        public string ProductClass { get; set; }
        public string TaxCode { get; set; }
        public string GlCode { get; set; }
        public string AnalysisEntry { get; set; }
        public string SuspenseAccount { get; set; }
        public string Invoice { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string Username { get; set; }
        public bool PoAttached { get; set; }
        public bool GrnAttached { get; set; }
        public bool PodAttached { get; set; }
        public bool CastAndExtensions { get; set; }
        public bool GlCodeChecked { get; set; }
        public bool SupplierCodeChecked { get; set; }
        public bool AuthorizedLevel1 { get; set; }
        public string Level1AuthorizedBy { get; set; }
        public bool AuthorizedLevel2 { get; set; }
        public string Level2AuthorizedBy { get; set; }
        public Nullable<int> PostStatus { get; set; }
        public Nullable<System.DateTime> GrnPostDate { get; set; }
        public Nullable<System.DateTime> InvoicePostDate { get; set; }
        public string SysproGrn { get; set; }
        public string GrnError { get; set; }
        public string InvoiceError { get; set; }
        public string Journal { get; set; }
        public string MaterialAllocationError { get; set; }
        public string MaterialIssueError { get; set; }
        public string Requisition { get; set; }
        public string Branch { get; set; }
        public string Site { get; set; }
        public string GrnAdjustmentError { get; set; }
        public string ReceivedBy { get; set; }
        public string JournalUpdated { get; set; }
        public Nullable<decimal> GrnJournalYear { get; set; }
        public Nullable<decimal> GrnJournalMonth { get; set; }
        public Nullable<System.DateTime> OriginalInvoiceDate { get; set; }
        public string ApJournal { get; set; }
        public Nullable<decimal> ApJournalYear { get; set; }
        public Nullable<decimal> ApJournalMonth { get; set; }
        public string IssueJournal { get; set; }
        public Nullable<decimal> IssueJournalYear { get; set; }
        public Nullable<decimal> IssueJournalMonth { get; set; }
        public string Currency { get; set; }
        public string GrnDoneBy { get; set; }
    }
}
