using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class MaterialIssue
    {
        public string Job { get; set; }
        public string Warehouse { get; set; }
        public string StockCode { get; set; }
        public string LotNumber { get; set; }
        public decimal Quantity { get; set; }
        public string Barcode { get; set; }
        public string Bin { get; set; }
        public bool MaterialReturn { get; set; }
        public string Printer { get; set; }
        public bool PrintLabelOnReturn { get; set; }
        public string Department { get; set; }
        public int NumberofLabels { get; set; }
        public string Shift { get; set; }
        public string WorkCentre { get; set; }
    }
}