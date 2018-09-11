pageInit();

var navigateToUrl = '';

function pageInit() {
    $('menu-item').mousedown(function () {
        $(this).addClass('menu-item-select');
        navigateToUrl = $(this).data('url');
        setTimeout(
            function () {
                window.location.href = navigateToUrl;
            }, 250);
    });
}