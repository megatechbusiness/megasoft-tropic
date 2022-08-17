using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.ViewModel
{
    public class EmailSettingsViewModel 
    {
        public List<mtEmailSetting> EmailSettingsList { get; set; }
        public string EmailProgram { get; set; }
        public string SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string FromAddress { get; set; }
        public string FromAddressPassword { get; set; }
        public bool? EmailEnableSsl { get; set; }

    }
}
