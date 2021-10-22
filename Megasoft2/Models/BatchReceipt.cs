using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class BatchReceipt
    {
        public string PalletNo { get; set; }
        public string NewPalletNo { get; set; }
        public string BailNo { get; set; }
        public string Job { get; set; }
        public string BatchSeq { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public string JobStockCode { get; set; }
        public string JobStockDescription { get; set; }

        public List<sp_GetProductionReturnPalletDetails_Result> PalletDetails { get; set; }
        public List<sp_GetProductionReturnPalletDetails_Result> JobPalletDetails { get; set; }

        public List<sp_GetSplitPalletDetails_Result> SplitPalletDetails { get; set; }
        public List<sp_GetSplitPalletDetails_Result> PalletTransferedTo { get; set; }
        public ExportFile PalletReport { get; set; }
        public ExportFile PalletInformation { get; set; }

        public decimal QtyOnHand { get; set; }
        public decimal PackSize { get; set; }
        public int NoOfLabels { get; set; }
        public string TraceableType { get; set; }
        public string Printer { get; set; }
        public string Warehouse { get; set; }
        public string Uom { get; set; }

    }
}