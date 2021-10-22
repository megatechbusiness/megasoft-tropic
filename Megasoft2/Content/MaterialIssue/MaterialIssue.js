function PostMaterialIssue() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    showprogressbar();

    var mydata = [];
    $("#tblLines > tbody > tr ").each(function (i, el) {

        var myObject = new Object();
        var $tds = $(this).find('td');
        var Job = document.getElementById('txtJob').value;
        var Warehouse = $tds.eq(0).text();
        var Bin = $tds.eq(1).text();
        var StockCode = $tds.eq(2).text();
        var Lot = $tds.eq(3).text();
        var Quantity = $tds.eq(4).text();
        var Printer = $('#ddlPrinter').val();

        myObject.Job = Job;
        myObject.Warehouse = Warehouse;
        myObject.Bin = Bin;
        myObject.StockCode = StockCode;
        myObject.Quantity = Quantity;
        myObject.LotNumber = Lot;
        myObject.Printer = Printer;
        mydata.push(myObject);


    });

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;
    //alert(exportdata);
    $.ajax({
        type: "POST",
        url: "WhseManMaterialIssue/PostMaterialIssue",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response != "") {
            document.getElementById('txtJob').value = "";
            document.getElementById('txtBin').value = "";
            document.getElementById('txtStockCode').value = "";
            document.getElementById('txtLot').value = "";
            document.getElementById('txtQuantity').value = "";
            if (response.indexOf('Material Issue posted successfully') === 0) {
                $('#tblLines tbody').empty();
            }
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");

            $('#txtJob').removeAttr("disabled");

        }
        else {
            $('#tblLines tbody').empty();
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


function AddItem() {
    var AddReel = true;
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");



    var Job = document.getElementById('txtJob').value;
    var f = document.getElementById("ddlWarehouse");
    var Warehouse = f.options[f.selectedIndex].value;
    var Bin = document.getElementById('txtBin').value;
    var StockCode = document.getElementById('txtStockCode').value;
    var Lot = document.getElementById('txtLot').value;
    var Quantity = document.getElementById('txtQuantity').value;

    if ($('#MaterialReturn').is(":checked")) {
        Quantity = Quantity * -1;
    }

    if (StockCode == "") {
        alert("StockCode cannot be blank.");
        AddReel = false;
    }

    if (Lot == "") {
        alert("Lot cannot be blank.");
        AddReel = false;
    }

    if (Quantity == "") {
        alert("Quantity cannot be blank.");
        AddReel = false;
    }

    $("#tblLines > tbody > tr ").each(function (i, el) {
        var $tds = $(this).find('td');
        var ReelNumber = $tds.eq(2).text();
        if (Lot == ReelNumber) {
            AddReel = false;
            alert("Item " + Lot + " already scanned.");

        }
    });
    if ($('#MaterialReturn').is(":checked") == true) {
        if (parseFloat(Quantity) > -1) {
            alert("Quantity must be negative for material return.");
            AddReel = false;
        }
    }

    if (AddReel == true) {
        var mydata = [];
        var myObject = new Object();
        myObject.StockCode = StockCode;
        myObject.Quantity = Quantity;
        myObject.Job = Job;
        mydata.push(myObject);
        alert(StockCode);
        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url:  "../MaterialIssue/CalcNoOfLabels",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: OnErrorCall,
            async:false
        });

        function OnSuccess(response) {
            alert(response);
        $('#tblLines tbody').append("<tr class='nr'><td>" + Warehouse + "</td><td>" + Bin + "</td><td>" + StockCode + "</td><td>" + Lot + "</td><td>" + Quantity + "</td><td>"+response.NumberofLabels+"</td><td><a href='#' class='delLine btn btn-danger btn-xs'><span class='fa fa-trash-o' aria-hidden='true' title='Delete Line'></span></a></td></tr>");

        }

        function OnErrorCall(response) {
            if (response != "") {
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
            }
        }
    }

    document.getElementById('txtBarcode').value = "";
    document.getElementById('txtStockCode').value = "";
    document.getElementById('txtLot').value = "";
    document.getElementById('txtQuantity').value = "";
    $('#txtBarcode').focus();

    var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;

    if (rows > 0) {
        $('#txtJob').attr("disabled", "disabled");
    }


}

function ValidateBarcode() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");

    var str = document.getElementById('txtBarcode').value;

    var e = document.getElementById("ddlWarehouse");
    var SourceWarehouse = e.options[e.selectedIndex].value;


    //If Megasoft Barcode
    if (str.indexOf("|") > 0) {
        var Values = document.getElementById('txtBarcode').value.split("|");

        var StockCode = "";
        var Lot = "";
        var Quantity = "";
        var Pallet = "";

        //Check if old barcode or new barcode. new barcode starts with B| or P|
        var BarcodeCheck = str.substring(0, 2);
        if (BarcodeCheck == "B|" || BarcodeCheck == "P|") {
            if (BarcodeCheck == "B|") {
                StockCode = Values[1];
                Lot = Values[5];
                Quantity = Values[3];
            }
            else {
                //Pallet Barcode. Only need stockcode and Pallet
                StockCode = Values[1];
                Pallet = Values[8]
            }
        }
        else {
            //Old barcode was only batch barcodes. No Pallet barcodes. We are scanning a batch.
            StockCode = Values[0];
            Lot = Values[4];
            Quantity = Values[2];
        }

        if (BarcodeCheck != "P|") {
            document.getElementById('txtStockCode').value = StockCode;
            document.getElementById('txtLot').value = Lot;
            document.getElementById('txtQuantity').value = Quantity;


            var mydata = [];
            var myObject = new Object();

            myObject.SourceWarehouse = SourceWarehouse;
            myObject.StockCode = StockCode;
            myObject.LotNumber = Lot;
            myObject.Quantity = Quantity;

            mydata.push(myObject);

            var myString = JSON.stringify({ details: JSON.stringify(mydata) });
            var exportdata = myString;

            $.ajax({
                type: "POST",
                url: "MaterialIssue/ValidateDetails",
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
                    document.getElementById("txtQuantity").focus();
                    //AddItem();
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
                    document.getElementById("btnPost").focus();
                }
            }
        }
        else {
            //Get All Items From Pallet No
            GetPalletItems(StockCode, Pallet, SourceWarehouse, SourceWarehouse);

        }

    }
    else {

        var ScanSupplierBarcode = true;
        if (ScanSupplierBarcode == false) {
            alert("Invalid Barcode Scanned.");
            document.getElementById('txtBarcode').value = "";
            document.getElementById('txtBarcode').focus();
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
                    AddLine();
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

}

function GetPalletItems(StockCode, PalletNo, SrcWarehouse, DestWarehouse) {
    $.getJSON('WarehouseTransfer/GetPalletItems?StockCode=' + StockCode + '&PalletNo=' + PalletNo + '&SourceWarehouse=' + SrcWarehouse + '&DestWarehouse=' + DestWarehouse, function (data) {
        if (typeof data == 'object') {
            $.each(data, function (i, item) {
                $('#tblLines tbody').append("<tr class='nr'><td>" + SrcWarehouse + "</td><td>" + item.StockCode +
                    "</td><td>" + item.Lot + "</td><td>" + item.Quantity +
                    "</td><td><a href='#' class='delLine btn btn-danger btn-xs' tabindex='-1'><span class='fa fa-trash-o' aria-hidden='true' title='Delete Line' tabindex='-1'></span></a></td></tr>");
            });
            var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
            $('#txtCount').html('Total:' + rows);
        }
        else {
            if (data == "NONTRACEABLE") {
                document.getElementById('txtStockCode').value = StockCode;
                document.getElementById('txtLot').value = PalletNo;
                document.getElementById('txtQuantity').focus();
            }
            else {
                alert(data);
            }
        }
    });
}


function checkMultiBins(Warehouse) {
    var mydata = [];
    var myObject = new Object();
    myObject.Warehouse = Warehouse;

    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "../WarehouseTransfer/CheckWarehouseMultiBin",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response == "Y") {
            $('#bin').show();
        }
        else {
            $('#bin').hide();
        }

    }

    function OnErrorCall(response) {
        if (response != "") {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
        }
    }
}

$(document).ready(function () {

    if (screen.width < 300) {
        $('#above300width').hide();
        $('#below300width').show();
    }
    else {
        $('#above300width').show();
        $('#below300width').hide();
    }


    $('#tblLines').on("click", ".delLine", function () {
        $(this).closest('tr').remove();
        var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;

        if (rows == 0) {
            $('#txtJob').removeAttr("disabled");
        }
    });


    //On Load Check if source warehouse is multibin
    var f = document.getElementById("ddlWarehouse");
    var Warehouse = f.options[f.selectedIndex].value;
    checkMultiBins(Warehouse);


    //When warehouse changed, check if source warehouse is multibin
    $("#ddlWarehouse").change(function () {
        var e = document.getElementById("ddlWarehouse");
        var SourceWarehouse = e.options[e.selectedIndex].value;
        checkMultiBins(SourceWarehouse);
    });

    $('#txtBarcode').on('change', function (e) {
        ValidateBarcode();
    });

    $('#MaterialReturn').change(function () {
        if (this.checked) {
            $('#divPrint').show();
        }
        else {
            $('#divPrint').hide();
        }
    });

    $('#divPrint').hide();

});