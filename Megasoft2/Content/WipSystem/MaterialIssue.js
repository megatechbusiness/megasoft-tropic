//------------------------------------------------------------------------------------------------------------------------
//Progress Bar Functions
//var waitingDialog;



//waitingDialog = waitingDialog || (function () {


//    var $pleaseWaitDiv = $('<div class="modal" id="pleaseWaitDialog"  data-backdrop="static" tabindex="-1" data-focus-on="input:first" style="display: none;"><div class="modal-dialog"><div class="modal-content"><div class="modal-header"><img class="img-responsive" src="Images/Megatech/megatech_logo_Square.png" alt="image" style="max-height:200px;"/></div><div class="modal-body"><h1>Processing...</h1><div class="progress"><div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="45" aria-valuemin="0" aria-valuemax="100" style="width: 100%"><span class="sr-only">45% Complete</span></div></div></div></div></div></div>');
//    return {
//        showPleaseWait: function () {
//            $pleaseWaitDiv.modal();
//        },
//        hidePleaseWait: function () {
//            $pleaseWaitDiv.modal('hide');
//        },
//    };
//})();
//---------------------------------------------------------------------------------------------------------------------------

function closeJobModal(Job) {

    document.getElementById("txtJob").value = Job;
    $('#JobModal').modal('hide');

}

function openJobModal() {

    $('#JobModal').modal('show');

}

function closeAllocationModal() {
    $("#tblStock > tbody").empty();
    $('#txtSearchStockCode').val('');
    $('#AllocationModal').modal('hide');
    $('#hdselectedstockcode').val('');
}

function openAllocationModal() {
    GetWarehouses();
    $('#txtSearchStockCode').val('');
    $("#tblStock > tbody").empty();
    $('#AllocationModal').modal('show');
    $('#hdselectedstockcode').val('');
}

function LoadGrid() {

    $("#LotModals").html("");
    function getguid() { // Public Domain/MIT
        var d = new Date().getTime();
        if (typeof performance !== 'undefined' && typeof performance.now === 'function') {
            d += performance.now(); //use high-precision timer if available
        }
        return 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    }
    guid = getguid();
    // alert(guid);
    $('#hfGUID').val(guid);
    $("#formerror").hide();
    $("#gdvDisplay > tbody").empty();
    var mydata = [];
    var myObject = new Object();

    myObject.Job = $('#txtJob').val();

    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;


    $.ajax({
        type: "POST",
        url: "../MaterialIssue/LoadGrid",
        contentType: "application/json; charset=utf-8",
        data: exportdata,
        dataType: "json",
        success: OnSuccess,
        failure: function (response) {
            alert(response);

        },
        error: function (response) {
            alert(response);

        }
    });

    function OnSuccess(response) {
        //alert(response);
        if (response) {
            $('#tbStockCode').val(response[0].JobStockCode);
            $('#tbStockDescription').val(response[0].JobDescription);



            $.each(response, function () {
                var r = $('#gdvDisplay tr').length;
                var warehouse = this['Warehouse'];
                var stockcode = this['StockCode'];


                if (document.getElementById("#tblLot_" + r)) {
                    // alert("existsmodalafterrefresh");
                    $('#tblLot_"' + r).remove();
                }
                //show modal with data
                else { }

                if (this["QtyOnHand"] < 1) {
                    $('#gdvDisplay tbody').append("<tr class='nr' style='background-color:red;color:white;'><td><input type='hidden' value='" + this['Job'] + "'/>" + this['Line'] +
                        "</td><td><input type='hidden' value='false' id='addAllocation' />" + this['StockCode'] + "</td><td>" +
                        this['Description'] + "</td><td>" + this['Warehouse'] + "</td><td>" + this['QtyOnHand'] + "</td><td class='text-right'>" + this['QtyRequired'] + "</td><td class='text-right'>" + this['QtyIssued'] + "</td>" +
                        "<td class=''><input type='text' class='form-control tb_" + r + " input-sm text-right'/></td><td><input type='hidden' value='" + r + "'/>" + this['Traceable'] + "</td>"
                        + (this['Traceable'] == 'Yes' ? '<td class="" ><div class="input-group add-on col-sm-12 "><select id="mySelect_' + r + '" class="js-example-basic-single-grid myselect"></select>'
                            +
                            '<div class ="input-group-btn input-space" ><a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-success btn-sm open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + stockcode + '\',\'' + warehouse + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div></div>'
                            + '</td>' : '<td  class=""><div class="input-group add-on col-sm-12 "><input type="text" class="form-control text-right  input-sm "/>'
                            +
                            '<div class ="input-group-btn input-space" ><a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-success btn-sm open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + stockcode + '\',\'' + warehouse + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div></div></td>')
                        + "</tr>");
                }
                else {
                    $('#gdvDisplay tbody').append("<tr class='nr'><td><input type='hidden' value='" + this['Job'] + "'/>" + this['Line'] +
                        "</td><td><input type='hidden' value='false' id='addAllocation' />" + this['StockCode'] + "</td><td>" +
                        this['Description'] + "</td><td>" + this['Warehouse'] + "</td><td>" + this['QtyOnHand'] + "</td><td class='text-right'>" + this['QtyRequired'] + "</td><td class='text-right'>" + this['QtyIssued'] + "</td>" +
                        "<td class=''><input type='text' class='form-control tb_" + r + " input-sm text-right'/></td><td><input type='hidden' value='" + r + "'/>" + this['Traceable'] + "</td>"
                        + (this['Traceable'] == 'Yes' ? '<td class="" ><div class="input-group add-on col-sm-12 "><select id="mySelect_' + r + '" class="js-example-basic-single-grid myselect"></select>'
                            +
                            '<div class ="input-group-btn input-space" ><a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-success btn-sm open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + stockcode + '\',\'' + warehouse + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div></div>'
                            + '</td>' : '<td  class=""><div class="input-group add-on col-sm-12 "><input type="text" class="form-control text-right input-sm "/>'
                            +
                            '<div class ="input-group-btn input-space" ><a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-success btn-sm open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + stockcode + '\',\'' + warehouse + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div></div></td>')
                        + "</tr>");
                }
                $(".js-example-basic-single-grid").select2({
                    placeholder: "",
                    allowClear: true,
                    tags: true,
                    createTag: function (params) {
                        return {
                            id: params.term,
                            text: params.term,
                            newOption: true
                        }
                    },
                    templateResult: function (data) {
                        var $result = $("<span></span>");

                        $result.text(data.text);

                        if (data.newOption) {
                            $result.append(" <em>(new)</em>");
                        }

                        return $result;
                    }
                });
                $.fn.select2.defaults.set("", "");
                // alert('v2')
                var myString = JSON.stringify({
                    Warehouse: this['Warehouse'], StockCode: this['StockCode']
                });
                var exportdata = myString;
                $.ajax({
                    type: "POST",
                    data: exportdata,
                    url: "../MaterialIssue/GetLots",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    async: true,
                    success: OnSuccess,
                    failure: function () {
                        alert(response);
                    }
                });

                function OnSuccess(response) {
                    if (response) {

                        $("#mySelect_" + r + "").prepend("<option value='' selected='selected'></option>");
                        $.each(response, function () {

                            $("#mySelect_" + r + "")
                                .append($("<option></option>")
                                    .attr("value", this['Lot'].trim())
                                    .text(this['Description']));
                        });

                        //Add Qty On Change here
                        $("#mySelect_" + r + "").on('change', function () {

                            //Check Work Centre
                            var WorkCentre = $("#ddlWorkCentre option:selected").text();
                            //Check first value at for wicketing
                            // alert(WorkCentre);
                            var FirstWCChar = WorkCentre.charAt(0);
                            // alert(FirstWCChar);
                            if (FirstWCChar === "W") {
                                //Get text of lot selected
                                var Lot = $("#mySelect_" + r + " option:selected").text();

                                //get value within brackets
                                var LotQty = Lot.match(/\(([^)]+)\)/);
                                //     alert(LotQty);
                                if (LotQty) {
                                    $(".tb_" + r + "").val(LotQty[1])
                                }
                            }
                        });
                    }
                }

            });

        }
        else {
            $("#formerror").show();
            $("#formerror span").text("Job not found.");
            //alert("Job wasnt found in warehouse");
        }


    }


}

function AddLot(id, stockcode, warehouse, traceable, guid) {
    // alert('at lot check' + " " + id + traceable+"  "+guid);
    //check if modal exists
    var myElem = document.getElementById("LotModal_" + id + "");
    //if doesnt exist
    if ($(".lotno_" + id)[0]) {
        // alert("exists");
        $('#LotModal_' + id + '').modal('show');
    }
    //show modal with data
    else {
        r = 1;
        //alert("create");
        $('#LotModals').append(
            " <div class='modal fade lotno_" + id + "' id='LotModal_" + id + "' role='dialog'>" +
            " <div class='modal-dialog' style='width: 100%'>" +
            " <div class='modal-content'>" +
            " <div class='modal-header'>" +
            "<button type='button' class='close' data-dismiss='modal'>&times;</button>" +

            " <h4 class='modal-title'>Material Issue </h4>" +
            " </div>" +

            "<div class='modal-body'>" +

            " <div class='table-responsive'>" +
            "<table class='table table-striped table-bordered table-hover topTable' id='tblLot_" + id + "' style='font-size:8pt;'>" +
            " <thead>" +
            "  <tr>" +
            " <th>Lot</th>" +
            " <th>Quantity</th><th></th>" +
            "  </tr>" +
            " </thead>" +
            "<tbody>"
            +
            "<tr class = 'dellotLine_" + r + "'>" +
            (traceable == 'Yes' ? "<td><div class='input-group add-on'>" +
                "<select id='ddlLot_" + id + "_" + r + "' class='foc js-example-basic-single-grid-modal mylotselect_" + id + "_" + r + "' ></select>" +
                "</div></td>" : '<td><input type="text" class="foc form-control input-sm text-right "/></td>') +
            "<td><div class='input-group'><input class='form-control text-right input-sm lottabkey_" + id + "_" + guid + " ' id='tbLotQtys_" + id + "_" + r + "' type='text' value=''/></div></td>" +
            "<td><a  class='  btn btn-danger btn-xs' onclick='deleteLotRow(" + id + "," + r + ")' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            " </div>" +

            " </div>" +
            " <div class='modal-footer'> " +
            "<button type='button' class='btn btn-info close-modal' data-dismiss='modal'>Close</button>" +
            "<input type='hidden' name='hfRow' id='hfRow' value='" + id + "'>" +
            "</div></div></div></div>");
        // alert('show');
        $('#LotModal_' + id + '').modal('show');
        $('#LotModal_' + id + '').on('shown.bs.modal', function () {
            $('.foc').focus();
        })
        $(".js-example-basic-single-grid-modal").select2({
            placeholder: "",
            allowClear: true,
            tags: true,
            createTag: function (params) {
                return {
                    id: params.term,
                    text: params.term,
                    newOption: true
                }
            },
            templateResult: function (data) {
                var $result = $("<span></span>");

                $result.text(data.text);

                if (data.newOption) {
                    $result.append(" <em>(new)</em>");
                }

                return $result;
            }
        });

        //$.fn.select2.defaults.set("", "");
        //  alert('here');
        var myString = JSON.stringify({
            Warehouse: warehouse, StockCode: stockcode
        });
        var exportdata = myString;
        $.ajax({
            type: "POST",
            data: exportdata,
            url: "../MaterialIssue/GetLots",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            success: OnSuccess,
            failure: function () {
                alert(response);
            }
        });

        function OnSuccess(response) {
            if (response) {
                $(".mylotselect_" + id + "_" + r + "").prepend("<option value='' selected='selected'></option>");

                

                $.each(response, function () {
                    //alert("cant pop");

                    

                    //JR - 09/07/2020 - Modified to prevent user selecting same lot twice
                    //JR - Original
                    //$(".mylotselect_" + id + "_" + r + "")
                    //    .append($("<option></option>")
                    //        .attr("value", this['Lot'].trim())
                    //        .text(this['Description']));
                    var LotToAdd = this['Lot'].trim();
                    var addLot = true;
                    //alert($('#mySelect_' + id).val());
                    $('#tblLot_' + id + ' tbody tr').each(function () {
                        if ($('#mySelect_' + id).val() == LotToAdd) {
                            addLot = false
                        }
                    });

                    if (addLot) {
                        $(".mylotselect_" + id + "_" + r + "")
                            .append($("<option></option>")
                                .attr("value", this['Lot'].trim())
                                .text(this['Description']));
                    }

                    
                });
            }
            //Add Qty On Change here
            $(".mylotselect_" + id + "_" + r + "").on('change', function () {
                //Check Work Centre
                var WorkCentre = $("#ddlWorkCentre option:selected").text();
                //Check first value at for wicketing
                // alert(WorkCentre);
                var FirstWCChar = WorkCentre.charAt(0);
                // alert(FirstWCChar);
                if (FirstWCChar === "W") {
                    //Get text of lot selected
                    var Lot = $(".mylotselect_" + id + "_" + r + " option:selected").text();
                    // alert(Lot);
                    //get value within brackets
                    var LotQty = Lot.match(/\(([^)]+)\)/);
                    // alert(LotQty);
                    if (LotQty) {
                        $("#tbLotQtys_" + id + "_" + r + "").val(LotQty[1])
                    }
                }
            });
        }

        //allow tabbing
        Lottab(id, stockcode, warehouse, traceable, guid);

    }

}

function GetSelectedRow(lnk) {
    var row = lnk.parentNode.parentNode;
    var rowIndex = row.rowIndex;
    return rowIndex;

}

function deleteLotRow(id, row) {
    //alert(row);


    var table = document.getElementById('tblLot_' + id + '');
    if (table.rows.length > 2) {
        $('#tblLot_' + id + ' tr.dellotLine_' + row + '').remove();
    }
    else {
        // alert("Cannot delete the last row")
        //cant delete 1 row left
    }


}

function Lottab(id, stockcode, warehouse, traceable, guid) {
    //alert("in here id = " + guid);

    $(document).on('keydown change', '#tblLot_' + id + ' tr input:text.lottabkey_' + id + '_' + guid + ' ', function (e) {
        var lastid = $('#tblLot_' + id + ' tr:last').find('td:eq(1)').find('input[type=text]').attr('id');

        //alert(lastid);
        var fields = lastid.split('_');
        var LastRowId = fields[2];
        r = parseInt(LastRowId) + 1
        //  alert(r);
        var keyCode = e.keyCode || e.which;
        if (keyCode == 9) {
            // showprogressbar();
            // alert("intable table");

            //  alert('here');

            //JR
            //alert(LastRowId);
            //alert($(this).closest('tr').find(":selected").val());

            if (traceable === 'Yes') {
                var myString = JSON.stringify({
                    Warehouse: warehouse, StockCode: stockcode
                });
                var exportdata = myString;
                $.ajax({
                    type: "POST",
                    data: exportdata,
                    url: "../MaterialIssue/GetLots",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    async: true,
                    success: OnSuccess,
                    failure: function () {
                        alert(response);
                    }
                });

                function OnSuccess(response) {
                    if (response) {
                        // alert(response);
                        $('#tblLot_' + id + ' tbody').append(
                            "<tr class = 'dellotLine_" + r + " '>" +
                            (traceable == 'Yes' ? "<td><div class='input-group add-on'>" +
                                "<select id='ddlLot_" + id + "_" + r + "' class='js-example-basic-single-grid-modal mylotselect_" + id + "_" + r + "' ></select>" +
                                "</div></td>" : '<td><input type="text" class="form-control text-right input-sm "/></td>') + "<td>" +
                            "<div class='input-group'>" +
                            "<input class='form-control text-right input-sm lottabkey_" + id + "_" + guid + " ' id='tbLotQtys_" + id + "_" + r + "' type='text' value=''/>" +
                            "</div></td>" +
                            "<td><a  class='  btn btn-danger btn-xs' onclick='deleteLotRow(" + id + "," + r + ")' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td>" +
                            "</tr>");

                        $(".mylotselect_" + id + "_" + r).select2({
                            placeholder: "",
                            allowClear: true,
                            tags: true,
                            createTag: function (params) {
                                return {
                                    id: params.term,
                                    text: params.term,
                                    newOption: true
                                }
                            },
                            templateResult: function (data) {
                                var $result = $("<span></span>");

                                $result.text(data.text);

                                if (data.newOption) {
                                    $result.append(" <em>(new)</em>");
                                }

                                return $result;
                            }
                        });

                        $(".mylotselect_" + id + "_" + r + "").prepend("<option value='' selected='selected'></option>");
                        setTimeout(function () {
                            $.each(response, function () {


                                //JR - 30/06/2020 - Modified to prevent user selecting same lot twice
                                //JR - Original
                                //$(".mylotselect_" + id + "_" + r + "")
                                //    .append($("<option></option>")
                                //        .attr("value", this['Lot'].trim())
                                //        .text(this['Description']));
                                var LotToAdd = this['Lot'].trim();
                                var addLot = true;
                                //alert($('#mySelect_' + r).val());
                                $('#tblLot_' + id + ' tbody tr').each(function () {
                                    if ($(this).find(":selected").val() == LotToAdd || $('#mySelect_' + r).val() == LotToAdd) {
                                        addLot = false
                                    }
                                });

                                if (addLot) {
                                    $(".mylotselect_" + id + "_" + r + "")
                                        .append($("<option></option>")
                                            .attr("value", LotToAdd)
                                            .text(this['Description']));
                                }

                                //END JR - 30/06/2020 


                            });
                            $(".mylotselect_" + id + "_" + r + "").select2('open');
                        }, 100);

                        //Add Qty On Change here
                        $(".mylotselect_" + id + "_" + r + "").on('change', function () {
                            //Check Work Centre
                            var WorkCentre = $("#ddlWorkCentre option:selected").text();
                            //Check first value at for wicketing
                            // alert(WorkCentre);
                            var FirstWCChar = WorkCentre.charAt(0);
                            // alert(FirstWCChar);
                            if (FirstWCChar === "W") {
                                //Get text of lot selected
                                var Lot = $(".mylotselect_" + id + "_" + r + " option:selected").text();
                                // alert(Lot);
                                //get value within brackets
                                var LotQty = Lot.match(/\(([^)]+)\)/);
                                // alert(LotQty);
                                if (LotQty) {
                                    $("#tbLotQtys_" + id + "_" + r + "").val(LotQty[1])
                                }
                            }
                        });







                    }
                }
            }
            else {
                $('#tblLot_' + id + ' tbody').append(
                    "<tr class = 'dellotLine_" + r + " '>" +
                    (traceable == 'Yes' ? "<td><div class='input-group add-on'>" +
                        "<select id='ddlLot_" + id + "_" + r + "' class='js-example-basic-single-grid-modal mylotselect_" + id + "_" + r + "' ></select>" +
                        "</div></td>" : '<td><input type="text" class="form-control text-right input-sm "/></td>') + "<td>" +
                    "<div class='input-group'>" +
                    "<input class='form-control text-right input-sm lottabkey_" + id + "_" + guid + " ' id='tbLotQtys_" + id + "_" + r + "' type='text' value=''/>" +
                    "</div></td>" +
                    "<td><a  class='  btn btn-danger btn-xs' onclick='deleteLotRow(" + id + "," + r + ")' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td>" +
                    "</tr>");
            }
        }
    });
}

function PostMaterialIssue() {
    showprogressbar();
    var LotArr = [];
    var mydata = [];

    var e = document.getElementById("ddlWorkCentre");
    var WorkCentre = e.options[e.selectedIndex].value;
    var f = document.getElementById("ddlShift");
    var Shift = f.options[f.selectedIndex].value;
    var AllowPost = true;
    var NoLotEnt = false;
    var RefWarn = false;
    $("#gdvDisplay > tbody").find('tr').each(function () {

        var Job = $(this).find('td:eq(0)').find('input[type=hidden]').val();
        var LineForLot = $(this).find('td:eq(8)').find('input[type=hidden]').val();
        var AddAllocation = $(this).find('td:eq(1)').find('input[type=hidden]').val();
        var Line = $(this).find('td:eq(0)').text();
        var StockCode = $(this).find('td:eq(1)').text();
        var Warehouse = $(this).find('td:eq(3)').text();
        var Quantity = $(this).find('td:eq(7)').find('input[type=text]').val();
        var Traceable = $(this).find('td:eq(8)').text();
        var Lot;
        var Reference;
        if (Traceable == 'Yes') {
            Lot = $(this).find('td:eq(9) input[type="text"],select').val();
        }
        else {
            Reference = $(this).find('td:eq(9)').find('input[type=text]').val();
        }
        //var Reference = $(this).find('td:eq(10)').find('input[type=text]').val();
        if (parseFloat(Quantity) <= parseFloat($(this).find('td:eq(4)').text())) {
            RefWarn = true;
        }
        if (Quantity != '' && Traceable == 'Yes' && Lot == '') {
            NoLotEnt = true;
            alert(StockCode + " is traceable please enter a lot number");
        }


        if (parseFloat(Quantity) > parseFloat($(this).find('td:eq(4)').text())) {
            AllowPost = false;
            alert("Quantity to post exceeds Quantity On Hand for StockCode : " + StockCode);
        }

        if (Quantity != "" || Quantity != 0) {
            var myObject = new Object();
            myObject.Job = Job;
            myObject.AddAllocation = AddAllocation;
            myObject.Line = Line;
            myObject.StockCode = StockCode;
            myObject.Warehouse = Warehouse;
            myObject.Quantity = Quantity;
            myObject.Reference = Reference;
            myObject.Lot = Lot;
            myObject.WorkCentre = WorkCentre;
            myObject.Shift = Shift;
            myObject.TrnDate = $('#txtDate').val();
            mydata.push(myObject);

            //Rememeber to check if modal exists
            if (!document.getElementById("tblLot_" + LineForLot + "")) {
                // alert("noextralot");
            }
            else {

                //Get lot for corresponding row
                $("#tblLot_" + LineForLot + "").find('tbody > tr').each(function () {

                    var ChosenLot = $(this).find('td:eq(0) input[type="text"],select').val();
                    var ExtraRef = $(this).find('td:eq(0)').find('input[type=text]').val();
                    var LotQty = $(this).find('td:eq(1) input[type="text"]').val();

                    if (Traceable == 'Yes') {
                        if (ChosenLot != "" && LotQty != "") {

                            // alert(ChosenLot + "      " + "'" + LotQty + "'");

                            var myObject = new Object();
                            myObject.Job = Job;
                            myObject.AddAllocation = AddAllocation;
                            myObject.Line = Line;
                            myObject.StockCode = StockCode;
                            myObject.Warehouse = Warehouse;
                            myObject.Quantity = LotQty;
                            myObject.Reference = ExtraRef;
                            myObject.Lot = ChosenLot;
                            myObject.WorkCentre = WorkCentre;
                            myObject.Shift = Shift;
                            myObject.TrnDate = $('#txtDate').val();
                            mydata.push(myObject);
                        }
                    }
                    else if (ExtraRef != "" && LotQty != "") {
                        if (ChosenLot != "" && LotQty != "") {

                            // alert(ChosenLot + "      " + "'" + LotQty + "'");

                            var myObject = new Object();
                            myObject.Job = Job;
                            myObject.AddAllocation = AddAllocation;
                            myObject.Line = Line;
                            myObject.StockCode = StockCode;
                            myObject.Warehouse = Warehouse;
                            myObject.Quantity = LotQty;
                            myObject.Reference = ExtraRef;
                            myObject.Lot = ChosenLot;
                            myObject.WorkCentre = WorkCentre;
                            myObject.Shift = Shift;
                            myObject.TrnDate = $('#txtDate').val();
                            mydata.push(myObject);
                        }
                    }
                });
            }
        }
    });
    if (WorkCentre != "") {
        if (Shift != "") {
            if (NoLotEnt == false) {

                if (AllowPost == true) {
                    if (RefWarn == true) {
                        if (confirm('Some reference fields are empty do you want to continue?')) {
                            var myString = JSON.stringify({ details: JSON.stringify(mydata) });
                            var exportdata = myString;
                            //alert(exportdata);

                            $.ajax({
                                type: "POST",
                                url: "../MaterialIssue/PostJobMaterialIssue",
                                contentType: "application/json; charset=utf-8",
                                data: exportdata,
                                dataType: "json",
                                success: OnPost,
                                failure: function (response) {
                                    alert(response);
                                    waitingDialog.hidePleaseWait();
                                },
                                error: function (response) {
                                    alert(response);
                                    waitingDialog.hidePleaseWait();
                                }
                            });

                            function OnPost(response) {

                                alert(response);
                                if (response.startsWith("Error")) {
                                    hideprogressbar();
                                }
                                else {
                                    hideprogressbar();
                                    //LoadGrid();
                                    $("#gdvDisplay > tbody").empty();
                                    $('#txtJob').val('');
                                    $('#txtJob').focus();
                                    $('#ddlWarehouse').empty();
                                    $("#LotModals").html("");
                                    $('#ddlWarehouse').val('').trigger('change');
                                    //  window.location.reload(true);
                                }
                            }
                        }
                        else {
                            hideprogressbar();
                        }
                    }
                    else {
                        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
                        var exportdata = myString;
                        //alert(exportdata);

                        $.ajax({
                            type: "POST",
                            url: "../MaterialIssue/PostJobMaterialIssue",
                            contentType: "application/json; charset=utf-8",
                            data: exportdata,
                            dataType: "json",
                            success: OnPost,
                            failure: function (response) {
                                alert(response);
                                hideprogressbar();
                            },
                            error: function (response) {
                                alert(response);
                                hideprogressbar();
                            }
                        });

                        function OnPost(response) {

                            alert(response);
                            if (response.startsWith("Error")) {
                                hideprogressbar();
                            }
                            else {
                                hideprogressbar();
                                //LoadGrid();
                                $("#gdvDisplay > tbody").empty();
                                $('#txtJob').val('');
                                $('#txtJob').focus();
                                $('#ddlWarehouse').empty();
                                $("#LotModals").html("");
                                $('#ddlWarehouse').val('').trigger('change');
                                // window.location.reload(true)



                            }
                        }
                    }
                }
                else {
                    hideprogressbar();
                }
            }
            else { hideprogressbar(); } //No Lot Entered
        }
        else { alert('Select a Shift'); hideprogressbar(); }
    }
    else { alert('Select a Machine'); hideprogressbar(); }



}

function GetWorkCentres() {

    $.ajax({
        type: "POST",
        url: "../MaterialIssue/GetWorkCentres",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });
    function OnSuccess(response) {
        if (response) {
            var ddlWorkCentre = $("#ddlWorkCentre");

            ddlWorkCentre.empty();
            $("#ddlWorkCentre").prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                //alert(this['Value'] + ' space ' + this['Text']);
                ddlWorkCentre.append($("<option></option>").val(this['Value'].trim()).html(this['Text']));
            });
        }
    }
}

function GetShifts() {

    $.ajax({
        type: "POST",
        url: "../MaterialIssue/GetShifts",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });
    function OnSuccess(response) {
        if (response) {
            var ddlShift = $("#ddlShift");

            ddlShift.empty();
            $("#ddlShift").prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                //alert(this['Value'] + ' space ' + this['Text']);
                ddlShift.append($("<option></option>").val(this['Value'].trim()).html(this['Text']));
            });
        }
    }
}

function GetWarehouses() {

    $.ajax({
        type: "POST",
        url: "../MaterialIssue/GetWarehouses",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });
    function OnSuccess(response) {
        if (response) {

            var ddlWarehouse = $("#ddlWarehouse");
            if ($('#ddlWarehouse > option').length == 0) {

                ddlWarehouse.empty();
                $("#ddlWarehouse").prepend("<option value='' selected='selected'></option>");

                $.each(response, function () {
                    //alert(this['Value'] + ' space ' + this['Text']);
                    ddlWarehouse.append($("<option></option>").val(this['Value'].trim()).html(this['Text']));
                });
            }

        }
    }
}

function PostMaterialAllocation() {
    showprogressbar();
    guid = $('#hfGUID').val();
    // alert(guid);
    var mydata = [];
    var myObject = new Object();
    var e = document.getElementById("ddlWarehouse");
    var Warehouse = e.options[e.selectedIndex].value;

    myObject.Warehouse = Warehouse;
    myObject.Job = $('#txtJob').val();
    myObject.StockCode = $('#hdselectedstockcode').val();
    mydata.push(myObject);

    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    var exportdata = myString;
    //alert(exportdata);

    $.ajax({
        type: "POST",
        url: "../MaterialIssue/PostMaterialAllocation",
        contentType: "application/json; charset=utf-8",
        data: exportdata,
        dataType: "json",
        success: OnPostAdd,
        failure: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.responseText);
            showprogressbar();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.responseText);
            showprogressbar();
        }
    });

    function OnPostAdd(response) {
        if (typeof response == 'object') {
            $.each(response, function () {

                alert('Posting Complete!');
                var r = $('#gdvDisplay tr').length;
                if (this["QtyOnHand"] < 1) {
                    $('#gdvDisplay tbody').append("<tr class='nr' style='background-color:red;color:white;'><td><input type='hidden' value='" + this['Job'] + "'/>" + this['Line'] +
                        "</td><td><input type='hidden' value='false' id='addAllocation' />" + this['StockCode'] + "</td><td>" +
                        this['Description'] + "</td><td>" + this['Warehouse'] + "</td><td>" + this['QtyOnHand'] + "</td><td class='text-right'>" + this['QtyRequired'] + "</td><td class='text-right'>" + this['QtyIssued'] + "</td>" +
                        "<td class=''><input type='text' class='form-control .tb_" + r + " input-sm text-right'/></td><td><input type='hidden' value='" + r + "'/>" + this['Traceable'] + "</td>"
                        + (this['Traceable'] == 'Yes' ? '<td class="" ><div class="input-group add-on col-sm-12 "><select id="mySelect_' + r + '" class="js-example-basic-single-grid myselect"></select>'
                            +
                            '<a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-primary btn-xs open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + this['StockCode'] + '\',\'' + this['Warehouse'] + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div>'
                            + '</td>' : '<td  class=""><div class="input-group add-on col-sm-12 "><input type="text"tabindex="-1" class="form-control input-sm "/>'
                            +
                            '<a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-primary btn-xs open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + this['StockCode'] + '\',\'' + this['Warehouse'] + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div></td>')
                        + "</tr>");
                }
                else {
                    $('#gdvDisplay tbody').append("<tr class='nr'><td><input type='hidden' value='" + this['Job'] + "'/>" + this['Line'] +
                        "</td><td><input type='hidden' value='false' id='addAllocation' />" + this['StockCode'] + "</td><td>" +
                        this['Description'] + "</td><td>" + this['Warehouse'] + "</td><td>" + this['QtyOnHand'] + "</td><td class='text-right'>" + this['QtyRequired'] + "</td><td class='text-right'>" + this['QtyIssued'] + "</td>" +
                        "<td class=''><input type='text' class='form-control .tb_" + r + " input-sm text-right'/></td><td><input type='hidden' value='" + r + "'/>" + this['Traceable'] + "</td>"
                        + (this['Traceable'] == 'Yes' ? '<td class="" ><div class="input-group add-on col-sm-12 "><select id="mySelect_' + r + '" class="js-example-basic-single-grid myselect"></select>'
                            +
                            '<a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-primary btn-xs open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + this['StockCode'] + '\',\'' + this['Warehouse'] + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div>'
                            + '</td>' : '<td  class=""><div class="input-group add-on col-sm-12 "><input type="text"tabindex="-1" class="form-control input-sm "/>'
                            +
                            '<a href ="#LotModal_' + r + '" class="lot_' + r + '  btn btn-primary btn-xs open-AddLot pull-right" onclick="AddLot(' + r + ',\'' + this['StockCode'] + '\',\'' + this['Warehouse'] + '\',\'' + this['Traceable'] + '\',\'' + guid + '\')" tabIndex="-1"><span class="fa fa-plus" aria-hidden="true"  title="Add Lot"></span></a></div></td>')
                        + "</tr>");
                }
                $(".js-example-basic-single-grid").select2({
                    placeholder: "",
                    allowClear: true,
                    tags: true,
                    createTag: function (params) {
                        return {
                            id: params.term,
                            text: params.term,
                            newOption: true
                        }
                    },
                    templateResult: function (data) {
                        var $result = $("<span></span>");

                        $result.text(data.text);

                        if (data.newOption) {
                            $result.append(" <em>(new)</em>");
                        }

                        return $result;
                    }
                });

                $.fn.select2.defaults.set("", "");
                // alert('v2')
                var myString = JSON.stringify({
                    Warehouse: this['Warehouse'], StockCode: this['StockCode']
                });
                var exportdata = myString;
                $.ajax({
                    type: "POST",
                    data: exportdata,
                    url: "../MaterialIssue/GetLots",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    async: true,
                    success: OnSuccess,
                    failure: function () {
                        alert(response);
                    }
                });

                function OnSuccess(response) {
                    if (response) {
                        //alert(response.d);
                        $("#mySelect_" + r + "").prepend("<option value='' selected='selected'></option>");
                        $.each(response, function () {
                            $("#mySelect_" + r + "")
                                .append($("<option></option>")
                                    .attr("value", this['Lot'].trim())
                                    .text(this['Description']));

                        });

                        //Add Qty On Change here
                        $("#mySelect_" + r + "").on('change', function () {
                            //Check Work Centre
                            var WorkCentre = $("#ddlWorkCentre option:selected").text();
                            //Check first value at for wicketing
                            // alert(WorkCentre);
                            var FirstWCChar = WorkCentre.charAt(0);
                            //alert(FirstWCChar);
                            if (FirstWCChar === "W") {
                                //Get text of lot selected
                                var Lot = $("#mySelect_" + r + " option:selected").text();
                                //alert(Lot);
                                //get value within brackets
                                var LotQty = Lot.match(/\(([^)]+)\)/);
                                //alert(LotQty);
                                if (LotQty) {
                                    $(".tb_" + r + "").val(LotQty[1])
                                }
                            }
                        });
                    }
                }
            });
            hideprogressbar();
            $("#tblStock > tbody").empty();
            $('#txtSearchStockCode').val('');
            //$('#AllocationModal').modal('hide');
            $('#hdselectedstockcode').val('');
            $('#selectedStockCode').html('')
        }
        else {
            alert(response);
            hideprogressbar();
        }
    }


}

$(document).ready(function () {
    $('#wrapper').addClass('toggled-2');
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
    //*****************************************************************************************


    GetWorkCentres();
    GetShifts();
    $(".js-example-basic-single").select2({
        placeholder: "",
        allowClear: true
    });
    $.fn.select2.defaults.set("", "");

    $('body').on('hidden.bs.modal', '.modal', function () {
        $(this).removeData('bs.modal');
    });

    $('#btnSearch').click(function () {
        $("#tblStock > tbody").empty();
        var mydata = [];
        var myObject = new Object();

        var e = document.getElementById("ddlWarehouse");
        var Warehouse = e.options[e.selectedIndex].value;

        myObject.Warehouse = Warehouse;
        myObject.FilterText = $('#txtSearchStockCode').val();

        mydata.push(myObject);

        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;


        $.ajax({
            type: "POST",
            url: "../MaterialIssue/GetStockCodes",
            contentType: "application/json; charset=utf-8",
            data: exportdata,
            dataType: "json",
            success: OnResult,
            failure: function (response) {
                if (response == 'undefined') { }
                else { alert(response); }


            },
            error: function (response) {
                if (response == 'undefined') { }
                else { alert(response); }

            }
        });

        function OnResult(response) {
            if (response != null) {
                $.each(response, function () {
                    if (this["QtyOnHand"] < 1) {
                        $('#tblStock tbody').append("<tr class='nr' style='background-color:red;color:white'><td><input type='hidden' value='" + this['StockCode'] + "'/>" + this['StockCode'] +
                            "</td><td>" + this['Description'] + "</td><td>" + this['Warehouse'] + "</td><td>" + this['QtyOnHand'] + "</td></tr>");
                    }
                    else {
                        $('#tblStock tbody').append("<tr class='nr'><td><input type='hidden' value='" + this['StockCode'] + "'/>" + this['StockCode'] +
                            "</td><td>" + this['Description'] + "</td><td>" + this['Warehouse'] + "</td><td>" + this['QtyOnHand'] + "</td></tr>");
                    }

                });
            }

        }
    });

    $('#tblStock').on("click", ".nr", function () {

        var StockCode = $(this).find('td:eq(0)').text();
        var Description = $(this).find('td:eq(1)').text();

        $('#hdselectedstockcode').val(StockCode);
        $('#selectedStockCode').html('StockCode : ' + StockCode + ' - ' + Description);
        $('#AllocationModal').scrollTop(0);
    });

    $('#btnSave').on("click", function (e) {
        e.preventDefault();
        PostMaterialIssue();
    });



});