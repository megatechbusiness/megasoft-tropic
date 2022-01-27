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
                        var AppSettings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                        if (AppSettings.DispatchNoteVerification == true)
                        {
                            item.DispatchNote = item.DispatchNote.PadLeft(15, '0');
                        }

                        bool addOk = true;

                        //Check if lot No matches barcode
                        var LotNoCheck = (from a in wdb.mt_GetDetailLot(item.DispatchNote) where a.Lot == item.LotNumber select a).FirstOrDefault();

                        string Lot = "";
                        string Pallet = "";
                        string DispatchNote = "";
                        decimal DispatchNoteLine = 0;
                        string SalesOrder = "";
                        decimal SalesOrderLine = 0;
                        decimal StockQtyToShip = 0;

                        if (LotNoCheck == null)
                        {
                            var PalletCheck = (from a in wdb.mt_GetDetailLot(item.DispatchNote) where a.PalletId == item.LotNumber select a).FirstOrDefault();
                            if(PalletCheck == null)
                            {
                                return "Error: Item scanned does not exist. " + item.LotNumber;
                            }
                            else
                            {
                                //Lot = PalletCheck.Lot;
                                Pallet = item.LotNumber;
                                DispatchNote = PalletCheck.DispatchNote;
                                DispatchNoteLine = PalletCheck.DispatchNoteLine;
                                SalesOrder = PalletCheck.SalesOrder;
                                SalesOrderLine = PalletCheck.SalesOrderLine;
                                StockQtyToShip = PalletCheck.StockQtyToShip;
                            }
                        }
                        else
                        {
                            Lot = LotNoCheck.Lot;
                            Pallet = LotNoCheck.PalletId;
                            DispatchNote = LotNoCheck.DispatchNote;
                            DispatchNoteLine = LotNoCheck.DispatchNoteLine;
                            SalesOrder = LotNoCheck.SalesOrder;
                            SalesOrderLine = LotNoCheck.SalesOrderLine;
                            StockQtyToShip = LotNoCheck.StockQtyToShip;
                        }

                        if (string.IsNullOrEmpty(item.DispatchNote))
                        {
                            addOk = false;
                            return "Please scan Dispatch Note.";
                        }



                        //if (Lot != item.LotNumber)
                        //{
                        //    addOk = false;
                        //    return "Error: Lot does not exist. " + item.LotNumber;
                        //}

                        // check if dispatch note exists
                        if (DispatchNote != item.DispatchNote)
                        {
                            addOk = false;
                            return "Dispatch Note does not exist. Please scan an existing Dispatch Note.";
                        }


                        //check if item is aready scanned
                        var trackid = (from a in wdb.mtTransportWaybillDetails where a.DispatchNote == item.DispatchNote select a.TrackId).FirstOrDefault();

                        if (Lot == "")
                        {
                            var scannedItem = (from a in wdb.mtDispatchVerifications where a.TrackId == trackid && a.DispatchNote == item.DispatchNote && a.Pallet == Pallet select a).ToList();

                            if (scannedItem.Count > 0)
                            {
                                addOk = false;
                                return "Error: Item " + Lot + " has already been scanned.";
                            }
                        }
                        else
                        {
                            var scannedItem = (from a in wdb.mtDispatchVerifications where a.TrackId == trackid && a.DispatchNote == item.DispatchNote && a.Lot == Lot select a).ToList();

                            if (scannedItem.Count > 0)
                            {
                                addOk = false;
                                return "Error: Item " + Lot + " has already been scanned.";
                            }
                        }

                        //Flag as Scanned - checks passed
                        if (addOk)
                        {
                            mtDispatchVerification dv = new mtDispatchVerification();
                            dv.TrackId = trackid;
                            dv.DispatchNote = DispatchNote;
                            dv.DispatchNoteLine = DispatchNoteLine;
                            dv.SalesOrder = SalesOrder;
                            dv.SalesOrderLine = SalesOrderLine;
                            dv.Lot = Lot;
                            dv.StockQtyToShip = StockQtyToShip;
                            dv.Pallet = Pallet;
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
                    foreach (var item in myDeserializedObjList)
                    {
                        var AppSettings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                        if (AppSettings.DispatchNoteVerification == true)
                        {
                            item.DispatchNote = item.DispatchNote.PadLeft(15, '0');
                        }

                        var complete = (from a in wdb.mtDispatchVerifications where a.TrackId == item.TrackId && a.DispatchNote == item.DispatchNote && a.VerificationComplete == "N" select a).ToList();

                        if (complete != null)
                        {
                            foreach (var i in complete)
                            {
                                i.VerificationComplete = "Y";
                                wdb.Entry(i).State = System.Data.EntityState.Modified;
                                wdb.SaveChanges();

                                
                            }
                            return "Verification successful " + item.DispatchNote + " ";
                        }
                        else
                        {
                            return "Error: Verification failed. " + item.DispatchNote + " ";
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


        public string ValidateDispatchNote(string DispatchNote, int TrackId)
        {
            try
            {
                var AppSettings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                if (AppSettings.DispatchNoteVerification == true)
                {
                    DispatchNote = DispatchNote.PadLeft(15, '0');
                }

                var dNote = wdb.mt_GetDispatchNote(DispatchNote).FirstOrDefault();
                var trackid = (from a in wdb.mtTransportWaybillDetails where a.DispatchNote == DispatchNote && a.TrackId == TrackId select a.TrackId).ToList();

                    // check if dispatch note exists
                    if (dNote == null)
                    {
                        return "Error: Dispatch Note " + DispatchNote + " does not exist.";
                    }
                    
                    // check if trackid exists
                    if (trackid.Count == 0)
                    {
                        return "Error: Track ID " + TrackId + " does not exist.";
                    }
            }
               
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }

            return "";
        }

        public List<mtDispatchVerification> GetScansByTrackIdDispatch(string DispatchNote, int TrackId)
        {
            try
            {
                var AppSettings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                if (AppSettings.DispatchNoteVerification == true)
                {
                    DispatchNote = DispatchNote.PadLeft(15, '0');
                }

                return (from a in wdb.mtDispatchVerifications where a.DispatchNote == DispatchNote && a.TrackId == TrackId select a).OrderByDescending(a => a.ScanDate).ToList();                
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }

        public string DeleteScanByTrackIdDispatch(string details)
        {
            try
            {
                List<mtDispatchVerification> myDeserializedObjList = (List<mtDispatchVerification>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtDispatchVerification>));
                foreach (var item in myDeserializedObjList)
                {
                    var AppSettings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                    if (AppSettings.DispatchNoteVerification == true)
                    {
                        item.DispatchNote = item.DispatchNote.PadLeft(15, '0');
                    }

                    if (item.Lot == "")
                    {
                        var GetItemToDelete = (from a in wdb.mtDispatchVerifications where a.TrackId == item.TrackId && a.DispatchNote == item.DispatchNote && a.Pallet == item.Pallet select a).FirstOrDefault();
                        wdb.Entry(GetItemToDelete).State = System.Data.EntityState.Deleted;
                        wdb.SaveChanges();
                    }
                    else
                    {
                        var GetItemToDelete = (from a in wdb.mtDispatchVerifications where a.TrackId == item.TrackId && a.DispatchNote == item.DispatchNote && a.Lot == item.Lot select a).FirstOrDefault();
                        wdb.Entry(GetItemToDelete).State = System.Data.EntityState.Deleted;
                        wdb.SaveChanges();
                    }
                    
                }
                return "Deleted Successfully.";
            }
            catch (Exception ex)
            {

                throw new Exception("Error: " + ex.Message);
            }
        }
    }
}