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
    
    public partial class sp_mtReqGetRequisitionLinesForPo_Result
    {
        public string Requisition { get; set; }
        public decimal Line { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public decimal OrderQty { get; set; }
        public string OrderUom { get; set; }
        public string Supplier { get; set; }
        public string SupplierName { get; set; }
        public decimal Price { get; set; }
        public string PriceUom { get; set; }
        public string Warehouse { get; set; }
        public string Job { get; set; }
        public string GlCode { get; set; }
        public string ProductClass { get; set; }
        public string TaxCode { get; set; }
        public string ReasonForReqn { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string Currency { get; set; }
        public Nullable<decimal> ExchangeRate { get; set; }
        public Nullable<decimal> LocalCurrency { get; set; }
        public decimal SubOperationNum { get; set; }
        public string Buyer { get; set; }
        public string TermsCode { get; set; }
    }
}
