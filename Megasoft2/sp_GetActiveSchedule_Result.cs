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
    
    public partial class sp_GetActiveSchedule_Result
    {
        public Nullable<long> SeqNo { get; set; }
        public string Customer { get; set; }
        public string Job { get; set; }
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string MasterJob { get; set; }
        public Nullable<decimal> Mass { get; set; }
        public Nullable<decimal> QtyToMake { get; set; }
        public Nullable<decimal> QtyManufactured { get; set; }
        public Nullable<decimal> QtyToPlan { get; set; }
        public string OrderDate { get; set; }
        public string JobDeliveryDate { get; set; }
        public string PlanDate { get; set; }
        public Nullable<int> DaysOverdue { get; set; }
        public Nullable<decimal> WeeksOverdue { get; set; }
        public Nullable<decimal> Produced { get; set; }
        public Nullable<decimal> Balance { get; set; }
        public int Rolls { get; set; }
        public string Urgent { get; set; }
    }
}
