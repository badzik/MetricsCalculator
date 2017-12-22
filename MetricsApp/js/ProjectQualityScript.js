$(document).ready(function ($) {
    Chart.defaults.global.defaultFontFamily = '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
    Chart.defaults.global.defaultFontColor = '#292b2c';
    var projectQuality = $('#projectQuality').val();
    var codeQualityRatio = Number($('#codeQualityRatio').val().replace(/,/g, '.'));
    var minorIssues = Number($('#minorIssues').val());
    var knownIssues = Number($('#knownIssues').val());

    //Code quality factors
    var complexity = Math.round(Number($('#complexity').val()) * codeQualityRatio);
    var duplicatedLines = Math.round(Number($('#duplicatedLines').val()) * codeQualityRatio);
    var majorIssues = Math.round(Number($('#majorIssues').val()) * codeQualityRatio);
    var testUncoverage = Math.round(Number($('#testUncoverage').val()) * codeQualityRatio);
    var codeQuality = Math.round(Number($('#codeQuality').val()) * codeQualityRatio);

    adjustProjectQualityColour(projectQuality);

    var ctx = document.getElementById("qualityFactorsPieChart");
    var qualityFactorsPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ["Project Quality", "Complexity", "Duplicated Lines", "Test Uncoverage", "Code problems(bugs)","Known issues"],
            datasets: [{
                data: [projectQuality, complexity, duplicatedLines, testUncoverage, minorIssues + majorIssues, knownIssues],
                backgroundColor: ['#25d133', '#b2a2a3', '#485970', '#5c427c', "#7c5542","#893e35"],
            }],
        },
    });


    function adjustProjectQualityColour(pqVal) {

        if (pqVal >= 0 && pqVal <= 30) {
            $('#projectQualityBox').css('color', 'red');
        }
        if (pqVal > 30 && pqVal <= 70) {
            $('#projectQualityBox').css('color', '#e0ab0d');
        }
        if (pqVal > 70) {
            $('#projectQualityBox').css('color', 'green');
        }
    }
});