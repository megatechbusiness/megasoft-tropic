using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class AuthorizationLimit
    {
        public string GlCode { get; set; }
        public string CostCode { get; set; }
        public string Description { get; set; }
        public decimal Limit { get; set; }
    }
}