var count = 0;
function ValidateBarcode() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");

    var str = document.getElementById('txtStockCode').value;
    //If Megasoft Barcode
    if (str.indexOf("|") > 0) {
        var Values = document.getElementById('txtStockCode').value.split("|");

        document.getElementById('txtStockCode').value = Values[0];
        document.getElementById('txtLot').value = Values[4];
        document.getElementById('txtQuantity').value = Values[2];
        var Bin = document.getElementById('txtBin').value ;

        var mydata = [];
        var myObject = new Object();
        var e = document.getElementById("ddlWarehouse");
        var Warehouse = e.options[e.selectedIndex].value;


        myObject.Warehouse = Warehouse;
        myObject.Bin = Bin;
        myObject.StockCode = Values[0];
        myObject.Lot = Values[4];
        myObject.Quantity = Values[2]
        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "WhseManPalletAudit/ValidateDetails",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: OnErrorCall
        });

        function OnSuccess(response) {
            if (response != "") {
                document.getElementById('txtBin').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
                alert(response);
            }
            else {
                PostStockTakeOn();
                $("#txtStockCode").focus();
            }

        }

        function OnErrorCall(response) {
            alert(response);
            if (response != "") {
                document.getElementById('txtBin').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");        
            }
            else {
                document.getElementById("btnPost").focus();
            }
        }
    }
    else {}
    
    
    

}

function checkMultiBins(Warehouse)
{
    var mydata = [];
    var myObject = new Object();
    myObject.Warehouse = Warehouse;

    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "WhseManPalletAudit/CheckWarehouseMultiBin",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response == "Y") {

                $('#Bin').show();
           
        }
        else {
            $('#Bin').hide();
            
        }

    }

    function OnErrorCall(response) {
        if (response != "") {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
        }
    }
}


function SavePalletAudit()
{
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");

    showprogressbar();
    var AddReel = true; 

    var mydata = [];
    var myObject = new Object();

    var e = document.getElementById("ddlWarehouse");
    var Warehouse = e.options[e.selectedIndex].value;

    var Bin = document.getElementById('txtBin').value;
    var StockCode = document.getElementById('txtStockCode').value;
    var Lot = document.getElementById('txtLot').value;
    var Quantity = document.getElementById('txtQuantity').value;

    if (Quantity.trim() == "") {
        alert("Quantity missing.");
        AddReel = false;
    }

    //If multiBin and txtBin is empty
    if ($("#Bin").is(":visible")) {
        if (Bin == "") {
            alert("Please enter bin");
            AddReel = false;
        }
    }

   
    if (AddReel == true) {
        myObject.Warehouse = Warehouse;
        myObject.Bin = Bin;
        myObject.StockCode = StockCode;
        myObject.Lot = Lot;
        myObject.Quantity = Quantity;
        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "WhseManPalletAudit/SavePalletAudit",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess1,
            failure: OnErrorCall1,
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
        });

    }
        function OnSuccess1(response) {
          
            //alert(response.d);
            //If validation Error
            if (response != "" && response != "Saved Successfully.") {
                
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                document.getElementById('txtBin').value = "";

                alert(response);
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");

                $("#txtStockCode").focus();
            }
            else {             
               

                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
        
                //counter
                count += 1;
                $('#txtCount').html('Total:' + count);

                $("#tblLines tbody>tr").remove();
                GetLast3Scanned();

                $("#txtStockCode").focus();
            }

            hideprogressbar();
        }

        function OnErrorCall1(response) {
            alert(response);
            if (response != "") {
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
            }
            hideprogressbar();
        }
        $("#txtBin").focus();
    
}




$(document).ready(function () {

    $('#Bin').hide();
    GetLast3Scanned();
    //change page title text
    $('#megasoftTitle').text(' Megasoft - Pallet Audit');
    var span = document.getElementById("megasoftTitle");
    span.style.fontSize = "x-small";

    //On Load Check if source warehouse is multibin
    var e = document.getElementById("ddlWarehouse");
    var Warehouse = e.options[e.selectedIndex].value;
    checkMultiBins(Warehouse);

    //When warehouse changed, check if source warehouse is multibin
    $("#ddlWarehouse").change(function () {
        var e = document.getElementById("ddlWarehouse");
        var Warehouse = e.options[e.selectedIndex].value;
        checkMultiBins(Warehouse);
        count = 0;
    });

    $('#txtStockCode').on('change', function (e) {
        ValidateBarcode();   
    });


    $('#tblLines').on("click", ".delLine", function () {

        $('#errordiv').text("");
        $('#errordiv').removeClass("alert alert-danger");

        var $row = $(this).closest("tr").attr('StockTake');    // Find the row
       
        var e = document.getElementById("ddlWarehouse");
        var Warehouse = e.options[e.selectedIndex].value;


        //alert($(this).closest("tr").find('td:eq(0)').text());
        var Id = $(this).closest("tr").find('td:eq(0)').text()

        var mydata = [];
        var myObject = new Object();     

        myObject.Warehouse = Warehouse;
        myObject.Id = Id;
       
        mydata.push(myObject);  

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;
       
            $.ajax({
                type: "POST",
                url: "WhseManPalletAudit/DeleteItem",
                data: exportdata,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: OnSuccess,
                failure: function () {
                    alert(response);
                }
            });

            function OnSuccess(response) {
                if(response =="Deleted Successfully.")
                {
                    
                    $('#errordiv').text(response);
                    $('#errordiv').addClass("alert alert-danger");

                    $("#tblLines tbody>tr").remove();
                    GetLast3Scanned();

                }
                else
                {
                    $('#errordiv').text(response);
                    $('#errordiv').addClass("alert alert-danger");
                }
        }
            if (count > 0)
            {
                count = count - 1;
                $('#txtCount').html('Total:' + count);
            }     
    });


});

function GetLast3Scanned() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");

    var e = document.getElementById("ddlWarehouse");
    var Warehouse = e.options[e.selectedIndex].value;

    var myString = JSON.stringify({
        Warehouse: Warehouse
    });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "WhseManPalletAudit/GetLast3Scans",
        contentType: "application/json; charset=utf-8",
        data: exportdata,
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });

    function OnSuccess(response)
    {
        var trHTML = '';
        $.each(response, function (index) {
            
            trHTML += '<tr><td class="nowrap" style="display:none"">' + response[index].ID + '</td><td class="nowrap">' + response[index].StockCode + '</td><td class="nowrap">' + response[index].Bin + '</td><td class="nowrap">' + response[index].Lot + '</td><td>' + response[index].Quantity + '</td>' +
                '<td><a href="#" class="delLine btn btn-danger btn-xs" tabindex="-1"><span class="fa fa-trash-o aria-hidden="true" title="Delete Line" tabindex="-1"></span></a></td></tr>';
        });
        

        $('#tblLines').append(trHTML);
        $("#txtStockCode").focus();
    }
}
