using Megasoft2.Models;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.BusinessLogic
{
    public class Customer
    {
        SysproEntities db = new SysproEntities("");
        public List<CustomForm> GetCustomFormModel()
        {
            try
            {
                var _customForm = (from a in db.AdmFormControls 
                                   where a.FormType == "CUS" 
                                   select new { FormType = a.AllowNull, 
                                                FieldName = a.FieldName, 
                                                FieldType = a.FieldType, 
                                                DisplayName = a.FieldDescription 
                                              }).ToList();

                //List<SelectListItem> _customFormListValues = (from a in db.AdmFormValidations where a.FormType == "CUS" select new SelectListItem { Value = a.Item, Text = a.Description }).ToList();

                var model = new List<CustomForm>();

                foreach(var cf in _customForm)
                {
                    var currentCustomForm = new CustomForm();

                    currentCustomForm.FieldName = cf.FieldName;
                    currentCustomForm.FormType = cf.FormType.ToString().Trim();
                    currentCustomForm.FieldType = cf.FieldType.ToString().Trim();
                    currentCustomForm.DisplayName = cf.DisplayName.ToString().Trim();
                    if(cf.FormType.ToString().Trim() == "2")
                    {
                        var ListValue = (from a in db.AdmFormLists where a.FormType == "CUS" && a.FieldName == cf.FieldName select a.FieldList).FirstOrDefault().ToString().Trim();
                        if(!string.IsNullOrEmpty(ListValue))
                        {
                            List<SelectListItem> _list = new List<SelectListItem>();

                            string[] words = ListValue.Split(';');
                            foreach (string word in words)
                            {
                                var newItem = new SelectListItem { Text = word, Value = word };
                                _list.Add(newItem);
                            }
                            currentCustomForm.customFormListValues = _list;
                        }
                    }
                    else
                    {
                        currentCustomForm.customFormListValues = (from a in db.AdmFormValidations where a.FormType == "CUS" && a.FieldName == cf.FieldName select new SelectListItem { Value = a.Item, Text = a.Description, Selected = true }).ToList();
                    }
                    
                    model.Add(currentCustomForm);
                }

                return model;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        
        }


        public CustomerViewModel GetCustomerModel(CustomerViewModel _customerModel)
        {
            try
            {

               

                //CustomerViewModel _customerModel = new CustomerViewModel();

                _customerModel.Branch = (from a in db.SalBranches select new SelectListItem { Value = a.Branch, Text = a.Description }).ToList();
                _customerModel.GeographicArea = (from a in db.SalAreas select new SelectListItem { Value = a.Area, Text = a.Description }).ToList();
                _customerModel.SalesPerson = (from a in db.SalSalespersons select new SelectListItem { Value = a.Salesperson, Text = a.Name }).ToList();
                _customerModel.InvoiceTerms = (from a in db.TblArTerms select new SelectListItem { Value = a.TermsCode, Text = a.Description }).ToList();
                _customerModel.Currency = (from a in db.TblCurrencies select new SelectListItem { Value = a.Currency, Text = a.Description }).ToList();
                _customerModel.CustomerClass = (from a in db.TblCustomerClasses select new SelectListItem { Value = a.Class, Text = a.Class + " - " + a.Description }).ToList();
                _customerModel.DefaultOrderType = (from a in db.TblSoTypes select new SelectListItem { Value = a.OrderType, Text = a.OrderType + " - " + a.Description }).ToList();

                List<SelectListItem> SalesOrderType = new List<SelectListItem>
                {
                    new SelectListItem{Text = "Operator Default", Value = "Operator Default"},
                    new SelectListItem{Text = "Sales order", Value = "O"},
                    new SelectListItem{Text = "Billing", Value = "B"},
                    new SelectListItem{Text = "Scheduled order", Value = "S"},
                    new SelectListItem{Text = "Debit note", Value = "D"},
                    new SelectListItem{Text = "Forward order", Value = "F"},
                    new SelectListItem{Text = "Counter sale", Value = "U"},
                    new SelectListItem{Text = "IBT", Value = "U"},
                    new SelectListItem{Text = "Hierachical order", Value = "U"}


                };

                _customerModel.DefaultSalesOrderType = SalesOrderType;

                _customerModel.StandardCommentCode = (from a in db.TblSoStdComs select new SelectListItem { Value = a.Comment, Text = a.CommentCode }).ToList();
                //var result = (from a in db.in select a).ToList();
                //_customerModel.DefaultWarehouse = (from a in result select new SelectListItem { Value = a.Warehouse, Text = a.Warehouse + " - " + a.Description }).ToList();
                _customerModel.SalesPerson2 = _customerModel.SalesPerson;
                _customerModel.SalesPerson3 = _customerModel.SalesPerson;
                _customerModel.SalesPerson4 = _customerModel.SalesPerson;
                _customerModel.BuyingGroup1 = (from a in db.TblSoBuyingGroups select new SelectListItem { Value = a.BuyingGroup, Text = a.BuyingGroup + " - " + a.Description }).ToList();
                _customerModel.BuyingGroup2 = _customerModel.BuyingGroup1;
                _customerModel.BuyingGroup3 = _customerModel.BuyingGroup1;
                _customerModel.BuyingGroup4 = _customerModel.BuyingGroup1;
                _customerModel.BuyingGroup5 = _customerModel.BuyingGroup1;


                _customerModel.ShipViaCode = (from a in db.TblSoShipInsts select new SelectListItem { Value = a.ShippingInstrs, Text = a.ShippingInstrs + " - " + a.InstructionText }).ToList();

                List<SelectListItem> StatementFormat = new List<SelectListItem>
                {
                    new SelectListItem{Text = "Customer Statement", Value = "01"},
                    new SelectListItem{Text = "AR Statement ZIM", Value = "02"},
                };

                _customerModel.StatementFormat = StatementFormat;

                List<SelectListItem> BalancePrintType = new List<SelectListItem>
                {
                    new SelectListItem{Text = "Open item", Value = "Open item"},
                    new SelectListItem{Text = "Balance forward", Value = "Balance forward"},
                };

                _customerModel.BalancePrintType = BalancePrintType;

                List<SelectListItem> TransmissionMethod = new List<SelectListItem>
                {                    
                    new SelectListItem{Text = "Fax", Value = "Fax"},
                    new SelectListItem{Text = "Email", Value = "Email"},
                    new SelectListItem{Text = "None", Value = "None"}
                };

                _customerModel.DocFaxSoDocuments = TransmissionMethod;
                _customerModel.DocFaxStatements = TransmissionMethod;
                _customerModel.DocFaxQuotations = TransmissionMethod;


                //_customerModel.CustomForms = this.GetCustomFormModel();
                //_customForm.SelectedBranch = "30"; //To set a default selected value.
                
                return _customerModel;
                

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


       

      


     
     
    }
}