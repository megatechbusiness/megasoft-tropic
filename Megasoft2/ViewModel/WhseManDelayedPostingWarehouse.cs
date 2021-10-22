using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WhseManDelayedPostingWarehouse
    {
        public List<sp_GetDelayedPostingWarehouses_Result> Warehouse { get; set; }
        public List<sp_GetDelayedPostingErrors_Result> Errors { get; set; }
        public List<sp_GetDelayedPostingCueData_Result> Cue { get; set; }
    }
}