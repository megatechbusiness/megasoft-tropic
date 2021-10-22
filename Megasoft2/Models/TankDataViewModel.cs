using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class TankDataViewModel
    {
        public List<TankLevels> lstTankLevels { get; set; }
        public List<mtTankLevelStaging> tankData { get; set; }

    }
}