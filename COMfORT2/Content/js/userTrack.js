var UserTracker = {
    Email: null,
    CurrentLocation: null,
    QuizResponses: null,
    SurveyResponses: null,
    StartTime: null,
    ActivityTracking: [],
    VisitedPages: []
};

function initTracking() {
    var existingData = window.localStorage.getItem("tsmale@rktcreative.com");

    if (existingData == null) {
        // new person, initialize everything
        
        UserTracker.CurrentLocation = CurrentLocation;
        UserTracker.Email = "tsmale@rktcreative.com";
        UserTracker.StartTime = new Date();

    } else {
        UserTracker = JSON.parse(existingData);
    }
}

function UpdateCurrentLocation(loc) {
    UserTracker.CurrentLocation = loc;
    saveTracker();
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
    UserTracker.ActivityTracking.push(navOb);

    if (UserTracker.VisitedPages.indexOf(to.Page) == -1) 
        UserTracker.VisitedPages.push(to.Page);
    

    saveTracker();
}

function saveTracker() {
    window.localStorage.setItem(UserTracker.Email, JSON.stringify(UserTracker));
}