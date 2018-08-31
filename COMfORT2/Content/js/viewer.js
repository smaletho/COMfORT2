var pageXml;
var resizeTimer;

function viewerInit() {
    $("#shell-assets").on('click', function () {
        // get all assets with PageId == 0
        window.open(URL_ViewAssets);
    });

    // keep aspect ratio
    maintainAspectRatio();

    $(window).on('resize', function (e) {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            maintainAspectRatio();
        }, 1000);
    });

    pageReceived_Render();

    $("#submit-page").on('click', function () {
        if (confirm("Would you like to save this page?")) {

            var email = prompt("Please enter your email address for saving.");
            if (email !== null) {
                //.item src
                var arr = $(".item").map(function () { return $(this).prop('src'); }).get();

                var s = new XMLSerializer();
                var str = s.serializeToString(xml);

                transmitAction(URL_SavePage, savePageSuccess, savePageFail, "", { xml: str, images: arr, email: email });
            }
        }
    });
}

function savePageSuccess(data) {
    alert("Page saved.");
    window.location = URL_GoHome;
}

function savePageFail(data) {
    alert("Fail :( Check the console");
    console.log('fail', data);
}


function pageReceived_Render() {
    $("#page-content").empty();
    appendStandardContent();

    // targetPage.content should be just the file contents from above
    $(xml).contents().contents().each(function recursivePageLoad() {

        if (!blankTextNode(this)) {
            //var innerPage = $(this).html().trim();
            var innerPage = $.trim(this.textContent);

            if ((innerPage.indexOf('<text>') == -1) && (innerPage.indexOf('</text>') == -1)) {
                // this is a normal, empty node

                renderElement(this);
            } else {
                alert("I found nodes inside of nodes, and I haven't accounted for that...");
                //processTextElement(this);
                //$(this).contents().each(recursivePageLoad);
            }
        }
    });
    renderInit();
    //$("#loading").hide();
}