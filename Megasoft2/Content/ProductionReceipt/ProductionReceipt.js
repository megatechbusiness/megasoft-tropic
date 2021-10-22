function ValidateBarcode()
{
    $('#loadingDiv').modal('show');


    //alert("Barcode Validated 3");
    //$('#loadingDiv').modal('hide');


}


$(document).ready(function () {
    
    $('#btnPost').click(function (e) {
        
            e.preventDefault();
            $('#errordiv').text("");
            $('#errordiv').removeClass("alert alert-danger");
            $('#loadingDiv').modal('show');
            var Job = document.getElementById('txtJob').value
            var StockCode = document.getElementById('txtStockCode').value;
            var Lot = document.getElementById('txtLot').value;
            var Quantity = document.getElementById('txtQuantity').value;



            var mydata = [];
            var myObject = new Object();
            myObject.Job = Job;
            myObject.StockCode = StockCode;
            myObject.LotNumber = Lot;
            myObject.Quantity = Quantity;

            mydata.push(myObject);

            var myString = JSON.stringify({ details: JSON.stringify(mydata) });
            var exportdata = myString;
            //alert(exportdata);
            $.ajax({
                type: "POST",
                url: "../ProductionReceipt/PostProductionReceipt",
                data: exportdata,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: OnSuccess,
                failure: OnErrorCall
            });

            function OnSuccess(response) {
                if (response != "") {
                    document.getElementById('txtJob').value = "";
                    document.getElementById('txtStockCode').value = "";
                    document.getElementById('txtLot').value = "";
                    document.getElementById('txtQuantity').value = "";
                    $('#errordiv').text(response);
                    $('#errordiv').addClass("alert alert-danger");
                    $('#loadingDiv').modal('hide');
                }

            }

            function OnErrorCall(response) {
                if (response != "") {
                    $('#errordiv').text(response);
                    $('#errordiv').addClass("alert alert-danger");
                    $('#loadingDiv').modal('hide');
                }
            }
            
        
    });
});