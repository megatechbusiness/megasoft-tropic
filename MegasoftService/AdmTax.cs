//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MegasoftService
{
    using System;
    using System.Collections.Generic;
    
    public partial class AdmTax
    {
        public AdmTax()
        {
            this.PorMasterDetails = new HashSet<PorMasterDetail>();
            this.GenMasters = new HashSet<GenMaster>();
        }
    
        public string TaxCode { get; set; }
        public string Description { get; set; }
        public decimal CurTaxRate { get; set; }
        public decimal PrvTaxRate { get; set; }
        public Nullable<System.DateTime> TaxEffDate { get; set; }
        public string TaxBasis { get; set; }
        public string TaxNeutralBasis { get; set; }
        public string TaxPriceCode { get; set; }
        public string TaxIncExcl { get; set; }
        public string GlTaxGlCode { get; set; }
        public string ApTaxGlCode { get; set; }
        public decimal PtdTotExempt { get; set; }
        public decimal PtdTotNonExempt { get; set; }
        public decimal PtdTotTaxAmt { get; set; }
        public decimal MtdTotExempt { get; set; }
        public decimal MtdTotNonExempt { get; set; }
        public decimal MtdTotTaxAmt { get; set; }
        public decimal YtdTotExempt { get; set; }
        public decimal YtdTotNonExempt { get; set; }
        public decimal YtdTotTaxAmt { get; set; }
        public decimal TaxRateDecimals { get; set; }
        public string UseGstFlag { get; set; }
        public decimal PtdGst { get; set; }
        public decimal YtdGst { get; set; }
        public byte[] TimeStamp { get; set; }
    
        public virtual ICollection<PorMasterDetail> PorMasterDetails { get; set; }
        public virtual ICollection<GenMaster> GenMasters { get; set; }
    }
}
