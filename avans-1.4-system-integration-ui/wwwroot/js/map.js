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

    var kleurPerType = {
        'Plastic': 'red', 'Papier/karton': 'blue', 'Glas': 'goldenrod',
        'Blik': 'green', 'Grofvuil': 'orange', 'Restafval': 'hotpink'
    };
    var scorePerType = {
        'Plastic': 7, 'Papier/karton': 4, 'Glas': 9,
        'Blik': 6, 'Grofvuil': 8, 'Restafval': 5
    };

    window._alleData = [];

    window.loadTrashData = function (data) {
        window._alleData = data;
        window.toonCirkels('alles');
    };

    var cirkels = [];
    var clusterGroep = null;

    //code voor WasteAmount met de data van Sensoring
    function buildLocatieTellingen(data) {
        var tellingen = {};

        data.forEach(function (item) {

            var sleutel =
                parseFloat(item.Latitude).toFixed(4) + "_" +
                parseFloat(item.Longitude).toFixed(4);

            tellingen[sleutel] = (tellingen[sleutel] || 0) + 1;
        });

        return tellingen;
    }

    window.toonCirkels = function (periode) {
        var locatieTellingen = buildLocatieTellingen(window._alleData);
        cirkels.forEach(c => map.removeLayer(c));
        cirkels = [];

        if (clusterGroep) map.removeLayer(clusterGroep);
        clusterGroep = L.markerClusterGroup({
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
            //Deze gebruiken als de data van sensoring niet werkt!!!
            // var datum = new Date(item.DateTime);
            var datum = new Date(item.dateTime);

            if (datum >= vanafDatum) {
                //Deze gebruiken als de data van sensoring niet werkt!!!
                // var kleur = kleurPerType[item.WasteType] || 'gray';
                var kleur = kleurPerType[item.trashType] || 'gray';

                //Deze gebruiken als de data van sensoring niet werkt!!!
                // var sleutel =
                //     parseFloat(item.Latitude).toFixed(4) + "_" +
                //     parseFloat(item.Longitude).toFixed(4);
                var sleutel =
                    parseFloat(item.latitude).toFixed(4) + "_" +
                    parseFloat(item.longitude).toFixed(4);

                var wasteAmount = locatieTellingen[sleutel] || 1;

                //Deze gebruiken als de data van sensoring niet werkt!!!
                // var opacity = parseFloat(item.WasteAmount) / 20;
                var opacity = Math.min(wasteAmount / 10, 1);

                //Deze gebruiken als de data van sensoring niet werkt!!!
                // var prioriteitScore = scorePerType[item.WasteType] * parseFloat(item.WasteAmount);
                var prioriteitScore = (scorePerType[item.trashType] || 1) * wasteAmount;

                var cirkel = L.circle(
                    //Deze gebruiken als de data van sensoring niet werkt!!!
                    // [parseFloat(item.Latitude), parseFloat(item.Longitude)],
                    [parseFloat(item.latitude), parseFloat(item.longitude)],
                    {
                        color: kleur, fillColor: kleur,
                        fillOpacity: opacity, opacity: opacity, radius: 5
                    }
                    //Deze gebruiken als de data van sensoring niet werkt!!!
                    // ).addTo(map).bindPopup(
                    //     '<b>' + item.WasteType + '</b><br>' +
                    //     'Aantal: ' + item.WasteAmount + '<br>' +
                    //     'Datum: ' + item.DateTime + '<br>' +
                    //     'Prioriteitsscore: ' + prioriteitScore
                    //     );
                ).addTo(map).bindPopup(
                    //Deze gebruiken als de data van sensoring niet werkt!!!
                    // '<b>' + item.WasteType + '</b><br>' +
                    // 'Datum: ' + item.DateTime + '<br>' +
                    '<b>' + item.trashType + '</b><br>' +
                    'Aantal op locatie: ' + wasteAmount + '<br>' +
                    'Datum: ' + item.dateTime + '<br>' +
                    'Prioriteitsscore: ' + prioriteitScore
                );
                );

                cirkels.push(cirkel);

                if (prioriteitScore > 120) {
                    var waarschuwing = L.divIcon({
                        html: '<div style="background:red;color:white;border-radius:50%;width:20px;height:20px;display:flex;align-items:center;justify-content:center;font-weight:bold;">!</div>',
                        className: '', iconSize: [20, 20]
                    });
                    clusterGroep.addLayer(L.marker(
                        //Deze gebruiken als de data van sensoring niet werkt!!!
                        // [parseFloat(item.Latitude), parseFloat(item.Longitude)],
                        [parseFloat(item.latitude), parseFloat(item.longitude)],
                        { icon: waarschuwing }
                    ));
                }
            }
        });
        map.addLayer(clusterGroep);
    };

    window.toonCirkelsIfReady = function (periode) {
        if (typeof window.toonCirkels === 'function') {
            window.toonCirkels(periode);
        }
    };
};