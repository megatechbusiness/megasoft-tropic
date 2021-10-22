using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class SupplierPriceColumnMapping
    {
        public string Username { get; set; }
        public string ContractReference { get; set; }
        public string Supplier { get; set; }
        public string StockCode { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string PurchasePrice { get; set; }
        public string FilePath { get; set; }
        public string SheetName { get; set; }
    }
}