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
    
    public partial class sp_GetProductionJobDetails_Result
    {
        public string Job { get; set; }
        public string Customer { get; set; }
        public string JobDescription { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public string Reference { get; set; }
        public string BagSpecs { get; set; }
        public string BailQty { get; set; }
        public string Operator { get; set; }
        public string Packer { get; set; }
        public string QC1 { get; set; }
        public string QC2 { get; set; }
        public string Supervisor { get; set; }
        public Nullable<decimal> QtyToMake { get; set; }
        public Nullable<decimal> QtyManufactured { get; set; }
        public Nullable<decimal> QtyOutstanding { get; set; }
        public string BatchQty { get; set; }
        public Nullable<decimal> ProductionQty { get; set; }
        public Nullable<decimal> NoOfLabels { get; set; }
        public string WorkCentre { get; set; }
        public string StockUom { get; set; }
    }
}
