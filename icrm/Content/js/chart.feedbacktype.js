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

    
    
    */

    var mnt1feedbacksinquiries = $("#mnt1feedbacksinquiries").val();
   
    var mnt1feedbackscompliants = $("#mnt1feedbackscompliants").val();
   
    var mnt1feedbacksappreciations = $("#mnt1feedbacksappreciations").val();
    var mnt1feedbackssuggestions = $("#mnt1feedbackssuggestions").val();


    var mnt2feedbacksinquiries = $("#mnt2feedbacksinquiries").val();
    var mnt2feedbackscompliants = $("#mnt2feedbackscompliants").val();
    var mnt2feedbacksappreciations = $("#mnt2feedbacksappreciations").val();
    var mnt2feedbackssuggestions = $("#mnt2feedbackssuggestions").val();


    var mnt3feedbacksinquiries = $("#mnt3feedbacksinquiries").val();
    var mnt3feedbackscompliants = $("#mnt3feedbackscompliants").val();
    var mnt3feedbacksappreciations = $("#mnt3feedbacksappreciations").val();
    var mnt3feedbackssuggestions = $("#mnt3feedbackssuggestions").val();


    console.log(mnt1feedbacksappreciations + mnt1feedbackscompliants + mnt1feedbacksinquiries + mnt1feedbackssuggestions);
    console.log(mnt2feedbacksappreciations + mnt2feedbackscompliants + mnt2feedbacksinquiries + mnt2feedbackssuggestions);
    console.log(mnt3feedbacksappreciations + mnt3feedbackscompliants + mnt3feedbacksinquiries + mnt3feedbackssuggestions);



    var mnt1feedbacksall = $("#mnt1feedbacksall").val();
    var mnt2feedbacksall = $("#mnt2feedbacksall").val();
    var mnt3feedbacksall = $("#mnt3feedbacksall").val();

    console.log(mnt1feedbacksall + mnt2feedbacksall + mnt3feedbacksall);

    if (!parseFloat(mnt1feedbacksall) == 0) {
        mnt1feedbacksappreciations = ((parseFloat(mnt1feedbacksappreciations) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbackscompliants = ((parseFloat(mnt1feedbackscompliants) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbacksinquiries = ((parseFloat(mnt1feedbacksinquiries) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
        mnt1feedbackssuggestions = ((parseFloat(mnt1feedbackssuggestions) * 100) / parseFloat(mnt1feedbacksall)).toFixed(2);
    }


    if (!parseFloat(mnt2feedbacksall) == 0)
    {
        mnt2feedbacksappreciations = ((parseFloat(mnt2feedbacksappreciations) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbackscompliants = ((parseFloat(mnt2feedbackscompliants) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbacksinquiries = ((parseFloat(mnt2feedbacksinquiries) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
        mnt2feedbackssuggestions = ((parseFloat(mnt2feedbackssuggestions) * 100) / parseFloat(mnt2feedbacksall)).toFixed(2);
    }

    if (!parseFloat(mnt3feedbacksall) == 0) {
        mnt3feedbacksappreciations = ((parseFloat(mnt3feedbacksappreciations) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbackscompliants = ((parseFloat(mnt3feedbackscompliants) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbacksinquiries = ((parseFloat(mnt3feedbacksinquiries) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
        mnt3feedbackssuggestions = ((parseFloat(mnt3feedbackssuggestions) * 100) / parseFloat(mnt3feedbacksall)).toFixed(2);
    }

    console.log(mnt1feedbacksappreciations + mnt1feedbackscompliants + mnt1feedbacksinquiries + mnt1feedbackssuggestions);
    console.log(mnt2feedbacksappreciations + mnt2feedbackscompliants + mnt2feedbacksinquiries + mnt2feedbackssuggestions);
    console.log(mnt3feedbacksappreciations + mnt3feedbackscompliants + mnt3feedbacksinquiries + mnt3feedbackssuggestions);

     



    var month1 = $("#month1").val();
    var month2 = $("#month2").val();
    var month3 = $("#month3").val();

    var colorbgmonth1 = [];
    var colormonth1 = [];

    var colorbgmonth2 = [];
    var colormonth2 = [];

    var colorbgmonth3 = [];
    var colormonth3 = [];

    if (month1 == "Jan")
    {
        colorbgmonth1[0] = " #9b59b6";
        colormonth1[0] = "black";
    }
    else if (month1 == "Feb")
    {
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



    if (!month1 == "") {
        var d1_1 = [
            [1325376000000, mnt1feedbacksinquiries],
            [1328054400000, mnt1feedbackscompliants],
            [1330560000000, mnt1feedbacksappreciations],
            [1333238400000, mnt1feedbackssuggestions]

        ];
    }

    if (!month2 == "") {
        var d1_2 = [
            [1325376000000, mnt2feedbacksinquiries],
            [1328054400000, mnt2feedbackscompliants],
            [1330560000000, mnt2feedbacksappreciations],
            [1333238400000, mnt2feedbackssuggestions]
        ];
    }


    if (!month3 == "") {
        var d1_3 = [
            [1325376000000, mnt3feedbacksinquiries],
            [1328054400000, mnt3feedbackscompliants],
            [1330560000000, mnt3feedbacksappreciations],
            [1333238400000, mnt3feedbackssuggestions]
        ];
    }
   /* var d1_4 = [
        [1325376000000, 15],
        [1328054400000, 10],
        [1330560000000, 15],
        [1333238400000, 20],
        [1335830400000, 15]
    ];*/



    var data1=[];


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

   var p= $.plot($("#placeholder"), data1, {
        xaxis: {
            min: (new Date(2011, 11, 15)).getTime(),
            max: (new Date(2012, 03, 18)).getTime(),
            mode: "time",
            timeformat: "%b",
            tickSize: [1, "month"],
            monthNames: ["Inquiries", "Complaints", "Appreciation", "Suggestions"],
            tickLength: 0, // hide gridlines
            axisLabel: 'Feedback Type',
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
            borderWidth: { top: 0, right: 0, bottom: 1, left: 1 }
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

    function getMonthName(newTimestamp) {
        var d = new Date(newTimestamp);

        var numericMonth = d.getMonth();
        var monthArray = ["Inquiries", "Complaints", "Appreciation", "Suggestions"];

        var alphaMonth = monthArray[numericMonth];

        return alphaMonth;
    }

    $("#placeholder").bind("plothover", function (event, pos, item) {
        if (item) {
            if (previousPoint != item.datapoint) {
                previousPoint = item.datapoint;
                $("#flot-tooltip").remove();

                var originalPoint;

                if (item.datapoint[0] == item.series.data[0][3]) {
                    originalPoint = item.series.data[0][0];
                } else if (item.datapoint[0] == item.series.data[1][3]) {
                    originalPoint = item.series.data[1][0];
                } else if (item.datapoint[0] == item.series.data[2][3]) {
                    originalPoint = item.series.data[2][0];
                } else if (item.datapoint[0] == item.series.data[3][3]) {
                    originalPoint = item.series.data[3][0];
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

        try{
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
        catch(e){}
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
            catch(e){}
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