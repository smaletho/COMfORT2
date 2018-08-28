function loadingInit() {
    console.log('loading.js init');
}


function renderInit() {
    $(".navigateTo").on('click', function () {
        loadPage($(this).data('id'), "page");
        return false;
    });

    $(".definitionPopup").on('click', function () {
        var pos = $(this).offset();


        $("#definitionWindow").css('top', (pos.top - 50) + "px");
        $("#definitionWindow").css('left', pos.left + "px");

        $("#definitionWindow").show();
        $("#definitionWindow").text($(this).data('text'));
        return false;
    });

    $("#page-content").on('click', function () {
        $("#definitionWindow").hide();
        $("#definitionWindow").css('top', '0');
        $("#definitionWindow").css('left', '0');
        $("#definitionWindow").text('');
    });
}

function renderElement(element) {
    var newNode;
    switch (element.nodeName.toLowerCase()) {
        case "text": 
            newNode = textNode(element);
            break;
        case "image":
            newNode = imageNode(element);
            break;
        case "button":
            newNode = buttonNode(element);
            break;
    }


    for (var i = 0; i < element.attributes.length; i++) {
        switch (element.attributes[i].nodeName.toLowerCase()) {
            case "position-x": {
                $(newNode).css('left', element.attributes[i].value + "px");
                break;
            }
            case "position-y": {
                $(newNode).css('top', element.attributes[i].value + "px");
                break;
            }
            case "width": {
                $(newNode).width(element.attributes[i].value);
                break;
            }
            case "height": {
                $(newNode).height(element.attributes[i].value);
                break;
            }
            case "class": {
                var classes = element.attributes[i].value.split(' ');
                for (var ii = 0; ii < classes.length; ii++) {
                    $(newNode).addClass(classes[ii]);
                }
                break;
            }
            case "style": {
                newNode = textStyleMap(newNode, element.attributes[i].value);
                break;
            }
            default:
                var a = element.attributes[i];
                var b = 0;
                break;
        }
    }
    
    $("#page-content").append(newNode);
}

function buttonNode(element) {
    var newNode = $("<button></button>");
    $(newNode).addClass("item");
    $(newNode).html($.trim(element.innerHTML));
    return newNode;
}

function imageNode(element) {
    var newNode = $("<img></img>");
    $(newNode).addClass("item");

    for (var i = 0; i < element.attributes.length; i++) {
        switch (element.attributes[i].nodeName.toLowerCase()) {
            case "source": {
                // "i_{id}"
                // find the associated file
                var src = element.attributes[i].value;

                // TODO: set up URL Content
                var newSrc = URL_Content + "ImageManager.ashx?id=" + src;
                

                $(newNode).prop('src', newSrc);
                break;
            }
            default:
                break;
        }
    }
    return newNode;
}



function textNode(element) {
    var newNode = $("<div></div>");
    $(newNode).addClass("item");
    $(newNode).addClass('inner-text');
    $(newNode).html($.trim(element.innerHTML));
    //$(newNode).html($(element).html().trim());

    return newNode;
}

function textStyleMap(element, styles) {
    var styleArr = styles.split(';');
    for (var i = 0; i < styleArr.length; i++) {
        var block = styleArr[i].split(':');
        $(element).css(block[0], block[1]);
    }
    return element;
}






