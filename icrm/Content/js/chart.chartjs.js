$(function () {
	'use strict';

    var open = $("#opencount").val();
    var closed = $("#closedcount").val();
    var resolved = $("#resolvedcount").val();
    var all = $("#allcount").val();

	var ctx1 = document.getElementById('chartBar1').getContext('2d');
	var myChart1 = new Chart(ctx1, {
		type: 'bar',
		data: {
			labels: ['Open', 'Closed', 'Resolved'],
			datasets: [{
				label: '# of Ticket',
                data: [open, closed, resolved],
				backgroundColor: '#27AAC8'
      }]
		},
		options: {
			legend: {
				display: false,
				labels: {
					display: false
				}
			},
			scales: {
				yAxes: [{
					ticks: {
						beginAtZero: true,
						fontSize: 10,
						max: 80
					}
        }],
				xAxes: [{
					ticks: {
						beginAtZero: true,
						fontSize: 11
					}
        }]
			}
		}
	});

	var ctx2 = document.getElementById('chartBar2').getContext('2d');
	var myChart2 = new Chart(ctx2, {
		type: 'bar',
		data: {
			labels: ['Open', 'Closed', 'Resolved'],
			datasets: [{
                label: '# of Tickets',
                data: [open, closed, resolved],
				backgroundColor: [
          '#29B0D0',
          '#2A516E',
          '#F07124'
        ]
      }]
		},
		options: {
			legend: {
				display: false,
				labels: {
					display: false
				}
			},
			scales: {
				yAxes: [{
					ticks: {
						beginAtZero: true,
						fontSize: 10,
						max: 80
					}
        }],
				xAxes: [{
					ticks: {
						beginAtZero: true,
						fontSize: 11
					}
        }]
			}
		}
	});

	


	});