
function LoadReference() {
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
                $('#tblLines tbody').append("<tr><td><input type='checkbox' disabled='disabled' id='scanned_" + c + "'/></td><td>" + Ref.Line + "</td><td>" + Ref.StockCode + "</td><td>" + Ref.ReelNo + "</td><td>" + Ref.Quantity + "</td></tr>");
                c++;
                $('#txtCount').html('Total:' + c);
            });
        });
        //var rows = document.getElementById('tblLines').getElementsByTagName("tbody")[0].getElementsByTagName("tr").length;

    }, 500);
    
    


}


function PostTransferIn() {
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


    var AllScanned = true;
    $("#tblLines > tbody > tr ").each(function (i, el) {
        var $tds = $(this).find('td');        
        if ($tds.find("input[type='checkbox']").is(":checked") == false)
        {
            AllScanned = false;
        }
    });


    if (AllScanned == true)
    {
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
    else
    {
        alert('Not all items scanned in.');
        hideprogressbar();
        document.getElementById('txtBarcode').value = "";
        $('#txtBarcode').focus();
    }

    
}

$(document).ready(function () {

    //change page title text
    $('#megasoftTitle').text(' Megasoft - Transfer in(2)');
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


    $('#txtBarcode').on('change', function (e) {
        var t = document.getElementById('tblLines');
        
        var StockCode = $(t.rows[1].cells[2]).text();
        
        var str = document.getElementById('txtBarcode').value;
        if (str.indexOf("|") > 0) {
            var Values = document.getElementById('txtBarcode').value.split("|");
            document.getElementById('txtBarcode').value = Values[4];
        }
        else if (StockCode.substring(0, 4) == 'MOND')
        {
        
            var Values = str.substring(0, 9);
            document.getElementById('txtBarcode').value = Values;
        }
        else if (StockCode.substring(0, 3) == 'JAC')
        {
            //Barcode will contain Lot
        }
        else if (StockCode.substring(0, 3) == 'KOR') {
            var Values = str.substring(0, 8);
            document.getElementById('txtBarcode').value = Values;
        }
        var LotFound = false;

        //find lot in grid       

        $("#tblLines > tbody > tr ").each(function (i, el) {
            
            var $tds = $(this).find('td');
            var Lot = $tds.eq(3).text();
            if(document.getElementById('txtBarcode').value == Lot)
            {
                //Lot found
                
                LotFound = true;
                $tds.find("input[type='checkbox']").prop('checked', true);

                var checked = $('#tblLines > tbody > tr ').find('input:checked').length;


                $('#txtScanCount').html('Scanned:' + checked);
                
            }
        });

        if(LotFound == false)
        {
            alert('Item not found!');
            
        }
        document.getElementById('txtBarcode').value = "";
        $('#txtBarcode').focus();
    });

});