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
    
    public partial class mtMasterCardOther
    {
        public int Id { get; set; }
        public bool CorePlugs { get; set; }
        public bool Interleaves { get; set; }
        public bool TopBoard { get; set; }
        public bool PalletMarkings { get; set; }
        public bool Strapping { get; set; }
        public bool StretchWrap { get; set; }
        public string NameOfProduct { get; set; }
        public string Order { get; set; }
        public string WorkOrder { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<System.DateTime> ProductionDate { get; set; }
        public string SpecialInstructions { get; set; }
        public string DeliveryInstructions { get; set; }
        public bool Issued { get; set; }
    }
}
