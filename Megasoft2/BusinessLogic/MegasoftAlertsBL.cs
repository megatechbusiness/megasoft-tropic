using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class MegasoftAlertsBL
    {
        WarehouseManagementEntities sdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        public void SaveMegasoftAlert(string AlertMessage)
        {
            try
            {
                HttpCookie database = HttpContext.Current.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                mtMegasoftAlert obj = new mtMegasoftAlert();
                obj.Username = HttpContext.Current.User.Identity.Name.ToUpper();
                obj.AlertMessage = AlertMessage;
                obj.AlertSendDate = DateTime.Now;
                obj.ExpiryDate = DateTime.Now.AddMinutes(20);
                obj.AlertSent = false;
                obj.AlertType = "I";
                obj.AlertFrom = "Megasoft";
                obj.ContinuousAlert = false;
                sdb.Entry(obj).State = System.Data.EntityState.Added;
                sdb.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}