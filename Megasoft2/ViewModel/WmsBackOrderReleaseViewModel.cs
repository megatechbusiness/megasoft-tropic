using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WmsBackOrderReleaseViewModel
    {
        public List<sp_GetWmsBackOrderReleaseItems_Result> ReleaseItems { get; set; }//look at syspro serials
        public List<sp_GetWmsBackOrderReleaseLines_Result> SoLines { get; set; }//look mt
        public string MStockDes { get; set; }
        public string MStockCode { get; set; }
        public string MWarehouse { get; set; }
        public string MStockingUom { get; set; }
        public string Picker { get; set; }
        public decimal? MOrderQty { get; set; }
        public decimal? MBackOrderQty { get; set; }
        public string SalesOrder { get; set; }
        public int Line { get; set; }
        public int WmsId { get; set; }
        public string Comment { get; set; }
        public decimal SalesReleaseQty { get; set; }
        public List<sp_GetWmsDespatchItems_Result> DespatchLines { get; set; }
        public List<sp_GetWmsDespatchLines_Result> LinesForDespatch { get; set; }
    }
}