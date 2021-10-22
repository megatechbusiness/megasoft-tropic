
function ApplyPercentage(ControlId)
{
    var amount = $('#' + ControlId + '.tk').attr('data-amount'),
    height = (100 - amount) * 80 / 100 + 20;

    $('#' + ControlId + ' .green').css({ height: height + '%' }).fadeIn(4000);
    //alert(ControlId + ' - ' + height + ' - ' + amount);
}


function FindTanks()
{//children('.level .col-lg-6').children('.tank').
    //alert("Here");
    $('#tanks').children('.trow').children('#tankrow').each(function () {
        $('.level').children('.tank').each(function () {
            //alert($(this).attr('id'));
            ApplyPercentage($(this).attr('id'));
        });
        
        //ApplyPercentage($(this).attr('id')); // "this" is the current element in the loop
    });
}

FindTanks();
//alert($('#tank1.tk').attr('data-amount'));
//$('#tank1 .green').css({ height: '90%' });