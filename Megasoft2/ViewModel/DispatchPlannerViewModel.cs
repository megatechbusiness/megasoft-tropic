using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class DispatchPlannerViewModel
    {
        public List<mt_DispatchPlanGetOrders_Result> OpenOrders { get; set; }
        public List<mt_DispatchPlanGetOrders_Result> OrderPlans { get; set; }
        public System.DateTime DispatchDate { get; set; }
        public List<string> TruckList { get; set; }
        public List<string> SaveTL { get; set; }
        public List<mtDispatchPlan> Plans { get; set; }
        public string DeliveryNo { get; set; }
        public string Messages { get; set; }
    }
}