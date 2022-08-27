(function ($) {
    'use strict';
    $(function () {

        var timeleft = 5;
        var redirectTimer = setInterval(function () {
            if (timeleft <= 1) {
                clearInterval(redirectTimer);
                const redirectUri = $('#lnkPostLogoutUri').attr("href");
                if (redirectUri) {
                    window.location.href = redirectUri;
                }
            }
            $('#spnTimer').text(timeleft);
            timeleft -= 1;
        }, 1000);
    });
})(jQuery);