using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Models
{
    public class CustomerViewModel
    {

        
        //------------------------------------------Customer Details------------------------------------------------------------------------------
        public string Customer { get; set; }
        
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }
        public IEnumerable<SelectListItem> Branch { get; set; }
        public string SelectedBranch { get; set; }
        public IEnumerable<SelectListItem> GeographicArea { get; set; }
        public string SelectedGeographicArea { get; set; }
        public IEnumerable<SelectListItem> SalesPerson { get; set; }
        public string SelectedSalesPerson { get; set; }
        public IEnumerable<SelectListItem> InvoiceTerms { get; set; }
        public string SelectedInvoiceTerms { get; set; }
        public IEnumerable<SelectListItem> Currency { get; set; }
        public string SelectedCurrency { get; set; }
        public IEnumerable<SelectListItem> CustomerClass { get; set; }
        public string SelectedCustomerClass { get; set; }
        public string Nationality { get; set; }
        public decimal CreditLimit { get; set; }
        public bool CustomerOnHold { get; set; }
        public bool ExemptFromFinCharge { get; set; }
        public bool DetailHistoryRequired { get; set; }
        public bool RetainDetailMovements { get; set; }
        public bool ContractPricing { get; set; }
        public bool CounterSalesOnly { get; set; }
        public IEnumerable<SelectListItem> DefaultOrderType { get; set; }
        public string SelectedDefaultOrderType { get; set; }
        public IEnumerable<SelectListItem> DefaultSalesOrderType { get; set; }
        public string SelectedDefaultSalesOrderType { get; set; }
        public string DefaultDocumentFormat { get; set; }
        public IEnumerable<SelectListItem> StandardCommentCode { get; set; }
        public string SelectedStandardCommentCode { get; set; }
        public string SpecialInstructions { get; set; }
        public IEnumerable<SelectListItem> DefaultWarehouse { get; set; }
        public string SelectedDefaultWarehouse { get; set; }
        public bool TradePromotionsCustomer { get; set; }
        public bool TradePromotionsPricing{ get; set; }
        public bool IBTCustomer { get; set; }
        public IEnumerable<SelectListItem> SalesPerson2 { get; set; }
        public string SelectedSalesPerson2 { get; set; }
        public IEnumerable<SelectListItem> SalesPerson3 { get; set; }
        public string SelectedSalesPerson3 { get; set; }
        public IEnumerable<SelectListItem> SalesPerson4 { get; set; }
        public string SelectedSalesPerson4 { get; set; }
        public IEnumerable<SelectListItem> BuyingGroup1 { get; set; }
        public string SelectedBuyingGroup1 { get; set; }
        public IEnumerable<SelectListItem> BuyingGroup2 { get; set; }
        public string SelectedBuyingGroup2 { get; set; }
        public IEnumerable<SelectListItem> BuyingGroup3 { get; set; }
        public string SelectedBuyingGroup3 { get; set; }
        public IEnumerable<SelectListItem> BuyingGroup4 { get; set; }
        public string SelectedBuyingGroup4 { get; set; }
        public IEnumerable<SelectListItem> BuyingGroup5 { get; set; }
        public string SelectedBuyingGroup5 { get; set; }



        //------------------------------------------Contact Details------------------------------------------------------------------------------
        public string SoldToBuilding { get; set; }
        public string SoldToStreet { get; set; }
        public string SoldToCity { get; set; }
        public string SoldToLocality { get; set; }
        public string SoldToState { get; set; }
        public string SoldToCountry { get; set; }
        public string SoldToZip { get; set; }
        public string SoldToGeolocation { get; set; }

        public string ShipToBuilding { get; set; }
        public string ShipToStreet { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToLocality { get; set; }
        public string ShipToState { get; set; }
        public string ShipToCountry { get; set; }
        public string ShipToZip { get; set; }
        public string ShipToGeolocation { get; set; }

        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Extension { get; set; }
        public string AdditionalTelephone { get; set; }
        public string Fax { get; set; }
        public string Telex { get; set; }
        public string Contact { get; set; }        
        public IEnumerable<SelectListItem> ShipViaCode { get; set; }
        public string SelectedShipViaCode { get; set; }
        public string EDISenderCode { get; set; }
        public bool EdiTradingPartner { get; set; }
        public bool StatementsRequired { get; set; }
        public IEnumerable<SelectListItem> StatementFormat { get; set; }
        public string SelectedStatementFormat { get; set; }
        public IEnumerable<SelectListItem> BalancePrintType { get; set; }
        public string SelectedBalancePrintType { get; set; }
        public string DocFax { get; set; }
        public string DocFaxContact { get; set; } 
        public IEnumerable<SelectListItem> DocFaxSoDocuments { get; set; }
        public string SelectedDocFaxSoDocuments { get; set; }
        public bool DocFaxOrderAcknowledgement { get; set; }
        public bool DocFaxOrderDeliveryNote { get; set; }
        public bool DocFaxOrderInvoice { get; set; }
        public bool DocFaxMultipleDispatch { get; set; }
        public IEnumerable<SelectListItem> DocFaxStatements { get; set; }
        public string SelectedDocFaxStatements { get; set; }
        public IEnumerable<SelectListItem> DocFaxQuotations { get; set; }
        public string SelectedDocFaxQuotations { get; set; }





        //------------------------------------------Custom Forms------------------------------------------------------------------------------
        public List<CustomForm> CustomForms { get; set; }

    }

    

    

    public class CustomForm
    {
        public string FormType { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<SelectListItem> customFormListValues { get; set; }
        public string SelectedFormValue { get; set; }
        public string InputFormValue { get; set; }
    }
 

    


   
}