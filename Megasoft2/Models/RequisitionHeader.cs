using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Models
{
    public class RequisitionHeader
    {
        public string Requisition { get; set; }
        public Nullable<System.DateTime> RequisitionDate { get; set; }
        public string Requisitioner { get; set; }
        public IEnumerable<SelectListItem> Branch { get; set; }
        public string SelectedBranch { get; set; }
        public string Site { get; set; }
        public string Buyer { get; set; }
        public string Supplier { get; set; }
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> DueDate { get; set; }
        public string FromAddress1 { get; set; }
        public string FromAddress2 { get; set; }
        public string FromAddress3 { get; set; }
        public string FromAddress4 { get; set; }
        public string FromAddress5 { get; set; }
        public string ToAddress1 { get; set; }
        public string ToAddress2 { get; set; }
        public string ToAddress3 { get; set; }
        public string ToAddress4 { get; set; }
        public string ToAddress5 { get; set; }
        public bool Emergency { get; set; }
        public string EmergencyReason { get; set; }
        public bool Completed { get; set; }
        public Nullable<System.DateTime> CompletedDate { get; set; }
        public bool Accepted { get; set; }
        public Nullable<System.DateTime> AcceptedDate { get; set; }
        public bool QuoteReceived { get; set; }
        public Nullable<System.DateTime> QuoteReceivedDate { get; set; }
        public bool Authorised { get; set; }
        public Nullable<System.DateTime> AuthorisedDate { get; set; }
        public string AuthorisedBy { get; set; }
        public string PurchaseOrder { get; set; }
        public Nullable<System.DateTime> PoDate { get; set; }
    }
}