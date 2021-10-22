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

namespace MegasoftService
{
    public partial class Megasoft : ServiceBase
    {
        private Timer timer1 = null;
        MegasoftEntities db = new MegasoftEntities();
        PurchasingIntegration PBL = new PurchasingIntegration();
        SysproCore objsys = new SysproCore();
        public Megasoft()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1 = new Timer();
            var interval = (from a in db.mtSystemSettings select a.MegasoftServiceIntervalMin).FirstOrDefault();
            this.timer1.Interval = TimeSpan.FromMinutes(Convert.ToDouble(interval)).TotalMilliseconds;
            this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Enabled = true;
            ErrorEventLog.WriteErrorLog("I", "Megasoft Posting Service started.");
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;
            ErrorEventLog.WriteErrorLog("I", "Megasoft Posting Service stopped.");
        }


        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer1.Stop();
                //ErrorEventLog.WriteErrorLog("I", "Posting process started.");
                //write code here for posting
                
                PBL.PostGrn();


                PBL.PostMaterialAllocation();


                PBL.DoMaterialIssuePost();                

                PBL.PostInvoiceMatch();

                PBL.CreatePurchaseOrder();

                //Guid = objsys.SysproLogin(Properties.Settings.Default.SysproUser);

                //PBL.PostGrnAdjustment(Guid);

                //objsys.SysproLogoff(Guid);
                //ErrorEventLog.WriteErrorLog("I", "Posting process ended.");

                PBL.PostBulkCancel();

                timer1.Start();
            }
            catch (Exception ex)
            {
                ErrorEventLog.WriteErrorLog("E", ex.Message);
                timer1.Start();
            }
        }
    }
}
