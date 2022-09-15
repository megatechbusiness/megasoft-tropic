﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class MegasoftEntities : DbContext
    {
        public MegasoftEntities()
            : base("name=MegasoftEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<mtRole> mtRoles { get; set; }
        public DbSet<mtRoleFunction> mtRoleFunctions { get; set; }
        public DbSet<mtUserRole> mtUserRoles { get; set; }
        public DbSet<mtUserCompany> mtUserCompanies { get; set; }
        public DbSet<mtTankMaster> mtTankMasters { get; set; }
        public DbSet<mtTankMovementStaging> mtTankMovementStagings { get; set; }
        public DbSet<mtTankProductHistory> mtTankProductHistories { get; set; }
        public DbSet<mtTankLevelStaging> mtTankLevelStagings { get; set; }
        public DbSet<mtOpBranchAccess> mtOpBranchAccesses { get; set; }
        public DbSet<mtUserGlCodeAccess> mtUserGlCodeAccesses { get; set; }
        public DbSet<mtUserProductClassLimit> mtUserProductClassLimits { get; set; }
        public DbSet<mtUserWarehouse> mtUserWarehouses { get; set; }
        public DbSet<mtSmartScanType> mtSmartScanTypes { get; set; }
        public DbSet<mtSmartScanMatrix> mtSmartScanMatrices { get; set; }
        public DbSet<mtDelayedPostingWarehouse> mtDelayedPostingWarehouses { get; set; }
        public DbSet<mtUserPrinterAccess> mtUserPrinterAccesses { get; set; }
        public DbSet<mtUserScaleAccess> mtUserScaleAccesses { get; set; }
        public DbSet<mtUserDepartment> mtUserDepartments { get; set; }
        public DbSet<mtUser> mtUsers { get; set; }
        public DbSet<mtUserReportAccess> mtUserReportAccesses { get; set; }
        public DbSet<mtOpFunction> mtOpFunctions { get; set; }
        public DbSet<mtSystemSetting> mtSystemSettings { get; set; }
        public DbSet<mtProgramFunction> mtProgramFunctions { get; set; }
        public DbSet<mtUserPreference> mtUserPreferences { get; set; }
        public DbSet<mtBuyerRequisitioner> mtBuyerRequisitioners { get; set; }
        public DbSet<mtUserBuyerStat> mtUserBuyerStats { get; set; }
        public DbSet<mtSysproAdmin> mtSysproAdmins { get; set; }
        public DbSet<mtReqCostCentre> mtReqCostCentres { get; set; }
        public DbSet<mtLabelPrinter> mtLabelPrinters { get; set; }
        public DbSet<mtReqRoutingTracking> mtReqRoutingTrackings { get; set; }
        public DbSet<mtReqUserCostCentreSpendLimit> mtReqUserCostCentreSpendLimits { get; set; }
        public DbSet<mtEmailSetting> mtEmailSettings { get; set; }
        public DbSet<mtDistributionSetup> mtDistributionSetups { get; set; }
    
        public virtual ObjectResult<sp_GetRoleAccess_Result> sp_GetRoleAccess(string role)
        {
            var roleParameter = role != null ?
                new ObjectParameter("Role", role) :
                new ObjectParameter("Role", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetRoleAccess_Result>("sp_GetRoleAccess", roleParameter);
        }
    
        public virtual ObjectResult<sp_GetUserRole_Result> sp_GetUserRole(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetUserRole_Result>("sp_GetUserRole", usernameParameter);
        }
    
        public virtual ObjectResult<sp_GetUserCompanies_Result> sp_GetUserCompanies(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetUserCompanies_Result>("sp_GetUserCompanies", usernameParameter);
        }
    
        public virtual int sp_UpdateBlendMassAndDensity(Nullable<System.Guid> gUID)
        {
            var gUIDParameter = gUID.HasValue ?
                new ObjectParameter("GUID", gUID) :
                new ObjectParameter("GUID", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_UpdateBlendMassAndDensity", gUIDParameter);
        }
    
        public virtual ObjectResult<sp_GetOpFunctionAccess_Result> sp_GetOpFunctionAccess(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetOpFunctionAccess_Result>("sp_GetOpFunctionAccess", usernameParameter);
        }
    
        public virtual int sp_DeleteUserGlCodeAccess(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserGlCodeAccess", companyParameter, usernameParameter);
        }
    
        public virtual int sp_DeleteUserBranchAccess(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserBranchAccess", companyParameter, usernameParameter);
        }
    
        public virtual int sp_DeleteUserSecurityAccess(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserSecurityAccess", usernameParameter);
        }
    
        public virtual int sp_DeleteUserSpendLimits(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserSpendLimits", companyParameter, usernameParameter);
        }
    
        public virtual int sp_DeleteUserBuyerStats(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserBuyerStats", usernameParameter);
        }
    
        public virtual ObjectResult<sp_GetUserBuyerStats_Result> sp_GetUserBuyerStats(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetUserBuyerStats_Result>("sp_GetUserBuyerStats", usernameParameter);
        }
    
        public virtual ObjectResult<sp_GetBuyerRequisitionerLink_Result> sp_GetBuyerRequisitionerLink(string buyer)
        {
            var buyerParameter = buyer != null ?
                new ObjectParameter("Buyer", buyer) :
                new ObjectParameter("Buyer", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetBuyerRequisitionerLink_Result>("sp_GetBuyerRequisitionerLink", buyerParameter);
        }
    
        public virtual int sp_DeleteBuyerRequisitionerLink(string buyer)
        {
            var buyerParameter = buyer != null ?
                new ObjectParameter("Buyer", buyer) :
                new ObjectParameter("Buyer", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteBuyerRequisitionerLink", buyerParameter);
        }
    
        public virtual int sp_DeleteUserWarehouse(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserWarehouse", companyParameter, usernameParameter);
        }
    
        public virtual ObjectResult<sp_UpdateInvoicingPreference_Result> sp_UpdateInvoicingPreference(string username, string company, string startIndex, string endIndex)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var startIndexParameter = startIndex != null ?
                new ObjectParameter("StartIndex", startIndex) :
                new ObjectParameter("StartIndex", typeof(string));
    
            var endIndexParameter = endIndex != null ?
                new ObjectParameter("EndIndex", endIndex) :
                new ObjectParameter("EndIndex", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_UpdateInvoicingPreference_Result>("sp_UpdateInvoicingPreference", usernameParameter, companyParameter, startIndexParameter, endIndexParameter);
        }
    
        public virtual int sp_DeleteDelayedPostingWarehouse(string company)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteDelayedPostingWarehouse", companyParameter);
        }
    
        public virtual ObjectResult<sp_GetUserPrinters_Result> sp_GetUserPrinters(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetUserPrinters_Result>("sp_GetUserPrinters", companyParameter, usernameParameter);
        }
    
        public virtual int sp_DeleteUserPrinters(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserPrinters", companyParameter, usernameParameter);
        }
    
        public virtual int sp_SavePrinterAccess(string company, string username, string printerName)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            var printerNameParameter = printerName != null ?
                new ObjectParameter("PrinterName", printerName) :
                new ObjectParameter("PrinterName", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_SavePrinterAccess", companyParameter, usernameParameter, printerNameParameter);
        }
    
        public virtual int sp_SaveScaleAccess(string company, string username, Nullable<int> scaleModelId)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            var scaleModelIdParameter = scaleModelId.HasValue ?
                new ObjectParameter("ScaleModelId", scaleModelId) :
                new ObjectParameter("ScaleModelId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_SaveScaleAccess", companyParameter, usernameParameter, scaleModelIdParameter);
        }
    
        public virtual int sp_DeleteUserScales(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserScales", companyParameter, usernameParameter);
        }
    
        public virtual int sp_DeleteUserDepartment(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserDepartment", companyParameter, usernameParameter);
        }
    
        public virtual int sp_SaveDepartmentAccess(string company, string username, string department)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            var departmentParameter = department != null ?
                new ObjectParameter("Department", department) :
                new ObjectParameter("Department", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_SaveDepartmentAccess", companyParameter, usernameParameter, departmentParameter);
        }
    
        public virtual int sp_DeleteUserReport(string company, string username)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_DeleteUserReport", companyParameter, usernameParameter);
        }
    
        public virtual int sp_SaveReportAccess(string company, string username, string program, string report)
        {
            var companyParameter = company != null ?
                new ObjectParameter("Company", company) :
                new ObjectParameter("Company", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            var programParameter = program != null ?
                new ObjectParameter("Program", program) :
                new ObjectParameter("Program", typeof(string));
    
            var reportParameter = report != null ?
                new ObjectParameter("Report", report) :
                new ObjectParameter("Report", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_SaveReportAccess", companyParameter, usernameParameter, programParameter, reportParameter);
        }
    
        public virtual ObjectResult<sp_GetSmartScanGuidDetail_Result1> sp_GetSmartScanGuidDetail(string smartId)
        {
            var smartIdParameter = smartId != null ?
                new ObjectParameter("SmartId", smartId) :
                new ObjectParameter("SmartId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetSmartScanGuidDetail_Result1>("sp_GetSmartScanGuidDetail", smartIdParameter);
        }
    
        public virtual ObjectResult<sp_GetOpFunctionMenu_Result> sp_GetOpFunctionMenu(string username)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetOpFunctionMenu_Result>("sp_GetOpFunctionMenu", usernameParameter);
        }
    }
}
