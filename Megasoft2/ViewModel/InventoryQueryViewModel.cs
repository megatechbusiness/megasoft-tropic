using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class InventoryQueryViewModel
    {
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string StockUom { get; set; }
        public List<mt_InvQueryStockCodeDetails_Result> Items { get; set; }
    }
}