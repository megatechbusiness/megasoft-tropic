//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Megasoft2
{
    using System;
    using System.Collections.Generic;
    
    public partial class InvWhControl
    {
        public InvWhControl()
        {
            this.GrnDetails = new HashSet<GrnDetail>();
            this.InvStockTakes = new HashSet<InvStockTake>();
            this.InvWarehouses = new HashSet<InvWarehouse>();
            this.InvWhBins = new HashSet<InvWhBin>();
            this.LotDetails = new HashSet<LotDetail>();
            this.PorMasterDetails = new HashSet<PorMasterDetail>();
            this.PorMasterHdrs = new HashSet<PorMasterHdr>();
            this.WipJobAllMats = new HashSet<WipJobAllMat>();
            this.WipMasters = new HashSet<WipMaster>();
            this.BomStructures = new HashSet<BomStructure>();
        }
    
        public string Warehouse { get; set; }
        public string Description { get; set; }
        public decimal MtdTrnsVar { get; set; }
        public decimal NextTicketNo { get; set; }
        public string StockTakeFlag { get; set; }
        public string WhLdgCtlAcc { get; set; }
        public string WhVarLdg { get; set; }
        public decimal TrnMonth { get; set; }
        public decimal TrnYear { get; set; }
        public string GrnLdgAcc { get; set; }
        public string DeliveryAddr1 { get; set; }
        public string DeliveryAddr2 { get; set; }
        public string DeliveryAddr3 { get; set; }
        public string DeliveryAddr3Loc { get; set; }
        public string DeliveryAddr4 { get; set; }
        public string DeliveryAddr5 { get; set; }
        public string PostalCode { get; set; }
        public decimal DeliveryGpsLat { get; set; }
        public decimal DeliveryGpsLong { get; set; }
        public string NegStockAllow { get; set; }
        public string GtrCtlAccl { get; set; }
        public string GtrPrefix { get; set; }
        public decimal GtrNextNo { get; set; }
        public string Branch { get; set; }
        public string Area { get; set; }
        public string FaxContact { get; set; }
        public string Fax { get; set; }
        public string FaxDocIwsFlag { get; set; }
        public string GitAdjAcc { get; set; }
        public string WhForComp { get; set; }
        public string Route { get; set; }
        public string InclPlanning { get; set; }
        public string UseMultipleBins { get; set; }
        public string CostingMethod { get; set; }
        public string UseFifoBuckets { get; set; }
        public string PoPrefix { get; set; }
        public decimal PoNextNumber { get; set; }
        public string GrnPrefix { get; set; }
        public decimal GrnNextNumber { get; set; }
        public string WipInspectGlCode { get; set; }
        public string Nationality { get; set; }
        public string SoDefaultDoc { get; set; }
        public string RouteCode { get; set; }
        public decimal RouteDistance { get; set; }
        public string WipControlGlCode { get; set; }
        public string WipVarCtlGlCode { get; set; }
        public string WipAutoVarGlCode { get; set; }
        public string SiteId { get; set; }
        public string RollParentGl { get; set; }
        public string WmsActive { get; set; }
        public decimal PickLeadHours { get; set; }
        public string StockTakeUpdate { get; set; }
        public string NonStockedGl { get; set; }
        public string State { get; set; }
        public string CountyZip { get; set; }
        public string City { get; set; }
        public string LanguageCode { get; set; }
        public string ShippingLocation { get; set; }
        public byte[] TimeStamp { get; set; }
        public string DeliveryTerms { get; set; }
        public string UseFixedBins { get; set; }
        public string UsePicking { get; set; }
        public string ShortagesMethod { get; set; }
        public string LostFoundWarehouse { get; set; }
        public string SiteCode { get; set; }
        public string ShiftCode { get; set; }
    
        public virtual BomRoute BomRoute { get; set; }
        public virtual ICollection<GrnDetail> GrnDetails { get; set; }
        public virtual ICollection<InvStockTake> InvStockTakes { get; set; }
        public virtual ICollection<InvWarehouse> InvWarehouses { get; set; }
        public virtual ICollection<InvWhBin> InvWhBins { get; set; }
        public virtual ICollection<LotDetail> LotDetails { get; set; }
        public virtual ICollection<PorMasterDetail> PorMasterDetails { get; set; }
        public virtual ICollection<PorMasterHdr> PorMasterHdrs { get; set; }
        public virtual ICollection<WipJobAllMat> WipJobAllMats { get; set; }
        public virtual ICollection<WipMaster> WipMasters { get; set; }
        public virtual ICollection<BomStructure> BomStructures { get; set; }
    }
}
