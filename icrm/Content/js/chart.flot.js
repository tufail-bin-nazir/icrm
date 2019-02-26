$(function() {
  'use strict';


    var open = $("#opencount").val();
    var closed = $("#closedcount").val();
    var resolved = $("#resolvedcount").val();
    var all = $("#allcount").val();
    
    



   /**************** PIE CHART *******************/
   var piedata = [
       { label: "Open", data: open, color: '#9A3267'},
       { label: "Closed", data: closed, color: '#ED4151'},
       { label: "Resolved", data: resolved, color: '#F89D44'}
      
	 ];

    $.plot('#flotPie1', piedata, {
      series: {
        pie: {
          show: true,
          radius: 1,
          label: {
            show: true,
            radius: 2/3,
            formatter: labelFormatter,
            threshold: 0.1
          }
        }
      },
      grid: {
        hoverable: true,
        clickable: true
        },
        tooltip: true,
        tooltipOpts: {
            content: function (label, x, y) {
                return y + ", " + label;
            }, // show percentages, rounding to 2 decimal places
            shifts: {
                x: 20,
                y: 0
            },
            defaultTheme: false
        }
        
        /*legend: { show: true, labelFormatter: legendFormatter }*/
    });

    $.plot('#flotPie2', piedata, {
      series: {
        pie: {
          show: true,
          radius: 1,
          innerRadius: 0.5,
          label: {
            show: true,
            radius: 2/3,
            formatter: labelFormatter,
            threshold: 0.1
          }
        }
      },
      grid: {
        hoverable: true,
        clickable: true
        },
        tooltip: true,
        tooltipOpts: {
            content: function (label, x, y) {
                return y + ", " + label;
            }, // show percentages, rounding to 2 decimal places
            shifts: {
                x: 20,
                y: 0
            },
            defaultTheme: false
        }
    });

    function labelFormatter(label, series) {
        return "<div style='font-size:8pt; text-align:center; padding:2px; color:white;' alt="+ label + ":" + Math.round(series.data[0][1]) +">" + label + ":" + Math.round(series.data[0][1]) + "</div>";
	  }

});
