using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class CustomerInvoiceViewModel
    {
        public List<sp_GetInvoices_Result> Invoices { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string CustomerClass { get; set; }
    }
}