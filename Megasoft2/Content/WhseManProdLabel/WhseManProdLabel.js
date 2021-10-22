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


