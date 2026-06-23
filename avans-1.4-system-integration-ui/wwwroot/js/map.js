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
        'Plastic': 'red', 'Paper/Cardboard': 'blue', 'Glass': 'goldenrod',
        'Cans': 'green', 'Electronics': 'purple', 'Bulky': 'orange', 'Residual': 'hotpink'
    };
    var scorePerType = {
        'Plastic': 7, 'Paper/Cardboard': 4, 'Glass': 9,
        'Cans': 6, 'Electronics': 10, 'Bulky': 8, 'Residual': 5
    };

    window._alleData = [];
    var cirkels = [];
    var clusterGroep = null;

    window.toonCirkels = function (periode) {
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
            var datum = new Date(item.DateTime);
            if (datum >= vanafDatum) {
                var kleur = kleurPerType[item.WasteType] || 'gray';
                var opacity = parseFloat(item.WasteAmount) / 20;
                var prioriteitScore = scorePerType[item.WasteType] * parseFloat(item.WasteAmount);

                var cirkel = L.circle(
                    [parseFloat(item.Latitude), parseFloat(item.Longitude)],
                    {
                        color: kleur, fillColor: kleur,
                        fillOpacity: opacity, opacity: opacity, radius: 5
                    }
                ).addTo(map).bindPopup(
                    '<b>' + item.WasteType + '</b><br>' +
                    'Aantal: ' + item.WasteAmount + '<br>' +
                    'Datum: ' + item.DateTime + '<br>' +
                    'Prioriteitsscore: ' + prioriteitScore
                );
                cirkels.push(cirkel);

                if (prioriteitScore > 120) {
                    var waarschuwing = L.divIcon({
                        html: '<div style="background:red;color:white;border-radius:50%;width:20px;height:20px;display:flex;align-items:center;justify-content:center;font-weight:bold;">!</div>',
                        className: '', iconSize: [20, 20]
                    });
                    clusterGroep.addLayer(L.marker([parseFloat(item.Latitude), parseFloat(item.Longitude)], { icon: waarschuwing }));
                }
            }
        });

        map.addLayer(clusterGroep);
    };

    fetch('/js/litter_breda.csv')
        .then(r => r.text())
        .then(function (csvText) {
            var regels = csvText.trim().split('\n');
            var headers = regels[0].split(',').map(h => h.trim());
            for (var i = 1; i < regels.length; i++) {
                var kolommen = regels[i].split(',');
                var item = {};
                headers.forEach(function (h, index) {
                    item[h] = kolommen[index]?.trim();
                });
                window._alleData.push(item);
            }
            window.toonCirkels('alles');
        });
};

window.toonCirkelsIfReady = function (periode) {
    if (typeof window.toonCirkels === 'function') {
        window.toonCirkels(periode);
    }
};
