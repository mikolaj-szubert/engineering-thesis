import React from 'react'
import { Button, Input } from '@nextui-org/react'
import { instance } from '../../Helpers'
import { translations } from '../../lang'
import { toast } from 'react-toastify'
import { MapContainer, TileLayer, Marker, Popup, useMapEvents, useMap, ZoomControl } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import '../../CustomStyles/address.css'

const FitBounds = ({ locations }) => {
    const map = useMap();

    React.useEffect(() => {
        if (locations && locations.length > 0) {
            const bounds = L.latLngBounds(locations.map(({ lat, lon }) => [lat, lon]));
            map.fitBounds(bounds, { padding: [50, 50] });
        }
    }, [locations, map]);

    return null;
};

const FindMe = () => {
    const map = useMap()
    const locate = () => map.locate();
    return <div style={{ zIndex: 401 }} className="cursor-pointer border-2 border-gray-400 rounded bg-white w-[30px] h-[30px] p-1 absolute bottom-[100px] right-3" aria-label="Zoom out" title="Find me"><img onClick={locate} src="../findme.png" alt="Find me" /></div>
}

const LocationMarker = () => {
    const [position, setPosition] = React.useState(null)
    const customIcon = new L.Icon({
        iconUrl: '../marker.png',
        iconSize: [24, 24],
        iconAnchor: [12, 12],
        popupAnchor: [0, -24],
    });
    const map = useMapEvents({
        locationfound(e) {
            setPosition(e.latlng)
            map.setView(e.latlng, 18)
        },
    })
    return position === null ? null : <Marker position={position} icon={customIcon} />;
}

const SearchableMap = React.memo(({ apiData, isDarkMode, onMarkerClick }) => {
    const lightTileLayer = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
    const darkTileLayer = 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png';
    const icon = new L.Icon({
        iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
        iconSize: [25, 41],
        iconAnchor: [13, 41],
        popupAnchor: [0, -35],
    });
    return (
        <MapContainer
            center={[52.1, 18.5]}
            zoom={5}
            className="h-[40dvh] w-full cursor-pointer map"
            scrollWheelZoom={true}>
            <TileLayer
                attribution={isDarkMode ? '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>' : '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'}
                url={isDarkMode ? darkTileLayer : lightTileLayer}
            />
            <FitBounds locations={apiData} />
            {typeof apiData === "object" &&
                apiData.map((item, index) => {
                    const popupRef = React.createRef();
                    return (
                        <Marker key={index} icon={icon} position={[item.lat, item.lon]}>
                            <Popup ref={popupRef}>
                                {item.name}
                                <Button className="block w-full" onPress={() => onMarkerClick(item.lat, item.lon, popupRef)}>{translations.selectThisPlace}</Button>
                            </Popup>
                        </Marker>
                    );
                })
            }
            <ZoomControl position="bottomright" className="testing" />
            <FindMe />
            <LocationMarker />
        </MapContainer>
    );
});

export default ({ data, setData, isDarkMode }) => {
    const [address, setAddress] = React.useState('');
    const [apiData, setApiData] = React.useState();
    const [isLoading, setIsLoading] = React.useState(false);

    const fetchAddress = () => {
        setIsLoading(true);
        instance.get(`address/list/${address}`)
            .then((res) => {
                if (res.status === 200)
                    setApiData(res.data)
                else toast.error(res.data);
            })
            .catch(err => console.error(err))
            .finally(() => setIsLoading(false))
    }

    const handleCoordsChange = (lat, lon, popupRef) => {
        setData({
            ...data,
            lat,
            lon
        });
        if (popupRef && popupRef.current) {
            popupRef.current._source._map.closePopup(popupRef.current); // Zamknięcie popupu
        }
    };

    return (
        <div className="w-full pt-24 px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96">
            <Input className="mb-4" autocomplete={false} type="text" value={address} onValueChange={setAddress} label={translations.address} placeholder={translations.giveAddress} />
            <Button
                isLoading={isLoading}
                radius="full"
                className="mb-4 w-full text-white text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                onPress={fetchAddress}
            >
                {translations.search}
            </Button>
            <SearchableMap
                apiData={apiData}
                isDarkMode={isDarkMode}
                onMarkerClick={handleCoordsChange}
            />
        </div>
    );
}