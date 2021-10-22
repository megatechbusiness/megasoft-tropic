using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class WhseManJobLabel
    {
        public string Job { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public string JobDescription { get; set; }
        public decimal QtyToMake { get; set; }
        public decimal QtyManufactured { get; set; }
        public decimal QtyOutstanding { get; set; }
        public decimal BatchQty { get; set; }
        public decimal ProductionQty { get; set; }
        public decimal NoOfLabels { get; set; }
    }
}