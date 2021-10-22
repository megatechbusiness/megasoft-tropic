//This is a test for tfs
function toSeconds(time_str) {
    // Extract hours, minutes and seconds
    var parts = time_str.split(':');

    // compute  and return total seconds
    return parts[0] * 3600 + // an hour has 3600 seconds
        parts[1] * 60 + // a minute has 60 seconds
        +
        parts[2]; // seconds
}
function AddRow() {
    $("#formerror").hide();

    //Update Existing
    if (document.getElementById("hdRowNumber").value.trim() != "") {
        UpdateRow();
    }
    //add new row

    else {
        if (document.getElementById("ddlDepartment").value.trim() == "") {
            //	alert("Please enter a Department");
            $("#formerror").show();
            $("#formerror span").text("Please enter a Department");
            return;
        }
        else if (document.getElementById("ddlJobs").value.trim() == "") {
            //alert("Please select a Job");
            $("#formerror").show();
            $("#formerror span").text("Please select a Job");
            return;
        }
        else if (document.getElementById("txtDate").value.trim() == "") {
            //alert("Please select a Date");
            $("#formerror").show();
            $("#formerror span").text("Please select a Date");
            return;
        }
        else if (document.getElementById("ddlShift").value.trim() == "") {
            //  alert("Please select a Shift");
            $("#formerror").show();
            $("#formerror span").text("Please select a Shift");
            return;
        }
        else if (document.getElementById("ddlTimeCode").value.trim() == "") {
            //  alert("Please select a TimeCode");
            $("#formerror").show();
            $("#formerror span").text("Please select a TimeCode");
            return;
        }
        else if (document.getElementById("ddlWorkCentre").value.trim() == "") {
            //alert("Please select Work Centre");
            $("#formerror").show();
            $("#formerror span").text("Please select Work Centre");
            return;
        }
        else if (document.getElementById("tbStartTime").value.trim() == "") {
            //  alert("Please enter Start Time");
            $("#formerror").show();
            $("#formerror span").text("Please enter Start Time");
            return;
        }
        else if (document.getElementById("tbEndTime").value.trim() == "") {
            //alert("Please enter End Time");
            $("#formerror").show();
            $("#formerror span").text("Please enter End Time");
            return;
        }

        else if (document.getElementById("tbElapsedTime").value.trim() == "") {
            //alert("Elapsed Time cannot be blank");
            $("#formerror").show();
            $("#formerror span").text("Elapsed Time cannot be blank");
            return;
        }

        else {
            //  var f = document.getElementById("ddlDepartment");
            // var Department = f.options[f.selectedIndex].value;

            //var e = document.getElementById("ddlJobs");
            // var Job = e.options[e.selectedIndex].value;

            var d = document.getElementById("ddlWorkCentre");
            var WorkCentre = d.options[d.selectedIndex].value;

            var z = document.getElementById("ddlShift");
            var Shift = z.options[z.selectedIndex].value;

            var t = document.getElementById("ddlTimeCode");
            var TimeCode = t.options[t.selectedIndex].value;
            var tcText = t.options[t.selectedIndex].text;

            var Date = document.getElementById("txtDate").value.trim();
            var StartTime = document.getElementById("tbStartTime").value.trim();
            var EndTime = document.getElementById("tbEndTime").value.trim();
            var ElapsedTime = document.getElementById("tbElapsedTime").value.trim();

            var row = document.getElementById('gdvDisplay').rows.length;

            var buttons = '<a class="edit-row btn btn-primary btn-sm">' +
                '<span class="fa fa-pencil-square-o" aria-hidden="true" title="Edit"></span>' +
                '</a></td><td>' +
                '<a href="#" class="delete-row btn btn-danger btn-sm">' +
                '<span class="fa fa-trash-o" aria-hidden="true" title="Delete"></span>' +
                '</a>'

            $('#gdvDisplay').append('<tr class="newtr"><td class="nr">' + row + '</td><td>' + WorkCentre + '</td><td>' + StartTime + '</td><td>' + EndTime + '</td><td>' + ElapsedTime + '</td><td>' + Shift + '</td><td>' + tcText + " - " + TimeCode + '</td><td>' + Date + '</td><td>' + buttons + '</td></tr>');

            //set all on screen values to null after saved to grid
            document.getElementById("hdRowNumber").value = "";
            // document.getElementById("MainContent_txtDate").value = "";
            document.getElementById("tbStartTime").value = "";
            document.getElementById("tbEndTime").value = "";
            document.getElementById("tbElapsedTime").value = "";

            document.getElementById("ddlDepartment").disabled = true;
            document.getElementById("ddlJobs").disabled = true;
            document.getElementById("ddlWorkCentre").disabled = true;
            document.getElementById("txtDate").disabled = true;
            // document.getElementById("ddlTimeCode").disabled = true;
            document.getElementById("ddlShift").disabled = true;

            //var c = "";
            // $('#ddlWorkCentre').val(c).change()

            //  $Shift = document.getElementById("MainContent_ddlShift");
            //  $Shift.value = "";

            $TimeCode = document.getElementById("ddlTimeCode");
            $TimeCode.value = "";
        }
    }
}
function UpdateRow() {
    $("#formerror").hide();
    if (document.getElementById("ddlDepartment").value.trim() == "") {
        //	alert("Please enter a Department");
        $("#formerror").show();
        $("#formerror span").text("Please enter a Department");
        return;
    }
    else if (document.getElementById("ddlJobs").value.trim() == "") {
        //alert("Please select a Job");
        $("#formerror").show();
        $("#formerror span").text("Please select a Job");
        return;
    }
    else if (document.getElementById("txtDate").value.trim() == "") {
        //alert("Please select a Date");
        $("#formerror").show();
        $("#formerror span").text("Please select a Date");
        return;
    }
    else if (document.getElementById("ddlShift").value.trim() == "") {
        //  alert("Please select a Shift");
        $("#formerror").show();
        $("#formerror span").text("Please select a Shift");
        return;
    }
    else if (document.getElementById("ddlTimeCode").value.trim() == "") {
        //  alert("Please select a TimeCode");
        $("#formerror").show();
        $("#formerror span").text("Please select a TimeCode");
        return;
    }
    else if (document.getElementById("ddlWorkCentre").value.trim() == "") {
        //alert("Please select Work Centre");
        $("#formerror").show();
        $("#formerror span").text("Please select Work Centre");
        return;
    }
    else if (document.getElementById("tbStartTime").value.trim() == "") {
        //  alert("Please enter Start Time");
        $("#formerror").show();
        $("#formerror span").text("Please enter Start Time");
        return;
    }
    else if (document.getElementById("tbEndTime").value.trim() == "") {
        //alert("Please enter End Time");
        $("#formerror").show();
        $("#formerror span").text("Please enter End Time");
        return;
    }
    else if (document.getElementById("tbElapsedTime").value.trim() == "") {
        //alert("Elapsed Time cannot be blank");
        $("#formerror").show();
        $("#formerror span").text("Elapsed Time cannot be blank");
        return;
    }
    else {
        var row = document.getElementById("hdRowNumber").value;

        //var f = document.getElementById("ddlDepartment");
        //var Department = f.options[f.selectedIndex].value;
        //var e = document.getElementById("ddlJobs");
        //var Job = e.options[e.selectedIndex].value;

        var d = document.getElementById("ddlWorkCentre");
        var WorkCentre = d.options[d.selectedIndex].value;

        var z = document.getElementById("ddlShift");
        var Shift = z.options[z.selectedIndex].value;

        var t = document.getElementById("ddlTimeCode");
        var TimeCode = t.options[t.selectedIndex].value;

        var Date = document.getElementById("txtDate").value.trim();
        var StartTime = document.getElementById("tbStartTime").value.trim();
        var EndTime = document.getElementById("tbEndTime").value.trim();
        var ElapsedTime = document.getElementById("tbElapsedTime").value.trim();

        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(2)").html(WorkCentre);
        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(3)").html(StartTime);
        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(4)").html(EndTime);
        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(5)").html(ElapsedTime);
        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(6)").html(Shift);
        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(7)").html(TimeCode);
        $("table#gdvDisplay tr:nth-child(" + row + ")").find("td:nth-child(8)").html(Date);

        document.getElementById("txtDate").disabled = false;
        document.getElementById("hdRowNumber").value = "";
        document.getElementById("txtDate").value = "";
        document.getElementById("tbStartTime").value = "";
        document.getElementById("tbEndTime").value = "";
        document.getElementById("tbElapsedTime").value = "";

        // var c = "";
        // $('#ddlWorkCentre').val(c).change()

        $Shift = document.getElementById("ddlShift");
        $Shift.value = "";

        $TimeCode = document.getElementById("ddlTimeCode");
        $TimeCode.value = "";
    }
}
function GetDepartment() {
    $.ajax({
        type: "POST",
        url: "LabourPost/GetDepartment",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });

    function OnSuccess(response) {
        if (response) {
            var ddlDepartment = $("#ddlDepartment");

            ddlDepartment.empty();
            $("#ddlDepartment").prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                //alert(this['Value'] + ' space ' + this['Text']);
                ddlDepartment.append($("<option></option>").val(this['Value'].trim()).html(this['Text']));
            });
        }
    }
}
function GetJobs() {
    $("#formerror").hide();

    var e = document.getElementById("ddlDepartment");
    var Department = e.options[e.selectedIndex].value;

    var myString = JSON.stringify({
        dept: Department
    });
    var exportdata = myString;

    //Get Jobs for Department
    $.ajax({
        type: "POST",
        url: "LabourPost/GetJobs",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });

    function OnSuccess(response) {
        alert(response.d);

        if (response && response.length > 0) {
            var ddlJobs = $("#ddlJobs");

            ddlJobs.empty();
            $("#ddlJobs").prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                //alert(this['Value'] + ' space ' + this['Text']);
                ddlJobs.append($("<option></option>").val(this['Value'].trim()).html(this['Text']));
            });
        }
        else {
            $("#formerror").show();
            $("#formerror span").text("No Jobs Found");
        }
    }
}
function GetOperation() {
    var Operation = "";
    var Milestone = "";

    $("#formerror").hide();
    var e = document.getElementById("ddlDepartment");
    var Department = e.options[e.selectedIndex].value;

    var Job = document.getElementById("tbJob").value.trim();

    // alert(Job+" "+Department);

    var myString = JSON.stringify({
        Job: Job,
        Dept: Department
    });
    var exportdata = myString;

    //Get Jobs for Department

    $.ajax({
        type: "POST",
        url: "LabourPost/GetOperation",
        data: exportdata,
        async: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            Operation = response[0].OperationNo;
            Milestone = response[0].Milestone;
            // alert(Operation);
        },
        failure: function () {
            alert(response);
        }
    });
    return { Operation: Operation, Milestone: Milestone };
}
function GetPreviousOperation(CurrOperation) {
    var Operation = "";
    var Complete

    $("#formerror").hide();

    var Job = document.getElementById("tbJob").value.trim();

    // alert(Job+" "+Department);

    var myString = JSON.stringify({
        Job: Job,
        Operation: CurrOperation
    });
    var exportdata = myString;

    //Get Jobs for Department

    $.ajax({
        type: "POST",
        url: "LabourPost/GetPreviousOperation",
        data: exportdata,
        async: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            Operation = response[0].OperationNo;
            Complete = response[0].Complete;
            // alert(Operation);
        },
        failure: function () {
            alert(response);
        }
    });

    // alert(Complete);
    return Complete;
}
function GetJobDetails() {
    $("#formerror").hide();

    if ($('#tbJob').val() === "") {

        $("#formerror").show();
        $("#formerror span").text("Enter a job number.");
    }
    else {

        var Job = document.getElementById("tbJob").value.trim();
        var e = document.getElementById("ddlDepartment");
        var Department = e.options[e.selectedIndex].value;

        var myString = JSON.stringify({
            Job: Job, Department: Department
        });
        var exportdata = myString;

        //Get Job Details
        $.ajax({
            type: "POST",
            url: "LabourPost/GetJobDetails",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: function () {
                alert(response);
            }
        });

        function OnSuccess(response) {
            if (response) {
                $('#tbStockCode').val(response[0].StockCode);
                $('#tbDescription').val(response[0].StockDescription);
                $('#tbQtyToMake').val(response[0].QtyToMake);
                $('#tbJobUom').val(response[0].JobUom);
                $('#hfComplete').val(response[0].JobComplete);
                GetTimeCodes(1);
            }
            else {
                $("#formerror").show();
                $("#formerror span").text("Job doesnt exist in department");
            }
        }
    }

}
function GetWorkCentres() {
    //$("#formerror").hide();

    var e = document.getElementById("ddlDepartment");
    var Department = e.options[e.selectedIndex].value;

    if (Department == "") {
    }
    else {
        var myString = JSON.stringify({
            Department: Department
        });
        var exportdata = myString;

        $.ajax({
            type: "POST",
            url: "LabourPost/GetWorkCentres",
            contentType: "application/json; charset=utf-8",
            data: exportdata,
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
}
function ElapsedTime(r) {
    //alert("row:" + r)
    $("#formerror").hide();

    var a = document.getElementById("tbStartTimes_" + r + "").value.trim() + ":00";
    var b = document.getElementById("tbEndTimes_" + r + "").value.trim() + ":00";

    var f = document.getElementById("ddlDepartment");
    var Department = f.options[f.selectedIndex].value;

    $("#formerror").hide();
    var e = document.getElementById("ddlShift");
    var Shift = e.options[e.selectedIndex].value;

    if (Shift == "") {
        $("#formerror").show();
        $("#formerror span").text("Please select Shift");
    }
    else if (a == ":00") {
        $("#formerror").show();
        $("#formerror span").text("Please enter Start time");
    }
    else if (b == ":00") {
        $("#formerror").show();
        $("#formerror span").text("Please enter End time");
    }

    else {
        calc();
    }


    function calc() {
        var myString = JSON.stringify({
            ShiftID: Shift
        });
        var exportdata = myString;

        //Get ShiftDetails
        $.ajax({
            type: "POST",
            url: "LabourPost/GetShiftDetails",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: function () {
                alert(response);
            }
        });
        function OnSuccess(response) {


            //Validate if times entered fall within range of shift

            //22/08/2019 - JR - SG - Remove validation that checks if time entered falls within shift. - Commented out Original - Starts Here
            //if (response[0].NextDay == false) {
            //    if (toSeconds(a) >= toSeconds(response[0].StartTime + ":00") && toSeconds(b) <= toSeconds(response[0].EndTime + ":00")) {
            //        if (Department == "EXTR") {
            //            var difference = Math.abs(toSeconds(a) - toSeconds(b));

            //            // format time differnece
            //            var result = [
            //                Math.floor(difference / 3600), // an hour has 3600 seconds
            //                Math.floor((difference % 3600) / 60), // a minute has 60 seconds
            //                difference % 60
            //            ];
            //            // 0 padding and concatation
            //            result = result.map(function (v) {
            //                return v < 10 ? '0' + v : v;
            //            }).join(':');
            //            // alert(result);
            //            $("#tbElapsedTimes_" + r + "").val(result);
            //        }
            //        else {
            //            //Shift StartTime <= tbStartTime &&  tbEndTime <= ShiftEndTime
            //            //if (toSeconds(a) >= toSeconds(response.d[0].StartTime + ":00") && toSeconds(b) <= toSeconds(response.d[0].EndTime + ":00")) {
            //            var difference = Math.abs(toSeconds(a) - toSeconds(b));

            //            // format time differnece
            //            var result = [
            //                Math.floor(difference / 3600), // an hour has 3600 seconds
            //                Math.floor((difference % 3600) / 60), // a minute has 60 seconds
            //                difference % 60
            //            ];
            //            // 0 padding and concatation
            //            result = result.map(function (v) {
            //                return v < 10 ? '0' + v : v;
            //            }).join(':');
            //            // alert(result);
            //            $("#tbElapsedTimes_" + r + "").val(result);
            //            //}
            //            //else {
            //            //   $("#formerror").show();
            //            //  $("#formerror span").text("Start Time and End Time isnt within the selected shift period");
            //            //}
            //        }
            //    }
            //    else {
            //        $("#formerror").show();
            //        $("#formerror span").text("Start Time and End Time isnt within the selected shift period");
            //        alert('Start Time and End Time isnt within the selected shift period.');
            //    }
            //}
            ////Shift Overlaps next Day
            //else {
            //    //alert('here');

            //    // Textbox
            //    var q = new Date('01/01/2001 ' + a).getTime();//StartTime
            //    var e = new Date('02/01/2001 ' + b).getTime();//EndTime
            //    var z = new Date('01/01/2001 ' + b).getTime();//EndTime

            //    // DB value shift
            //    var x = new Date('01/01/2001 ' + response[0].StartTime + ':00').getTime();//StartTime
            //    var y = new Date('02/01/2001 ' + response[0].EndTime + ':00').getTime();//EndTime
            //    var i = new Date('01/01/2001 ' + response[0].EndTime + ':00').getTime();//if End time is in same day
            //    var prev = new Date('01/01/2001 ' + '24:00').getTime();
            //    if (toSeconds(a) >= toSeconds(response[0].StartTime + ':00') && toSeconds(b) <= toSeconds('24:00:00') && toSeconds(b) >= toSeconds(response[0].StartTime + ':00') || toSeconds(a) >= toSeconds(response[0].StartTime + ':00') && toSeconds(b) <= toSeconds(response[0].EndTime + ':00') || toSeconds(a) <= toSeconds(response[0].EndTime + ':00') && toSeconds(b) <= toSeconds(response[0].EndTime + ':00')) {
            //        if (toSeconds(a) >= toSeconds(response[0].StartTime + ':00') && toSeconds(b) <= toSeconds('24:00:00') && toSeconds(b) >= toSeconds(response[0].StartTime + ':00') || toSeconds(a) >= toSeconds(response[0].StartTime + ':00') && toSeconds(b) <= toSeconds(response[0].EndTime + ':00') || toSeconds(a) <= toSeconds(response[0].EndTime + ':00') && toSeconds(b) <= toSeconds(response[0].EndTime + ':00')) {
            //            {
            //                // alert("BETWEEN");
            //                var c = "24:00:00";
            //                if (b == "00:00") {
            //                    b = "24:00";
            //                }
            //                if (a == "00:00") {
            //                    a = "24:00";
            //                }
            //                // alert(a);
            //                var difference = Math.abs(toSeconds(c) - (toSeconds(a) - toSeconds(b)));

            //                // format time differnece
            //                var result = [
            //                    Math.floor(difference / 3600), // an hour has 3600 seconds
            //                    Math.floor((difference % 3600) / 60), // a minute has 60 seconds
            //                    difference % 60
            //                ];
            //                // 0 padding and concatation
            //                result = result.map(function (v) {
            //                    return v < 10 ? '0' + v : v;
            //                }).join(':');
            //                // alert(result);
            //                if (toSeconds(result) > toSeconds('24:00:00')) {
            //                    var difference = Math.abs(toSeconds(result) - toSeconds('24:00:00'));

            //                    // format time differnece
            //                    var result = [
            //                        Math.floor(difference / 3600), // an hour has 3600 seconds
            //                        Math.floor((difference % 3600) / 60), // a minute has 60 seconds
            //                        difference % 60
            //                    ];
            //                    // 0 padding and concatation
            //                    result = result.map(function (v) {
            //                        return v < 10 ? '0' + v : v;
            //                    }).join(':');
            //                    $("#tbElapsedTimes_" + r + "").val(result);
            //                }
            //                else {
            //                    $("#tbElapsedTimes_" + r + "").val(result);
            //                }
            //            }
            //        }
            //        else {
            //            $("#formerror").show();
            //            $("#formerror span").text("Start Time and End Time isnt within the selected shift period");
            //            alert('Start Time and End Time isnt within the selected shift period.');
            //        }
            //    }
            //}
            //22/08/2019 - JR - SG - Remove validation that checks if time entered falls within shift. - Commented out Original - Ends Here


            //22/08/2019 - JR - SG - Remove validation that checks if time entered falls within shift. - Calculation without shift Validation - Starts Here

            if (response[0].NextDay == false) {
                var difference = Math.abs(toSeconds(a) - toSeconds(b));

                // format time differnece
                var result = [
                    Math.floor(difference / 3600), // an hour has 3600 seconds
                    Math.floor((difference % 3600) / 60), // a minute has 60 seconds
                    difference % 60
                ];
                // 0 padding and concatation
                result = result.map(function (v) {
                    return v < 10 ? '0' + v : v;
                }).join(':');
                // alert(result);
                $("#tbElapsedTimes_" + r + "").val(result);
            }
            else {
                // Textbox
                var q = new Date('01/01/2001 ' + a).getTime();//StartTime
                var e = new Date('02/01/2001 ' + b).getTime();//EndTime
                var z = new Date('01/01/2001 ' + b).getTime();//EndTime

                // DB value shift
                var x = new Date('01/01/2001 ' + response[0].StartTime + ':00').getTime();//StartTime
                var y = new Date('02/01/2001 ' + response[0].EndTime + ':00').getTime();//EndTime
                var i = new Date('01/01/2001 ' + response[0].EndTime + ':00').getTime();//if End time is in same day
                var prev = new Date('01/01/2001 ' + '24:00').getTime();

                var c = "24:00:00";
                if (b == "00:00") {
                    b = "24:00";
                }
                if (a == "00:00") {
                    a = "24:00";
                }
                // alert(a);
                var difference = Math.abs(toSeconds(c) - (toSeconds(a) - toSeconds(b)));

                // format time differnece
                var result = [
                    Math.floor(difference / 3600), // an hour has 3600 seconds
                    Math.floor((difference % 3600) / 60), // a minute has 60 seconds
                    difference % 60
                ];
                // 0 padding and concatation
                result = result.map(function (v) {
                    return v < 10 ? '0' + v : v;
                }).join(':');
                // alert(result);
                if (toSeconds(result) > toSeconds('24:00:00')) {
                    var difference = Math.abs(toSeconds(result) - toSeconds('24:00:00'));

                    // format time differnece
                    var result = [
                        Math.floor(difference / 3600), // an hour has 3600 seconds
                        Math.floor((difference % 3600) / 60), // a minute has 60 seconds
                        difference % 60
                    ];
                    // 0 padding and concatation
                    result = result.map(function (v) {
                        return v < 10 ? '0' + v : v;
                    }).join(':');
                    $("#tbElapsedTimes_" + r + "").val(result);
                }
                else {
                    $("#tbElapsedTimes_" + r + "").val(result);
                }

            }
            //22/08/2019 - JR - SG - Remove validation that checks if time entered falls within shift. - Calculation without shift Validation - Ends Here

        }
    }
}

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


function openWarningModal() {
    $("#completewarning").hide();
    $("#setComplete").hide();
    var f = document.getElementById("ddlDepartment");
    var Department = f.options[f.selectedIndex].value;

    if ($('#hfComplete').val() == "Y") {
        $("#completewarning").show();
        $("#setComplete").hide();
    }
    else {
        $("#completewarning").hide();
        $("#setComplete").show();
    }

    $('#warningmodal').modal('show');
    $("#extrref").hide();

    if (Department == "EXTR") {
        $("#extrref").show();
    }
    $('#tbRef').focus();
}
function closeWarningModal() {
    $('#warningmodal').modal('hide');
}
function SaveForm(OverrideWarning) {
    closeWarningModal();
    showprogressbar();
    $("#formerror").hide();
    var mydata = [];
    var scrapdata = [];
    var CloseJob;
    var NoTimeCode = false;
    var RunTimeWithNoProduction = false;

    CloseJob = false;
    var f = document.getElementById("ddlDepartment");
    var Department = f.options[f.selectedIndex].value;

    var d = document.getElementById("ddlWorkCentre");
    var WorkCentre = d.options[d.selectedIndex].value;

    var z = document.getElementById("ddlShift");
    var Shift = z.options[z.selectedIndex].value;

    var Date = document.getElementById("txtDate").value.trim();

    var JobUom = document.getElementById("tbJobUom").value.trim();

    var StockCode = document.getElementById("tbStockCode").value.trim();

    var Reference = document.getElementById("tbRef").value.trim();

    var Job = document.getElementById("tbJob").value.trim();

    var ProductionQuantity = document.getElementById("tbProdQty").value.trim();

    var Operation = GetOperation();

    //  alert("Operation No From Method " + Operation);

    var table = document.getElementById("gdvDisplay");
    var column_count = table.rows[1].cells.length;
    var rowcount = table.rows.length;

    if (column_count > 0) {
        $("#gdvDisplay").find('tr.nr').each(function () {

            //GetRowID
            var rowid = $(this).find('td:eq(1) input[type="text"]').attr('id');
            var row = rowid.split('_');

            var ElapsedTime = $(this).find('td:eq(3) input[type="text"]').val();
            if (isNaN(ElapsedTime)) { ElapsedTime = timeToDecimal(($(this).find('td:eq(3) input[type="text"]').val())); }
            else { ElapsedTime = $(this).find('td:eq(3) input[type="text"]').val(); }

            var StartTime = $(this).find('td:eq(1) input[type="text"]').val();
            var EndTime = $(this).find('td:eq(2) input[type="text"]').val();
            var TimeCode = $(this).find('td:eq(4) input[type="text"],select').val();
            if (ElapsedTime && TimeCode === '') {
                NoTimeCode = true;
            }

            if (ProductionQuantity === '' && TimeCode === '01') {
                RunTimeWithNoProduction = true;
            }

            var myObject = new Object();
            myObject.row = row[1];
            myObject.StockCode = StockCode;
            myObject.Job = Job;
            myObject.Department = Department;
            myObject.WorkCentre = WorkCentre;
            myObject.Shift = Shift;
            myObject.Date = Date;
            myObject.StartTime = StartTime;
            myObject.EndTime = EndTime;
            myObject.ElapsedTime = ElapsedTime;
            myObject.TimeCode = TimeCode;
            myObject.ProductionQuantity = ProductionQuantity;
            myObject.JobUom = JobUom;
            myObject.Operation = Operation.Operation;
            myObject.CloseJob = CloseJob;
            myObject.Reference = Reference;

            mydata.push(myObject);
            //Rememeber to check if modal exists
            if (!document.getElementById("tblScrap_" + row[1] + "")) {
                // alert("noscrap");
                //NoScrap
            }
            else {
                //Get scrap for corresponding row
                $("#tblScrap_" + row[1] + "").find('tbody > tr').each(function () {
                    var ScrapCode = $(this).find('td:eq(1) input[type="text"],select').val();
                    var ScrapAmount = $(this).find('td:eq(1) input[type="text"]').val();
                    if (ScrapCode != "" && ScrapAmount != "") {
                        var myscrapObject = new Object();
                        myscrapObject.row = row[1];
                        myscrapObject.ScrapCode = ScrapCode;
                        myscrapObject.ScrapAmount = ScrapAmount;
                        scrapdata.push(myscrapObject);
                    }
                    //alert(ScrapCode + "   " + ScrapAmount + "   " + row[0])
                });
            }
        });
    }

    if (RunTimeWithNoProduction === false) {
        ValidateOperation();
    }
    else {
        hideprogressbar();
        if (confirm("Warning: You Have Run Time without any production do you want to continue?")) {
            showprogressbar();
            ValidateOperation();
        } else {

        }

    }
    function SaveScrap(data) {
        // alert(data);

        var myscrapString = JSON.stringify({ details: JSON.stringify(data) });
        var exportscrapdata = myscrapString;
        $.ajax({
            type: "POST",
            url: "LabourPost/SaveScrap",
            contentType: "application/json; charset=utf-8",
            data: exportscrapdata,
            dataType: "json",
            success: OnSuccess,
            failure: function (response) {
                //waitingDialog.hidePleaseWait();
                $("#formerror").show();
                $("#formerror span").text(response);
            },
            error: function (response) {
                //waitingDialog.hidePleaseWait();
                $("#formerror").show();
                $("#formerror span").text(response);
            }
        });

        function OnSuccess(response) {
            //waitingDialog.hidePleaseWait();

            if (response == 'Posted Successfully.') {
                $("#formerror").show();
                $("#formerror span").text(response);
            }
            else if (response.startsWith('Warning')) {
                $("#warningspan").text(response);
                openWarningModal();
            }
        }
    }
    function Save() {

        if (NoTimeCode === false) {
            SaveScrap(scrapdata);

            //Save Labor
            var myString = JSON.stringify({ details: JSON.stringify(mydata) });
            var exportdata = myString;
            $.ajax({
                type: "POST",
                url: "LabourPost/SaveForm",
                contentType: "application/json; charset=utf-8",
                data: exportdata,
                dataType: "json",
                success: OnSuccess,
                failure: function (response) {
                    //waitingDialog.hidePleaseWait();
                    $("#formerror").show();
                    $("#formerror span").text(response);
                    //waitingDialog.hidePleaseWait();
                },
                error: function (response) {
                    //waitingDialog.hidePleaseWait();
                    $("#formerror").show();
                    $("#formerror span").text(response);
                    // waitingDialog.hidePleaseWait();
                }
            });

            function OnSuccess(response) {
                hideprogressbar();
                //waitingDialog.hidePleaseWait();
                if (response == 'Posted Successfully.') {
                    $("#formerror").show();
                    $("#formerror span").text(response);
                    // ResetForm();
                    window.setTimeout(function () {
                        // Move to a new location or you can do something else
                        window.location.reload(true);
                    }, 2000);
                    ;
                }
                else if (response != "Posted Successfully.") {
                    // alert("ERROR");
                    $("#formerror").show();
                    $("#formerror span").text(response);
                }
            }

        }

        else {
            hideprogressbar();
            alert("Please review Time Codes. You cannot have Elapsed Time without a time code");
        }

    }
    function ValidateOperation() {
        if (Operation.Milestone === 'N' || Operation.Operation === '1') {
            Save();
        }
        else {
            //If Milestone, then check if previous operation is complete
            var PreviousOperation = Number(Operation.Operation) - 1;
            var Complete = GetPreviousOperation(PreviousOperation);
            if (Complete === 'Y') {
                //post
                Save();
            }
            else {
                hideprogressbar();
                //stop post
                //waitingDialog.hidePleaseWait();
                alert("The current operation is a milestone, and the previous operation is not complete for this job.Please complete the previous operation to continue.");
            }


        }
    }


}
function ResetForm() {
    document.getElementById("txtDate").value = "";

    $Shift = document.getElementById("ddlShift");
    $Shift.value = "";

    __doPostBack('__Page', '');
}
function GetTimeCodes(id) {
    // alert("here");
    var e = document.getElementById("ddlDepartment");
    var Department = e.options[e.selectedIndex].value;

    var myString = JSON.stringify({
        Department: Department
    });
    var exportdata = myString;

    $.ajax({
        type: "POST",
        url: "LabourPost/GetTimeCodes",
        data: exportdata,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {
            alert(response);
        }
    });

    function OnSuccess(response) {
        if (response) {
            var ddlTimeCodes = $("myselect_" + id + "");

            ddlTimeCodes.empty();
            $(".myselect_" + id + "").prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                $(".myselect_" + id + "")
                    .append($("<option></option>")
                        .attr("value", this['Value'].trim())
                        .text(this['Text']));

                //alert(this['Value'] + ' space ' + this['Text']);
            });
        }
    }
}
function GetScrapCodes(id, r) {
    $.ajax({
        type: "POST",
        url: "LabourPost/GetScrapCodes",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess,
        failure: function () {

        }
    });

    function OnSuccess(response) {
        if (response) {
            var ddlScrapCodes = $("myscrapselect_" + id + "_" + r + "");

            ddlScrapCodes.empty();
            $(".myscrapselect_" + id + "_" + r + "").prepend("<option value='' selected='selected'></option>");

            $.each(response, function () {
                $(".myscrapselect_" + id + "_" + r + "")
                    .append($("<option></option>")
                        .attr("value", this['Value'].trim())
                        .text(this['Text']));

                //alert(this['Value'] + ' space ' + this['Text']);
            });
        }
    }
}
function tabletab() {
    $(document).on('keydown change', '#gdvDisplay tr .select2-selection', function (e) {
        var keyCode = e.keyCode || e.which;
        var r = $('#gdvDisplay tr').length;
        i = r;
        // alert(i);
        //check if tb exists(for delete purposes)if row is deleted next Id will be duplicated
        var myEle = document.getElementById("tbStartTimes_" + i);
        if (myEle) {
            for (var index = 1; index < 15; index++) {
                if (document.getElementById("tbStartTimes_" + index)) { }
                else {
                    var row = index
                    r = row;
                    break;
                }
            }
        }
        if (keyCode == 9) {
            appendTable(r);
            // TimeCode Select initialization
            $(".tabselect").select2({
                placeholder: "",
                allowClear: true
            });
            $.fn.select2.defaults.set("", "");
            GetTimeCodes(r);
        }
        if (r != 0) {
            $('#tbStartTimes_' + (r)).val($('#tbEndTimes_' + (r - 1)).val());
        }

        $('#tbStartTimes_' + r).focus();//focus moved to first column of new row. Tabbing occures after code completed therefore second column of new row will have focus.
    });
}
function deleteRow(lnk) {
    // alert(lnk);
    var row = lnk.parentNode.parentNode;
    var rowIndex = row.rowIndex;
    if (rowIndex == 1) {
    }
    else {
        document.getElementById('gdvDisplay').deleteRow(rowIndex);
    }
}
function deleteScrapRow(id, row) {
    //alert(id,row);

    var table = document.getElementById('tblScrap_' + id + '');
    if (table.rows.length > 2) {
        $('#tblScrap_' + id + ' tr.delscrapLine_' + row + '').remove();
    }
    else {
        // alert("Cannot delete the last row")
        //cant delete 1 row left
    }
}
function tablecal() {
    $(document).on('keydown change', '#gdvDisplay tr input:text.tabcalc', function (e) {
        var keyCode = e.keyCode || e.which;

        if (keyCode == 9) {
            var r = $('#gdvDisplay tr').length;

            // gdvDisplay.CurrentCell.RowIndex
            // alert($(this).attr('id'));
            var row = $(this).attr('id').split('_');
            //alert(row[1]);
            ElapsedTime(row[1]);
        }
    });
}
function Scrap(id) {
    //check if modal exists
    var myElem = document.getElementById("scrapmodal_'" + id + "");
    //if doesnt exist
    if ($(".scrapmod_" + +id + "")[0]) {
        //alert("exists");
        $('#scrapmodal_' + id + '').modal('show');
        $('#ddlScrapCodes_1_1').focus();
    }
    //show modal with data
    else {
        r = 1;
        // alert("create");
        $('#modal ').append(

            " <div class='modal scrapmod_" + id + " ' id='scrapmodal_" + id + "' role='dialog'>" +
            " <div class='modal-dialog' style='width:100%;text-align: center'>" +
            " <div class='modal-content'>" +
            " <div class='modal-header'>" +
            "<button type='button' class='close' data-dismiss='modal'>&times;</button>" +

            " <h4 class='modal-title'>Scrap Posting </h4>" +
            "  </div>" +

            "<div class='modal-body'>" +

            " <div class='table-responsive'>" +
            " <table class='table table-striped table-bordered table-hover topTable' id='tblScrap_" + id + "' style='font-size:8pt;'>" +
            " <thead>" +
            "  <tr>" +
            " <th>Scrap Code</th>" +
            " <th>Scrap Amount</th><th></th>" +
            "  </tr>" +
            " </thead>" +
            "<tbody>" +
            "<tr class = 'delscrapLine_" + r + " '><td><div class='input-group add-on'>" +
            "<select id='ddlScrapCodes_" + id + "_" + r + "' class='js-example-basic-single myscrapselect_" + id + "_" + r + "' ></select>" +
            "</div><td>" +
            "<div class='input-group-btn'>" +
            "<input class='scraptabkey_" + id + " form-control input-sm' id='tbScrapQtys_" + id + "_" + r + "' type='text' value=''/>" +
            "</div></td>" +
            "<td><a  class='  btn btn-danger btn-xs' onclick='deleteScrapRow(" + id + "," + r + ")' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            " </div>" +

            " </div>" +
            " <div class='modal-footer'> " +
            "<button type='button' class='btn btn-info close-modal' data-dismiss='modal'>Close</button>" +
            "<input type='hidden' name='hfRow' id='hfRow' value='" + id + "'>" +
            "</div></div></div></div>");

        $('#scrapmodal_' + (id) + '').modal('show');
        $(".js-example-basic-single").select2({
            placeholder: "",
            allowClear: true
        });
        //alert(GetSelectedRow(id));
        GetScrapCodes(id, r);
        //allow tabbing
        scraptab((id));

        $('#ddlScrapCodes_1_1').focus();
        // var myBookId = $(this).data('id');
        // $(".modal-body #bookId").val(myBookId);
    }
}
function scraptab(id) {
    // alert("in here id = " + id);

    $(document).on('keydown change', '#tblScrap_' + id + ' tr input:text.scraptabkey_' + id + '', function (e) {
        var i = $('#tblScrap_' + id + ' tr').length;
        r = i;
        //check if tb exists(for delete purposes)if row is deleted next Id will be duplicated
        //var myEle = document.getElementById("tbScrapQtys_" + id + "_" + r + "");
        //if (myEle) {
        //          for (var index = 1; index < 15; index++) {
        //        //Find a Unique rowNo
        //        while (document.getElementById("tbScrapQtys_" + id + "_" + index + "") != "undefined" || document.getElementById("tbScrapQtys_" + id + "_" + index + "") != "14") {
        //            var row = index
        //            r = row;
        //            break;
        //        }
        //    }
        //}
        var keyCode = e.keyCode || e.which;
        if (keyCode == 9) {
            // alert("intable table");
            $('#tblScrap_' + id + ' tbody').append(

                "<tr  class = 'delscrapLine_" + r + "'id ='sr_" + id + "_" + r + "'><td><div class='input-group add-on'>" +
                "<select id='ddlScrapCodes_" + id + "_" + r + "' class='js-example-basic-single myscrapselect_" + id + "_" + r + "' ></select>" +
                "</div><td>" +
                "<div class='input-group-btn'>" +
                "<input class='scraptabkey_" + id + "  form-control input-sm' id='tbScrapQtys_" + id + "_" + r + "' type='text' value=''/>" +
                "</div></td>" +
                "<td><a  class=' delscrapLine btn btn-danger btn-xs' onclick='deleteScrapRow(" + id + "," + r + ")' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td>" +

                " </tr>");

            $(".js-example-basic-single").select2({
                placeholder: "",
                allowClear: true
            });
            GetScrapCodes(id, r);
        }
    });
}
function GetSelectedRow(lnk) {
    var row = lnk.parentNode.parentNode;
    var rowIndex = row.rowIndex;
    return rowIndex;
}
function appendTable(r) {
    $('#gdvDisplay tbody').append(
        "<tr class='nr'><td>" + r + "</td><td><div class='input-group add-on'><input class='time form-control input-sm' id='tbStartTimes_" + r + "' type='text' value=''/>" +
        "</td><td><div class='input-group add-on'>" +
        " <input class='time form-control input-sm tabcalc' id=tbEndTimes_" + r + " type='text' value=''/>" +
        "</div></td>" +
        "<td><div class='input-group-btn'>" +
        "<input class=' form-control input-sm ' id=tbElapsedTimes_" + r + " type='text' value=''/>" +
        "</div>" +
        "</td><td><div class='input-group add-on '>" +
        "<select id='ddlTimeCodes_" + r + "' class='js-example-basic-single tabselect myselect_" + r + "'></select>" +
        "</div>" +
        "" +
        "<td><a href='#' class='ref_" + r + " delLine btn btn-danger btn-xs' onclick='deleteRow(this)' tabIndex='-1'><span class='fa fa-trash-o' aria-hidden='true'  title='Delete Line'></span></a></td></tr>");

    var time = document.getElementsByClassName('time');
    //var EndTime = document.getElementsByClassName('tbEndTimes_'+r);

    for (var i = 0; i < time.length; i++) {
        time[i].addEventListener('keyup', function (e) {
            var reg = /[0-9]/;
            if (this.value.length == 2 && reg.test(this.value)) {
                this.value = this.value + ":";
            } //Add colon if string length > 2 and string is a number
            if (this.value.length == 5) {
                //this.value = this.value.substr(0, this.value.length - 1);
                $('#tbEndTimes_' + r).focus();
            } //Delete the last digit if string length > 5
        }
        );
    }
}
function timeToDecimal(t) {
    return t.split(':')
        .map(function (val) { return parseInt(val, 10); })
        .reduce(function (previousValue, currentValue, index, array) {
            return previousValue + currentValue / Math.pow(60, index);
        });
}

function ExtrRef() {
    var f = document.getElementById("ddlDepartment");
    var Department = f.options[f.selectedIndex].value;

    $("#extrref").hide();

    if (Department == "EXTR") {
        $("#extrref").show();
    }
}