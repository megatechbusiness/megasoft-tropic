using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PalletAuditViewModel
    {
        public string Increase { get; set; }
        public string Warehouse { get; set; }
        public string Reference { get; set; }
        public string StockCode { get; set; }
        public List<sp_PalletOrderReport_Result> Detail { get; set; }
    }
}