using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class StereoSystemGrnViewModel
    {
        public string PurchaseOrder { get; set; }
        public string Invoice { get; set; }
        public string Grn { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public List<sp_StereoPoLinesForGrn_Result> PoLine { get; set; }
        public decimal GrnAmountSelected { get; set; }
    }
}