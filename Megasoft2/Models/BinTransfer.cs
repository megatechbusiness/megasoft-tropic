using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class BinTransfer
    {
        public string Warehouse { get; set; }
        public string SourceBin { get; set; }
        public string DestinationBin { get; set; }
        public string Barcode { get; set; }
        public string StockCode { get; set; }
        public decimal Quantity { get; set; }
        public string LotNumber { get; set; }


    }
}