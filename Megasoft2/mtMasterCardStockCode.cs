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
    
    public partial class mtMasterCardStockCode
    {
        public int Id { get; set; }
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string LongDesc { get; set; }
        public string StockUom { get; set; }
        public string AlternateUom { get; set; }
        public string OtherUom { get; set; }
        public Nullable<decimal> ConvFactAltUom { get; set; }
        public string ConvMulDiv { get; set; }
        public Nullable<decimal> ConvFactOthUom { get; set; }
        public string MulDiv { get; set; }
        public Nullable<decimal> Mass { get; set; }
        public Nullable<decimal> Volume { get; set; }
        public Nullable<decimal> Decimals { get; set; }
        public string PriceCategory { get; set; }
        public string PriceMethod { get; set; }
        public string PartCategory { get; set; }
        public string WarehouseToUse { get; set; }
        public Nullable<decimal> Dimension { get; set; }
        public Nullable<decimal> Micron { get; set; }
        public string JobClassification { get; set; }
        public string ListPriceCode { get; set; }
        public string ProductClass { get; set; }
        public string TaxCode { get; set; }
        public string StockCodeCreated { get; set; }
        public string WarehouseCreated { get; set; }
        public string PriceCodeCreated { get; set; }
        public string Traceable { get; set; }
        public Nullable<System.DateTime> DateUpdated { get; set; }
        public string Username { get; set; }
    }
}
