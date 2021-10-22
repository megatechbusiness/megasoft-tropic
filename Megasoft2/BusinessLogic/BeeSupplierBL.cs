using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class BeeSupplierBL
    {
        SysproEntities sdb = new SysproEntities("");

        public void AuditBeeSupplier(string Supplier, string KeyField, string oldValue, string newValue)
        {
            try
            {
                mtBeeSupplierAudit bee = new mtBeeSupplierAudit();
                bee.Supplier = Supplier;
                bee.KeyField = KeyField;
                bee.OldValue = oldValue;
                bee.NewValue = newValue;
                bee.TrnDate = DateTime.Now;
                bee.Username = HttpContext.Current.User.Identity.Name.ToUpper();
                sdb.mtBeeSupplierAudits.Add(bee);
                sdb.SaveChanges();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}