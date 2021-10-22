using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class BeeSupplier
    {
        public string ExpiryDate { get; set; }
        public Nullable<decimal> PurchaseValue { get; set; }
        public string FilterText { get; set; }
        public List<sp_GetBeeSuppliers_Result> Detail { get; set; }
    }
}