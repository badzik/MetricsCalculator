$(document).ready(function ($) {
    makeAjax();

    function makeAjax() {
        return $.ajax({
            url: '/Project/GetBestContributorAsync',
            type: 'POST',
            datatype: "html",
            success: function (html) {
                $('.Results').replaceWith(html);
            },
            error: function () {
                alert("error");
            }
        });
    };
});