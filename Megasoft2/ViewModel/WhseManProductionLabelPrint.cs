using System.Collections.Generic;

namespace Megasoft2.ViewModel
{
    public class WhseManProductionLabelPrint
    {
        public string Printer { get; set; }

        public sp_GetProductionJobDetails_Result JobDetails { get; set; }

        public List<mtProductionLabel> LabelDetail { get; set; }

        public string Department { get; set; }
    }
}
