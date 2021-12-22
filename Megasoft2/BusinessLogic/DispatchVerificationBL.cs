using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class DispatchVerificationBL
    {
        private SysproCore objSyspro = new SysproCore();
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();

        public string GetCompany()
        {
            HttpCookie database = HttpContext.Current.Request.Cookies.Get("SysproDatabase");
            var company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            return company;
        }

        public string GetUsername()
        {
            var Username = HttpContext.Current.User.Identity.Name.ToUpper();
            return Username;
        }

        public mtWhseManSetting GetSettings()
        {
            var settings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
            return settings;
        }

        public MegasoftEntities GetMegasoftEntities()
        {
            return mdb;
        }

        public WarehouseManagementEntities GetWarehouseManagementEntities()
        {
            return wdb;
        }

        public string ValidateBarcode(string details)
        {
            try
            {
                List<DispatchVerification> myDeserializedObjList = (List<DispatchVerification>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<DispatchVerification>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        bool addOk = true;

                        //Check if lot No matches barcode
                        var LotNoCheck = wdb.mt_GetDispatchNote(item.DispatchNote).ToList();
                        var DispNoteCheck = wdb.mt_GetDetailLot(item.DispatchNote).ToList().FirstOrDefault();

                        if (string.IsNullOrEmpty(item.DispatchNote))
                        {
                            addOk = false;
                            return "Please scan Dispatch Note.";
                        }

                        if (LotNoCheck == null)
                        {
                            addOk = false;
                            return "Error: Lot does not exist. " + item.LotNumber;
                        }

                        // check if dispatch note exists
                        if (LotNoCheck == null)
                        {
                            addOk = false;
                            return "Dispatch Note does not exist. Please scan an existing Dispatch Note.";
                        }

                        item.DispatchNote = DispNoteCheck.DispatchNote;

                        //Flag as Scanned - checks passed
                        if (addOk)
                        {
                            mtDispatchVerification dv = new mtDispatchVerification();
                            dv.DispatchNote = DispNoteCheck.DispatchNote;
                            dv.DispatchNoteLine = DispNoteCheck.DispatchNoteLine;
                            dv.SalesOrder = DispNoteCheck.SalesOrder;
                            dv.SalesOrderLine = DispNoteCheck.SalesOrderLine;
                            dv.Lot = DispNoteCheck.Lot;
                            dv.StockQtyToShip = DispNoteCheck.StockQtyToShip;
                            dv.Username = GetUsername();
                            dv.ScanDate = DateTime.Now;
                            dv.VerificationComplete = "N";
                            wdb.Entry(dv).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();

                            return "Dispatch Note " + item.DispatchNote + " added succesfully. Click Complete.";
                        }

                        else
                        {
                            return "Error: Dispatch Note " + item.DispatchNote + " save failed.";
                        }


                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }

        public string btnComplete(string details)
        {
            try
            {
                List<DispatchVerification> myDeserializedObjList = (List<DispatchVerification>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<DispatchVerification>));
                if (myDeserializedObjList.Count > 0)
                {
                    var complete = (from a in wdb.mtDispatchVerifications where a.VerificationComplete == "N" select a).FirstOrDefault();

                    if (complete != null)
                    {
                        complete.VerificationComplete = "Y";
                        wdb.Entry(complete).State = System.Data.EntityState.Modified;
                        wdb.SaveChanges();

                        return "Verification successful" + complete.DispatchNote;
                    }
                    else
                    {
                        return "Error: Verification failed." + complete.DispatchNote;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }


        public string ValidateDispatchNote(string DispatchNote)
        {
            try
            {
                    var dNote = wdb.mt_GetDispatchNote(DispatchNote).ToList().FirstOrDefault();

                    // check if dispatch note exists
                    if (dNote == null)
                    {
                        return "Error: Dispatch Note" + DispatchNote + "does not exist.";
                    }
            }
               
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }

            return "";
        }
    }
}