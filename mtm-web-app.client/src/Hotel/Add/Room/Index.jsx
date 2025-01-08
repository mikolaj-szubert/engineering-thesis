import React from 'react'
import { Breadcrumbs, BreadcrumbItem, Button } from '@nextui-org/react'
import Basic from './Basic'
import More from './More'
import Images from './Images'
import { translations } from '../../../lang'
import { instance } from '../../../Helpers'
import { Helmet } from 'react-helmet'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useCookies } from "react-cookie"

export default () => {
    const [cookies, , removeCookie] = useCookies(['addRoom']);
    const [searchParams,] = useSearchParams();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = React.useState(false);
    const [canAddMore, setCanAddMore] = React.useState(false);
    const [tab, setTab] = React.useState("basic");
    const [visitedTabs, setVisitedTabs] = React.useState(["basic"]);
    const [data, setData] = React.useState({
        name: "",
        description: "",
        price: "",
        roomType: "Standard",
        facilities: [],
        personCount: "",
        numberOfGivenRooms: "",
        images: [],
    });

    React.useEffect(() => {
        if (cookies.addRoom && cookies.addRoom.name === searchParams.get("hotelName")) {
            removeCookie('addRoom');
        }
    }, []);

    const isReadyToProceed = (tab === "basic" && data.name !== "" && !isNaN(parseFloat(data.price)) && data.price > 0 && !isNaN(parseInt(data.numberOfGivenRooms)) && data.numberOfGivenRooms > 0) ||
        (tab === "more" && data.roomType !== "" && data.facilities.length > 0 && data.personCount > 0) ||
        (tab === "images" && data.images.length > 2);

    const handleTabChange = (arg) => {
        setTab(arg);
        setVisitedTabs(oldArray => [...oldArray, arg]);
    }

    const sendData = () => {
        setIsLoading(true);
        const formData = new FormData();
        formData.append('Name', data.name);
        formData.append('RoomType', data.roomType);
        data.facilities.forEach((facility) => {
            formData.append('Facilities', facility);
        });
        formData.append('PersonCount', data.personCount);
        if (data.description !== "") formData.append('Description', data.description);
        formData.append('Price', data.price);
        formData.append('HotelName', searchParams.get("hotelName"));
        formData.append('NumberOfGivenRooms', data.numberOfGivenRooms);
        data.images.forEach((file) => {
            formData.append(`Files`, file);
        });
        instance.post('rooms', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            }
        })
            .then((res) => {
                if (res.status === 200) {
                    setCanAddMore(true)
                }
                else console.error(res.data);
            })
            .catch(err => console.error(err))
            .finally(() => setIsLoading(false))
    }
    if (!searchParams.get("hotelName") || searchParams.get("hotelName") === "")
        return (
            <div className="w-full h-[95vh] text-center place-content-center">
                <Helmet>
                    <title>Błąd</title>
                </Helmet>
                <h1 className="text-3xl font-semibold mb-4">Błąd</h1>
                <h3>Aby dostać się na tą stronę, musisz najpierw dodać hotel</h3>
            </div>
        );

    return (
        <div className="relative h-[93dvh] flex flex-col">
            <Helmet>
                <title>Dodawanie Pokoju | MTM Project</title>
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
                <BreadcrumbItem isDisabled={!visitedTabs.includes("more")} key="more" isCurrent={tab === "more"}>
                    {translations.addInfo}
                </BreadcrumbItem>
                <BreadcrumbItem isDisabled={!visitedTabs.includes("images")} key="images" isCurrent={tab === "images"}>
                    {translations.images}
                </BreadcrumbItem>
            </Breadcrumbs>

            {tab === "basic" && <Basic data={data} setData={setData} />}
            {tab === "more" && <More data={data} setData={setData} />}
            {tab === "images" && <Images data={data} setData={setData} />}
            <div className="z-10 fixed mx-auto bottom-16 left-2 right-2 sm:left-12 sm:right-12 xl:left-96 xl:right-96 md:left-36 md:right-36 lg:left-64 lg:right-64 text-lg font-medium">
                {!canAddMore ?
                    <Button
                        isLoading={isLoading}
                        radius="full"
                        className={`w-full ${isReadyToProceed ? 'bg-gradient-to-r from-gradient-zielony to-gradient-bezowy' : null}`}
                        onPress={() => {
                            switch (tab) {
                                case "basic":
                                    if (isReadyToProceed) handleTabChange("more");
                                    break;
                                case "more":
                                    if (isReadyToProceed) handleTabChange("images");
                                    break;
                                default:
                                    if (isReadyToProceed) sendData();
                                    break;
                            }
                        }}
                    >
                        {tab === "images" ? translations.save : translations.continue}
                    </Button>
                    :
                    <div>
                        <Button
                            radius="full"
                            className="w-full bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                            onPress={() => {
                                navigate(`/hotels/${searchParams.get("hotelName")}`);
                            }}
                        >
                            Zobacz swój hotel
                        </Button>
                        <Button
                            radius="full"
                            className='w-full mt-4'
                            onPress={() => {
                                setTab("basic")
                                setVisitedTabs(["basic"])
                                setData({
                                    name: "",
                                    description: "",
                                    price: 1,
                                    roomType: "Standard",
                                    facilities: [],
                                    personCount: 2,
                                    numberOfGivenRooms: 1,
                                    images: [],
                                });
                                setCanAddMore(false)
                            }}
                        >
                            Dodaj więcej
                        </Button>
                    </div>
                }
            </div>
        </div>
    );
}