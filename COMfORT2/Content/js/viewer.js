﻿var pageXml;
var resizeTimer;

function viewerInit() {
    $("#shell-assets").on('click', function () {
        // get all assets with PageId == 0
        window.open(URL_ViewAssets);
    });

    $("#editor").on('click', function () {
        window.location = URL_ViewEditor;
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

                if ($("#PageName").val().trim() === "") {
                    alert("Please enter a page name.");
                    return false;
                } 


                //.item src
                var arr = $(".item").map(function () { return $(this).prop('src'); }).get();

                var s = new XMLSerializer();
                var str = s.serializeToString(xml);
                var pId = $("#PageId").val();
                var name = $("#PageName").val().trim();
                transmitAction(URL_SavePage, savePageSuccess, savePageFail, "", { xml: str, images: arr, email: email, pageId: pId, name: name });
            }
        }
    });
}

function savePageSuccess(data) {
    alert("Page saved.");
    
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

            if ((innerPage.indexOf('<text>') === -1) && (innerPage.indexOf('</text>') === -1)) {
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