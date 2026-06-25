window.initMapPredictions = function () {
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

    var geplaatsteMarker = null;

    map.on('click', function (e) {
        var lat = e.latlng.lat.toFixed(6);
        var lng = e.latlng.lng.toFixed(6);

        if (geplaatsteMarker) map.removeLayer(geplaatsteMarker);

        geplaatsteMarker = L.marker([lat, lng], {
            title: "Geselecteerde locatie"
        }).addTo(map)
            .bindPopup(
                '<div style="font-weight:bold;color:#2d6a4f;">Geselecteerde locatie</div>' +
                'Lat: ' + lat + '<br>' +
                'Lng: ' + lng
            )
            .openPopup();

        DotNet.invokeMethodAsync('avans-1.4-system-integration-ui', 'OnMarkerGeplaatst', parseFloat(lat), parseFloat(lng));
    });
};


