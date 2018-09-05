
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
    } else if (location.pathname.indexOf("Build") != -1) {
        // using the page viewer.
        viewerInit();
    } else {
        //load the book
        $("#loading").show();

        updateLoadingText('Loading file');
        //loading
        transmitAction(URL_LoadBook, gotBookInfo, fail_BookInfo, "", null);
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
    updateLoadingText("Failed to load :(")
}

function secondInit() {
    updateLoadingText("Building content");
    buildTableOfContents();
    initTocLinks();
    initTracking();

    updateLoadingText('Done!');
    setTimeout(function () {
        updateLoadingText('');
        $("#loading").hide();
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

    //console.log('all modules', $(ConfigXml).find('chapter').prop('id'));

    // traverse the tree and build the TOC
    var tocHtmlString = "";

    $(ConfigXml).contents().contents().each(function processNodes() {
        // if it's the main book, render different
        if (this.nodeName.toLowerCase() === "module") {

            var id = $(this).prop('id');
            var type = this.nodeName.toLowerCase();


            var name = this.attributes.name.value;


            tocHtmlString += "<div data-id='" + id + "' class='nav-link " + type + " theme" + this.attributes.theme.value + "'>";
            tocHtmlString += '<div class="color-block"><div class="dark-box"><img src="' + URL_Content + 'Content/images/left-mask.png" width="20" height="40" /></div><div class="light-box"><img src="' + URL_Content + 'Content/images/right-mask.png" width="20" height="40" /></div></div>';
            tocHtmlString += "<div class='text'>" + name + "</div>";
            tocHtmlString += "</div>";
        } 
    });

    $("#pageList").html(tocHtmlString);

    // subtracting 2 accounts for the border
    $("#toc").height($("#inner-window").height() - 2);
}




function openNav() {
    if ($("#toc").width() === 0) {
        $("#toc").css('width', '20%');
        $("#menu-open").css('margin-left', '20%');
    } else {
        closeNav();
    }
}

function closeNav() {
    $("#toc").width(0);
    $("#menu-open").css('margin-left', '-1px');
}