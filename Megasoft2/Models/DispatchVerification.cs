using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class DispatchVerification
    {
        public string Barcode { get; set; }
        public string DispatchNote { get; set; }
        public string LotNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal DispatchNoteLine { get; set; }
        public string SalesOrder { get; set; }
        public decimal SalesOrderLine { get; set; }
        public int TrackId { get; set; }
    }
}