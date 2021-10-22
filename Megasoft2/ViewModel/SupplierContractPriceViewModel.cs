using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class SupplierContractPriceViewModel
    {
        public List<sp_GetSupplierContractsPricingData_Result> Detail { get; set; }
        public string Supplier { get; set; }
        public string Contract { get; set; }
        public string StockCode { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string PriceMethod { get; set; }
        public string FixedUom { get; set; }
        public decimal Price { get; set; }
        public List<sp_GetSupContractsStockCodesBySupplier_Result> SupplierListing { get; set; }
        public string SheetName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SelectedSheet { get; set; }
        public string DownloadFilePath { get; set; }
        public string DownloadFileName { get; set; }
    }
}