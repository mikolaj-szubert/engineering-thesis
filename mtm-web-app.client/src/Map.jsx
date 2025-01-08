import { useState, useEffect } from "react";
import { useOutletContext } from 'react-router-dom';
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import { SuperClustering } from 'react-leaflet-supercluster'
import 'react-leaflet-supercluster/src/styles.css';
import 'leaflet/dist/leaflet.css';
import "./CustomStyles/map.css";
import { Helmet } from "react-helmet";

export default function Map() {
    const { isDarkMode } = useOutletContext();
    const lightTileLayer = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
    const darkTileLayer = 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png';
    const [hotels, setHotels] = useState();

    useEffect(() => {
        populateHotelData();
    }, []);

    const populateHotelData = async () => {
        const response = await fetch('/api/hotels'); //zamienić na instance
        const data = await response.json();
        setHotels(data.result);
    }

    const dataMarkers = hotels === undefined
        ? null
        : <SuperClustering>
            {hotels.map(hotel =>
                <Marker key={hotel.name} position={hotel.coordinates}>
                    <Popup>{hotel.name}</Popup>
                </Marker>
            )}
        </SuperClustering>;
    return (
        <div className="h-full w-screen overflow-hidden max-h-[calc(100%-2.5rem)]">
            <Helmet>
                <title>Mapa | MTM Project</title>
            </Helmet>
            <MapContainer className="map h-full max-h-full max-w-full block z-0 overflow-hidden" center={[52.405, 16.937]} zoom={13} scrollWheelZoom={true}>
                <TileLayer
                    attribution={isDarkMode ? '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>' : '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'}
                    url={isDarkMode ? darkTileLayer : lightTileLayer }
                />
                {dataMarkers}
            </MapContainer>
        </div>
    );
}