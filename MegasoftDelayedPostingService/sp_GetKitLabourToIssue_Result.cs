//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MegasoftDelayedPostingService
{
    using System;
    
    public partial class sp_GetKitLabourToIssue_Result
    {
        public string Job { get; set; }
        public Nullable<decimal> Operation { get; set; }
        public string WorkCentre { get; set; }
        public Nullable<decimal> RequiredSetupTime { get; set; }
        public Nullable<decimal> RunTimeToPost { get; set; }
        public Nullable<decimal> ProductionRunTime { get; set; }
        public Nullable<decimal> RunTimeIssued { get; set; }
        public Nullable<decimal> ProdQty { get; set; }
        public decimal QtyManufactured { get; set; }
        public Nullable<decimal> IExpUnitRunTim { get; set; }
    }
}
