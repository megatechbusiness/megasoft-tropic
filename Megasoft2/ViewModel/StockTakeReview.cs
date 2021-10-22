using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class StockTakeReview
    {
        public string Warehouse { get; set; }
        public List<sp_GetStockReview_Result> Stock { get; set; }
        public string Reference { get; set; }
        public string Increase { get; set; }
    }
}