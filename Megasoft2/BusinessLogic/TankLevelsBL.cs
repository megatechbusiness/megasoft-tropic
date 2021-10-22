using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class TankLevelsBL
    {
        Adr_LoggingEntities adb = new Adr_LoggingEntities();
        MegasoftEntities mdb = new MegasoftEntities();
        public string SaveTankData(List<TankLevels> lstTankLevels, string Operator)
        {
            try
            {
                Guid entryGuid = Guid.Empty;
                string BlendNo = lstTankLevels.FirstOrDefault().BlendNo;
                
                
                List<mtTankLevelStaging> checkBlend = (from a in mdb.mtTankLevelStagings where a.BlendNo == BlendNo select a).ToList();
                if(checkBlend.Count > 0)
                {
                    foreach (mtTankLevelStaging item in checkBlend)
                    {
                        mdb.mtTankLevelStagings.Remove(item);
                        mdb.SaveChanges();
                    }
                    
                }


                bool HasErrors = false;

                foreach (var item in lstTankLevels)
                {
                    var TankDetails = (from a in mdb.mtTankMasters where a.Tank == item.Tank select a).FirstOrDefault();
                                   
                    
                    var CurrentProduct = "";
                    
                    List<mtTankProductHistory> Products = (from a in mdb.mtTankProductHistories where a.Tank == item.Tank && a.DateUpdated <= item.FromDate select a).OrderByDescending(a => a.DateUpdated).ToList();
                    if(Products.Count > 0)
                    {
                        CurrentProduct = Products.FirstOrDefault().Product.ToString().Trim();
                    }
                    else
                    {
                        CurrentProduct = (from a in mdb.mtTankMasters where a.Tank == item.Tank select a.Product).FirstOrDefault().Trim();
                    }

                    bool GetTemp = true;
                    if(item.FromTemperature != 0 || item.ToTemperature != 0)
                    {
                        GetTemp = false;
                    }


                    List<sp_GetTankLevels_Result> lstOut = adb.sp_GetTankLevels(entryGuid, TankDetails.Tagname, item.FromDate, item.ToDate, GetTemp).ToList();
                    if (lstOut.Count > 0)
                    {
                        entryGuid = Guid.Parse(lstOut.FirstOrDefault().GUID.ToString());

                        foreach (var lst in lstOut)
                        {

                            decimal StartVolumeST, StartMassST, StartDensityST, StartLitresST, StartVolumeTT, StartMassTT, StartDensityTT, StartLitresTT, StartVolumeRT, StartMassRT, StartDensityRT, StartLitresRT;
                            decimal EndVolumeST, EndMassST, EndDensityST, EndLitresST, EndVolumeTT, EndMassTT, EndDensityTT, EndLitresTT, EndVolumeRT, EndMassRT, EndDensityRT, EndLitresRT;
                            decimal ReportingTemp = item.ReportingTemperature;
                            decimal FromTemperature, ToTemperature;

                            if(item.FromTemperature != 0)
                            {
                                FromTemperature = item.FromTemperature;
                            }
                            else
                            {
                                FromTemperature = lst.FromTemperature;
                            }

                            if (item.ToTemperature != 0)
                            {
                                ToTemperature = item.ToTemperature;
                            }
                            else
                            {
                                ToTemperature = lst.ToTemperature;
                            }


                            double DensityFactor = 0.00063;
                            if (item.SGMethod != "Hydrometer")
                            {
                                StartDensityTT = Math.Round((decimal)TankDetails.StandardDensity + (20 - FromTemperature) * (decimal)DensityFactor, 4);
                                EndDensityTT = Math.Round((decimal)TankDetails.StandardDensity + (20 - ToTemperature) * (decimal)DensityFactor, 4);
                            }
                            else
                            {
                                StartDensityTT = Math.Round((decimal)TankDetails.StandardDensity + ((20 - FromTemperature) * (decimal)DensityFactor + Convert.ToDecimal(0.0007)), 4);
                                //EndDensityTT = Math.Round((decimal)TankDetails.StandardDensity + ((20 - item.FromTemperature) * (decimal)DensityFactor + Convert.ToDecimal(0.0007)), 4);
                                EndDensityTT = Math.Round((decimal)TankDetails.StandardDensity + ((20 - ToTemperature) * (decimal)DensityFactor + Convert.ToDecimal(0.0007)), 4);
                            }

                            StartVolumeTT = lst.StartValue;
                            EndVolumeTT = lst.EndValue;
                            StartMassTT = StartVolumeTT * StartDensityTT;
                            EndMassTT = EndVolumeTT * EndDensityTT;
                            StartLitresTT = StartVolumeTT * 1000;
                            EndLitresTT = EndVolumeTT * 1000;

                            StartMassST = StartMassTT;
                            EndMassST = EndMassTT;
                            StartDensityST = (decimal)TankDetails.StandardDensity;
                            EndDensityST = (decimal)TankDetails.StandardDensity;

                            StartVolumeST = StartMassST / StartDensityST;
                            EndVolumeST = EndMassST / EndDensityST;
                            StartLitresST = StartVolumeST * 1000;
                            EndLitresST = EndVolumeST * 1000;

                            if (item.SGMethod != "Hydrometer")
                            {
                                StartDensityRT = Math.Round((decimal)TankDetails.StandardDensity + (20 - ReportingTemp) * (decimal)DensityFactor, 4);
                                EndDensityRT = Math.Round((decimal)TankDetails.StandardDensity + (20 - ReportingTemp) * (decimal)DensityFactor, 4);
                            }
                            else
                            {
                                StartDensityRT = Math.Round((decimal)TankDetails.StandardDensity + ((20 - ReportingTemp) * (decimal)DensityFactor + Convert.ToDecimal(0.0007)), 4);
                                EndDensityRT = Math.Round((decimal)TankDetails.StandardDensity + ((20 - ReportingTemp) * (decimal)DensityFactor + Convert.ToDecimal(0.0007)), 4);
                            }
                           

                            StartMassRT = StartMassTT;
                            EndMassRT = EndMassTT;

                            StartVolumeRT = StartMassRT / StartDensityRT;
                            EndVolumeRT = EndMassRT / EndDensityRT;
                            StartLitresRT = StartVolumeRT * 1000;
                            EndLitresRT = EndVolumeRT * 1000;
                           
                            mdb.mtTankLevelStagings.Add(new mtTankLevelStaging
                            {
                                GUID = entryGuid,
                                Tank = item.Tank,
                                Description = TankDetails.Description,
                                Tagname = TankDetails.Tagname,
                                TankType = item.TankType,
                                FromDate = lst.FromDate,
                                ToDate = lst.ToDate,
                                StartVolumePerc = (Convert.ToDecimal(lst.StartValue) / TankDetails.TankCapacity) * 100,
                                EndVolumePerc = (Convert.ToDecimal(lst.EndValue) / TankDetails.TankCapacity) * 100,
                                TankCapacity = TankDetails.TankCapacity,
                                FromTemperature = Convert.ToDecimal(FromTemperature),
                                ToTemperature = Convert.ToDecimal(ToTemperature),
                                StartVolumeTT = StartVolumeTT,
                                EndVolumeTT = EndVolumeTT,
                                StartVolumeST = StartVolumeST,
                                EndVolumeST = EndVolumeST,
                                StartVolumeRT = StartVolumeRT,
                                EndVolumeRT = EndVolumeRT,
                                StartDensityTT = StartDensityTT,
                                EndDensityTT = EndDensityTT,
                                StartDensityST = StartDensityST,
                                EndDensityST = EndDensityST,
                                StartDensityRT = StartDensityRT,
                                EndDensityRT = EndDensityRT,
                                StartLitresTT = StartLitresTT,
                                EndLitresTT = EndLitresTT,
                                StartLitresST = StartLitresST,
                                EndLitresST = EndLitresST,
                                StartLitresRT = StartLitresRT,
                                EndLitresRT = EndLitresRT,
                                StartMassTT = StartMassTT,
                                EndMassTT = EndMassTT,
                                StartMassST = StartMassST,
                                EndMassST = EndMassST,
                                StartMassRT = StartMassRT,
                                EndMassRT = EndMassRT,
                                TrnDate = DateTime.Now,
                                strMessage = lst.strMessage,
                                ReportingTemperature = item.ReportingTemperature,
                                Product = CurrentProduct,
                                BlendNo = item.BlendNo,
                                Operator = Operator
                            });

                            mdb.SaveChanges();

                            if(!string.IsNullOrEmpty(lst.strMessage))
                            {
                                HasErrors = true;
                            }
                        }
                    }



                }

                if(HasErrors == false)
                {
                    mdb.sp_UpdateBlendMassAndDensity(entryGuid);
                }
                
                
                return entryGuid.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public decimal GetSumOfMassDelta(List<TankLevels> lstTankLevels, List<sp_GetTankLevels_Result> lstOut)
        {
            try
            {
                decimal StartDensityTT, EndDensityTT, EndVolumeTT, StartVolumeTT, StartMassTT, EndMassTT, DeltaMass;
                decimal SumOfDeltaMass = 0;
                foreach(var item in lstTankLevels)
                {
                    
                    var TankDetails = (from a in mdb.mtTankMasters where a.Tank == item.Tank select a).FirstOrDefault();
                    foreach (var lst in lstOut)
                    {
                        
                        if (item.SGMethod == "Density Cup")
                        {
                            StartDensityTT = Convert.ToDecimal(TankDetails.StandardDensity + ((20 - item.FromTemperature) * Convert.ToDecimal(0.00063)));
                            EndDensityTT = Convert.ToDecimal(TankDetails.StandardDensity + ((20 - item.ToTemperature) * Convert.ToDecimal(0.00063)));
                        }
                        else
                        {
                            StartDensityTT = Convert.ToDecimal(TankDetails.StandardDensity + ((20 - item.FromTemperature) * Convert.ToDecimal(0.00063) + Convert.ToDecimal(0.0007)));
                            EndDensityTT = Convert.ToDecimal(TankDetails.StandardDensity + ((20 - item.ToTemperature) * Convert.ToDecimal(0.00063) + Convert.ToDecimal(0.0007)));
                        }

                        EndVolumeTT = lst.EndValue;
                        StartVolumeTT = lst.StartValue;

                        EndMassTT = EndVolumeTT * EndDensityTT;
                        StartMassTT = StartVolumeTT * StartDensityTT;
                        DeltaMass = StartMassTT - EndMassTT;

                        SumOfDeltaMass += DeltaMass;
                    }
                }
                return SumOfDeltaMass;
                
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public List<mtTankLevelStaging> GetTankLevelsData(string entryGuid)
        {
            try
            {
                Guid outGuid = Guid.Parse(entryGuid);
                return (from a in mdb.mtTankLevelStagings where a.GUID == outGuid select a).OrderByDescending(a => a.TankType).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SaveMovements(List<TankLevels> lstTankLevels)
        {
            try
            {
                Guid entryGuid = Guid.NewGuid();
                foreach (var item in lstTankLevels)
                {
                    var TankDetails = (from a in mdb.mtTankMasters where a.Tank == item.Tank select a).FirstOrDefault();
                    List<sp_GetTankMovements_Result> lstOut = adb.sp_GetTankMovements(entryGuid, TankDetails.Tagname, item.FromDate, item.ToDate).ToList();
                    if (lstOut.Count > 0)
                    {
                        foreach (var lst in lstOut)
                        {
                            mdb.mtTankMovementStagings.Add(new mtTankMovementStaging
                            {
                                GUID = entryGuid,
                                Tank = item.Tank,
                                Description = TankDetails.Description,
                                Tagname = TankDetails.Tagname,
                                FromDate = lst.FromDate,
                                ToDate = lst.ToDate,
                                TrnDate = DateTime.Now,
                                Receipt = Math.Round(Convert.ToDecimal(lst.Receipt),3),
                                Delivery = Math.Round(Convert.ToDecimal(lst.Delivery),3)
                            });

                            mdb.SaveChanges();
                        }
                    }
                }
                return entryGuid.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<mtTankMovementStaging> GetTankMovementsData(string entryGuid)
        {
            try
            {
                Guid outGuid = Guid.Parse(entryGuid);
                return (from a in mdb.mtTankMovementStagings where a.GUID == outGuid select a).OrderByDescending(a => a.Tank).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}