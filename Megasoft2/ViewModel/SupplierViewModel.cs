using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Megasoft2.Models;

namespace Megasoft2.ViewModel
{
    public class SupplierViewModel
    {
        public vwApSupplier Suppliers { get; set; }

        public string SupplierCode { get; set; }
        public string isSysproSupplier { get; set; }

        public List<mtSupplierContact> SupplierContact { get; set; }

        public List<sp_GetSupplierPurchaseHistoryHeader_Result> GrnSupplier { get; set; }
        public string GetSupplier { get; set; }

    }
}