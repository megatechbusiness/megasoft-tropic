using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.ViewModel
{
    public class LabourPostViewModel
    {
        public string Shift { get; set; }
        public string Date { get; set; }
        public string WorkCentre { get; set; }
        public string Job { get; set; }
        public decimal ProductionQty { get; set; }
        public decimal TotalScrap { get; set; }
        public string JobDescription { get; set; }
        public List<sp_GetCostCentreControl_Result> CostCentres { get; set; }
        public List<sp_GetReversalJobsInfo_Result> JobLines { get; set; }
        public IEnumerable<SelectListItem> myJobList { get; set; }
    }
}