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
    
    public partial class sp_GetRequisitionStatusAwaitingAuthList_Result
    {
        public string Grn { get; set; }
        public string PurchaseOrder { get; set; }
        public string Supplier { get; set; }
        public string SupplierName { get; set; }
        public string Requisition { get; set; }
        public string Branch { get; set; }
        public string Site { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string DeliveryNote { get; set; }
        public Nullable<System.DateTime> DeliveryNoteDate { get; set; }
        public string OrigReceiptDate { get; set; }
        public string Invoice { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }
        public Nullable<int> GrnYear { get; set; }
        public Nullable<int> GrnMonth { get; set; }
    }
}
