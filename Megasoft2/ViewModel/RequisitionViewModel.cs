using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionViewModel
    {
        public List<sp_mtReqGetRequisitionList_Result> ReqList { get; set; }
        public sp_mtReqGetRequisitionHeader_Result Header { get; set; }
        public List<sp_mtReqGetRequisitionLines_Result> Lines { get; set; }
        public string LineMethod { get; set; } //Change or Add
        public sp_mtReqGetRequisitionLines_Result Line { get; set; }
        public bool StockedLine { get; set; }
        public string RouteNote { get; set; }
        public List<sp_mtReqGetRouteOnUsers_Result> RouteOn { get; set; }
        public string Requisition { get; set; }
        public bool FinalApproval { get; set; }
        public string RouteTo { get; set; }
        public string LocalCurrency { get; set; }
        public string PriceMethod { get; set; }
        public bool SubContract { get; set; }
        public string SupCatNumber { get; set; }

        public string RequestedBy1 { get; set; }
        public string RequestedBy2 { get; set; }
        public string RequestedBy3 { get; set; }
        public string PurchDepartment { get; set; }
        public string PurchaseCategory { get; set; }

        //15122020
        public mtRequisitionDeletedLine mtRequisitionDeletedLine { get; set; }
        public decimal ReqLines { get; set; }
        public string Reason { get; set; }
        public string DeleteSingleOrAll { get; set; }



    }

}