using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PeriodControlViewModel
    {
        public int FinYear { get; set; }
        public List<sp_GetPeriodControlSetup_Result> Periods { get; set; }
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
    }
}