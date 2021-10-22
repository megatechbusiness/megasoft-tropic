using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class MasterCardComponent
    {
        public int Id { get; set; }
        public string ParentPart { get; set; }
        public string Component { get; set; }
        public string Route { get; set; }
        public string SequenceNum { get; set; }
        public Nullable<decimal> QtyPer { get; set; }
        public Nullable<decimal> LayerPerc { get; set; }
        public Nullable<decimal> ScrapPercentage { get; set; }
        public Nullable<decimal> ScrapQuantity { get; set; }
        public string LastSavedBy { get; set; }
        public Nullable<System.DateTime> DateLastSaved { get; set; }
        public string Mode { get; set; }
        public string Levelid { get; set; }
        public string CopyOption { get; set; }
        public string ToStockCode { get; set; }
    }
}