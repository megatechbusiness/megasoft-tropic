using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WhseManProductMatIssueViewModel
    {
        public string Lot { get; set; }
        public string Printer { get; set; }

        public mt_ProductionLotDetails_Result JobDetails { get; set; }

        public List<mtProductionLabel> LabelDetail { get; set; }

        public string Department { get; set; }
        public decimal? LastBatch { get; set; }
    }
}