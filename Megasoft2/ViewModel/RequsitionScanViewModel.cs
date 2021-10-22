using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequsitionScanViewModel
    {
        public string CostCentre { get; set; }
        public string StockCode { get; set; }
        public string WareHouse { get; set; }
        public string StockCodeDescription { get; set; }
        public int Quantity { get; set; }
        public string Requisition { get; set; }
        public List<sp_mtReqGetRequisitionLines_Result> Lines { get; set; }
        public List<sp_mtReqGetRouteOnUsers_Result> RouteOn { get; set; }
        public sp_mtReqGetRequisitionHeader_Result Header { get; set; }
        public List<string> Image { get; set; }
        public string ReqComment { get; set; }
        // Added
        public string Urgent { get; set; }
    }
}