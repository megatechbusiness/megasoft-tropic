using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WhseManBackOrderReleaseViewModel
    {
        public List<sp_GetBackOrderReleaseItems_Result> ReleaseItems { get; set; }
        public List<sp_GetBackOrderReleaseLines_Result> SoLines { get; set; }
        public string MStockDes { get;  set; }
        public string MStockCode { get;  set; }
        public string MWarehouse { get;  set; }
        public string MStockingUom { get;  set; }
        public decimal? MOrderQty { get;  set; }
        public decimal? MBackOrderQty { get;  set; }
        public string SalesOrder { get; set; }
        public int Line { get;  set; }

    }
}