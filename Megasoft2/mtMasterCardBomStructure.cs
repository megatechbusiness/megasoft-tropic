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
    
    public partial class mtMasterCardBomStructure
    {
        public int Id { get; set; }
        public string ParentPart { get; set; }
        public string Component { get; set; }
        public string Route { get; set; }
        public string SequenceNum { get; set; }
        public string Source { get; set; }
        public Nullable<decimal> QtyPer { get; set; }
        public Nullable<decimal> LayerPerc { get; set; }
        public Nullable<decimal> ScrapPercentage { get; set; }
        public Nullable<decimal> ScrapQuantity { get; set; }
        public string LastSavedBy { get; set; }
        public Nullable<System.DateTime> DateLastSaved { get; set; }
    }
}
