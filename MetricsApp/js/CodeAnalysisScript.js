$(document).ready(function ($) {
    Chart.defaults.global.defaultFontFamily = '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
    Chart.defaults.global.defaultFontColor = '#292b2c';

    var complexity = $('#complexity').val();
    var duplicatedLines = $('#duplicatedLines').val();
    var majorIssues = $('#majorIssues').val();
    var testUncoverage = $('#testUncoverage').val();
    var codeQuality = $('#codeQuality').val();
    adjustCodeQualityColour(codeQuality);
    var severitiesCounter = $('#issuesTypesCount').val();
    var severityNames = [];
    var issuesCounter = [];

    for (var i = 0; i < severitiesCounter; i++) {
        severityNames.push($('#severityName_' + i).val());
        issuesCounter.push(parseInt($('#issuesCounter_' + i).val()));
    }
    var coloursForIssues = prepareColours(severityNames);

    var ctx = document.getElementById("issuesPieChart");
    var issuesPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: severityNames,
            datasets: [{
                data: issuesCounter,
                backgroundColor: coloursForIssues,
            }],
        },
    });

    var ctx = document.getElementById("qualityFactorsPieChart");
    var qualityFactorsPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ["Code Quality","Complexity", "Duplicated Lines","Major issues","Test Uncoverage"],
            datasets: [{
                data: [codeQuality, complexity, duplicatedLines, majorIssues, testUncoverage],
                backgroundColor: ['#25d133', '#b2a2a3', '#485970', '#a03d43','#5c427c'],
            }],
        },
    });

    function prepareColours(severities) {
        var colours = [];
        for (var i= 0; i < severities.length;i++){
            switch (severities[i]) {
                case "BLOCKER":
                    colours.push("#000000");
                    break;
                case "CRITICAL":
                    colours.push("#f72525");
                    break;
                case "MAJOR":
                    colours.push("#895244");
                    break;
                case "MINOR":
                    colours.push("#4c967a");
                    break;
                case "INFO":
                    colours.push("#2acbe0");
                    break;
            }
        }
        return colours;
    }

    function adjustCodeQualityColour(cqVal) {

        if (cqVal >= 0 && cqVal <= 30) {
            $('#codeQualityBox').css('color', 'red');
        }
        if (cqVal > 30 && cqVal <= 70) {
            $('#codeQualityBox').css('color', '#e0ab0d');
        }
        if (cqVal > 70) {
            $('#codeQualityBox').css('color', 'green');
        }
    }
});