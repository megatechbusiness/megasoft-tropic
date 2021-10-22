//------------------------------------------------------------------------------------------------------------------------
//Progress Bar Functions
var waitingDialog;
waitingDialog = waitingDialog || (function () {


    var $pleaseWaitDiv = $('<div class="modal" id="pleaseWaitDialog"  data-backdrop="static" tabindex="-1" data-focus-on="input:first" style="display: none;"><div class="modal-dialog"><div class="modal-content"><div class="modal-header"><img class="img-responsive" src="Images/Megatech/megatech_logo_Square.png" alt="image" style="max-height:200px;"/></div><div class="modal-body"><h1>Processing...</h1><div class="progress"><div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="45" aria-valuemin="0" aria-valuemax="100" style="width: 100%"><span class="sr-only">45% Complete</span></div></div></div></div></div></div>');
    return {
        showPleaseWait: function () {
            $pleaseWaitDiv.modal();
        },
        hidePleaseWait: function () {
            $pleaseWaitDiv.modal('hide');
        },
    };
})();
//---------------------------------------------------------------------------------------------------------------------------

function closeJobModal(Job) {

    document.getElementById("MainContent_txtJob").value = Job;
    $('#JobModal').modal('hide');

}


function openJobModal() {

    $('#JobModal').modal('show');

}

function LoadJob() {
    $('#txtReceipt').val('');
    document.getElementById("txtReceipt").readOnly = false;
    $("#gdvLot > tbody").html("");
    $('#Lotdiv').hide();

    waitingDialog.showPleaseWait();
    var mydata = [];
    var myObject = new Object();

    myObject.Job = $('#MainContent_txtJob').val();
    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;
    //alert(exportdata);

    $.ajax({
        type: "POST",
        url: "JobReceipt.aspx/LoadJob",
        contentType: "application/json; charset=utf-8",
        data: exportdata,
        dataType: "json",
        success: OnPostAdd,
        failure: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.responseText);
            waitingDialog.hidePleaseWait();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.responseText);
            waitingDialog.hidePleaseWait();
        }
    });

    function OnPostAdd(response) {
        if (typeof response === 'object') {
            $.each(response.d, function () {
                $('#txtJob').val(this['Job']);
                $('#txtDescription').val(this['Description']);
                $('#txtStockCode').val(this['StockCode']);
                $('#txtStockDesc').val(this['StockDesc']);
                $('#txtQtyToMake').val(this['QtyToMake']);
                $('#txtQtyManu').val(this['QtyManufactured']);
                $('#txtOutstanding').val(this['QtyOutstanding']);
                $('#txtTraceable').val(this['Traceable']);
                if (this['Traceable'] === 'T') {
                    var v1 = document.getElementById("txtReceipt");
                    v1.setAttribute("readOnly", "true");

                    $('#Lotdiv').show();
                    var row = $('#gdvDisplay tr').length;
                    //alert(r);
                    if (row === 0) { appendTable(1); }  
                }    
            });
            waitingDialog.hidePleaseWait();
            $('#ddlShift').focus();
        }
        else {
            alert(response.d);
            waitingDialog.hidePleaseWait();
        }
    }


}


function PostJob() {
    waitingDialog.showPleaseWait();
    var mydata = [];

    var f = document.getElementById("ddlShift");
    var Shift = f.options[f.selectedIndex].value;

    var Traceable = $('#txtTraceable').val();

    if (Traceable ==='T')
    {
        var table = document.getElementById("gdvLot");
        var column_count = table.rows[1].cells.length;

        if (column_count > 0) {
            $("#gdvLot").find('tr.nr').each(function () {
                var myTraceableObject = new Object();
                myTraceableObject.Job = $('#MainContent_txtJob').val();
                myTraceableObject.ReceiptQty = $(this).find('td:eq(2) input[type="text"]').val();;
                myTraceableObject.TrnDate = $('#txtDate').val();
                myTraceableObject.Shift = Shift;
                myTraceableObject.Reference = $('#txtRef').val();
                myTraceableObject.Traceable = $('#txtTraceable').val();
                myTraceableObject.Lot = $(this).find('td:eq(1) input[type="text"]').val();
               // myTraceableObject.LotQuantity = $(this).find('td:eq(2) input[type="text"]').val();

                mydata.push(myTraceableObject);
            });
        }
    }
        else
    {
        var myObject = new Object();
        myObject.Job = $('#MainContent_txtJob').val();
        myObject.ReceiptQty = $('#txtReceipt').val();
        myObject.TrnDate = $('#txtDate').val();
        myObject.Shift = Shift;
        myObject.Reference = $('#txtRef').val();
        myObject.Traceable = $('#txtTraceable').val();
        mydata.push(myObject);
    }

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "JobReceipt.aspx/PostJob",
        contentType: "application/json; charset=utf-8",
        data: exportdata,
        dataType: "json",
        success: OnPost,
        failure: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.responseText);
            waitingDialog.hidePleaseWait();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.responseText);
            waitingDialog.hidePleaseWait();
        }
    });

    function OnPost(response) {
        if (response.d.startsWith('Error'))
        {
            alert(response.d);
        }
        else
        {
            // LoadJob();
            $('#MainContent_txtJob').val('');
            $('#MainContent_txtJob').focus();
            $('#txtDescription').val('');
            $('#txtStockCode').val('');
            $('#txtStockDesc').val('');
            $('#txtQtyToMake').val('');
            $('#txtQtyManu').val("");
            $('#txtOutstanding').val('');
            $('#txtReceipt').val('');
            $('#txtTraceable').val('');
            $('#txtRef').val('');
            $("#gdvLot > tbody").html("");
            $('#Lotdiv').hide();
    
        }
        alert(response.d);
        waitingDialog.hidePleaseWait();
        
    }
    


}


function GetShifts() {

    $.ajax({
        type: "POST",
        url: "MaterialIssue.aspx/GetShifts",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });
    function OnSuccess(response) {
        if (response.d) {
            var ddlShift = $("#ddlShift");

            ddlShift.empty();
            $("#ddlShift").prepend("<option value='' selected='selected'></option>");

            $.each(response.d, function () {
                //alert(this['Value'] + ' space ' + this['Text']);
                ddlShift.append($("<option></option>").val(this['Value'].trim()).html(this['Text']));
            });
        }
    }
}


$(document).ready(function () {
    var r = $('#gdvLot tr').length;
    //Add new blank row
   // appendTable(r);
    tabletab();
    //DatePicker
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();

    if (dd < 10) {
        dd = '0' + dd
    }

    if (mm < 10) {
        mm = '0' + mm
    }

    today = yyyy + '-' + mm + '-' + dd;

    $('#txtDate').val(today);
    $(".datepicker").datepicker({ format: 'yyyy-mm-dd', autoclose: true, todayBtn: 'linked' });
    
    GetShifts();
    $(".js-example-basic-single").select2({
        placeholder: "",
        allowClear: true
    });
    $.fn.select2.defaults.set("", "");

});
function deleteRow(lnk) {
    // alert(lnk);
    var row = lnk.parentNode.parentNode;
    var rowIndex = row.rowIndex;
    if (rowIndex == 1) {
    }
    else {
        document.getElementById('gdvLot').deleteRow(rowIndex);
    }
}
function appendTable(r) {
    $('#gdvLot tbody').append(
        "<tr class='nr'><td>" + r + "</td><td><div class='input-group add-on'><input class='form-control input-sm' id='tbLot_" + r + "' type='text' value=''/>" +
        "</td><td><div class='input-group add-on'>" +
        " <input class='tab form-control input-sm ' id=tbQuantity_" + r + " type='text' value=''/>" +
        "</div></td>" +
        "<td><a href='#' class='ref_" + r + " delLine btn btn-danger btn-xs' onclick='deleteRow(this)' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td></tr>");

}
function tabletab() {
    $(document).on('keydown change', '#gdvLot tr .tab', function (e) {
        var keyCode = e.keyCode || e.which;
        var r = $('#gdvLot tr').length;
        i = r;
        // alert(i);
        //check if tb exists(for delete purposes)if row is deleted next Id will be duplicated
        var myEle = document.getElementById("tbLot_" + i);
        if (myEle) {
            for (var index = 1; index < 15; index++) {
                if (document.getElementById("tbLot_" + index)) { }
                else {
                    var row = index;
                    r = row;
                    break;
                }
            }
        }
        if (keyCode === 9) {
            appendTable(r);
        }
    });
}