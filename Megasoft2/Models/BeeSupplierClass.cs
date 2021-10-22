using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class BeeSupplierClass
    {
        public string Supplier { get; set; }
        public string SupplierName { get; set; }
        public string EnterpriseType { get; set; }
        public string BeeLevel { get; set; }
        public Nullable<decimal> BlackOwnership { get; set; }
        public Nullable<decimal> BlackWomenOwnershi { get; set; }
        public string EmpoweringSupplier { get; set; }
        public string ExpiryDate { get; set; }
        public int PurchaseValue { get; set; }
        public Nullable<System.DateTime> ExpDate { get; set; }
    }
}