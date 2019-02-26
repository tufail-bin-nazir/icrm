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
    /*
   mnt1feedbackscrews" value="@ViewBag.mnt1feedbacksfinance" />
  mnt1feedbacksdrivers" value="@ViewBag.mnt1feedbackstalentmanagement" />
   mnt1feedbacksmanagers" value="@ViewBag.mnt1feedbacksadministrations" />
    mnt1feedbacksmds" value="@ViewBag.mnt1feedbacksoperations" />
   mnt1feedbacksstars" value="@ViewBag.mnt1feedbackssahlfeedback" />
  mnt1feedbacksmaintenance" value="@ViewBag.mnt1feedbackssahlmds" />
  mnt1feedbacksgel" value="@ViewBag.mnt1feedbackssahltraining" />
   mnt1feedbacksspecialist" value="@ViewBag.mnt1feedbackssahltraining" />
  mnt1feedbacksconsultants" value="@ViewBag.mnt1feedbackssahltraining" />

    */



    var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();


    var mnt2feedbackscrews = $("#mnt2feedbackscrews").val();
    var mnt2feedbacksdrivers = $("#mnt2feedbacksdrivers").val();
    var mnt2feedbacksmanagers = $("#mnt2feedbacksmanagers").val();
    var mnt2feedbacksmds = $("#mnt2feedbacksmds").val();
    var mnt2feedbacksstars = $("#mnt2feedbacksstars").val();
    var mnt2feedbacksmaintenance = $("#mnt2feedbacksmaintenance").val();
    var mnt2feedbacksgel = $("#mnt2feedbacksgel").val();
    var mnt2feedbacksspecialist = $("#mnt2feedbacksspecialist").val();
    var mnt2feedbacksconsultants = $("#mnt2feedbacksconsultants").val();


    var mnt3feedbackscrews = $("#mnt3feedbackscrews").val();
    var mnt3feedbacksdrivers = $("#mnt3feedbacksdrivers").val();
    var mnt3feedbacksmanagers = $("#mnt3feedbacksmanagers").val();
    var mnt3feedbacksmds = $("#mnt3feedbacksmds").val();
    var mnt3feedbacksstars = $("#mnt3feedbacksstars").val();
    var mnt3feedbacksmaintenance = $("#mnt3feedbacksmaintenance").val();
    var mnt3feedbacksgel = $("#mnt3feedbacksgel").val();
    var mnt3feedbacksspecialist = $("#mnt3feedbacksspecialist").val();
    var mnt3feedbacksconsultants = $("#mnt3feedbacksconsultants").val();


    /*  console.log(mnt1feedbacksappreciations + mnt1feedbackscompliants + mnt1feedbacksinquiries + mnt1feedbackssuggestions);
      console.log(mnt2feedbacksappreciations + mnt2feedbackscompliants + mnt2feedbacksinquiries + mnt2feedbackssuggestions);
      console.log(mnt3feedbacksappreciations + mnt3feedbackscompliants + mnt3feedbacksinquiries + mnt3feedbackssuggestions);
      */


    var mnt1feedbacksall = $("#mnt1feedbacksall").val();
    var mnt2feedbacksall = $("#mnt2feedbacksall").val();
    var mnt3feedbacksall = $("#mnt3feedbacksall").val();

    console.log(mnt1feedbacksall + mnt2feedbacksall + mnt3feedbacksall);

    /*
      var mnt1feedbackscrews = $("#mnt1feedbackscrews").val();
    var mnt1feedbacksdrivers = $("#mnt1feedbacksdrivers").val();
    var mnt1feedbacksmanagers = $("#mnt1feedbacksmanagers").val();
    var mnt1feedbacksmds = $("#mnt1feedbacksmds").val();
    var mnt1feedbacksstars = $("#mnt1feedbacksstars").val();
    var mnt1feedbacksmaintenance = $("#mnt1feedbacksmaintenance").val();
    var mnt1feedbacksgel = $("#mnt1feedbacksgel").val();
    var mnt1feedbacksspecialist = $("#mnt1feedbacksspecialist").val();
    var mnt1feedbacksconsultants = $("#mnt1feedbacksconsultants").val();
    
    */




    if (!parseFloat(mnt1feedbacksall) == 0) {
        mnt1feedbackscrews = ((parseFloat(mnt1feedbackscrews) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksdrivers = ((parseFloat(mnt1feedbacksdrivers) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksmanagers = ((parseFloat(mnt1feedbacksmanagers) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksmds = ((parseFloat(mnt1feedbacksmds) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksstars = ((parseFloat(mnt1feedbacksstars) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksmaintenance = ((parseFloat(mnt1feedbacksmaintenance) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksgel = ((parseFloat(mnt1feedbacksgel) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksspecialist = ((parseFloat(mnt1feedbacksspecialist) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksconsultants = ((parseFloat(mnt1feedbacksconsultants) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
    }


    if (!parseFloat(mnt2feedbacksall) == 0) {
        mnt2feedbackscrews = ((parseFloat(mnt2feedbackscrews) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksdrivers = ((parseFloat(mnt2feedbacksdrivers) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksmanagers = ((parseFloat(mnt2feedbacksmanagers) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksmds = ((parseFloat(mnt2feedbacksmds) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksstars = ((parseFloat(mnt2feedbacksstars) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksmaintenance = ((parseFloat(mnt2feedbacksmaintenance) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksgel = ((parseFloat(mnt2feedbacksgel) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksspecialist = ((parseFloat(mnt2feedbacksspecialist) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksconsultants = ((parseFloat(mnt2feedbacksconsultants) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
    }

    if (!parseFloat(mnt3feedbacksall) == 0) {
        mnt3feedbackscrews = ((parseFloat(mnt3feedbackscrews) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksdrivers = ((parseFloat(mnt3feedbacksdrivers) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksmanagers = ((parseFloat(mnt3feedbacksmanagers) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksmds = ((parseFloat(mnt3feedbacksmds) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksstars = ((parseFloat(mnt3feedbacksstars) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksmaintenance = ((parseFloat(mnt3feedbacksmaintenance) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksgel = ((parseFloat(mnt3feedbacksgel) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksspecialist = ((parseFloat(mnt3feedbacksspecialist) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksconsultants = ((parseFloat(mnt3feedbacksconsultants) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
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
      mnt1feedbackscrews = ((parseFloat(mnt1feedbackscrews) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksdrivers = ((parseFloat(mnt1feedbacksdrivers) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksmanagers = ((parseFloat(mnt1feedbacksmanagers) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksmds = ((parseFloat(mnt1feedbacksmds) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksstars = ((parseFloat(mnt1feedbacksstars) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksmaintenance = ((parseFloat(mnt1feedbacksmaintenance) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksgel = ((parseFloat(mnt1feedbacksgel) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksspecialist = ((parseFloat(mnt1feedbacksspecialist) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksconsultants = ((parseFloat(mnt1feedbacksconsultants) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
    
     
   */



    if (!month1 == "") {
        var d1_1 = [
            [1325376000000, mnt1feedbackscrews],
            [1328054400000, mnt1feedbacksdrivers],
            [1330560000000, mnt1feedbacksmanagers],
            [1333238400000, mnt1feedbacksmds],
            [1335916800000, mnt1feedbacksstars],
            [1337238400000, mnt1feedbacksmaintenance],
            [1339916800000, mnt1feedbacksgel],
            [1341238400000, mnt1feedbacksspecialist],
            [1343916800000, mnt1feedbacksconsultants]

        ];
    }

    if (!month2 == "") {
        var d1_2 = [
            [1325376000000, mnt2feedbackscrews],
            [1328054400000, mnt2feedbacksdrivers],
            [1330560000000, mnt2feedbacksmanagers],
            [1333238400000, mnt2feedbacksmds],
            [1335916800000, mnt2feedbacksstars],
            [1337238400000, mnt2feedbacksmaintenance],
            [1339916800000, mnt2feedbacksgel],
            [1341238400000, mnt2feedbacksspecialist],
            [1343916800000, mnt2feedbacksconsultants]
        ];
    }


    if (!month3 == "") {
        var d1_3 = [
            [1325376000000, mnt3feedbackscrews],
            [1328054400000, mnt3feedbacksdrivers],
            [1330560000000, mnt3feedbacksmanagers],
            [1333238400000, mnt3feedbacksmds],
            [1335916800000, mnt3feedbacksstars],
            [1337238400000, mnt3feedbacksmaintenance],
            [1339916800000, mnt3feedbacksgel],
            [1341238400000, mnt3feedbacksspecialist],
            [1343916800000, mnt3feedbacksconsultants]
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
            max: (new Date(2012, 08, 18)).getTime(),
            mode: "time",
            timeformat: "%b",
            tickSize: [1, "month"],
            monthNames: ["Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"],
            tickLength: 0, // hide gridlines
            axisLabel: 'Position',
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
        var monthArray = ["Crews", "Drivers", "Managers", "MDS", "Stars", "Maintenance", "GEL", "Specialist", "Consultants"];

        var alphaMonth = monthArray[numericMonth];

        return alphaMonth;
    }

    $("#placeholder").bind("plothover", function (event, pos, item) {
        if (item) {
            if (previousPoint != item.datapoint) {
                previousPoint = item.datapoint;
                $("#flot-tooltip").remove();

                var originalPoint;

                if (item.datapoint[0] == item.series.data[0][8]) {
                    originalPoint = item.series.data[0][0];
                } else if (item.datapoint[0] == item.series.data[1][8]) {
                    originalPoint = item.series.data[1][0];
                } else if (item.datapoint[0] == item.series.data[2][8]) {
                    originalPoint = item.series.data[2][0];
                } else if (item.datapoint[0] == item.series.data[3][8]) {
                    originalPoint = item.series.data[3][0];
                }
                else if (item.datapoint[0] == item.series.data[4][8]) {
                    originalPoint = item.series.data[4][0];
                }
                else if (item.datapoint[0] == item.series.data[5][8]) {
                    originalPoint = item.series.data[5][0];
                }
                else if (item.datapoint[0] == item.series.data[6][8]) {
                    originalPoint = item.series.data[6][0];
                }
                else if (item.datapoint[0] == item.series.data[7][8]) {
                    originalPoint = item.series.data[7][0];
                }
                else if (item.datapoint[0] == item.series.data[8][8]) {
                    originalPoint = item.series.data[8][0];
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