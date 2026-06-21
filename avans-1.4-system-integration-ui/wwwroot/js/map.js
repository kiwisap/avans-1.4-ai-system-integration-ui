window.initMap = function () {
    var map = L.map('map').setView([51.5895, 4.7765], 15);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    var kleurPerType = {
        'Plastic': 'red',
        'Papier/Karton': 'blue',
        'Glas': 'goldenrod',
        'Blik': 'green',
        'Elektrische Apparaten': 'purple',
        'Grofvuil': 'orange',
        'Rest': 'hotpink'
    };
    var scorePerType = {
        'Plastic': 7,
        'Papier/Karton': 4,
        'Glas': 9,
        'Blik': 6,
        'Elektrische Apparaten': 10,
        'Grofvuil': 8,
        'Rest': 5
    };
    var alleData = [];
    var cirkels = [];
    var clusterGroep = null; // cluster groep voor waarschuwingen

    function toonCirkels(periode) {
        cirkels.forEach(c => map.removeLayer(c));
        cirkels = [];

        // Verwijder oude cluster groep
        if (clusterGroep) {
            map.removeLayer(clusterGroep);
        }
        clusterGroep = L.markerClusterGroup({
            iconCreateFunction: function (cluster) {
                return L.divIcon({
                    html: '<div style="background:red;color:white;border-radius:50%;width:36px;height:36px;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:14px;">!</div>',
                    className: '',
                    iconSize: [36, 36]
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

        alleData.forEach(function (item) {
            var datum = new Date(item.DatumTijd);
            if (datum >= vanafDatum) {
                var kleur = kleurPerType[item.AfvalType] || 'gray';
                var opacity = parseFloat(item.AantalAfval) / 10;
                var prioriteitScore = scorePerType[item.AfvalType] * parseFloat(item.AantalAfval);

                var cirkel = L.circle(
                    [parseFloat(item.Latitude), parseFloat(item.Longitude)],
                    {
                        color: kleur,
                        fillColor: kleur,
                        fillOpacity: opacity,
                        opacity: opacity,
                        radius: 5
                    }
                ).addTo(map).bindPopup(
                    '<b>' + item.AfvalType + '</b><br>' +
                    'Aantal: ' + item.AantalAfval + '<br>' +
                    'Datum: ' + item.DatumTijd + '<br>' +
                    'Prioriteitsscore: ' + prioriteitScore
                );
                cirkels.push(cirkel);

                // Waarschuwingsmarker toevoegen aan cluster
                if (prioriteitScore > 80) {
                    var marker = L.marker(
                        [parseFloat(item.Latitude), parseFloat(item.Longitude)],
                        {
                            icon: L.divIcon({
                                html: '<div style="background:red;color:white;border-radius:50%;width:20px;height:20px;display:flex;align-items:center;justify-content:center;font-weight:bold;">!</div>',
                                className: '',
                                iconSize: [20, 20]
                            })
                        }
                    );
                    clusterGroep.addLayer(marker);
                }
            }
        });

        map.addLayer(clusterGroep);
    }

    // Dropdown toevoegen aan de kaart
    var dropdown = L.control({ position: 'topright' });
    dropdown.onAdd = function () {
        var div = L.DomUtil.create('div');
        div.innerHTML = `
            <select id="periodeSelect" style="padding: 6px 10px; font-size: 14px; border-radius: 6px; border: 1px solid #ccc; cursor: pointer;">
                <option value="alles">Alle data</option>
                <option value="1dag">Afgelopen dag</option>
                <option value="1week">Afgelopen week</option>
                <option value="1maand">Afgelopen maand</option>
                <option value="3maanden">Afgelopen 3 maanden</option>
                <option value="6maanden">Afgelopen 6 maanden</option>
                <option value="1jaar">Afgelopen jaar</option>
            </select>
        `;
        L.DomEvent.disableClickPropagation(div);
        return div;
    };
    dropdown.addTo(map);

    fetch('/js/zwerfafval_breda.csv')
        .then(r => r.text())
        .then(function (csvText) {
            var regels = csvText.trim().split('\n');
            var headers = regels[0].split(',');

            for (var i = 1; i < regels.length; i++) {
                var kolommen = regels[i].split(',');
                var item = {};
                headers.forEach(function (h, index) {
                    item[h.trim()] = kolommen[index]?.trim();
                });
                alleData.push(item);
            }

            toonCirkels('alles');

            document.getElementById('periodeSelect').addEventListener('change', function () {
                toonCirkels(this.value);
            });
        });
};