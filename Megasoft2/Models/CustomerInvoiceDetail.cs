using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class CustomerInvoiceDetail
    {
        public string RowNumber { get; set; }
        public string Invoice { get; set; }
        public string CustomerPoNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string SalesOrder { get; set; }
        public string CustomerRef { get; set; }
        public string CurrencyValue { get; set; }
        public string Branch { get; set; }
        public string Customer { get; set; }
        public string DocumentType { get; set; }
    }
}