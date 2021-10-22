using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Megasoft2.Controllers
{
    public class CustomerInvoiceExtractApiController : ApiController
    {
        InvoiceExtractBL BL = new InvoiceExtractBL();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities();//We not passing empty quotes here due to api fixed login to live company
        // GET api/HelloWorld
        public string Get()
        {
            return "Hello World";
        }

        // GET api/HelloWorld/id
        public string Get(string id)
        {
            try
            {
                DatabaseSwitcher.Connstr("WarehouseManagementEntities");
                if(id == "NG")
                {
                    

                    //Do Nestle Extract
                    var result = wdb.sp_GetInvoices(Convert.ToDateTime("1990-01-01"), Convert.ToDateTime(DateTime.Now), id, "N").ToList();
                  
                    if (result.Count > 0)
                    {
                        CustomerInvoiceViewModel model = new CustomerInvoiceViewModel();
                        model.Invoices = result;
                        model.CustomerClass = id;
                        model.FromDate = "2016-01-01";
                        model.ToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        //return "Api Called Successfully for :" + id;
                        return BL.DownloadNestleInvoice(model, true, true);

                    }
                    return "";
                }
                else if(id == "UG")
                {
                    //Do Unilever Extract
                    var result = wdb.sp_GetInvoices(Convert.ToDateTime("1990-01-01"), Convert.ToDateTime(DateTime.Now), id, "N").ToList();
                    if(result.Count > 0)
                    {
                        CustomerInvoiceViewModel model = new CustomerInvoiceViewModel();
                        model.Invoices = result;
                        model.CustomerClass = id;
                        model.FromDate = "2016-01-01";
                        model.ToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        //return "Api Called Successfully for :" + id;
                        return BL.DownloadUnileverInvoice(model, true, true);
                    }
                    return "";
                }
                else
                {
                    return "No Automation defined for Customer Class :" + id;
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }            
        }
    }
}
