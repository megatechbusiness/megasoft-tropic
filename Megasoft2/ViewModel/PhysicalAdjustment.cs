using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PhysicalAdjustment
    {
        public string Warehouse { get; set; }
        public string StockCode { get; set; }
        public decimal Limit { get; set; }
        public string Reference { get; set; }
        public List<sp_GetLotsToAdjust_Result> Stock { get; set; }
    }
}