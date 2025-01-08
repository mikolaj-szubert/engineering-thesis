import React from 'react'
import { Breadcrumbs, BreadcrumbItem, Button } from '@nextui-org/react'
import Basic from './Basic'
import More from './More'
import Address from './Address'
import Images from './Images'
import { translations } from '../../lang'
import { instance } from '../../Helpers'
import { Helmet } from 'react-helmet'
import { useOutletContext, useNavigate, useSearchParams } from 'react-router-dom'

export default () => {
    const [searchParams,] = useSearchParams();
    const { isDarkMode } = useOutletContext();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = React.useState(false);
    const [tab, setTab] = React.useState("basic");
    const [data, setData] = React.useState({
        name: searchParams.get("name"),
        lat: "",
        lon: "",
        currency: searchParams.get("currency"),
        description: searchParams.get("description"),
        cusines: searchParams.get("facilities").split(","),
        images: [],
    });
    const [openDays, setOpenDays] = React.useState(searchParams.get("openDays").split(";").map(e => {
        const d = e.split(",");
        return {
            dayOfWeek: d[0],
            openingTime: d[1],
            closingTime: d[2]
        }
    }));

    const handleTabChange = (arg) => {
        setTab(arg);
    }

    const prev = {
        name: searchParams.get("name"),
        address: searchParams.get("address"),
        description: searchParams.get("description"),
        cusines: searchParams.get("facilities").split(","),
        currency: searchParams.get("currency")
    };

    const prevOpenDays = searchParams.get("openDays").split(";").map(e => {
        const d = e.split(",");
        return {
            dayOfWeek: d[0],
            openingTime: d[1],
            closingTime: d[2]
        }
    });

    const sendData = () => {
        setIsLoading(true);
        const formData = new FormData();
        formData.append('Name', prev.name);
        if (data.name !== prev.name && data.name !== "") formData.append('NewName', data.name);
        if (data.lat !== "") formData.append('Lat', data.lat);
        if (data.lon !== "") formData.append('Lon', data.lon);
        if (JSON.stringify(data.cusines) !== JSON.stringify(prev.cusines) && data.cusines.length > 0) {
            data.cusines.forEach((cusine) => {
                formData.append(`Cusines`, cusine);
            });
        }
        if (JSON.stringify(openDays) !== JSON.stringify(prevOpenDays) && openDays.length > 0)
        openDays.forEach((day) => {
            formData.append(`OpenDays`, day.dayOfWeek);
            formData.append(`StartHours`, day.openingTime);
            formData.append(`EndHours`, day.closingTime);
        });
        if(data.currency !== prev.currency && data.currency) formData.append('RestaurantCurrency', data.currency);
        if (data.description !== prev.description) formData.append('Description', data.description);
        if (data.images.length > 0) {
            data.images.forEach((file) => {
                formData.append(`Files`, file);
            });
        }
        instance.put('restaurants', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            }
        })
            .then((res) => {
                if (res.status === 200) {
                    navigate('/restaurants/' + data.name);
                }
                else console.error(res.data);
            })
            .catch(err => console.error(err))
            .finally(() => setIsLoading(false))
    }

    return (
        <div className="relative h-[93dvh] flex flex-col">
            <Helmet>
                <title>Edycja Hotelu | MTM Project</title>
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
                <BreadcrumbItem key="more" isCurrent={tab === "more"}>
                    {translations.addInfo}
                </BreadcrumbItem>
                <BreadcrumbItem key="address" isCurrent={tab === "address"}>
                    {translations.address}
                </BreadcrumbItem>
                <BreadcrumbItem key="images" isCurrent={tab === "images"}>
                    {translations.images}
                </BreadcrumbItem>
            </Breadcrumbs>

            {tab === "basic" && <Basic data={data} setData={setData} />}
            {tab === "more" && <More data={data} setData={setData} openDays={openDays} setOpenDays={setOpenDays} />}
            {tab === "address" && <Address data={data} setData={setData} isDarkMode={isDarkMode} prev={prev} />}
            {tab === "images" && <Images data={data} setData={setData} />}

            <Button
                isLoading={isLoading}
                radius="full"
                className='z-10 fixed mx-auto bottom-8 left-2 right-2 sm:left-12 sm:right-12 xl:left-96 xl:right-96 md:left-36 md:right-36 lg:left-64 lg:right-64 text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy'
                onPress={() => {
                    switch (tab) {
                        case "basic":
                            handleTabChange("more");
                            break;
                        case "more":
                            handleTabChange("address");
                            break;
                        case "address":
                            handleTabChange("images");
                            break;
                        default:
                            sendData();
                            break;
                    }
                }}
            >
                {tab === "images" ? translations.save : translations.continue}
            </Button>
        </div>
    );
}