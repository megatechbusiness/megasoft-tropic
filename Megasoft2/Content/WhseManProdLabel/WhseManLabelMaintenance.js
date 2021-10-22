function LoadGrid() {
    $('#tblLines tbody').empty();
    var Job = $('#txtJob').val();
    $.getJSON('WhseManLabelMaintenance/GetGridData?Job=' + Job, function (data) {
        $.each(data, function (i, job) {
            var disabled = ""
            if (job.LabelReceipted == "Y" && job.TraceableType != 'T') {
                $('#tblLines tbody').append("<tr ><td><input type='checkbox'></td><td>" + parseInt(job.Job) + "</td><td>" + job.BatchId + "</td><td ><input   type='text' class='form-control input-sm tdtextbox' style='max-width: 150px; text-align:right;' value='" + parseFloat(job.BatchQty) + "' readonly='readonly'></input></td><td>" + job.PalletNo + "</td><td></td></tr>");
            }
            else if (job.LabelReceipted === "Y" && job.TraceableType === 'T') {
                $('#tblLines tbody').append("<tr ><td><input type='checkbox'></td><td>" + parseInt(job.Job) + "</td><td>" + job.BatchId + "</td><td ><input readonly='readonly'  type='text' class='form-control input-sm tdtextbox' style='max-width: 150px; text-align:right;' value='" + parseFloat(job.BatchQty) + "' readonly='readonly'></input></td><td>" + job.PalletNo + "</td><td></td></tr>");
            }
            else {
                $('#tblLines tbody').append("<tr><td><input type='checkbox'></td><td>" + parseInt(job.Job) + "</td><td>" + job.BatchId + "</td><td ><input type='text' class='form-control input-sm tdtextbox' style='max-width: 150px; text-align:right;' value='" + parseFloat(job.BatchQty) + "'></input></td><td>" + job.PalletNo + "</td><td><a href='#' class='delLine btn btn-danger btn-xs'  ><span class='fa fa-trash-o' aria-hidden='true' title='Delete Line'></span></a></td></tr>");
            }
            
        });
    });
}


function calcNoOfLabels()

{
    var Qty ='';
    if ($('#JobDetails_BatchQty').val().indexOf('=') >= 0) {
        //alert('here')
        var input = $('#JobDetails_BatchQty').val();
        var BAILQTY = input.split('=');
        Qty = BAILQTY[1];
    }
    else {
       // alert($('#JobDetails_BatchQty').val());
        Qty = $('#JobDetails_BatchQty').val();
    }
   // alert(Qty);
    var BatchQty = parseFloat(Qty);
    var ProductionQty = parseFloat($('#JobDetails_ProductionQty').val());
    if (Qty != 0 || Qty != "") { //Dont use converted variables here in case of blanks
        if ($('#JobDetails_ProductionQty').val() != 0 || $('#JobDetails_ProductionQty').val() != "") { //Dont use converted variables here in case of blanks
            var NoOfLabels = Math.ceil(ProductionQty / BatchQty);
            $('#JobDetails_NoOfLabels').val(NoOfLabels);
            var LastBatch = BatchQty;
            var TotalBatch = NoOfLabels * BatchQty;
            if (TotalBatch != ProductionQty)
            {
                if (BatchQty >= ProductionQty)
                {
                    LastBatch = ProductionQty;
                    //alert('1');
                }
                else
                {
                    LastBatch = ProductionQty - ((NoOfLabels - 1) * BatchQty);
                    //alert('2');
                }
                
            }
            $('#LastBatch').val(parseFloat(LastBatch).toFixed(2));
        }
        else {
            alert('Please enter a production quantity.');
        }
        
    }
}


$(document).ready(function () {
    $('#JobDetails_BatchQty').on('change', function (e) {
        calcNoOfLabels();
    });

    $('#JobDetails_ProductionQty').on('change', function (e) {
        calcNoOfLabels();
    });

    //$('#btnAdd').on('click', function (e) {
    //    e.preventDefault();

    //    var Job = $('#txtJob').val();
    //    var ProductionQty = $('#JobDetails_ProductionQty').val();
    //    var BatchQty = $('#JobDetails_BatchQty').val();
    //    var NoOfLabels = $('#JobDetails_NoOfLabels').val();
    //    var LastBatch = $('#LastBatch').val();
    //    var e = document.getElementById("ddlPrinter");
    //    var Printer = e.options[e.selectedIndex].value;

    //    var mydata = [];

    //    var myObject = new Object();

    //    myObject.Job = Job;
    //    myObject.ProductionQty = ProductionQty;
    //    myObject.BatchQty = BatchQty;
    //    myObject.LastBatch = LastBatch;
    //    myObject.NoOfLabels = NoOfLabels;
    //    myObject.Printer = Printer;
    //    mydata.push(myObject);

    //    var myString = JSON.stringify({ details: JSON.stringify(mydata) });
    //    var exportdata = myString;



    //    $.ajax({
    //        type: "POST",
    //        url: "WhseManProductionLabel/PrintLabel",
    //        data: exportdata,
    //        contentType: "application/json; charset=utf-8",
    //        dataType: "json",
    //        success: OnSuccess,
    //        failure: OnErrorCall
    //    });

    //    function OnSuccess(response) {
    //        $('#errordiv').text(response);
    //        $('#errordiv').addClass("alert alert-danger");
    //        LoadGrid();
    //    }

    //    function OnErrorCall(response) {
    //        $('#errordiv').text(response);
    //        $('#errordiv').addClass("alert alert-danger");
    //    }
    //});


    $('#tblLines').on("click", ".delLine", function () {
        var tr = $(this).closest('tr');
        var $tds = tr.find('td');
        var Job = $tds.eq(1).text();
        var BatchId = $tds.eq(2).text();

        var mydata = [];
        var myObject = new Object();
        myObject.Job = Job;
        myObject.Lot = BatchId;
        mydata.push(myObject);
        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;



        $.ajax({
            type: "POST",
            url: "WhseManLabelMaintenance/DeletePallet",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: OnErrorCall
        });

        function OnSuccess(response) {
            if (response != "") {
                alert(response);
            }
            else {
                LoadGrid();
            }
        }

        function OnErrorCall(response) {
            $('#errordiv').text(response);
            $('#errordiv').addClass("alert alert-danger");
        }

    });

    $('#btnPrint').on('click', function (e) {
        e.preventDefault();
        if ($('#Department').val() == "") {
            alert('Please select a Department!');
        }
        else {

            var result = confirm("Warning this will print a label?");
            alert(result);
            if (result) {
                alert('1');
                var e = document.getElementById("ddlPrinter");
                var Printer = e.options[e.selectedIndex].value;

                var mydata = [];
                $("#tblLines > tbody > tr ").each(function (i, el) {

                    var myObject = new Object();
                    var $tds = $(this).find('td');
                    var Job = $tds.eq(1).text();
                    var BatchId = $tds.eq(2).text();
                    var BatchQty = $tds.eq(3).find(":input[type=text]").val();
                 //   alert(BailQty);
                 //   return false;
                   

                    var PrintLabel = $tds.eq(0).find("input[type='checkbox']").is(":checked");

                    if (PrintLabel == true) {

                        myObject.Job = Job;
                        myObject.BatchId = BatchId;
                        myObject.BatchQty = BatchQty;
                        myObject.Printer = Printer;
                        myObject.Department = $('#Department').val();
                        mydata.push(myObject);
                    }
                });
                var myString = JSON.stringify({ details: JSON.stringify(mydata) });
                var exportdata = myString;
                $.ajax({
                    type: "POST",                  
                    url: "WhseManLabelMaintenance/PrintLabel",
                    data: exportdata,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: OnSuccessPoP,
                    failure: OnErrorPoP
                });

                function OnSuccessPoP(response) {
                    $('#errordiv').text(response);
                    $('#errordiv').addClass("alert alert-danger");
                }

                function OnErrorPoP(response) {
                    $('#errordiv').text(response);
                    $('#errordiv').addClass("alert alert-danger");
                }

            }
            else {
            }
        }
    });
});

        