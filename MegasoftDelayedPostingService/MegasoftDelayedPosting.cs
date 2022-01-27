using MegasoftDelayedPosting;
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

namespace MegasoftDelayedPostingService
{
    public partial class MegasoftDelayedPosting : ServiceBase
    {
        private Timer timer1 = null;
        SysproEntities sdb = new SysproEntities();
        SysproCore objSyspro = new SysproCore();
        SysproBusinessLogic BL = new SysproBusinessLogic();
        MegasoftEntities mdb = new MegasoftEntities();
        public MegasoftDelayedPosting()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1 = new Timer();
            var interval = Properties.Settings.Default.ServiceIntervalMins;
            this.timer1.Interval = TimeSpan.FromMinutes(Convert.ToDouble(interval)).TotalMilliseconds;
            this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Enabled = true;
            ErrorEventLog.WriteErrorLog("I", "Megasoft Delayed Posting Service started.");
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;
            ErrorEventLog.WriteErrorLog("I", "Megasoft Delayed Posting Service stopped.");
        }

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer1.Stop();
                try
                {
                    DoProcessing();
                }
                catch (Exception dp)
                {
                    ErrorEventLog.WriteErrorLog("I", "Failure on delayed posting transaction. " + dp.Message);
                }

                try
                {
                    DoProductionPosting();
                }
                catch (Exception pp)
                {
                    ErrorEventLog.WriteErrorLog("I", "Failure on production posting transaction. " + pp.Message);
                }

                try
                {
                    ReportAutomation();
                }
                catch (Exception ra)
                {
                    ErrorEventLog.WriteErrorLog("I", "Failure on report automation. " + ra.Message);
                }


                timer1.Start();

            }
            catch (Exception ex)
            {
                ErrorEventLog.WriteErrorLog("E", ex.Message);
                timer1.Start();
            }
        }

        public void DoProcessing()
        {
            try
            {

                //Inventory Movements
                var result = sdb.sp_GetDelayedPostingData(Properties.Settings.Default.CompanyId).ToList();
                if (result.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        throw new Exception("Failed to Log in to Syspro.");
                    }
                    foreach (var item in result)
                    {
                        if (item.MovementType == "Bin")
                        {
                            BL.PostBinTransfer(item, Guid);
                        }
                        else if (item.MovementType == "Immediate")
                        {
                            BL.PostWarehouseTransfer(item, Guid);
                        }
                    }

                    objSyspro.SysproLogoff(Guid);
                }



                //Production Receipts - Bagging/Wicketting





            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DoProductionPosting()
        {
            try
            {
                //Prodution Receipting
                var result = sdb.sp_GetDelayedPostingProductionPallets(Properties.Settings.Default.CompanyId).ToList();
                if (result.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        throw new Exception("Failed to Log in to Syspro.");
                    }

                    foreach (var item in result)
                    {
                        BL.PostJobReceiptByBatch(item.PalletNo, Guid);
                    }

                    objSyspro.SysproLogoff(Guid);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void ReportAutomation()
        {
            try
            {
                var mtReportAutomations = (from a in sdb.mtReportAutomations where a.ServiceActive == "Y" select a).ToList();

                foreach (var item in mtReportAutomations)
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

                                if (DateTime.Now >= NextRunDate)
                                {
                                    //Run Extract
                                    okToRun = true;
                                }
                            }
                        }




                        if (okToRun)
                        {

                            ErrorEventLog.WriteErrorLog("I", "Extracting Report " + item.Report);

                            var emailSettings = (from a in mdb.mtSystemSettings select a).FirstOrDefault();

                            Mail objMail = new Mail();
                            objMail.From = emailSettings.FromAddress;
                            objMail.To = item.ToEmail;
                            objMail.Subject = item.Title;
                            objMail.Body = "Powered By Megasoft Automated Reporting Service.";
                            objMail.CC = emailSettings.FromAddress;

                            List<string> attachments = new List<string>();

                            string AttachmentPath = "";

                            try
                            {
                                AttachmentPath = BL.ExportPdf(item.Report);
                                attachments.Add(AttachmentPath);

                                //Email _email = new Email();
                                BL.SendEmail(objMail, attachments);

                                var updateRuntime = (from a in sdb.mtReportAutomations where a.Report == item.Report select a).FirstOrDefault();
                                updateRuntime.LastRunDate = DateTime.Now;
                                sdb.SaveChanges();
                            }
                            catch (Exception emailFail)
                            {
                                ErrorEventLog.WriteErrorLog("E", emailFail.Message);
                                if (!string.IsNullOrWhiteSpace(AttachmentPath))
                                {
                                    BL.DeleteSingleFile(AttachmentPath);
                                }

                                ErrorEventLog.WriteErrorLog("E", "Deleting file " + AttachmentPath + " due to send failure.");
                                continue;
                            }

                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " - " + ex.StackTrace);
            }
        }

        public class Mail
        {
            public string From { get; set; }
            public string To { get; set; }
            public string CC { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Program { get; set; }
        }

    }
}
