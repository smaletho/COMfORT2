pageInit();

var navigateToUrl = '';

function pageInit() {
    $('menu-item').click(function () {
        $(this).addClass('menu-item-select');
        navigateToUrl = $(this).data('url');
        setTimeout(
            function () {
                window.location.href = navigateToUrl;
            }, 250);
    });
}