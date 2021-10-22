using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class MasterCardStockCodeCustomForm
    {
        public string InvoiceDim { get; set; }
        public string BarCode { get; set; }
        public decimal GenWidth { get; set; }
        public decimal GenLength { get; set; }
        public decimal GenLayFlatWidthSiz { get; set; }
    }
}