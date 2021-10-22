using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class TransportSystemCustomerEmail
    {
        public string Customer { get; set; }
        public string EmailTo { get; set; }
        public string EmailFrom { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
        public string CustomerName { get; set; }
        public string Location { get; set; }
    }
}