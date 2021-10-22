using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class StockTakeImport
    {
        public string Increase { get; set; }
        public string Warehouse { get; set; }
        public string Reference { get; set; }
        public List<sp_GetStockTakeCaptureByWarehouse_Result> Detail { get; set; }
    }
}