pageInit();

var headerButtonAction = '';

function pageInit() {
    $('.button').mousedown(function () {
        $(this).addClass('select');
        headerButtonAction = $(this).data('action');
        setTimeout( performAction(headerButtonAction), 250);
    });
}

function performAction(action) {
    switch (action) {
        case "back":
            goBackOrClose();
            break;
        case "menu":
            window.location.href = "/";
            break;
        case "logout":
            window.location.href = "/";
            break;

    }
}

function goBackOrClose() {
    if (window.history.length <= 1) {
        var objWin = window.self;
        objWin.open('', '_self', '');
        objWin.close();
    } else {
        window.history.back();
    }
}