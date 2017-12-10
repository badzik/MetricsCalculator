$(document).ready(function ($) {

    Chart.defaults.global.defaultFontFamily = '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
    Chart.defaults.global.defaultFontColor = '#292b2c';

    var monthsCounter = $('#monthsCounter').val();
    var monthsLabels=[];
    var issuesPerMonth=[];
    for (var i = 0; i < monthsCounter; i++) {
        monthsLabels.push($('#monthName_' + i).val());
        issuesPerMonth.push(parseInt($('#issuesForMonth_' + i).val()));
    }
    var maxVal = Math.max.apply(null,issuesPerMonth);

    var ctx = document.getElementById("closedIssuesPerMonthBarChart");
    var myLineChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: monthsLabels,
            datasets: [{
                label: "Issues closed",
                backgroundColor: "rgba(2,117,216,1)",
                borderColor: "rgba(2,117,216,1)",
                data: issuesPerMonth,
            }],
        },
        options: {
            scales: {
                xAxes: [{
                    time: {
                        unit: 'month'
                    },
                    gridLines: {
                        display: false
                    },
                    ticks: {
                        maxTicksLimit: 6
                    }
                }],
                yAxes: [{
                    ticks: {
                        min: 0,
                        max: Math.ceil(maxVal / 10) * 10,
                        maxTicksLimit: 5
                    },
                    gridLines: {
                        display: true
                    }
                }],
            },
            legend: {
                display: false
            }
        }
    });


    var ctx = document.getElementById("issuesPieChart");
    var myPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ["Opened", "Closed"],
            datasets: [{
                data: [$('#OpenedIssues').val(), $('#ClosedIssues').val()],
                backgroundColor: ['#007bff', '#dc3545'],
            }],
        },
    });
});