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
    
    public partial class mtInvTransfer
    {
        public int TrnId { get; set; }
        public string MovementType { get; set; }
        public string FromWarehouse { get; set; }
        public string FromBin { get; set; }
        public string ToWarehouse { get; set; }
        public string ToBin { get; set; }
        public string StockCode { get; set; }
        public string ReelNo { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public string Journal { get; set; }
        public string GtrReference { get; set; }
        public string Username { get; set; }
        public Nullable<System.DateTime> TrnDate { get; set; }
    }
}
