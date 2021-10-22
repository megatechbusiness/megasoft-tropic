using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Megasoft2
{
    public partial class StockTakeEntities : DbContext
    {
        public StockTakeEntities(string s = "")//s used to override original generated DbContext Class
            : base(DatabaseSwitcher.Connstr("StockTakeEntities"))
        {
        }
    }
}