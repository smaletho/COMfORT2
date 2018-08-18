function offlineLoadInit() {



    //$("#offline-load").css('display', 'table');
    $("#loading").show();

    updateLoadingText('Loading file');
    ConfigXml = offline_ConfigXml;
    //processOfflineXml();
    processOfflinePageContents();
}


function processOfflinePageContents() {
    // offline_PageContents is a string.
    // first, parse it into JSON
    //var json = JSON.parse(offline_pa)
}

function processOfflineXml() {
    // replace all "page" nodes with empty ones that reference other objects
    PageContent = [];


    $(ConfigXml).contents().each(function processNodes() {
        
        // check for empty node
        if (!blankTextNode(this)) {
            if (this.nodeName.toLowerCase() === "page") {
                // I'm cutting out all the stuff page contents when it can be sent from the server
                var id = $(this).prop('id');

                var parentChapter = $(this).parent("chapter");
                var parentSection = $(parentChapter).parent("section");
                var parentModule = $(parentSection).parent("module");

                PageContent.push({
                    Module: $(parentModule).prop('id'),
                    Section: $(parentSection).prop('id'),
                    Chapter: $(parentChapter).prop('id'),
                    Page: id,
                    content: this.cloneNode(true),
                });

                // make an empty page piece
                var blankPage = document.createElement("page");
                $(blankPage).prop('id', id);
                this.parentNode.replaceChild(blankPage, this);

            }

            // recurse
            $(this).contents().each(processNodes);
        }
    });

    updateLoadingText('Finished processing book');

    setTimeout(function () {
        updateLoadingText('');
        $("#loading").hide();
        $("#offline-load").css('display', 'none');
        $("#main-window").css('display', 'table');

        secondInit();
    }, 1000);

    // process the pages now
    //  to: base.js
    //processPages(PageContent);

    
}