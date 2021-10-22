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
    
    public partial class sp_GetTransWaybillsPoEntry_Result
    {
        public int TrackId { get; set; }
        public string Waybill { get; set; }
        public string DispatchNote { get; set; }
        public int DispatchNoteLine { get; set; }
        public string Customer { get; set; }
        public string Province { get; set; }
        public string Town { get; set; }
        public string StockCode { get; set; }
        public string StockDesc { get; set; }
        public Nullable<decimal> DispatchQty { get; set; }
        public string DispatchUom { get; set; }
        public Nullable<decimal> LoadQty { get; set; }
        public string LoadUom { get; set; }
        public Nullable<decimal> Pallets { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> TrnDate { get; set; }
        public string Username { get; set; }
        public string PurchaseOrder { get; set; }
        public Nullable<int> PurchaseOrderLine { get; set; }
        public string PoCreatedBy { get; set; }
        public Nullable<System.DateTime> PoCreatedDate { get; set; }
        public string Grn { get; set; }
        public Nullable<System.DateTime> GrnDate { get; set; }
        public string GrnUser { get; set; }
        public string Invoice { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }
        public string ApJournal { get; set; }
        public string ApUser { get; set; }
        public Nullable<System.DateTime> ApDate { get; set; }
        public Nullable<decimal> ApYear { get; set; }
        public Nullable<decimal> ApMonth { get; set; }
        public string Taxable { get; set; }
        public bool WaybillReturn { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public int SeqNo { get; set; }
        public Nullable<System.DateTime> PODDate { get; set; }
        public string PODComment { get; set; }
    }
}
