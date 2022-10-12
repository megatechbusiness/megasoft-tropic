using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WhseManPoReceipt
    {
        public string PurchaseOrder { get; set; }
        public List<sp_GetPoLabelLines_Result> ReelLines { get; set; }
        public string DeliveryNote { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Supplier { get; set; }
        public string SupplierName { get; set; }
    }
}