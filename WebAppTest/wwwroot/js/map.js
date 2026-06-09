window.initMap = function () {
    var map = L.map('map').setView([51.591, 4.78], 16);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    var circle = L.circle([51.5889, 4.7752], {
        color: 'red',
        fillColor: '#f03',
        fillOpacity: 0.5,
        radius: 50
    }).addTo(map);
};