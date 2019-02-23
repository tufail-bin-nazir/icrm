$(document).ready(function () {
    /*
    "mnt1feedbacksinquiries"  
    "mnt1feedbackscompliants"  
     "mnt1feedbacksappreciations"  
    "mnt1feedbackssuggestions"  

   "mnt2feedbacksinquiries"  
   "mnt2feedbackscompliants"  
    "mnt2feedbacksappreciations"  
    "mnt2feedbackssuggestions" 

   "mnt3feedbacksinquiries"  
   "mnt3feedbackscompliants"  
  "mnt3feedbacksappreciations"  
    "mnt3feedbackssuggestions"  

 mnt1feedbackswalkin
 mnt1feedbackswhatsapp 
 mnt1feedbacksmobile 
 mnt1feedbackstollfree 
 mnt1feedbacksemail   
    
    */

  /*  mnt1feedbacksfinance 
 mnt1feedbackstalentmanagement 
   mnt1feedbacksadministrations 
   mnt1feedbacksoperations 
   mnt1feedbackssahlfeedback 
  mnt1feedbackssahlmds 
   mnt1feedbackssahltraining 
   */




    var mnt1feedbacksfinance = $("#mnt1feedbacksfinance").val();
    var mnt1feedbackstalentmanagement = $("#mnt1feedbackstalentmanagement").val();
    var mnt1feedbacksadministrations = $("#mnt1feedbacksadministrations").val();
    var mnt1feedbacksoperations = $("#mnt1feedbacksoperations").val();
    var mnt1feedbackssahlfeedback = $("#mnt1feedbackssahlfeedback").val();
    var mnt1feedbackssahlmds = $("#mnt1feedbackssahlmds").val();
    var mnt1feedbackssahltraining = $("#mnt1feedbackssahltraining").val();


    var mnt2feedbacksfinance = $("#mnt2feedbacksfinance").val();
    var mnt2feedbackstalentmanagement = $("#mnt2feedbackstalentmanagement").val();
    var mnt2feedbacksadministrations = $("#mnt2feedbacksadministrations").val();
    var mnt2feedbacksoperations = $("#mnt2feedbacksoperations").val();
    var mnt2feedbackssahlfeedback = $("#mnt2feedbackssahlfeedback").val();
    var mnt2feedbackssahlmds = $("#mnt2feedbackssahlmds").val();
    var mnt2feedbackssahltraining = $("#mnt2feedbackssahltraining").val();


    var mnt3feedbacksfinance = $("#mnt3feedbacksfinance").val();
    var mnt3feedbackstalentmanagement = $("#mnt3feedbackstalentmanagement").val();
    var mnt3feedbacksadministrations = $("#mnt3feedbacksadministrations").val();
    var mnt3feedbacksoperations = $("#mnt3feedbacksoperations").val();
    var mnt3feedbackssahlfeedback = $("#mnt3feedbackssahlfeedback").val();
    var mnt3feedbackssahlmds = $("#mnt3feedbackssahlmds").val();
    var mnt3feedbackssahltraining = $("#mnt3feedbackssahltraining").val();


    /*  console.log(mnt1feedbacksappreciations + mnt1feedbackscompliants + mnt1feedbacksinquiries + mnt1feedbackssuggestions);
      console.log(mnt2feedbacksappreciations + mnt2feedbackscompliants + mnt2feedbacksinquiries + mnt2feedbackssuggestions);
      console.log(mnt3feedbacksappreciations + mnt3feedbackscompliants + mnt3feedbacksinquiries + mnt3feedbackssuggestions);
      */


    var mnt1feedbacksall = $("#mnt1feedbacksall").val();
    var mnt2feedbacksall = $("#mnt2feedbacksall").val();
    var mnt3feedbacksall = $("#mnt3feedbacksall").val();

    console.log(mnt1feedbacksall + mnt2feedbacksall + mnt3feedbacksall);

    /*
     var mnt3feedbacksfinance = $("#mnt3feedbacksfinance").val();
    var mnt3feedbackstalentmanagement = $("#mnt3feedbackstalentmanagement").val();
    var mnt3feedbacksadministrations = $("#mnt3feedbacksadministrations").val();
    var mnt3feedbacksoperations = $("#mnt3feedbacksoperations").val();
    var mnt3feedbackssahlfeedback = $("#mnt3feedbackssahlfeedback").val();
    var mnt3feedbackssahlmds = $("#mnt3feedbackssahlmds").val();
    var mnt3feedbackssahltraining = $("#mnt3feedbackssahltraining").val();
    
    */




    if (!parseFloat(mnt1feedbacksall) == 0) {
        mnt1feedbacksfinance = ((parseFloat(mnt1feedbacksfinance) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbackstalentmanagement = ((parseFloat(mnt1feedbackstalentmanagement) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksadministrations = ((parseFloat(mnt1feedbacksadministrations) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksoperations = ((parseFloat(mnt1feedbacksoperations) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbackssahlfeedback = ((parseFloat(mnt1feedbackssahlfeedback) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbackssahlmds = ((parseFloat(mnt1feedbackssahlmds) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbackssahltraining = ((parseFloat(mnt1feedbackssahltraining) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
    }


    if (!parseFloat(mnt2feedbacksall) == 0) {
        mnt2feedbacksfinance = ((parseFloat(mnt2feedbacksfinance) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbackstalentmanagement = ((parseFloat(mnt2feedbackstalentmanagement) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksadministrations = ((parseFloat(mnt2feedbacksadministrations) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksoperations = ((parseFloat(mnt2feedbacksoperations) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbackssahlfeedback = ((parseFloat(mnt2feedbackssahlfeedback) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbackssahlmds = ((parseFloat(mnt2feedbackssahlmds) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbackssahltraining = ((parseFloat(mnt2feedbackssahltraining) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
    }

    if (!parseFloat(mnt3feedbacksall) == 0) {
        mnt3feedbacksfinance = ((parseFloat(mnt3feedbacksfinance) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbackstalentmanagement = ((parseFloat(mnt3feedbackstalentmanagement) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksadministrations = ((parseFloat(mnt3feedbacksadministrations) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksoperations = ((parseFloat(mnt3feedbacksoperations) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbackssahlfeedback = ((parseFloat(mnt3feedbackssahlfeedback) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbackssahlmds = ((parseFloat(mnt3feedbackssahlmds) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbackssahltraining = ((parseFloat(mnt3feedbackssahltraining) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
    }
    /* console.log(mnt1feedbacksappreciations + mnt1feedbackscompliants + mnt1feedbacksinquiries + mnt1feedbackssuggestions);
     console.log(mnt2feedbacksappreciations + mnt2feedbackscompliants + mnt2feedbacksinquiries + mnt2feedbackssuggestions);
     console.log(mnt3feedbacksappreciations + mnt3feedbackscompliants + mnt3feedbacksinquiries + mnt3feedbackssuggestions);
 
     */



    var month1 = $("#month1").val();
    var month2 = $("#month2").val();
    var month3 = $("#month3").val();

    var colorbgmonth1 = [];
    var colormonth1 = [];

    var colorbgmonth2 = [];
    var colormonth2 = [];

    var colorbgmonth3 = [];
    var colormonth3 = [];

    if (month1 == "Jan") {
        colorbgmonth1[0] = " #9b59b6";
        colormonth1[0] = "black";
    }
    else if (month1 == "Feb") {
        colorbgmonth1[0] = " #eb984e";
        colormonth1[0] = "black";
    }
    else if (month1 == "Mar") {
        colorbgmonth1[0] = "blue";
        colormonth1[0] = "black";
    }
    else if (month1 == "Apr") {
        colorbgmonth1[0] = "green";
        colormonth1[0] = "black";
    }
    else if (month1 == "May") {
        colorbgmonth1[0] = "yellow";
        colormonth1[0] = "black";
    }
    else if (month1 == "Jun") {
        colorbgmonth1[0] = "orange";
        colormonth1[0] = "black";
    }
    else if (month1 == "Jul") {
        colorbgmonth1[0] = "red";
        colormonth1[0] = "black";
    }
    else if (month1 == "Aug") {
        colorbgmonth1[0] = "#DA70D6";
        colormonth1[0] = "black";
    }
    else if (month1 == "Sep") {
        colorbgmonth1[0] = "#FF00FF";
        colormonth1[0] = "black";
    }
    else if (month1 == "Oct") {
        colorbgmonth1[0] = "#FF00FF";
        colormonth1[0] = "black";
    }
    else if (month1 == "Nov") {
        colorbgmonth1[0] = "#F08080";
        colormonth1[0] = "black";
    }
    else if (month1 == "Dec") {
        colorbgmonth1[0] = "#CD5C5C";
        colormonth1[0] = "black";
    }







    console.log("months" + month1 + month2 + month3);

        /*    
         var mnt3feedbacksfinance = $("#mnt3feedbacksfinance").val();
        var mnt3feedbackstalentmanagement = $("#mnt3feedbackstalentmanagement").val();
        var mnt3feedbacksadministrations = $("#mnt3feedbacksadministrations").val();
        var mnt3feedbacksoperations = $("#mnt3feedbacksoperations").val();
        var mnt3feedbackssahlfeedback = $("#mnt3feedbackssahlfeedback").val();
        var mnt3feedbackssahlmds = $("#mnt3feedbackssahlmds").val();
        var mnt3feedbackssahltraining = $("#mnt3feedbackssahltraining").val();
        
         
       */



    if (!month1 == "") {
        var d1_1 = [
            [1325376000000, mnt1feedbacksfinance],
            [1328054400000, mnt1feedbackstalentmanagement],
            [1330560000000, mnt1feedbacksadministrations],
            [1333238400000, mnt1feedbacksoperations],
            [1335916800000, mnt1feedbackssahlfeedback],
            [1337238400000, mnt1feedbackssahlmds],
            [1339916800000, mnt1feedbackssahltraining]

        ];
    }

    if (!month2 == "") {
        var d1_2 = [
            [1325376000000, mnt2feedbacksfinance],
            [1328054400000, mnt2feedbackstalentmanagement],
            [1330560000000, mnt2feedbacksadministrations],
            [1333238400000, mnt2feedbacksoperations],
            [1335916800000, mnt2feedbackssahlfeedback],
            [1337238400000, mnt2feedbackssahlmds],
            [1339916800000, mnt2feedbackssahltraining]
        ];
    }


    if (!month3 == "") {
        var d1_3 = [
            [1325376000000, mnt3feedbacksfinance],
            [1328054400000, mnt3feedbackstalentmanagement],
            [1330560000000, mnt3feedbacksadministrations],
            [1333238400000, mnt3feedbacksoperations],
            [1335916800000, mnt3feedbackssahlfeedback],
            [1337238400000, mnt3feedbackssahlmds],
            [1339916800000, mnt3feedbackssahltraining]
        ];
    }
    /* var d1_4 = [
         [1325376000000, 15],
         [1328054400000, 10],
         [1330560000000, 15],
         [1333238400000, 20],
         [1335830400000, 15]
     ];*/



    var data1 = [];


    if (!month1 == "") {
        data1.push({
            label: month1,
            data: d1_1,
            bars: {
                show: true,
                barWidth: 12 * 24 * 60 * 60 * 560,
                fill: true,
                lineWidth: 1,
                order: 1,
                fillColor: colorbgmonth1[0]
            },
            color: colorbgmonth1[0]
        });
    }

    if (!month2 == "") {
        data1.push({
            label: month2,
            data: d1_2,
            bars: {
                show: true,
                barWidth: 12 * 24 * 60 * 60 * 560,
                fill: true,
                lineWidth: 1,
                order: 2,
                fillColor: "#89A54E"
            },
            color: "#89A54E"
        });
    }

    if (!month3 == "") {
        data1.push({
            label: month3,
            data: d1_3,
            bars: {
                show: true,
                barWidth: 12 * 24 * 60 * 60 * 560,
                fill: true,
                lineWidth: 1,
                order: 3,
                fillColor: "#4572A7"
            },
            color: "#4572A7"
        });
    }

    /*
        var data1 = [
            {
                label: month1,
                data: d1_1,
                bars: {
                    show: true,
                    barWidth: 12 * 24 * 60 * 60 * 560,
                    fill: true,
                    lineWidth: 1,
                    order: 1,
                    fillColor: "#AA4643"
                },
                color: "#AA4643"
            },
            {
                label: month2,
                data: d1_2,
                bars: {
                    show: true,
                    barWidth: 12 * 24 * 60 * 60 * 560,
                    fill: true,
                    lineWidth: 1,
                    order: 2,
                    fillColor: "#89A54E"
                },
                color: "#89A54E"
            },
            {
                label: month3,
                data: d1_3,
                bars: {
                    show: true,
                    barWidth: 12 * 24 * 60 * 60 * 560,
                    fill: true,
                    lineWidth: 1,
                    order: 3,
                    fillColor: "#4572A7"
                },
                color: "#4572A7"
            }
            */
    /*,
    {
        label: "Product 4",
        data: d1_4,
        bars: {
            show: true,
            barWidth: 12 * 24 * 60 * 60 * 300,
            fill: true,
            lineWidth: 1,
            order: 4,
            fillColor: "#80699B"
        },
        color: "#80699B"
    }*/
    //];


    /*  [1325376000000, mnt1feedbackswalkin],
            [1328054400000, mnt1feedbackswhatsapp],
            [1330560000000, mnt1feedbacksmobile],
            [1333238400000, mnt1feedbackstollfree],
            [1335916800000, mnt1feedbacksemail]*/

    var p = $.plot($("#placeholder"), data1, {
        xaxis: {
            min: (new Date(2011, 11, 15)).getTime(),
            max: (new Date(2012, 06, 18)).getTime(),
            mode: "time",
            timeformat: "%b",
            tickSize: [1, "month"],
            monthNames: ["Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training"],
            tickLength: 0, // hide gridlines
            axisLabel: 'Department',
            axisLabelUseCanvas: true,
            axisLabelFontSizePixels: 12,
            axisLabelFontFamily: 'Verdana, Arial, Helvetica, Tahoma, sans-serif',
            axisLabelPadding: 5
        },
        yaxis: {
            axisLabel: 'Value',
            axisLabelUseCanvas: true,
            axisLabelFontSizePixels: 12,
            axisLabelFontFamily: 'Verdana, Arial, Helvetica, Tahoma, sans-serif',
            axisLabelPadding: 5
        },
        grid: {
            hoverable: true,
            clickable: false,
            borderWidth: 1
        },
        legend: {
            labelBoxBorderColor: "none",
            position: "right"
        },
        series: {
            shadowSize: 1
        }
    });

    function showTooltip(x, y, contents, z) {
        $('<div id="flot-tooltip">' + contents + '</div>').css({
            top: y - 20,
            left: x - 90,
            'border-color': z,
        }).appendTo("body").show();
    }

    /*  mnt1feedbackswalkin
      mnt1feedbackswhatsapp
      mnt1feedbacksmobile
      mnt1feedbackstollfree
      mnt1feedbacksemail
      */


    function getMonthName(newTimestamp) {
        var d = new Date(newTimestamp);

        var numericMonth = d.getMonth();
        var monthArray = ["Finance", "Talent-Management", "Administration", "Operations", "Sahl-Feedback", "Mds", "Training"];

        var alphaMonth = monthArray[numericMonth];

        return alphaMonth;
    }

    $("#placeholder").bind("plothover", function (event, pos, item) {
        if (item) {
            if (previousPoint != item.datapoint) {
                previousPoint = item.datapoint;
                $("#flot-tooltip").remove();

                var originalPoint;

                if (item.datapoint[0] == item.series.data[0][6]) {
                    originalPoint = item.series.data[0][0];
                } else if (item.datapoint[0] == item.series.data[1][6]) {
                    originalPoint = item.series.data[1][0];
                } else if (item.datapoint[0] == item.series.data[2][6]) {
                    originalPoint = item.series.data[2][0];
                } else if (item.datapoint[0] == item.series.data[3][6]) {
                    originalPoint = item.series.data[3][0];
                }
                else if (item.datapoint[0] == item.series.data[4][6]) {
                    originalPoint = item.series.data[4][0];
                }
                else if (item.datapoint[0] == item.series.data[5][6]) {
                    originalPoint = item.series.data[5][0];
                }
                else if (item.datapoint[0] == item.series.data[6][6]) {
                    originalPoint = item.series.data[6][0];
                }
                /*
                else if (item.datapoint[0] == item.series.data[4][3]) {
                    originalPoint = item.series.data[4][0];
                }
                */
                var x = getMonthName(originalPoint);
                y = item.datapoint[1];
                z = item.series.color;

                showTooltip(item.pageX, item.pageY,
                    "<b>" + item.series.label + "</b><br /> " + x + " = " + y + "&",
                    z);
            }
        } else {
            $("#flot-tooltip").remove();
            previousPoint = null;
        }
    });


    if (!month1 == "") {

        try {
            $.each(p.getData()[0].data, function (i, el) {
                var o = p.pointOffset({ x: el[0], y: el[1] });
                $('<div class="data-point-label">' + el[1] + '%</div>').css({
                    position: 'absolute',
                    left: o.left - 25,
                    top: o.top - 20,
                    display: 'none'
                }).appendTo(p.getPlaceholder()).fadeIn('slow');
            });
        }
        catch (e) { }
    }

    if (!month2 == "") {
        try {
            $.each(p.getData()[1].data, function (i, el) {
                var o = p.pointOffset({ x: el[0], y: el[1] });
                $('<div class="data-point-label">' + el[1] + '%</div>').css({
                    position: 'absolute',
                    left: o.left - 4,
                    top: o.top - 20,
                    display: 'none'
                }).appendTo(p.getPlaceholder()).fadeIn('slow');
            });
        }
        catch (e) { }
    }


    if (!month3 == "") {
        try {
            $.each(p.getData()[2].data, function (i, el) {
                var o = p.pointOffset({ x: el[0], y: el[1] });
                $('<div class="data-point-label">' + el[1] + '%</div>').css({
                    position: 'absolute',
                    left: o.left + 24,
                    top: o.top - 20,
                    display: 'none'
                }).appendTo(p.getPlaceholder()).fadeIn('slow');
            });
        }
        catch (e) { }
    }



});