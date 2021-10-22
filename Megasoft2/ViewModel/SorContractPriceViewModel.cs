using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class SorContractPriceViewModel
    {
        public List<sp_GetSalesContractPricingForExpiry_Result> Detail { get; set; }
        public string Customer { get; set; }
        public string Contract { get; set; }
        public string StockCode { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string PriceMethod { get; set; }
        public string FixedUom { get; set; }
        public decimal Price { get; set; }
        public List<sp_GetStockCodesByCustomer_Result> CustomerListing { get; set; }
    }
}