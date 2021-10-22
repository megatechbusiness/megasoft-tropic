using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PoMaintenance
    {
        public List<sp_GetPurchaseOrderLines_Result> PoLines { get; set; }
        public string Supplier { get; set; }
        public string SupplierName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }
    }
}