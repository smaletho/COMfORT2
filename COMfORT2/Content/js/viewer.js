var pageXml;
var resizeTimer;

function viewerInit() {
    // keep aspect ratio
    maintainAspectRatio();

    $(window).on('resize', function (e) {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            maintainAspectRatio();
        }, 1000);
    });

    pageReceived_Render();
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