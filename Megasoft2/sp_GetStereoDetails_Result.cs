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
    
    public partial class sp_GetStereoDetails_Result
    {
        public int ReqNo { get; set; }
        public int Line { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public string Colour { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> PoPrice { get; set; }
        public string GlCode { get; set; }
        public string TaxCode { get; set; }
        public Nullable<decimal> Width { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<int> SysproPurchaseOrderLine { get; set; }
        public Nullable<decimal> GrnLength { get; set; }
        public Nullable<decimal> GrnWidth { get; set; }
        public string Grn { get; set; }
        public Nullable<System.DateTime> GrnDate { get; set; }
        public string GrnUser { get; set; }
        public string ApJournal { get; set; }
        public Nullable<System.DateTime> ApDate { get; set; }
        public string ApUser { get; set; }
        public Nullable<decimal> ApMonth { get; set; }
        public Nullable<decimal> ApYear { get; set; }
        public string Invoice { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public Nullable<decimal> InvoiceAmount { get; set; }
        public string PurchaseOrder { get; set; }
        public Nullable<System.DateTime> PoCreatedDate { get; set; }
        public Nullable<decimal> SquareM { get; set; }
    }
}
