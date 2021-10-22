using System;

namespace Megasoft2.Models
{
    public class ProductionLabel
    {
        public string Job { get; set; }
        public string BatchId { get; set; }
        public string BatchQty { get; set; }
        public Nullable<decimal> ProductionQty { get; set; }
        public int NoOfLabels { get; set; }

        public Nullable<decimal> LastBatch { get; set; }
        public string Printer { get; set; }

        public decimal LabelQty { get; set; }

        public string WorkCentre { get; set; }
        public string Operator { get; set; }
        public string Customer { get; set; }
        public string QC1 { get; set; }
        public string Packer { get; set; }
        public string BagSpecs { get; set; }
        public string Supervisor { get; set; }
        public string QtyToMake { get; set; }
        public string QtyManufactured { get; set; }
        public string QtyOutstanding { get; set; }
        public string StockCode { get; set; }
        public string Department { get; set; }

        public string Reference { get; set; }


    }
}