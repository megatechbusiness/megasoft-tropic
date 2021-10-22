using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class TransportSystemPvJournalViewModel
    {
        public List<sp_GetTransPvJournal_Result> Detail { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Reference { get; set; }
    }
}