import React from 'react'
import { Breadcrumbs, BreadcrumbItem, Button } from '@nextui-org/react'
import Basic from './Basic'
import More from './More'
import Address from './Address'
import Images from './Images'
import { translations } from '../../lang'
import { parseTime } from '@internationalized/date'
import { instance } from '../../Helpers'
import { Helmet } from 'react-helmet'
import { useOutletContext, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { useCookies } from "react-cookie"

export default () => {
    const [cookies, setCookie, removeCookie] = useCookies(['addRoom']);
    const { curr, isDarkMode, user, onLogin } = useOutletContext();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = React.useState(false);
    const [tab, setTab] = React.useState("basic");
    const [visitedTabs, setVisitedTabs] = React.useState(["basic"]);
    const [disableAll, setDisableAll] = React.useState(false);
    const [data, setData] = React.useState({
        name: "",
        lat: "",
        lon: "",
        checkIn: '15:00',
        checkOut: '11:00',
        currency: curr,
        type: "Hotel",
        description: "",
        facilities: [],
        images: [],
    });

    const isReadyToProceed = (tab === "basic" && data.name !== "" && data.description !== "" && data.currency !== "") ||
        (tab === "more" && data.type !== "" && data.checkIn !== "" && data.checkOut != "" && parseTime(data.checkIn).compare(parseTime(data.checkOut)) > 0 && data.facilities.length > 0) ||
        (tab === "address" && data.lat !== "" && data.lon !== "") ||
        (tab === "images" && data.images.length > 2);

    const handleTabChange = (arg) => {
        setTab(arg);
        setVisitedTabs(oldArray => [...oldArray, arg]);
    }
    const sendData = async () => {
        setIsLoading(true);
        setDisableAll(true);
        const formData = new FormData();
        formData.append('Name', data.name);
        formData.append('Lat', data.lat);
        formData.append('Lon', data.lon);
        formData.append('CheckIn', data.checkIn);
        formData.append('CheckOut', data.checkOut);
        formData.append('Type', data.type);
        data.facilities.forEach((facility) => {
            formData.append(`Facilities`, facility);
        });
        formData.append('HotelCurrency', data.currency);
        formData.append('Description', data.description);
        data.images.forEach((file) => {
            formData.append(`Files`, file);
        });
        await instance.post('hotels', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            }
        })
            .then((res) => {
                if (res.status === 200) {
                    setCookie('addRoom', { name: data.name, currency: data.currency }, { path: '/', maxAge: 30 });
                    if (!user.Owner.includes("h")) {
                        onLogin({
                            ...user,
                            Owner: user.Owner + "h"
                        });
                    }
                }
                else {
                    setDisableAll(false);
                    toast.error(res.data);
                }
            })
            .catch(err => {
                setDisableAll(false);
                console.error(err);
            })
            .finally(() => {
                setIsLoading(false);
            })
    }

    React.useEffect(() => {
        if (cookies.addRoom) {
            navigate(`rooms?hotelName=${cookies.addRoom.name}&currency=${cookies.addRoom.currency}`);
            removeCookie("addRoom");
        }
    }, []);

    return (
        <div className="relative h-[93dvh] flex flex-col">
            <Helmet>
                <title>Dodawanie Hotelu | MTM Project</title>
            </Helmet>
            <Breadcrumbs
                className="m-auto place-items-center my-4"
                size="lg"
                underline="active"
                onAction={(key) => setTab(key)}
            >
                <BreadcrumbItem key="basic" isCurrent={tab === "basic"}>
                    {translations.basicInfo}
                </BreadcrumbItem>
                <BreadcrumbItem isDisabled={disableAll || !visitedTabs.includes("more")} key="more" isCurrent={tab === "more"}>
                    {translations.addInfo}
                </BreadcrumbItem>
                <BreadcrumbItem isDisabled={disableAll || !visitedTabs.includes("address")} key="address" isCurrent={tab === "address"}>
                    {translations.address}
                </BreadcrumbItem>
                <BreadcrumbItem isDisabled={disableAll || !visitedTabs.includes("images")} key="images" isCurrent={tab === "images"}>
                    {translations.images}
                </BreadcrumbItem>
            </Breadcrumbs>

            {tab === "basic" && <Basic data={data} setData={setData} />}
            {tab === "more" && <More data={data} setData={setData} />}
            {tab === "address" && <Address data={data} setData={setData} isDarkMode={isDarkMode} />}
            {tab === "images" && <Images data={data} setData={setData} />}

            <Button
                isDisabled={!isReadyToProceed}
                isLoading={isLoading}
                radius="full"
                className='z-10 fixed mx-auto bottom-8 left-2 right-2 sm:left-12 sm:right-12 xl:left-96 xl:right-96 md:left-36 md:right-36 lg:left-64 lg:right-64 text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy'
                onPress={() => {
                    switch (tab) {
                        case "basic":
                            if (isReadyToProceed) handleTabChange("more");
                            break;
                        case "more":
                            if (isReadyToProceed) handleTabChange("address");
                            break;
                        case "address":
                            if (isReadyToProceed) handleTabChange("images");
                            break;
                        default:
                            if (isReadyToProceed) sendData();
                            break;
                    }
                }}
            >
                {tab === "images" ? translations.save : tab === "addRoom" ? translations.addRooms : translations.continue}
            </Button>
        </div>
    );
}