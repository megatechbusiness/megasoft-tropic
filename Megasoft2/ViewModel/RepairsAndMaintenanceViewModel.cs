using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RepairsAndMaintenanceViewModel
    {
        public string CostCentre { get; set; }
        public string WorkCentre { get; set; }
        public string Warehouse { get; set; }
        public string Barcode { get; set; }
        public string StockCode { get; set; }
        public string Bin { get; set; }
        public string Lot { get; set; }
        public decimal Quantity { get; set; }
        public string Reference { get; set; }
        public string ProgramMode { get; set; }
        public string Employee { get; set; }
        public string StockUom { get; set; }
        public string Notes { get; set; }
    }
}