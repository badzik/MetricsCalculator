$(document).ready(function ($) {

    var showLoadingGif = function showLoadingGif(event) {
        $('#contentArea').hide();
        $('.metricsLoadingImg').fadeIn();
        $('.metricsLoadingImg').css('display', 'block');
    };

    $(".calculateMetrics").click(function (event) {
        showLoadingGif();
    });

});