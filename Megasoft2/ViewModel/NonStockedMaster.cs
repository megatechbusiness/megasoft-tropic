using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.ViewModel
{
    public class NonStockedMaster
    {
        public mtInvMaster InvMaster { get; set; }
        public List<sp_GetBranchByStockCode_Result> InvBranch { get; set; }
        public List<sp_GetContractPricingByStockCode_Result> Contract { get; set; }
        public string StockedItem { get; set; }
        public List<sp_GetWarehouseByStockCode_Result> InvWarehouse { get; set; }
        public List<sp_GetStockCodePurchaseHistory_Result> StockCodeSupplier { get; set; }
        public string Code { get; set; }
        public string NewStockCode { get; set; }
    }
}