
/********************************************
 *  application.js: Knockoutjs implementacion of weatherApi
 * 
 * */

// Failover Coords (Washington DC, USA)
failOverCoords = {
    coords: {
        latitude: 38.904722,
        longitude: -77.016389
    }
}

// Api Call
currentPosition = {
    baseURL: "./api/v1/",
    unit: "imperial",
    'ok': false
};

// Document.Ready: Handle geolocation permissions, and set Units click function...
$(document).ready(function () {
    handlePermission();
    $(document).on('click', '.btnUnits', function () {
        if ($(this).attr('id') == "btSetImperial") {
            btUnitSystem('imperial');
        } else {
            btUnitSystem('metrical');
        }
        goData();
    });

});

// ViewModel
function myViewModel() {
    var self = this;

    // cookies using
    self.units = ko.observable(getCookie('units'));
    self.colorBox = ko.observableArray(["bg-aqua", "bg-aquaLow"]);

    // units F or C
    if (!self.units) {
        setCookie('units', 'imperial', 30);
        self.units('imperial');
    }

    //btunits change units F or  C
    self.btUnitSystem = function (unit) {
        if (unit != self.units()) {
            self.units(unit);
            currentPosition.unit = unit;
            setCookie('units', unit, 30);
        }
    };

    // data from .net api
    self.theValues = ko.observableArray([]);

    self.goData = function () {
        $.get(currentPosition.baseURL + currentPosition.latitude + "/" + currentPosition.longitude + "/" + (currentPosition.unit == "metrical"), function (returnedData) {
            var data = ko.mapping.fromJS(returnedData);
            theValues(data);
        }).fail(function () {
            alert("Error getting forecast data!");
            theValues({ Name: 'Error Getting forecast data' });
        });
    };
}

// Parse and load current (or failover) Position
function getCoords(position) {
    currentPosition.latitude = position.coords.latitude;
    currentPosition.longitude = position.coords.longitude;
    currentPosition.ok = true;

    if (currentPosition.ok) {
        var view = ko.applyBindings(myViewModel);
        goData();

    } else {
        alert("not Data!");
    }
}

// Handle geolocalization permissions (or return failover location)

function handlePermission() {
    navigator.permissions.query({ name: 'geolocation' }).then(function (result) {
        if (result.state == 'granted') {
            return getCurrLoc();
        } else if (result.state == 'prompt') {
            return getCurrLoc();
        } else if (result.state == 'denied') {
            alert("geolocation is disabled!");
            getCoords(failOverCoords);
            return false;
        }
    });
}

// Get current Location
function getCurrLoc() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(getCoords, error, { enableHighAccuracy: true, timeout: 5000, maximumAge: 0 });
    } else {
        alert("geolocation disabled!");
        getCoords(failOverCoords);
        return;
    }
    function error(error) {
        alert("Error geolocating :" + error);
        getCoords(failOverCoords);
    }
}

// setCookie: Set navigator cookies
function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = escape(value) + "; expires=" + exdate.toUTCString();
    document.cookie = c_name + "=" + c_value;
}


// getCookie: Get navigator cookies
function getCookie(c_name) {
    var c_value = document.cookie;
    var c_start = c_value.indexOf(" " + c_name + "=");
    if (c_start == -1)
        c_start = c_value.indexOf(c_name + "=");
    if (c_start == -1)
        c_value = null;
    else {
        c_start = c_value.indexOf("=", c_start) + 1;
        var c_end = c_value.indexOf(";", c_start);
        if (c_end == -1)
            c_end = c_value.length;
        c_value = unescape(c_value.substring(c_start, c_end));
    }
    return c_value;
}