//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MegasoftDelayedPostingService
{
    using System;
    using System.Collections.Generic;
    
    public partial class mtWhseManSetting
    {
        public int SettingId { get; set; }
        public bool TransferInScanItem { get; set; }
        public bool ShortagesAllowedForKits { get; set; }
        public string LabelDateFormat { get; set; }
        public bool JobLabelAddField1 { get; set; }
        public bool PalletNoReq { get; set; }
        public string DefaultPalletNo { get; set; }
        public bool PostLabour { get; set; }
        public bool PostMaterialIssue { get; set; }
        public Nullable<decimal> JobReceiptTolerance { get; set; }
        public bool SalesReleaseAutoAllocation { get; set; }
    }
}
