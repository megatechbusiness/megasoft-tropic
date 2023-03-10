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
    
    public partial class BomWorkCentre
    {
        public BomWorkCentre()
        {
            this.BomOperations = new HashSet<BomOperation>();
        }
    
        public string WorkCentre { get; set; }
        public string Description { get; set; }
        public string CostCentre { get; set; }
        public decimal SetUpRate1 { get; set; }
        public decimal SetUpRate2 { get; set; }
        public decimal SetUpRate3 { get; set; }
        public decimal SetUpRate4 { get; set; }
        public decimal SetUpRate5 { get; set; }
        public decimal SetUpRate6 { get; set; }
        public decimal SetUpRate7 { get; set; }
        public decimal SetUpRate8 { get; set; }
        public decimal SetUpRate9 { get; set; }
        public decimal RunTimeRate1 { get; set; }
        public decimal RunTimeRate2 { get; set; }
        public decimal RunTimeRate3 { get; set; }
        public decimal RunTimeRate4 { get; set; }
        public decimal RunTimeRate5 { get; set; }
        public decimal RunTimeRate6 { get; set; }
        public decimal RunTimeRate7 { get; set; }
        public decimal RunTimeRate8 { get; set; }
        public decimal RunTimeRate9 { get; set; }
        public decimal FixOverRate1 { get; set; }
        public decimal FixOverRate2 { get; set; }
        public decimal FixOverRate3 { get; set; }
        public decimal FixOverRate4 { get; set; }
        public decimal FixOverRate5 { get; set; }
        public decimal FixOverRate6 { get; set; }
        public decimal FixOverRate7 { get; set; }
        public decimal FixOverRate8 { get; set; }
        public decimal FixOverRate9 { get; set; }
        public decimal VarOverRate1 { get; set; }
        public decimal VarOverRate2 { get; set; }
        public decimal VarOverRate3 { get; set; }
        public decimal VarOverRate4 { get; set; }
        public decimal VarOverRate5 { get; set; }
        public decimal VarOverRate6 { get; set; }
        public decimal VarOverRate7 { get; set; }
        public decimal VarOverRate8 { get; set; }
        public decimal VarOverRate9 { get; set; }
        public decimal StartupRate1 { get; set; }
        public decimal StartupRate2 { get; set; }
        public decimal StartupRate3 { get; set; }
        public decimal StartupRate4 { get; set; }
        public decimal StartupRate5 { get; set; }
        public decimal StartupRate6 { get; set; }
        public decimal StartupRate7 { get; set; }
        public decimal StartupRate8 { get; set; }
        public decimal StartupRate9 { get; set; }
        public decimal TeardownRate1 { get; set; }
        public decimal TeardownRate2 { get; set; }
        public decimal TeardownRate3 { get; set; }
        public decimal TeardownRate4 { get; set; }
        public decimal TeardownRate5 { get; set; }
        public decimal TeardownRate6 { get; set; }
        public decimal TeardownRate7 { get; set; }
        public decimal TeardownRate8 { get; set; }
        public decimal TeardownRate9 { get; set; }
        public string TimeUom { get; set; }
        public string EtCalcMeth { get; set; }
        public string CapacityUom { get; set; }
        public decimal CstToCapFact { get; set; }
        public string CstToCapMulDiv { get; set; }
        public string ProdUnitDesc { get; set; }
        public string SubcontractFlag { get; set; }
        public string UseEmployeeRate { get; set; }
        public string ProductClass { get; set; }
        public string RunTime { get; set; }
        public string SetupTime { get; set; }
        public string FixTime { get; set; }
        public string VarTime { get; set; }
        public string StartTime { get; set; }
        public string TeardownTime { get; set; }
        public string NonRunTime { get; set; }
        public string NonSetupTime { get; set; }
        public string NonFixTime { get; set; }
        public string NonVarTime { get; set; }
        public string NonStartTime { get; set; }
        public string NonTearTime { get; set; }
        public decimal PtdStdHours { get; set; }
        public decimal PtdNonHours { get; set; }
        public decimal PtdActHours { get; set; }
        public decimal MtdStdHours { get; set; }
        public decimal MtdNonHours { get; set; }
        public decimal MtdActHours { get; set; }
        public decimal YtdStdHours { get; set; }
        public decimal YtdNonHours { get; set; }
        public decimal YtdActHours { get; set; }
        public decimal UtilisePct { get; set; }
        public decimal OvertimeCapPct { get; set; }
        public string UseOvertimeCap { get; set; }
        public decimal NormalCapacity { get; set; }
        public decimal QueueTime { get; set; }
        public string DefSubSupplier { get; set; }
        public string DefSubPlanner { get; set; }
        public string DefSubBuyer { get; set; }
        public decimal WorkOperators { get; set; }
        public decimal ProdUnits { get; set; }
        public decimal TimePerUnit { get; set; }
        public string CalcCapConvFact { get; set; }
        public decimal LoadLevelScale { get; set; }
        public string LoadLevelSclFlg { get; set; }
        public string CellId { get; set; }
        public string ChargeCode { get; set; }
        public string OpSplitFlag { get; set; }
        public byte[] TimeStamp { get; set; }
        public string NestScrap { get; set; }
        public string WipBranch { get; set; }
        public string SiteCode { get; set; }
        public string ShiftCode { get; set; }
    
        public virtual BomCostCentre BomCostCentre { get; set; }
        public virtual SalProductClassDe SalProductClassDe { get; set; }
        public virtual ICollection<BomOperation> BomOperations { get; set; }
    }
}
