using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PurchaseOrderLabel
    {
        public List<sp_GetPurchaseOrderLinesForLabel_Result> PoLines { get; set; }
        public string PurchaseOrder { get; set; }
        public int Line { get; set; }
        public List<sp_GetLabelDataForReprint_Result> Labels { get; set; }
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string Printer { get; set; }

        public string StockPrinter { get; set; }
        public string Barcode { get; set; }
        public string Program { get; set; }

    }
}