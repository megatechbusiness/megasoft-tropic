using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class BuyerStatsViewModel
    {
        public List<sp_GetBuyerStatsBranchList_Result> Branch { get; set; }
        public List<sp_GetBuyerStatsBuyerList_Result> Buyer { get; set; }
        public Nullable<System.DateTime> FromDate { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }

        public List<sp_GetBuyerStats_Result> BuyerStats { get; set; }
        public List<sp_BuyerReqReport_Result> BuyerReqReport { get; set; }
        public List<sp_BuyerOutstandingPo_Result> BuyerOutstandingPo { get; set; }

        public List<sp_BuyerTurnaroundTimes_Result> TurnaroundTimes { get; set; }
        public Guid eGuid { get; set; }
        public string ReportName { get; set; }
        public List<sp_BuyerTurnaroundTimesDetail_Result> BuyerTurnaroundDetail { get; set; }
        public string TurnaroundDetailReport { get; set; }
    }
}