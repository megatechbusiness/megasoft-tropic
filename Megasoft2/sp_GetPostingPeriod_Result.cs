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
    
    public partial class sp_GetPostingPeriod_Result
    {
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public decimal FinMonth { get; set; }
        public decimal FinYear { get; set; }
        public Nullable<System.DateTime> CurFinMonthStartLess1 { get; set; }
        public Nullable<System.DateTime> CurFinMonthStart { get; set; }
        public Nullable<System.DateTime> CurFinMonthEnd { get; set; }
        public Nullable<System.DateTime> CurFinMonthEndPlus1 { get; set; }
        public Nullable<decimal> PostMonth { get; set; }
        public Nullable<decimal> PostYear { get; set; }
    }
}
