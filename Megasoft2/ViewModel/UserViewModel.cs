using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class UserViewModel
    {
        public mtUser User { get; set; }
        public List<sp_GetBranchAccess_Result> BranchAccess { get; set; }
        public List<sp_GetOpFunctionAccess_Result> FunctionAccess { get; set; }
        public List<sp_GetAuthorizationGlCodes_Result> Authorization { get; set; }
        public List<sp_GetProductClassLimits_Result> ProdClassLimit { get; set; }
        public List<sp_GetUserBuyerStats_Result> UserBuyerStats { get; set; }
        public List<sp_GetBuyerRequisitionerLink_Result> BuyerReq { get; set; }
        public List<sp_GetUserWarehouses_Result> Warehouses { get; set; }
        public List<sp_GetUserPrinters_Result> Printers { get; set; }
        public List<sp_GetUserScales_Result> Scales { get; set; }
        public List<sp_GetUserDepartments_Result> Departments { get; set; }
        public List<sp_GetUserReports_Result> Reports { get; set; }


    }
}