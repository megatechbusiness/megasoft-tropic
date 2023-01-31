using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Megasoft2.ViewModel
{
    public class PalletMatIssueViewModel
    {
        public string Job { get; set; }
        public string JobDescription { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public decimal QtyToMake { get; set; }
        public decimal QtyManufactured { get; set; }

        //public List<mt_PalletMatIssueGetJobDetails_Result> JobList { get; set; }
        public string Messages { get; set; }
        public string Pallet { get; set; }
        //public int SelectedRow { get; set; }
        public List<mt_PalletMatIssueGetPalletDetails_Result> PalletList { get; set; }
        //public class PalletData
        //{
        //    public bool checkedField { get; set; }
        //    public mt_PalletMatIssueGetPalletDetails_Result Pallet { get; set; }

        //    public PalletData(bool checkedField, mt_PalletMatIssueGetPalletDetails_Result pallet)
        //    {
        //        this.checkedField = checkedField;
        //        Pallet = pallet;
        //    }
        //}

        //public List<PalletData> PalletList { get; set; }

    }
}