var UserTracker = {
    Email: null,
    ConfigXml: null,
    PageContents: null,
    CurrentLocation: null,
    QuizResponses: null,
    SurveyResponses: null,
    ActivityTracking: []
};

function initTracking() {
    UserTracker.ConfigXml = ConfigXml;
    UserTracker.PageContents = PageContent;
    UserTracker.CurrentLocation = CurrentLocation;
    
}

function UpdateCurrentLocation(loc) {
    UserTracker.CurrentLocation = loc;
}

function addUserNavigation(from, to, how) {
    var f = "";
    if (typeof from === "undefined") {
        f = "start";
        how = "first load";
    } else {
        f = from.Module + ":" + from.Section + ":" + from.Chapter + ":" + from.Page;
    }

    var t = to.Module + ":" + to.Section + ":" + to.Chapter + ":" + to.Page;
    var dt;

    var navOb = {
        to: t,
        from: f,
        description: how,
        time: new Date()
    };
    UserTracker.ActivityTracking.push(navOb)
}