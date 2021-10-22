using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class SysproStockTakeOn
    {
        //WarehouseManagementEntities db = new WarehouseManagementEntities("");
        WarehouseManagementEntities st = new WarehouseManagementEntities("");
        SysproCore objSyspro = new SysproCore();
        public string ValidateBarcode(string details)
        {
            try
            {
                List<StockTakeOn> myDeserializedObjList = (List<StockTakeOn>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<StockTakeOn>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = st.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        ////JR 14/07/2020 - Commented out to collect data
                        //var StockWarehouseCheck = st.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse)).FirstOrDefault();
                        //if (StockWarehouseCheck == null)
                        //{
                        //    return "StockCode not stocked in Warehouse " + item.Warehouse + "!.";
                        //}

                        //*********COMMENTED OUT FOR TROPIC TAKE-ON LOTS ONLY******************
                        var StockTakeCheck = (from a in st.InvStockTakes where a.StockCode == item.StockCode && a.Warehouse == item.Warehouse select a).ToList();
                        if (StockTakeCheck.Count == 0)
                        {
                            return "StockCode: + " + item.StockCode + " for Warehouse:" + item.Warehouse + " not in Stock Take mode.";
                        }
                        //*********COMMENTED OUT FOR TROPIC TAKE-ON LOTS ONLY******************


                        ////JR 14/07/2020 - Commented out to collect data
                        //if (item.Quantity == 0)
                        //{
                        //    return "Quantity cannot be zero!";
                        //}


                        var PalletScanned = (from a in st.mtStockTakeCaptures where a.StockCode == item.StockCode && a.Lot == item.Lot select a).ToList();
                        if (PalletScanned.Count > 0)
                        {
                            return "Item already scanned!";
                        }
                        //var TraceableCheck = st.InvMasters.Where(a => a.StockCode.Equals(item.StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                        //if (TraceableCheck != null)
                        //{
                        //    //StockCode is Traceable -- Lot number required
                        //    if (string.IsNullOrEmpty(item.Lot))
                        //    {
                        //        return "StockCode is Lot Traceable. Lot number required";
                        //    }
                        //    else
                        //    {
                        //        var LotCheck = st.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse) && a.Lot.Equals(item.Lot)).FirstOrDefault();
                        //        if (LotCheck == null)
                        //        {
                        //            return "Lot " + item.Lot + " not found for StockCode " + item.StockCode + " in Warehouse " + item.Warehouse + "!";
                        //        }
                        //        else
                        //        {
                        //            var LotQtyCheck = st.LotDetails.Where(a => a.StockCode.Equals(item.StockCode)
                        //                                                    && a.Warehouse.Equals(item.Warehouse)
                        //                                                    && a.Lot.Equals(item.Lot)
                        //                                                  ).Select(a => a.QtyOnHand).Sum();
                        //            if (LotQtyCheck < item.Quantity)
                        //            {
                        //                return "Quantity on hand is less than Quantity to transfer for Lot " + item.Lot + "!";
                        //            }
                        //            else
                        //            {
                        //                return "";
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    //StockCode is not Traceable -- Check Quantity
                        //    var QtyCheck = st.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode)
                        //                                                  && a.Warehouse.Equals(item.Warehouse)
                        //                                                  ).Select(a => a.QtyOnHand).Sum();
                        //    if (QtyCheck < item.Quantity)
                        //    {
                        //        return "Quantity on hand is less than Quantity to transfer for StockCode " + item.StockCode + "!";
                        //    }
                        //    else
                        //    {
                        //        return "";
                        //    }
                        //}
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string PostStockTake(string details)
        {
            try
            {
                string Barcode = this.ValidateBarcode(details);
                if (Barcode == "")
                {
                    List<StockTakeOn> myDeserializedObjList = (List<StockTakeOn>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<StockTakeOn>));
                    foreach (var item in myDeserializedObjList)
                    {
                        st.sp_SaveStockTake(item.Warehouse, item.Bin, item.StockCode, item.Lot, (decimal)item.Quantity, HttpContext.Current.User.Identity.Name.ToUpper());
                    }
                    return "Saved Successfully.";
                }

                else return Barcode;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string Username()
        {
            try
            {
                string Username = HttpContext.Current.User.Identity.Name.ToUpper();
                return Username;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

}