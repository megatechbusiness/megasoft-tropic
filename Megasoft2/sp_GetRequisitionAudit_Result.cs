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
    
    public partial class sp_GetRequisitionAudit_Result
    {
        public int AuditId { get; set; }
        public string Requisition { get; set; }
        public Nullable<int> Line { get; set; }
        public string TrnType { get; set; }
        public string Program { get; set; }
        public string KeyField { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Username { get; set; }
        public Nullable<System.DateTime> TrnDate { get; set; }
    }
}
