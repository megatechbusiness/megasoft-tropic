
function LoadReference()
{
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    var e = document.getElementById("ddlSource");
    var SourceWarehouse = e.options[e.selectedIndex].value;
    $('#GtrRef').empty();
    $.getJSON('TransferIn/GetReferences?Warehouse=' + SourceWarehouse, function (data) {
        
        $.each(data, function (i, Ref) {
            $('#GtrRef').append($('<option></option>').val(Ref.GtrReference).html(Ref.GtrReference));
        });
    });
}


function LoadDetail() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    $('#tblLines tbody').empty();
    window.setTimeout(function () {
        var e = document.getElementById("GtrRef");
        var GtrRef = e.options[e.selectedIndex].value;
        var c = 0;
        $.getJSON('TransferIn/GetReferenceDetail?GtrReference=' + GtrRef, function (data) {
            
            $.each(data, function (i, Ref) {
                $('#tblLines tbody').append("<tr><td>" + Ref.Line + "</td><td>" + Ref.StockCode + "</td><td>" + Ref.ReelNo + "</td><td>" + Ref.Quantity + "</td></tr>");
                c++;
                $('#txtCount').html('Total:' + c);
            });
        });
        //var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;
        
    }, 500);
    
    
}


function PostTransferIn()
{
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    showprogressbar();
    var mydata = [];
    var myObject = new Object();
    var e = document.getElementById("ddlSource");
    var SourceWarehouse = e.options[e.selectedIndex].value;
    var f = document.getElementById("GtrRef");
    var GtrRef = f.options[f.selectedIndex].value;
    myObject.Warehouse = SourceWarehouse;
    myObject.GtrReference = GtrRef;


    

    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    

    $.ajax({
        type: "POST",
        url: "TransferIn/PostWarehouseTransfer",
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
            $("#tblLines > tbody > tr ").empty();
        }
    }

    function OnErrorCall(response) {
        hideprogressbar();
        $('#errordiv').text(response);
        $('#errordiv').addClass("alert alert-danger");        
    }
}

$(document).ready(function () {

    //change page title text
    $('#megasoftTitle').text(' Megasoft - Transfer in');
    var span = document.getElementById("megasoftTitle");
    span.style.fontSize = "x-small";

    LoadReference();

    LoadDetail()


    //When warehouse changed, check if source warehouse is multibin
    $("#ddlSource").change(function () {
        LoadReference();

        LoadDetail()
    });

    $("#GtrRef").change(function () {
        LoadDetail()
    });


});