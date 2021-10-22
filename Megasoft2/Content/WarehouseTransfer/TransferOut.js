
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
        var e = document.getElementById("ddlSource");
        var SourceWarehouse = e.options[e.selectedIndex].value;
        myObject.SourceWarehouse = SourceWarehouse;
        myObject.StockCode = Values[0];
        myObject.LotNumber = Values[4];
        myObject.Quantity = Values[2]

        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "WarehouseTransfer/ValidateDetails",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: OnErrorCall
        });

        function OnSuccess(response) {
            if (response != "") {
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
                document.getElementById('txtBarcode').focus();
            }
            else {
                document.getElementById("ddlDestination").focus();
                AddTransferOutLine();
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                document.getElementById('txtBarcode').focus();
            }

        }

        function OnErrorCall(response) {
            if (response != "") {
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
                document.getElementById('txtBarcode').focus();
            }
            else {
                document.getElementById("ddlDestination").focus();
                AddTransferOutLine();
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtStockCode').value = "";
                document.getElementById('txtLot').value = "";
                document.getElementById('txtQuantity').value = "";
                document.getElementById('txtBarcode').focus();
            }
        }
    }
    else {


        var mydata = [];
        var myObject = new Object();
        myObject.Barcode = document.getElementById('txtBarcode').value;
        var e = document.getElementById("ddlSource");
        var sw = e.options[e.selectedIndex].text.split(" - ");
        var SourceWarehouse = sw[0];
        myObject.SourceWarehouse = SourceWarehouse;

        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "WarehouseTransfer/GetStockAndSupplier",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: On2Success,
            failure: On2ErrorCall
        });

        function On2Success(response) {
            if (response[0].StockCode == null) {
                document.getElementById('txtStockCode').value = document.getElementById('txtBarcode').value;
                document.getElementById('hdSupplier').value = "";
                document.getElementById('txtBarcode').value = "";
                document.getElementById('txtLot').focus();
            }
            else if (response[0].ReelNumber != null) {

                //Assume Lot exists in File Import
                document.getElementById('txtStockCode').value = response[0].StockCode;
                document.getElementById('txtLot').value = response[0].ReelNumber;
                document.getElementById('txtQuantity').value = response[0].ReelQuantity;
                AddTransferOutLine();
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

function checkMultiBins(Warehouse, WhSource) {
    var mydata = [];
    var myObject = new Object();
    myObject.Warehouse = Warehouse;

    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "TransferOut/CheckWarehouseMultiBin",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        //alert(response);
        if (response == "Y") {
            if (WhSource == "Source") {
                $('#bin').show();
            }
            else {
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

function AddTransferOutLine() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    showprogressbar();



    var StockCode = document.getElementById('txtStockCode').value;
    var Lot = document.getElementById('txtLot').value;
    var Quantity = document.getElementById('txtQuantity').value;
    var AddReel = true;

    if (Quantity.trim() == "") {
        alert("Quantity missing.");
        AddReel = false;
    }

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


    document.getElementById('txtBarcode').value = "";
    document.getElementById('txtStockCode').value = "";
    document.getElementById('txtLot').value = "";
    document.getElementById('txtQuantity').value = "";
    document.getElementById('txtBarcode').focus();


    var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
    $('#txtCount').html('Total:' + rows);

    hideprogressbar();
}


function PostTransferOut() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    showprogressbar();

    var mydata = [];
    $("#tblLines > tbody > tr ").each(function (i, el) {

        var myObject = new Object();
        var $tds = $(this).find('td')
        var e = document.getElementById("ddlSource");
        var sw = e.options[e.selectedIndex].value;
        var SourceWarehouse = sw;
        var SourceBin = document.getElementById('txtSourceBin').value
        var StockCode = $tds.eq(0).text();
        var ReelQty = $tds.eq(2).text();
        var ReelNumber = $tds.eq(1).text();
        var f = document.getElementById("ddlDestination");
        var dw = f.options[f.selectedIndex].value;
        var DestinationWarehouse = dw;
        var DestinationBin = document.getElementById('txtDestinationBin').value;

        myObject.SourceWarehouse = SourceWarehouse;
        myObject.SourceBin = SourceBin;
        myObject.StockCode = StockCode;
        myObject.Quantity = ReelQty;
        myObject.LotNumber = ReelNumber;
        myObject.DestinationWarehouse = DestinationWarehouse;
        myObject.DestinationBin = DestinationBin;
        mydata.push(myObject);


    });




    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;



    $.ajax({
        type: "POST",
        url: "TransferOut/PostTransferOut",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        hideprogressbar();
        $('#errordiv').text(response);
        $('#errordiv').addClass("alert alert-danger");
        if (response.indexOf("Posting Complete.") == 0) {
            $('#tblLines tbody').empty();
        }


    }

    function OnErrorCall(response) {
        $('#errordiv').text(response);
        $('#errordiv').addClass("alert alert-danger");
        hideprogressbar();
    }

}



$(document).ready(function () {

    //change page title text
    $('#megasoftTitle').text(' Megasoft - Transfer out');
    var span = document.getElementById("megasoftTitle");
    span.style.fontSize = "x-small";


    //On Load Check if source warehouse is multibin
    var e = document.getElementById("ddlSource");
    var SourceWarehouse = e.options[e.selectedIndex].value;
    checkMultiBins(SourceWarehouse, "Source");

    //On Load Check if destination warehouse is multibin
    var f = document.getElementById("ddlDestination");
    var DestWarehouse = f.options[f.selectedIndex].value;
    checkMultiBins(DestWarehouse, "Destination");


    //When warehouse changed, check if source warehouse is multibin
    $("#ddlSource").change(function () {
        var e = document.getElementById("ddlSource");
        var SourceWarehouse = e.options[e.selectedIndex].value;
        checkMultiBins(SourceWarehouse, "Source");
    });

    //When warehouse changed, check if destination warehouse is multibin
    $("#ddlDestination").change(function () {
        var f = document.getElementById("ddlDestination");
        var DestWarehouse = f.options[f.selectedIndex].value;
        checkMultiBins(DestWarehouse, "Destination");
    });


    //To change quantity after item scanned
    //Scan in StockCode textbox
    //Click add line button to add line manually
    $('#txtStockCode').on('change', function (e) {
        var str = document.getElementById('txtStockCode').value;
        if (str.indexOf("|") > 0) {
            var Values = document.getElementById('txtStockCode').value.split("|");
            document.getElementById('txtStockCode').value = Values[0];
            document.getElementById('txtLot').value = Values[4];
            document.getElementById('txtQuantity').value = Values[2];
        }
    });

    $('#tblLines').on("click", ".delLine", function () {
        $(this).closest("tr").remove();
        var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
        $('#txtCount').html('Total:' + rows);
    });

    $('#txtLot').on('change', function (e) {
        if ($('#hdSupplier').val() == "MON001") {
            var str = document.getElementById('txtLot').value;
            document.getElementById('txtLot').value = str.substring(0, 9);
            document.getElementById('txtQuantity').value = str.substring(9, 13);
            AddTransferOutLine();
        }
        else if ($('#hdSupplier').val() == "KOR001") { //Newsprint - 
            var str = document.getElementById('txtLot').value;
            document.getElementById('txtLot').value = str.substring(0, 8);
            document.getElementById('txtQuantity').value = str.substring(8, 12);
            AddTransferOutLine();

        }

        //Jackaroo has seperate barcode for reel and seperate barcode for Qty - No decoding required.
        //MPact has seprate barcode for reel. Qty barcode need to be decoded.
    });

    $('#txtQuantity').on('change', function (e) {
        if ($('#hdSupplier').val() == "MON007") //MPact Decode Qty Barcode
        {
            var str = document.getElementById('txtLot').value;
            document.getElementById('txtQuantity').value = str.substring(9, 13);
            AddTransferOutLine();
        }
        else if ($('#hdSupplier').val() == "JAC002") //MPact Decode Qty Barcode
        {
            AddTransferOutLine();
        }
    });
});