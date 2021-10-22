using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class ProductionReceipt
    {
        public string Job { get; set; }
        public string StockCode { get; set; }
        public decimal Quantity { get; set; }
        public string LotNumber { get; set; }

    }
}