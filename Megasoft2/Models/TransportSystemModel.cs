using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class TransportSystemModel
    {
        public string TrackId { get; set; }
        public string Supplier { get; set; }
        public string Waybill { get; set; }
        public string Notes { get; set; }
        public string PoQty { get; set; }
        public string PoPrice { get; set; }
        public string DispatchNote { get; set; }
        public string DispatchNoteLine { get; set; }
        public string Town { get; set; }
        public string Customer { get; set; }
        public string Weight { get; set; }
        public string LineType { get; set; }
        public string Comment { get; set; }
        public bool Taxable { get; set; }
        public string TaxCode { get; set; }

    }
}