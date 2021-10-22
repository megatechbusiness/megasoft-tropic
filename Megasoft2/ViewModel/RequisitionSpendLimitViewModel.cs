using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionSpendLimitViewModel
    {
        public mtReqUserCostCentreSpendLimit UserSpendLimit { get; set; }
        public List<mtReqUserCostCentreSpendLimit> UserSpendLimitDetail { get; set; }
    }
}