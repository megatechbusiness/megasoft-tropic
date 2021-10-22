using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionAuditsViewModel
    {
        
        public List<sp_GetRequisitionAudit_Result> Audit { get; set; }

        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public string FilterText { get; set; }
        public string FilterText2 { get; set; }
        
    }
}