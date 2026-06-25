// ---- Shared state ----
window._leafletMap = null;
window._alleData = [];

var _cirkels = [];
var _clusterGroep = null;

var kleurPerType = {
    'Plastic': 'red', 'Papier/karton': 'blue', 'Glas': 'goldenrod',
    'Blik': 'green', 'Grofvuil': 'orange', 'Restafval': 'hotpink'
};
var scorePerType = {
    'Plastic': 7, 'Papier/karton': 4, 'Glas': 9,
    'Blik': 6, 'Grofvuil': 8, 'Restafval': 5
};

// ---- Map setup ----
window.initMap = function () {
    var container = document.getElementById('map');
    if (!container) return;

    if (window._leafletMap) {
        window._leafletMap.remove();
        window._leafletMap = null;
    }
    container._leaflet_id = null;

    var map = L.map('map').setView([51.5895, 4.7765], 15);
    window._leafletMap = map;

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);
};

// ---- Data entry point ----
window.loadTrashData = function (data) {
    if (!window._leafletMap) window.initMap();
    window._alleData = data || [];
    window.toonCirkels('alles');
};

// ---- WasteAmount per locatie ----
function buildLocatieTellingen(data) {
    var tellingen = {};
    data.forEach(function (item) {
        var sleutel =
            parseFloat(item.latitude).toFixed(4) + "_" +
            parseFloat(item.longitude).toFixed(4);
        tellingen[sleutel] = (tellingen[sleutel] || 0) + 1;
    });
    return tellingen;
}

// ---- Render cirkels ----
window.toonCirkels = function (periode) {
    var map = window._leafletMap;
    if (!map) return;

    var locatieTellingen = buildLocatieTellingen(window._alleData);

    _cirkels.forEach(function (c) { map.removeLayer(c); });
    _cirkels = [];

    if (_clusterGroep) map.removeLayer(_clusterGroep);
    _clusterGroep = L.markerClusterGroup({
        iconCreateFunction: function (cluster) {
            return L.divIcon({
                html: '<div style="background:red;color:white;border-radius:50%;width:36px;height:36px;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:14px;">!</div>',
                className: '', iconSize: [36, 36]
            });
        }
    });

    var nu = new Date();
    var vanafDatum = new Date();
    if (periode === '1dag') vanafDatum.setDate(nu.getDate() - 1);
    if (periode === '1week') vanafDatum.setDate(nu.getDate() - 7);
    if (periode === '1maand') vanafDatum.setMonth(nu.getMonth() - 1);
    if (periode === '3maanden') vanafDatum.setMonth(nu.getMonth() - 3);
    if (periode === '6maanden') vanafDatum.setMonth(nu.getMonth() - 6);
    if (periode === '1jaar') vanafDatum.setFullYear(nu.getFullYear() - 1);
    if (periode === 'alles') vanafDatum = new Date(0);

    window._alleData.forEach(function (item) {
        var datum = new Date(item.dateTime);
        if (datum < vanafDatum) return;

        var kleur = kleurPerType[item.trashType] || 'gray';

        var sleutel =
            parseFloat(item.latitude).toFixed(4) + "_" +
            parseFloat(item.longitude).toFixed(4);

        var wasteAmount = locatieTellingen[sleutel] || 1;
        var opacity = Math.min(wasteAmount / 10, 1);
        var prioriteitScore = (scorePerType[item.trashType] || 1) * wasteAmount;

        var cirkel = L.circle(
            [parseFloat(item.latitude), parseFloat(item.longitude)],
            {
                color: kleur, fillColor: kleur,
                fillOpacity: opacity, opacity: opacity, radius: 5
            }
        ).addTo(map).bindPopup(
            '<b>' + item.trashType + '</b><br>' +
            'Aantal op locatie: ' + wasteAmount + '<br>' +
            'Datum: ' + formatDatum(item.dateTime) + '<br>' +
            'Prioriteitsscore: ' + prioriteitScore
        );

        _cirkels.push(cirkel);

        if (prioriteitScore > 40) {
            var waarschuwing = L.divIcon({
                html: '<div style="background:red;color:white;border-radius:50%;width:20px;height:20px;display:flex;align-items:center;justify-content:center;font-weight:bold;">!</div>',
                className: '', iconSize: [20, 20]
            });
            _clusterGroep.addLayer(L.marker(
                [parseFloat(item.latitude), parseFloat(item.longitude)],
                { icon: waarschuwing }
            ));
        }
    });

    map.addLayer(_clusterGroep);
};

window.toonCirkelsIfReady = function (periode) {
    if (typeof window.toonCirkels === 'function') {
        window.toonCirkels(periode);
    }
};

function formatDatum(datum) { // "25-06-2026 14:30"
    return new Date(datum).toLocaleString('nl-NL', {
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
}