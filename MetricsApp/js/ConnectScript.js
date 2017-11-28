$(document).ready(function ($) {

    var showLoadingGif = function showLoadingGif(event) {
        $('#ConnectForm').hide();
        $('.loadingImg').fadeIn();
        $('.loadingImg').addClass("displayed");
    };

    $("#ConnectButton").click(function (event) {
        showLoadingGif();
    });

});