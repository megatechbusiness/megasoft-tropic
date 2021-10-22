
function AddLine()
{
    var AddReel = true;
    var rowCheck = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
    if (rowCheck == 1)
    {
        
        alert('Receipt one item at a time!');
        AddReel = false;
    }


    if ($('#txtJob').val() == "")
    {
        alert('StockCode missing.');
    }

    if ($('#txtLot').val() == "") {
        alert('Pallet missing.');
    }

    if ($('#txtQuantity').val() == "") {
        alert('Quantity missing.');
    }


    var Job = document.getElementById('txtJob').value;
    var Lot = document.getElementById('txtLot').value;
    var Quantity = document.getElementById('txtQuantity').value;
    

    
    $("#tblLines > tbody > tr ").each(function (i, el) {
        var $tds = $(this).find('td');
        var ReelNumber = $tds.eq(1).text();
        if (Lot == ReelNumber) {
            AddReel = false;
            alert("Pallet " + Lot + " already scanned.");
            return;
        }
    });

    if (AddReel == true) {
        $('#tblLines tbody').append("<tr class='nr'><td>" + Job +
        "</td><td>" + Lot + "</td><td>" + Quantity +
        "</td><td><a href='#' class='delLine btn btn-danger btn-xs' tabindex='-1'><span class='fa fa-trash-o' aria-hidden='true' title='Delete Line' tabindex='-1'></span></a></td></tr>");
    }

    document.getElementById('txtBarcode').value = "";
    document.getElementById('txtBarcode').focus();

    var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
    $('#txtCount').html('Total:' + rows);
}


function PostReceipt()
{
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    showprogressbar();    


    var mydata = [];
    $("#tblLines > tbody > tr ").each(function (i, el) {
        var $tds = $(this).find('td')
        var Job = $tds.eq(0).text();
        var Lot = $tds.eq(1).text();;
        var Quantity = $tds.eq(2).text();;

        var myObject = new Object();
        myObject.Job = Job;
        myObject.Lot = Lot;
        myObject.Quantity = Quantity;

        mydata.push(myObject);
    });

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "WhseManProductionReceipt/PostJobReceipt",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: OnErrorCall
    });

    function OnSuccess(response) {
        if (response != "") {
            document.getElementById('txtBarcode').value = "";
            document.getElementById('txtJob').value = "";
            document.getElementById('txtLot').value = "";
            document.getElementById('txtQuantity').value = "";
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
            if (response.indexOf("Job Receipt Completed Successfully.") == 0) {
                $('#tblLines tbody').empty();
                $('#txtBarcode').focus();
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
    $('#txtBarcode').focus();
}

$(document).ready(function () {
    $('#txtBarcode').on('change', function (e) {

        var str = document.getElementById('txtBarcode').value;
        //alert(str);
        //If Megasoft Barcode
        if (str.indexOf("|") > 0) {
            var Values = document.getElementById('txtBarcode').value.split("|");
            document.getElementById('txtJob').value = Values[6];
            document.getElementById('txtLot').value = Values[4];
            document.getElementById('txtQuantity').value = Values[2];
        }
        else
        {
            alert('No Job Found. Invalid barcode scanned.');
            
        }

        AddLine();
    });

    $('#tblLines').on("click", ".delLine", function () {
        $(this).closest("tr").remove();
        var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
        $('#txtCount').html('Total:' + rows);
        $('#txtBarcode').focus();
    });
});