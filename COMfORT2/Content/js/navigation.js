﻿function initNavigation() {
    unbindNavigation();

    $("#next-button").on('click', nextPage);
    $("#previous-button").on('click', previousPage);

    $(".chapter-item").on('click', function () {
        var id = $(this).data('id');
        loadPage(id, "chapter");
    });
    $(".section-item").on('click', function () {
        var id = $(this).data('id');
        loadPage(id, "section");
    });
    $(".dot").on('click', function () {
        var id = $(this).data('page');
        loadPage(id, "page");
    });
}

function unbindNavigation() {
    $("#next-button").unbind();
    $("#previous-button").unbind();
    $(".chapter-item").unbind();
    $(".section-item").unbind();
    $(".dot").unbind();
}


function nextPage() {
    // CurrentLocation
    var allPages = $(ConfigXml).find('page');

    for (var i = 0; i < allPages.length; i++) {
        if ($(allPages[i]).prop('id') === CurrentLocation.Page) {
            if (i + 1 >= allPages.length) {
                alert("you're off the edge");
            } else {
                loadPage($(allPages[i + 1]).prop('id'), "page");
            }
            break;
        }
    }
}
function previousPage() {
    var allPages = $(ConfigXml).find('page');

    for (var i = 0; i < allPages.length; i++) {
        if ($(allPages[i]).prop('id') === CurrentLocation.Page) {
            if (i - 1 < 0) {
                alert("you're off the edge");
            } else {
                loadPage($(allPages[i - 1]).prop('id'), "page");
            }
            break;
        }
    }
}
function loadPage(id, type) {
    if (typeof id === "undefined") {
        // load the first page of the module
        var newId = $(ConfigXml).find('page').first().prop('id');
        loadPage(newId, "page");
    } else {

        var targetPage;

        switch (type) {
            case "page":
                // id is already a page
                break;
            case "chapter":
                id = $(ConfigXml).find("#" + id).children("page").first().prop('id');
                
                break;
            case "section":
                id = $(ConfigXml).find("#" + id).children("chapter").first().children("page").first().prop('id');
                
                break;
            case "module":
                id = $(ConfigXml).find("#" + id).children("section").first().children("chapter").first().children("page").first().prop('id');
                
                break;
        }

        $(PageContent).each(function () {
            if (this.Page === id) {
                targetPage = this;
                return false;
            }
        });
        
        
        
        // if page has content
        $("#page-content").empty();
        appendStandardContent();

        var previousLocation = CurrentLocation;
        CurrentLocation = targetPage;
        addUserNavigation(previousLocation, CurrentLocation, "");

        closeNav();
        updateNavigation(previousLocation);

        $(targetPage.content).contents().each(function recursivePageLoad() {

            if (!blankTextNode(this)) {
                var innerPage = $(this).html().trim();

                if (innerPage.indexOf('<text>') === -1 && innerPage.indexOf('</text>') === -1) {
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
    }
}

function appendStandardContent() {
    $("#page-content").append("<div id='definitionWindow'></div>");
}

function updateNavigation(previousLocation) {

    var parentChapter = $(ConfigXml).find("#" + CurrentLocation.Page).parent("chapter");
    var parentModule = $(ConfigXml).find("#" + CurrentLocation.Page).parent("module");
    var parentSection = $(ConfigXml).find("#" + CurrentLocation.Page).parent("section");

    // check if they're populated
    populateMenus();

    if (typeof previousLocation !== "undefined") {

        if (previousLocation.Module !== CurrentLocation.Module) {
            // changing modules
            $("#module-name").html('');
            
            $("#section-list").empty();

            $("#dot-container").empty();
        }
        if (previousLocation.Section !== CurrentLocation.Section) {
            // changing sections
            $("#chapter-list").empty();

            // get all chapters for this section
            $(".section-item").removeClass("selected");
        } if (previousLocation.Chapter !== CurrentLocation.Chapter) {
            // changing chapters
            
            $(".chapter-item").removeClass("selected");
        }

        populateMenus();
        $(".section-item[data-id='" + CurrentLocation.Section + "']").addClass("selected");
        $(".chapter-item[data-id='" + CurrentLocation.Chapter + "']").addClass("selected");

        // move the page arrow
        var mod = $(ConfigXml).find("#" + CurrentLocation.Module).first();
        var marginCount = 0;
        $(mod).find("page").each(function (k, v) {
            if ($(this).prop('id') === CurrentLocation.Page) {
                $("#arrow-pointer").css('margin-left', marginCount + "px");
                return false;
            }
            marginCount += 30;
        });
        
    } else {
        // use the first page

        // set default section selected
        $("#section-list").find(".section-item[data-id='" + CurrentLocation.Section + "']").first().addClass("selected");
        $("#chapter-list").find(".chapter-item[data-id='" + CurrentLocation.Chapter + "']").first().addClass("selected");
    }

    updateTableOfContents();
    initNavigation();
    selectPageDots();
    setProgressBar();
}

function setProgressBar() {
    // progress bar width: 
}

function selectPageDots() {
    // set the current page to active
    $(".dot").removeClass("selected");

    $(".dot[data-page='" + CurrentLocation.Page + "']").addClass("selected");
    // set "viewed" pages as darkened
}

function updateTableOfContents() {
    // Update the TOC highlighting and stuff
    $(".sub-links").hide();

    $(".sub-links[data-id=" + CurrentLocation.Module + "]").show();
    $(".sub-links[data-id=" + CurrentLocation.Section + "]").show();
    $(".sub-links[data-id=" + CurrentLocation.Chapter + "]").show();

    $(".page").removeClass('activePage');
    $(".page[data-id='" + CurrentLocation.Page + "']").addClass('activePage');

    // update the plusMinus
    $(".plusMinus").html('+');
    $(".plusMinus").removeClass('activePlusMinus');
    $(".plusMinus[data-id='" + CurrentLocation.Chapter + "']").html("-");
    $(".plusMinus[data-id='" + CurrentLocation.Section + "']").html("-");
    $(".plusMinus[data-id='" + CurrentLocation.Module + "']").html("-");
    $(".plusMinus[data-id='" + CurrentLocation.Chapter + "']").addClass("activePlusMinus");
    $(".plusMinus[data-id='" + CurrentLocation.Section + "']").addClass("activePlusMinus");
    $(".plusMinus[data-id='" + CurrentLocation.Module + "']").addClass("activePlusMinus");
    
}

function populateMenus() {
    var mod = $(ConfigXml).find("#" + CurrentLocation.Module).first();
    var item;
    var arr;
    var i = 0;

    // if module banner is empty
    if ($("#module-name").html() === "") {
        
        var name = mod[0].attributes.name.value;
        //var color = mod[0].attributes.maincolor.value;
        //var fontColor = mod[0].attributes.fontcolor.value;

        $("#module-name").html(name);
        $("#inner-window").removeClass();
        $("#inner-window").addClass("theme" + mod[0].attributes.theme.value);
        //$("#top-bar").css('background-color', color);
        //$("#module-name").css('color', fontColor);
    }

    // check if there's anything in the box
    if ($('#section-list').is(':empty')) {
        // legit starting from scratch

        // find matching module
        arr = $(mod).find("section").map(function () {
            return {
                id: this.attributes.id.value,
                name: this.attributes.name.value
            };
        }).get();

        for (i = 0; i < arr.length; i++) {
            item = $("<div data-id='" + arr[i].id + "'></div>");
            $(item).addClass('section-item');
            $(item).html(arr[i].name);
            $("#section-list").append(item);
        }
    }

    // check if there's anything in the box
    if ($('#chapter-list').is(':empty')) {
        // legit starting from scratch

        // find matching section
        var sec = $(ConfigXml).find("#" + CurrentLocation.Section).first();
        arr = $(sec).find("chapter").map(function () {
            return {
                id: this.attributes.id.value,
                name: this.attributes.name.value
            };
        }).get();

        for (i = 0; i < arr.length; i++) {
            item = $("<div data-id='" + arr[i].id + "'></div>");
            $(item).addClass('chapter-item');
            $(item).html(arr[i].name);
            $("#chapter-list").append(item);
        }
    }

    // check if the page dots are there
    if ($("#dot-container").is(':empty')) {
        // get all pages in this module

        var currentSection = "";
        $(mod).find("page").each(function (k, v) {
            var dot = $("<div data-page='" + this.attributes.id.value + "' class='dot'></div>");
            $(dot).html(k + 1);
            if ($(this).closest("section").prop("id") !== currentSection) {
                $(dot).addClass('leftborder');
                currentSection = $(this).closest("section").prop('id');
            }
            $("#dot-container").append(dot);
        });

        
        $(mod).find("section").each(function (k, v) {
            var sec = $("<div data-id='" + this.attributes.id.value + "' class='dot-sect'></div>");
            $(sec).html("section " + (k + 1));

            var numChild = $(this).find("page").length;
            var wid = numChild * 30;

            $(sec).width(wid);
            $("#section-dots").append(sec);
        });
    }
}

