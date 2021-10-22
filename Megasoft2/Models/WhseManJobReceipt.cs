using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class WhseManJobReceipt
    {
        public string Barcode { get; set; }
        public string Job { get; set; }
        public string Lot { get; set; }
        public decimal Quantity { get; set; }
        public string PalletNo { get; set; }
    }
}