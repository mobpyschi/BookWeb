(function ($) {
    "use strict";

    $(document).ready(function () {
        $('.btn-open').click(function (event) {
            $('.box-note').addClass('box-open');
            $('div.background-blurry').addClass('box-open');
        })

        
    });


})(jQuery);