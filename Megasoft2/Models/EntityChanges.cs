using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class EntityChanges
    {
        public string KeyField { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}