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




    var mnt1feedbacksall = $("#mnt1feedbacksall").val();
    var mnt2feedbacksall = $("#mnt2feedbacksall").val();
    var mnt3feedbacksall = $("#mnt3feedbacksall").val();
    var mnt4feedbacksall = $("#mnt4feedbacksall").val();
    var mnt5feedbacksall = $("#mnt5feedbacksall").val();
    var mnt6feedbacksall = $("#mnt6feedbacksall").val();
    var mnt7feedbacksall = $("#mnt7feedbacksall").val();
    var mnt8feedbacksall = $("#mnt8feedbacksall").val();
    var mnt9feedbacksall = $("#mnt9feedbacksall").val();
    var mnt10feedbacksall = $("#mnt10feedbacksall").val();
    var mnt11feedbacksall = $("#mnt11feedbacksall").val();
    var mnt12feedbacksall = $("#mnt12feedbacksall").val();


    var month1 = $("#month1").val();
    var month2 = $("#month2").val();
    var month3 = $("#month3").val();
    var month4 = $("#month4").val();
    var month5 = $("#month5").val();
    var month6 = $("#month6").val();
    var month7 = $("#month7").val();
    var month8 = $("#month8").val();
    var month9 = $("#month9").val();
    var month10 = $("#month10").val();
    var month11 = $("#month11").val();
    var month12 = $("#month12").val();
    

     

  


    

    

    

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
            [1325376000000, mnt1feedbacksall],
            [1327376000000, mnt2feedbacksall],
            [1330376000000, mnt3feedbacksall],
            [1333376000000, mnt4feedbacksall],
            [1335376000000, mnt5feedbacksall],
            [1338376000000, mnt6feedbacksall],
            [1341376000000, mnt7feedbacksall],
            [1343376000000, mnt8feedbacksall],
            [1346376000000, mnt9feedbacksall],
            [1348376000000, mnt10feedbacksall],
            [1351376000000, mnt11feedbacksall],
            [1353376000000, mnt12feedbacksall],

           

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
                fillColor: "#eb984e"
            },
            color: "black"
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
            max: (new Date(2012, 11, 18)).getTime(),
            mode: "time",
            timeformat: "%b",
            tickSize: [1, "month"],
            monthNames: [month1, month2, month3, month4, month5, month6, month7, month8, month9, month10, month11, month12],
            tickLength: 0, // hide gridlines
            axisLabel: 'Feedback Last 12 Months',
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
       
        series: {
            shadowSize: 1
        }
    });


    $('.legend').hide();

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
        var monthArray = [month1, month2, month3, month4, month5, month6, month7, month8, month9, month10, month11, month12];

        var alphaMonth = monthArray[numericMonth];

        return alphaMonth;
    }

    $("#placeholder").bind("plothover", function (event, pos, item) {
        if (item) {
            if (previousPoint != item.datapoint) {
                previousPoint = item.datapoint;
                $("#flot-tooltip").remove();

                var originalPoint;

                if (item.datapoint[0] == item.series.data[0][11]) {
                    originalPoint = item.series.data[0][0];
                } else if (item.datapoint[0] == item.series.data[1][11]) {
                    originalPoint = item.series.data[1][0];
                } else if (item.datapoint[0] == item.series.data[2][11]) {
                    originalPoint = item.series.data[2][0];
                } else if (item.datapoint[0] == item.series.data[3][11]) {
                    originalPoint = item.series.data[3][0];
                }
                else if (item.datapoint[0] == item.series.data[4][11]) {
                    originalPoint = item.series.data[4][0];
                }
                else if (item.datapoint[0] == item.series.data[5][11]) {
                    originalPoint = item.series.data[5][0];
                }
                else if (item.datapoint[0] == item.series.data[6][11]) {
                    originalPoint = item.series.data[6][0];
                }
                else if (item.datapoint[0] == item.series.data[7][11]) {
                    originalPoint = item.series.data[7][0];
                }
                else if (item.datapoint[0] == item.series.data[8][11]) {
                    originalPoint = item.series.data[8][0];
                }
                else if (item.datapoint[0] == item.series.data[9][11]) {
                    originalPoint = item.series.data[9][0];
                }
                else if (item.datapoint[0] == item.series.data[10][11]) {
                    originalPoint = item.series.data[10][0];
                }
                else if (item.datapoint[0] == item.series.data[11][11]) {
                    originalPoint = item.series.data[11][0];
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
                $('<div class="data-point-label">' + el[1] + '</div>').css({
                    position: 'absolute',
                    left: o.left - 5,
                    top: o.top - 20,
                    display: 'none'
                }).appendTo(p.getPlaceholder()).fadeIn('slow');
            });
        }
        catch (e) { }
    }

    



});