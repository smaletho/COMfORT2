
var resizeTimer;


$(function () {
    firstLoadInit();
    loadingInit();
    validateInit();


    // If location.hostname == "", it means that the app is running as a "file" not on a server
    //  therefore, we're offline, so do that load instead
    if (location.hostname == "") {
        // when this function finishes, it will call the remaining init
        offlineLoadInit();
    } else {
        //load the book
        $("#loading").show();

        updateLoadingText('Loading file');
        //loading
        transmitAction(URL_GetBook, gotBookInfo, fail_BookInfo, "", null);
    }
});

function gotBookInfo(data) {

    updateLoadingText('Creating presentation');

    ConfigXml = $.parseXML(data.ConfigXml);

    for (var i = 0; i < data.PageContent.length; i++) {
        var tempXml = data.PageContent[i].content;
        data.PageContent[i].content = $($.parseXML(tempXml)).contents();
    }

    PageContent = data.PageContent;
    secondInit();
}

function fail_BookInfo(data) {
    console.log('fail', data);
}

function secondInit() {
    updateLoadingText("Building content");
    buildTableOfContents();
    initTocLinks();

    updateLoadingText('Done!');
    setTimeout(function () {
        updateLoadingText('');
        $("#loading").hide();
        $("#offline-load").css('display', 'none');
        $("#main-window").css('display', 'table');

        // find the first page, and load it
    //  eventually: check if they left off somewhere
        loadPage();
    }, 1000);

    
    

}

function firstLoadInit() {
    console.log('base.js loaded');


    maintainAspectRatio();

    $(window).on('resize', function (e) {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            maintainAspectRatio();
        }, 1000);
    });


    $("#menu-close").on('click', closeNav);
    $("#menu-open").on('click', openNav);
    initNavigation();
}


function initTocLinks() {
    $(".nav-link").on('click', function () {
        // navigate to the page/section/chapter/etc
        var id = $(this).data('id');
        var type = "";

        if ($(this).hasClass("page")) type = "page";
        else if ($(this).hasClass("chapter")) type = "chapter";
        else if ($(this).hasClass("section")) type = "section";
        else if ($(this).hasClass("module")) type = "module";

        loadPage(id, type);

    });



    $(".plusMinus").on('click', function () {
        var id = $(this).data('id');

        // look for the plusMinus
        if ($(this).hasClass("activePlusMinus")) {
            // collapsing
            $(this).text("+");
            $(this).removeClass("activePlusMinus");

            // find class .sub-links with same data-id
            $("#pageList").find('.sub-links[data-id=' + id + ']').first().css('display', 'none');
        } else {
            // expanding
            $(this).text("-");
            $(this).addClass("activePlusMinus");

            // find class .sub-links with same data-id
            $("#pageList").find('.sub-links[data-id=' + id + ']').first().css('display', 'block');
        }


        
        return false;
    });
}







function buildTableOfContents() {

    // make ConfigXml into an easier to deal with object to generate the 

    console.log('all modules', $(ConfigXml).find('chapter').prop('id'))

    // traverse the tree and build the TOC
    var tocHtmlString = "";

    $(ConfigXml).contents().each(function processNodes() {
        // if it's the main book, render different
        if (this.nodeName.toLowerCase() === "book") {
            // fill in the title
            $("#toc-book-title").text(this.attributes.name.value);

            // recurse
            $(this).contents().each(processNodes);
        } else if (!blankTextNode(this) && this.nodeName.toLowerCase() !== "page") {
            var id = $(this).prop('id');
            var type = this.nodeName.toLowerCase();
            

            var name = this.attributes.name.value;


            tocHtmlString += "<div data-id='" + id + "' class='nav-link " + type + "'>";
            tocHtmlString += "<div class='text'>" + name + "</div>";


            var children = $(this).children();
            

            if (children.length > 0) {
                tocHtmlString += "<div class='plusMinus' data-id='" + id + "'>+</div>";
            }

            tocHtmlString += "</div>";

            
            

            if (this.nodeName.toLowerCase() === "chapter") {
                // if it's a chapter, update page numbers
                tocHtmlString += "<div class='sub-links pages' data-id='" + id + "'>";

                for (var i = 0; i < children.length; i++) {
                    tocHtmlString += "<div class='nav-link page' data-id='" + children[i].attributes.id.value +"'>" + (i + 1) + "</div>";
                }
            } else {
                tocHtmlString += "<div class='sub-links' data-id='" + id + "'>";
            }

            // recurse
            $(this).contents().each(processNodes);

            // close out the div
            tocHtmlString += "</div>";
        }
    });

    $("#pageList").html(tocHtmlString);

    // subtracting 2 accounts for the border
    $("#toc").height($("#inner-window").height() - 2);
}




function openNav() {
    if ($("#toc").width() === 0) {
        $("#toc").css('width', '33%');
        $("#menu-open").css('margin-left', '33%');
    } else {
        closeNav();
    }
}

function closeNav() {
    $("#toc").width(0);
    $("#menu-open").css('margin-left', '-1px');
}