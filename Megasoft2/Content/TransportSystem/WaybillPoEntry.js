function SaveToGrid() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");
    var mydata = [];

    $('#tblDetail').find('tr').each(function () {
        var Exist = false;
        var row = $(this);
        if (row.find('input[type="checkbox"]').is(':checked')) {
            var TrackId = row.find('td:eq(1)').text();
            var Waybill = row.find('td:eq(2)').text();
            var DispatchNote = row.find('td:eq(3)').text();
            var DispatchNoteLine = row.find('td:eq(4)').text();
            var Customer = row.find('td:eq(5)').text();
            var Province = row.find('td:eq(6)').text();
            var Town = row.find('td:eq(7)').text();
            var StockCode = row.find('td:eq(8)').text();
            var StockDesc = row.find('td:eq(9)').text();
            var DispatchQty = row.find('td:eq(10)').text();
            var DispatchUom = row.find('td:eq(11)').text();
            var LoadQty = row.find('td:eq(12)').text();
            var LoadUom = row.find('td:eq(13)').text();
            var Pallets = row.find('td:eq(14)').text();
            var Weight = row.find('td:eq(15)').text();
            var Notes = row.find('td:eq(16)').text();

            $('#tblPo').find('tr').each(function () {
                var rows = $(this);
                var PoWaybill = rows.find('td:eq(2)').text();
                var PoDispatchNote = rows.find('td:eq(3)').text();
                var PoDispatchNoteLine = rows.find('td:eq(4)').text();
                //    alert(Waybill.trim() + PoWaybill.trim() + DispatchNote.trim() + PoDispatchNote.trim() + DispatchNoteLine.trim() + PoDispatchNoteLine.trim());
                if (Waybill.trim() == PoWaybill.trim() && DispatchNote.trim() == PoDispatchNote.trim() && DispatchNoteLine.trim() + PoDispatchNoteLine.trim()) {
                    Exist = true;
                }
            });
            if (Exist == false) {
                var myObject = new Object();
                myObject.TrackId = TrackId;
                myObject.Waybill = Waybill;
                myObject.DispatchNote = DispatchNote;
                myObject.DispatchNoteLine = DispatchNoteLine;
                myObject.Customer = Customer;
                myObject.Province = Province;
                myObject.Town = Town;
                myObject.StockCode = StockCode;
                myObject.StockDesc = StockDesc;
                myObject.DispatchQty = DispatchQty;
                myObject.DispatchUom = DispatchUom;
                myObject.LoadQty = LoadQty;
                myObject.LoadUom = LoadUom;
                myObject.Pallets = Pallets;
                myObject.Weight = Weight;
                myObject.Notes = Notes;
                mydata.push(myObject);
            }
        }
    });
    var trHTML = '';
    $.each(mydata, function (index) {
        trHTML +=
                 '<tr class="nr">'
                + '<td><a href="#" class="delLine btn btn-danger btn-xs"  tabindex="-1"><span class="fa fa-trash-o" aria-hidden="true" title="Delete Line" tabindex="-1"></span></a></td><td>' + mydata[index].TrackId + '</td><td>' + mydata[index].Waybill + '<input type="hidden" value="S"/></td><td>' + mydata[index].DispatchNote + '</td><td>' + mydata[index].DispatchNoteLine + '</td><td>' + mydata[index].Customer + '</td><td><input class="Price form-control input-xs" name="PoPrice" ></td><td><input class="Qty form-control input-xs" name="PoQty"></td><td>' + mydata[index].Province + '</td><td>' + mydata[index].Town + '</td><td>' + mydata[index].StockCode + '</td><td>' + mydata[index].DispatchQty + '</td><td>' + mydata[index].DispatchUom + '</td><td>' + mydata[index].LoadQty + '</td><td>' + mydata[index].LoadUom + '</td><td>' + mydata[index].Pallets + '</td><td>' + mydata[index].Weight + '</td><td>' + mydata[index].Notes + '</td>'
                + '</tr>'
    });

    $('#tblPo').append(trHTML);
    document.getElementById('txtSupplier').disabled = true;
    //$('modal-container').modal('hide');
}

$(document).ready(function () {
    //UltimateScrollingTable('#tblPo', 200, 2020, 0);

    $('#tblPo').on("click", ".delLine", function () {
        //2alert('deleted');
        $(this).closest("tr").remove();
    });
});

function PostPurchaseOrder() {
    $('#errordiv').text("");
    $('#errordiv').removeClass("alert alert-danger");

    if ($('#TaxCode').val() == "")
    {
        alert('Please select a Tax Code.');
        return false;
    }

    showprogressbar();
    var post = true;

    var mydata = [];


    var Taxable = $("#Taxable").is(":checked");
    var TaxCode = $('#TaxCode').val();



    $("#tblPo > tbody > tr ").each(function (i, el) {


        var myObject = new Object();
        var $tds = $(this).find('td');
        if ($tds.eq(2).find('input[type=hidden]').val() == "S")
        {
            var Supplier = document.getElementById('Supplier').value;
            var TrackId = $tds.eq(1).text();
            var Waybill = $tds.eq(2).text();
            var DispatchNote = $tds.eq(3).text();
            var DispatchNoteLine = $tds.eq(4).text();
            var Notes = $tds.eq(17).text();
            var PoQty = $(this).find('input.Qty').val();
            var PoPrice = $(this).find('input.Price').val();
            var Town = $tds.eq(9).text();
            var Customer = $tds.eq(5).text();
            var Weight = $tds.eq(16).text();
            var LineType = $tds.eq(2).find('input[type=hidden]').val();

            myObject.Supplier = Supplier;
            myObject.TrackId = TrackId;
            myObject.Waybill = Waybill;
            myObject.DispatchNote = DispatchNote;
            myObject.DispatchNoteLine = DispatchNoteLine;
            myObject.Notes = Notes;
            myObject.PoQty = PoQty;
            myObject.PoPrice = PoPrice;
            myObject.Town = Town;
            myObject.Customer = Customer;
            myObject.Weight = Weight;
            myObject.LineType = LineType;
            myObject.Taxable = Taxable;
            myObject.TaxCode = TaxCode;
        }
        else
        {
            var LineType = $tds.eq(2).find('input[type=hidden]').val();
            var Comment = $tds.eq(2).text();

            myObject.LineType = LineType;
            myObject.Comment = Comment;
        }






        mydata.push(myObject);
    });
    var error = "";
    //alert(mydata[0].DispatchNote);

    for (var i = 0; i < mydata.length; i++) {
        if (mydata[i].PoPrice === "") {
            error += "\u2022Purchase Order Price for dispatch note " + mydata[i].DispatchNote + ", line " + mydata[i].DispatchNoteLine + " cannot be empty.\n"
        }
        if (mydata[i].PoQty === "") {
            error += "\u2022Purchase Order Quantity for dispatch note " + mydata[i].DispatchNote + ", line " + mydata[i].DispatchNoteLine + " cannot be empty.\n"
        }
    }
    if (error === "") {
        var myString = JSON.stringify({ details: JSON.stringify(mydata) });
        var exportdata = myString;
        //alert(exportdata);
        $.ajax({
            type: "POST",
            url: "TransportSystemWaybillPoEntry/PostPurchaseOrder",
            data: exportdata,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccess,
            failure: OnErrorCall,
            fail: function (response) {
                alert(response);
            }
        });

        function OnSuccess(response) {
            hideprogressbar();
            if (response != "") {
                //alert(response);
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");

                if(response.startsWith("Waybills posted successfully. Purchase Order :"))
                {
                    $("#tblPo > tbody > tr ").remove();
                }
            }
        }

        function OnErrorCall(response) {
            hideprogressbar();
            if (response != "") {
                $('#errordiv').text(response);
                $('#errordiv').addClass("alert alert-danger");
            }
        }
    }
    else {
        hideprogressbar();
        //alert(error);
        $('#errordiv').text(error);
        $('#errordiv').addClass("alert alert-danger");
    }
}
