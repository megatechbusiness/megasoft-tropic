
function ValidateBarcode() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");

    var str = document.getElementById('txtBarcode').value;
    //If Megasoft Barcode
    if (str.indexOf("|") > 0) {
        var Values = document.getElementById('txtBarcode').value.split("|");
        document.getElementById('txtStockCode').value = Values[0];
        document.getElementById('txtLot').value = Values[4];
        document.getElementById('txtQuantity').value = Values[2];


        var mydata = [];
        var myObject = new Object();
        var e = document.getElementById("ddlWarehouse");
        var SourceWarehouse = e.options[e.selectedIndex].value;

        var f = document.getElementById("ddlSourceBin");
        var SourceBin = f.options[f.selectedIndex].value;

        var g = document.getElementById("ddlDestinationBin");
        var DestinationBin = g.options[g.selectedIndex].value;

        myObject.Warehouse = SourceWarehouse;
        myObject.SourceBin = SourceBin;
        myObject.DestinationBin = DestinationBin;
        myObject.StockCode = Values[0];
        myObject.LotNumber = Values[4];
        myObject.Quantity = Values[2]

        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "BinTransfer/ValidateDetails",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: OnErrorCall
        });

        function OnSuccess(response) {
            alert(response);
            if (response != "") {
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                alert(response);
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
                document.getElementById('txtBarcode').focus();
            }
            else {
                document.getElementById("txtBarcode").focus();
                AddLine();
            }

        }

        function OnErrorCall(response) {
            if (response != "") {
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                alert(response);
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
                document.getElementById('txtBarcode').focus();
            }
            else {
                document.getElementById("btnPost").focus();
            }
        }
    }
    else {

        var mydata = [];
        var myObject = new Object();
        myObject.Barcode = document.getElementById('txtBarcode').value;

        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "BinTransfer/GetStockAndSupplier",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: On2Success,
            failure: On2ErrorCall
        });

        function On2Success(response) {
            if (response[0].StockCode == null)
            {
                document.getElementById('txtStockCode').value = document.getElementById('txtBarcode').value;
                document.getElementById('hdSupplier').value = "";
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtLot').focus();
            }
            else {
                
                document.getElementById('txtStockCode').value = response[0].StockCode;
                document.getElementById('hdSupplier').value = response[0].Supplier;
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtLot').focus();
            }
            

        }

        function On2ErrorCall(response) {
            alert(response);
        }
    }
    
    
    

}

function checkMultiBins(Warehouse, WhSource)
{
    var mydata = [];
    var myObject = new Object();
    myObject.Warehouse = Warehouse;

    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "BinTransfer/CheckWarehouseMultiBin",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response == "Y") {
            if(WhSource == "Source")
            {
                $('#bin').show();
            }
            else
            {
                $('#destinationbin').show();
            }
        }
        else {
            if (WhSource == "Source") {
                $('#bin').hide();
            }
            else {
                $('#destinationbin').hide();
            }
        }

    }

    function OnErrorCall(response) {
        if (response != "") {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
        }
    }
}


function PostBinTransfer()
{
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    showprogressbar();

    var e = document.getElementById("ddlWarehouse");
    var sw = e.options[e.selectedIndex].text.split(" - ");
    var SourceWarehouse = sw[0];

    var f = document.getElementById("ddlSourceBin");
    var SourceBin = f.options[f.selectedIndex].value;

    var g = document.getElementById("ddlDestinationBin");
    var DestinationBin = g.options[g.selectedIndex].value;

   // var SourceBin = document.getElementById('txtSourceBin').value


    var mydata = [];
    $("#tblLines > tbody > tr ").each(function (i, el) {
        var $tds = $(this).find('td')
        var StockCode = $tds.eq(0).text();
        var Lot = $tds.eq(1).text();;
        var Quantity = $tds.eq(2).text();;

        var myObject = new Object();
        myObject.Warehouse = SourceWarehouse;
        myObject.SourceBin = SourceBin;
        myObject.DestinationBin = DestinationBin;
        myObject.StockCode = StockCode;
        myObject.LotNumber = Lot;
        myObject.Quantity = Quantity;
        

        mydata.push(myObject);
    });

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "BinTransfer/PostBinTransfer",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response != "") {
            $('#txtBarcode').val("");
            $('#txtStockCode').val("");
            $('#txtLot').val("");
            $('#txtQuantity').val("");
           // document.getElementById('txtSourceBin').value = "";
            //  document.getElementById('txtDestinationBin').value = "";
            
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
            if (response.indexOf("Posting Complete.") == 0) {
                $('#tblLines tbody').empty();
            }
            
        }
        hideprogressbar();
    }

    function OnErrorCall(response) {
        if (response != "") {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");

        }
        hideprogressbar();
    }
}


function AddLine() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    //showprogressbar();

    var mydata = [];

    var StockCode = document.getElementById('txtStockCode').value;
    var Lot = document.getElementById('txtLot').value;
    var Quantity = document.getElementById('txtQuantity').value;
    var AddReel = true;

    var e = document.getElementById("ddlWarehouse");
    var sw = e.options[e.selectedIndex].text.split(" - ");
    var SourceWarehouse = sw[0];

    var f = document.getElementById("ddlSourceBin");
    var SourceBin = f.options[f.selectedIndex].value;

    var g = document.getElementById("ddlDestinationBin");
    var DestinationBin = g.options[g.selectedIndex].value;

    if (Quantity.trim() == "") {
        alert("Quantity missing.");
        AddReel = false;
    }

    var myObject = new Object();
    myObject.Warehouse = SourceWarehouse;
    myObject.SourceBin = SourceBin;
    myObject.DestinationBin = DestinationBin;
    myObject.StockCode = StockCode;
    mydata.push(myObject);


    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "BinTransfer/CheckStockCodeBin",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response == "") {
            $("#tblLines > tbody > tr ").each(function (i, el) {
                var $tds = $(this).find('td');
                var ReelNumber = $tds.eq(1).text();
                if (Lot == ReelNumber) {
                    AddReel = false;
                    alert("Reel " + Lot + " already scanned.");
                    return;
                }
            });

            if (AddReel == true) {
                $('#tblLines tbody').append("<tr class='nr'><td>" + StockCode +
                "</td><td>" + Lot + "</td><td>" + Quantity +
                "</td><td><a href='#' class='delLine btn btn-danger btn-xs' tabindex='-1'><span class='fa fa-trash-o' aria-hidden='true' title='Delete Line' tabindex='-1'></span></a></td></tr>");
            }

            $('#txtBarcode').val("");
            $('#txtStockCode').val("");
            $('#txtLot').val("");
            $('#txtQuantity').val("");
            document.getElementById('txtBarcode').focus();


            var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
            $('#txtCount').html('Total:' + rows);

        //    hideprogressbar();
        }
        else {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
        }
      //  hideprogressbar();
    }

    function OnErrorCall(response) {
        if (response != "") {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");

        }
      //  hideprogressbar();
    }

}

$(document).ready(function () {

    //change page title text
    $('#megasoftTitle').text(' Megasoft - Location');
    var span = document.getElementById("megasoftTitle");
    span.style.fontSize = "x-small";

    //
    //var target = $('a[name=firstrow]');
    //if (target.length) {
    //    var top = target.offset().top;
    //    $('html,body').animate({ scrollTop: top }, 1000);
    //    return false;
    //}



    //On Load Check if source warehouse is multibin
    //if ($('#ddlSource').val() == "Y")
    //{
    //    $('#bin').show();
        
    //}
    //else {
    //    $('#bin').hide();
    //}

    ////On Load Check if destination warehouse is multibin
    //if ($('#ddlDestination').val() == "Y") {
    //    $('#destinationbin').show();

    //}
    //else {
    //    $('#destinationbin').hide();
    //}

    //On Load Check if source warehouse is multibin
    var e = document.getElementById("ddlWarehouse");
    var SourceWarehouse = e.options[e.selectedIndex].value;
    checkMultiBins(SourceWarehouse, "Source");
    GetBins(SourceWarehouse);

    //On Load Check if destination warehouse is multibin
    //var f = document.getElementById("ddlDestination");
    //var DestWarehouse = f.options[f.selectedIndex].value;
    //checkMultiBins(DestWarehouse, "Destination");


    //When warehouse changed, check if source warehouse is multibin
    $("#ddlWarehouse").change(function () {
        var e = document.getElementById("ddlWarehouse");
        var SourceWarehouse = e.options[e.selectedIndex].value;
        checkMultiBins(SourceWarehouse, "Source");
        GetBins(SourceWarehouse);
    });

    //When warehouse changed, check if destination warehouse is multibin
   // $("#ddlDestination").change(function () {
   //     var f = document.getElementById("ddlDestination");
    //    var DestWarehouse = f.options[f.selectedIndex].value;
    //    checkMultiBins(DestWarehouse, "Destination");
   // });



    $('#txtLot').on('change', function (e) {
        if($('#hdSupplier').val() == "MON001") {
            var str = document.getElementById('txtLot').value;
            document.getElementById('txtLot').value = str.substring(0, 9);
            document.getElementById('txtQuantity').value = str.substring(9, 13);
            document.getElementById("btnPost").focus();
            AddLine();
        }
    });


    $('#tblLines').on("click", ".delLine", function () {
        $(this).closest("tr").remove();
        var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
        $('#txtCount').html('Total:' + rows);
    });


});
function GetBins(Warehouse) {
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
        url: "BinTransfer/GetBins",
        contentType: "application/json; charset=utf-8",
        data:exportdata,
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });

    function OnSuccess(response) {
        var ddlSourceBin = $("#ddlSourceBin");
        var ddlDestinationBin = $("#ddlDestinationBin");

        ddlSourceBin.empty();
        ddlDestinationBin.empty();
        if (response != "") {
            

            //ddlSourceBin.prepend("<option value='' selected='selected'></option>");
            //ddlDestinationBin.prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                //alert(this['Value'] + ' space ' + this['Text']);
                ddlSourceBin.append($("<option></option>").val(this['ID'].trim()).html(this['Text']));
                ddlDestinationBin.append($("<option></option>").val(this['ID'].trim()).html(this['Text']));
            });

        }
        else
        {
            $('#errordiv').text("No Bin found for Warehouse");
            $('#errordiv').addClass("alert alert-danger");
        }
    }
}