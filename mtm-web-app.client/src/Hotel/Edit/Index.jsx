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
        checkIn: searchParams.get("checkIn"),
        checkOut: searchParams.get("checkOut"),
        currency: searchParams.get("currency"),
        type: "",
        description: searchParams.get("description"),
        facilities: searchParams.get("facilities").split(","),
        images: [],
    });

    const handleTabChange = (arg) => {
        setTab(arg);
    }

    const prev = {
        name: searchParams.get("name"),
        address: searchParams.get("address"),
        checkIn: searchParams.get("checkIn"),
        checkOut: searchParams.get("checkOut"),
        description: searchParams.get("description"),
        facilities: searchParams.get("facilities").split(","),
        currency: searchParams.get("currency")
    };

    const sendData = () => {
        setIsLoading(true);
        const formData = new FormData();
        formData.append('Name', prev.name);
        if (data.name !== prev.name && data.name !== "") formData.append('NewName', data.name);
        if (data.lat !== "") formData.append('Lat', data.lat);
        if (data.lon !== "") formData.append('Lon', data.lon);
        if (data.checkIn !== prev.checkIn) formData.append('CheckIn', data.checkIn);
        if (data.checkOut !== prev.checkOut) formData.append('CheckOut', data.checkOut);
        if (data.type !== "") formData.append('Type', data.type);
        if (JSON.stringify(data.facilities) !== JSON.stringify(prev.facilities) && data.facilities.length > 0) {
            data.facilities.forEach((facility) => {
                formData.append(`Facilities`, facility);
            });
        }
        if (data.currency !== prev.currency) formData.append('HotelCurrency', data.currency);
        if (data.description !== prev.description) formData.append('Description', data.description);
        if (data.images.length > 0) {
            data.images.forEach((file) => {
                formData.append(`Files`, file);
            });
        }
        instance.put('hotels', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            }
        })
            .then((res) => {
                if (res.status === 200) {
                    navigate("/hotels/"+data.name);
                }
                else console.error(res.data);
            })
            .catch(err => console.error(err))
            .finally(() => setIsLoading(false))
    }
    const isEditing = searchParams.get("name") && searchParams.get("name") !== "" &&
        searchParams.get("checkIn") && searchParams.get("checkIn") !== "" &&
        searchParams.get("checkOut") && searchParams.get("checkOut") !== "" &&
        searchParams.get("address") && searchParams.get("address") !== "" &&
        searchParams.get("description") && searchParams.get("description") !== "" &&
        searchParams.get("facilities") && searchParams.get("facilities") !== "" && searchParams.get("facilities").split(",").length > 0;
    if (!isEditing)
        return (
            <div className="w-full h-[95vh] text-center place-content-center">
                <Helmet>
                    <title>{translations.formatString(translations.title, "Błąd edycji")}</title>
                </Helmet>
                <h1 className="text-3xl font-semibold mb-4">Błąd edycji</h1>
                <h3>Aby dostać się na tą stronę, musisz wybrać opcję edytuj z poziomu strony swojego hotelu</h3>
            </div>
        );
    return (
        <div className="relative h-[93dvh] flex flex-col">
            <Helmet>
                <title>{translations.formatString(translations.title, "Edycja Hotelu")}</title>
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
            {tab === "more" && <More data={data} setData={setData} />}
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