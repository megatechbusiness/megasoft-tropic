using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class InkBomOperation
    {
        public string StockCode { get; set; }
        public string Route { get; set; }
        public decimal Operation { get; set; }
        public string WorkCentre { get; set; }
        public string Mode { get; set; }
        public decimal UnitRunTime { get; set; }
        public decimal Quantity { get; set; }
        public decimal TimeTaken { get; set; }
        public string Narrations { get; set; }
        public string Levelid { get; set; }
        public string CopyOption { get; set; }
        public string ToStockCode { get; set; }
    }
}