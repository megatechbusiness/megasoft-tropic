using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class Group<T, K>
    {
        public K Key {get; set;}
        public IEnumerable<T> Values { get; set; }
    }
}