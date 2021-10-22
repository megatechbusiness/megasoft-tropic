using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.BusinessLogic;

namespace Megasoft2.Controllers
{
    public class CustomerController : Controller
    {
        Customer objCustomer = new Customer();
        SysproEntities db = new SysproEntities("");
        //
        // GET: /Customer/

        //public ActionResult Index()
        //{
        //    //CustomerViewModel newcust = objCustomer.GetCustomerModel();

        //    //return View(newcust);
        //}


        public ActionResult Create()
        {
            
            CustomerViewModel _customerModel = new CustomerViewModel();
            CustomerViewModel newcust = objCustomer.GetCustomerModel(_customerModel);

            return View(newcust);
        }

        
        [HttpPost]
        public ActionResult Create(CustomerViewModel customerViewModel)
        {
                     
            if(ModelState.IsValid)
            {

            }
            customerViewModel = objCustomer.GetCustomerModel(customerViewModel);
            return View(customerViewModel);
        }

        private static List<CustomerViewModel> _customer = new List<CustomerViewModel>();

    }
}
