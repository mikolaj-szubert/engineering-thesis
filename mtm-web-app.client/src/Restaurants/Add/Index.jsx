import React from 'react'
import { Breadcrumbs, BreadcrumbItem, Button } from '@nextui-org/react'
import Basic from './Basic'
import More from './More'
import Address from './Address'
import Images from './Images'
import { translations } from '../../lang'
import { instance } from '../../Helpers'
import { Helmet } from 'react-helmet'
import { useOutletContext, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { useCookies } from "react-cookie"

export default () => {
    const [cookies, setCookie, removeCookie] = useCookies(['addTable']);
    const { curr, isDarkMode, onLogin, user } = useOutletContext();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = React.useState(false);
    const [tab, setTab] = React.useState("basic");
    const [visitedTabs, setVisitedTabs] = React.useState(["basic"]);
    const [disableAll, setDisableAll] = React.useState(false);
    const [data, setData] = React.useState({
        name: "",
        lat: "",
        lon: "",
        currency: curr,
        description: "",
        cusines: [],
        images: [],
    });
    const [openDays, setOpenDays] = React.useState([]);

    const isReadyToProceed = (tab === "basic" && data.name !== "" && data.description !== "" && data.currency !== "") ||
        (tab === "more" && data.cusines !== "" && data.cusines.length > 0 && openDays && openDays.length > 0) ||
        (tab === "address" && data.lat !== "" && data.lon !== "") ||
        (tab === "images" && data.images.length > 2);

    const handleTabChange = (arg) => {
        setTab(arg);
        setVisitedTabs(oldArray => [...oldArray, arg]);
    }

    const sendData = () => {
        setIsLoading(true);
        setDisableAll(true);
        const formData = new FormData();
        formData.append('Name', data.name);
        formData.append('Lat', data.lat);
        formData.append('Lon', data.lon);
        data.cusines.forEach((cusine) => {
            formData.append(`Cusines`, cusine);
        });
        openDays.forEach((day) => {
            formData.append(`OpenDays`, day.dayOfWeek);
            formData.append(`StartHours`, day.openingTime);
            formData.append(`EndHours`, day.closingTime);
        });
        formData.append('RestaurantCurrency', data.currency);
        formData.append('Description', data.description);
        data.images.forEach((file) => {
            formData.append(`Files`, file);
        });
        instance.post('restaurants', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            }
        })
            .then((res) => {
                if (res.status === 200) {
                    setTab("addTable");
                    setCookie('addTable', {name: data.name, currency: data.currency}, { path: '/', maxAge: 30 });
                    if (!user.Owner.includes("r")) {
                        onLogin({
                            ...user,
                            Owner: user.Owner + "r"
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
            .finally(() => setIsLoading(false))
    }

    React.useEffect(() => {
        if (cookies.addTable) {
            navigate(`tables?restaurantName=${cookies.addTable.name}&currency=${cookies.addTable.currency}`);
            removeCookie("addTable");
        }
    }, []);

    return (
        <div className="relative h-[93dvh] flex flex-col">
            <Helmet>
                <title>Dodawanie Restauracji | MTM Project</title>
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
            {tab === "more" && <More data={data} setData={setData} openDays={openDays} setOpenDays={setOpenDays} />}
            {tab === "address" && <Address data={data} setData={setData} isDarkMode={isDarkMode} />}
            {tab === "images" && <Images data={data} setData={setData} />}
            {tab === "addTable" && <Images data={data} setData={setData} />}
            
            <Button
                isDisabled={!isReadyToProceed && tab !== "addTable"}
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
                        case "addTable":
                            navigate(`tables?restaurantName=${data.name}&currency=${data.currency}`);
                            break;
                        default:
                            if (isReadyToProceed) sendData();
                            break;
                    }
                }}
            >
                {tab === "images" ? translations.save : null}
                {tab === "addTable" ? translations.addTables : null}
                {tab !== "images" && tab !== "addTable" ? translations.continue : null}
            </Button>
        </div>
    );
}