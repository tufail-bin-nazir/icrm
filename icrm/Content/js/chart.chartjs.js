$(function () {
	'use strict';

	var ctx1 = document.getElementById('chartBar1').getContext('2d');
	var myChart1 = new Chart(ctx1, {
		type: 'bar',
		data: {
			labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
			datasets: [{
				label: '# of Votes',
				data: [12, 39, 20, 10, 25, 18],
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
			labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
			datasets: [{
				label: '# of Votes',
				data: [12, 39, 20, 10, 25, 18],
				backgroundColor: [
          '#29B0D0',
          '#2A516E',
          '#F07124',
          '#CBE0E3',
          '#979193'
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