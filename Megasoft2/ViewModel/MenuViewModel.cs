using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class MenuViewModel
    {
        public List<sp_GetOpFunctionMenu_Result> Detail { get; set; }
        public List<MenuHeader> Header { get; set; }

        public class MenuHeader
        {
            public string Menu { get; set; }
            public string Icon { get; set; }
            public int Sequence { get; set; }
        }
    }
}