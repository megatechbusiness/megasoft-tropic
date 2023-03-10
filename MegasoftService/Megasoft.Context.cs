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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
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
    
        public virtual DbSet<mtSystemSetting> mtSystemSettings { get; set; }
        public virtual DbSet<mtUser> mtUsers { get; set; }
    
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
    }
}
