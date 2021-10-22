using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class LabelPrintPoLine
    {
        public bool PrintLabel { get; set; }
        public string PurchaseOrder { get; set; }
        public decimal Line { get; set; }
        public string StockCode { get; set; }
        public string Description { get; set; }
        public decimal ReelQuantity { get; set; }
        public string ReelNumber { get; set; }
        public int NoOfLables { get; set; }
        public string DeliveryNote { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Printer { get; set; }
        public decimal Price { get; set; }
        public string CostMultiplierRequired { get; set; }
        public decimal CostMultiplier { get; set; }
        public decimal CostMultiplierPrice { get; set; }
        public string Warehouse { get; set; }
        public string Bin { get; set; }
        public string UseMultipleBins { get; set; }
        public string GrnSuspense { get; set; }
        public string Supplier { get; set; }
        public bool AutoReel { get; set; }
        public string Barcode { get; set; }
        public int LabelMultiplier { get; set; }
        public string Reprint { get; set; }
        public string Printed { get; set; }
        public string FileImport { get; set; }
        public string QtyDesc { get; set; }
        public string Program { get; set; }
    }
}