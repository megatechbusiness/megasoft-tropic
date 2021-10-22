using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class CustomerOrderScheduleViewModel
    {
        public List<sp_GetSorOrderSchedule_Result> Unplanned { get; set; }
        public List<sp_GetSorOrderSchedule_Result> Planned { get; set; }
        public string Customer { get; set; }
    }
}