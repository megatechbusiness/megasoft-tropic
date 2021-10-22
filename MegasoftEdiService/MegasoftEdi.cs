using MegasoftEdi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MegasoftEdiService
{
    public partial class MegasoftEdi : ServiceBase
    {
        private Timer timer1 = null;
        SysproEntities sdb = new SysproEntities();
        public MegasoftEdi()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1 = new Timer();
            var interval = (from a in sdb.mtInvoiceExtractSettings select a.ServiceIntervalMin).FirstOrDefault();
            this.timer1.Interval = TimeSpan.FromMinutes(Convert.ToDouble(interval)).TotalMilliseconds;
            this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Enabled = true;
            ErrorEventLog.WriteErrorLog("I", TimeSpan.FromMinutes(Convert.ToDouble(interval)).TotalMilliseconds.ToString());
            ErrorEventLog.WriteErrorLog("I", "Megasoft Edi Service started.");
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;
            ErrorEventLog.WriteErrorLog("I", "Megasoft Edi Service stopped.");
        }

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer1.Stop();
                DoProcessing();
                timer1.Start();
                
            }
            catch (Exception ex)
            {
                ErrorEventLog.WriteErrorLog("E", ex.Message);
                timer1.Start();
            }
        }


        public async void DoProcessing()
        {
            try
            {
                ErrorEventLog.WriteErrorLog("I", "1");
                var CustomeClassList = (from a in sdb.mtInvoiceExtractSettings where a.ServiceActive == "Y" select a).ToList();
                foreach (var item in CustomeClassList)
                {
                    DateTime LastRunTime = (DateTime)item.LastRunDate;

                    if (LastRunTime.Date < DateTime.Now.Date)
                    {
                        bool okToRun = false;
                        DateTime NextRunDate;
                        //Determine next run time
                        if (item.ServiceScheduleMode == "Daily")
                        {

                            //Since mode is daily we just append the scheduled run time with todays date, 
                            //then compare NextRunDate to Current DateTime to determine if we need to run.
                            NextRunDate = DateTime.Now.Date + ((TimeSpan)item.ServiceRunTime);
                            ErrorEventLog.WriteErrorLog("I", "Next Run Time " + NextRunDate);
                            if (DateTime.Now >= NextRunDate)
                            {
                                //Run Extract
                                okToRun = true;
                            }
                        }
                        else if (item.ServiceScheduleMode == "Weekly")
                        {
                            //First Determine if we today is the specified day of the week required to run. Then we check the time as done in daily
                            if (DateTime.Now.DayOfWeek.ToString() == item.ServiceWeeklyDay)
                            {
                                NextRunDate = DateTime.Now.Date + ((TimeSpan)item.ServiceRunTime);
                                ErrorEventLog.WriteErrorLog("I", "Next Run Time " + NextRunDate);
                                if (DateTime.Now >= NextRunDate)
                                {
                                    //Run Extract
                                    okToRun = true;
                                }
                            }
                        }
                        else
                        {//Monthly
                            //We firstly have to check if the month contains specified date. eg. If the Specified date of month is 31, September only has 30 days, therefore we should run on the 30th. 
                            //Determine if we today is the specified date of the month required to run. Then we check the time as done in daily

                            int DayInMonthToRun;
                            if (item.ServiceMonthlyDate > DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
                            {
                                DayInMonthToRun = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                            }
                            else
                            {
                                DayInMonthToRun = (int)item.ServiceMonthlyDate;
                            }

                            if (DateTime.Now.Day == DayInMonthToRun)
                            {
                                NextRunDate = DateTime.Now.Date + ((TimeSpan)item.ServiceRunTime);
                                ErrorEventLog.WriteErrorLog("I", "Next Run Time " + NextRunDate);
                                if (DateTime.Now >= NextRunDate)
                                {
                                    //Run Extract
                                    okToRun = true;
                                }
                            }
                        }


                        ErrorEventLog.WriteErrorLog("I", "OK to run " + okToRun.ToString());

                        if (okToRun)
                        {
                            HttpClient hc = new HttpClient();
                            string uriStr = item.ApiExtractUri + item.CustomerClass;


                            hc.BaseAddress = new Uri(uriStr);
                            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await hc.GetAsync(uriStr);
                            response.EnsureSuccessStatusCode(); // Throw on error code.

                            var result = await response.Content.ReadAsStringAsync();
                            if(result != @"""")
                            {
                                ErrorEventLog.WriteErrorLog("I", result);
                                //Update Last Run Time Date
                                using (var udb = new SysproEntities())
                                {
                                    var custClass = (from a in udb.mtInvoiceExtractSettings where a.CustomerClass == item.CustomerClass select a).FirstOrDefault();
                                    custClass.LastRunDate = DateTime.Now;
                                    udb.Entry(custClass).State = System.Data.Entity.EntityState.Modified;
                                    udb.SaveChanges();
                                }

                                ErrorEventLog.WriteErrorLog("I", "EDI Extract Completed Successfully for Customer Class: " + item.CustomerClass);
                            }
                            
                          
                        }
                    }


                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message + " - " + ex.StackTrace);
            }
        }


        

    }
}
