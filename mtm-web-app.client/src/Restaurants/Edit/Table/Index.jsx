import React from 'react'
import { Breadcrumbs, BreadcrumbItem, Button } from '@nextui-org/react'
import Basic from './Basic'
import More from './More'
import Images from './Images'
import { translations } from '../../../lang'
import { instance } from '../../../Helpers'
import { Helmet } from 'react-helmet'
import { useNavigate, useSearchParams } from 'react-router-dom'

export default () => {
    const [searchParams,] = useSearchParams();
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = React.useState(false);
    const [tab, setTab] = React.useState("basic");
    const [data, setData] = React.useState({
        name: searchParams.get("name"),
        description: searchParams.get("description") === " " ? "" : searchParams.get("description"),
        price: "",
        personCount: searchParams.get("personCount"),
        numberOfGivenTables: "",
        images: [],
    });

    const handleTabChange = (arg) => {
        setTab(arg);
    }

    const prev = {
        name: searchParams.get("name"),
        price: "",
        personCount: searchParams.get("personCount"),
        description: searchParams.get("description") === " " ? "" : searchParams.get("description"),
    };

    const sendData = () => {
        setIsLoading(true);
        const formData = new FormData();
        formData.append('Name', prev.name);
        if (data.name !== prev.name && data.name !== "") formData.append('NewName', data.name);
        if (data.personCount !== prev.personCount && !isNaN(data.personCount) && parseInt(data.personCount) > 0) formData.append('PersonCount', data.personCount);
        if (data.description !== prev.description && data.description !== "") formData.append('Description', data.description);
        if (data.price !== prev.price) formData.append('Price', data.price);
        formData.append('RestaurantName', searchParams.get("restaurantName"));
        if (data.numberOfGivenTables !== "") formData.append('NumberOfGivenTables', data.numberOfGivenTables);
        if (data.images.length >= 3) {
            data.images.forEach((file) => {
                formData.append(`Files`, file);
            });
        }
        instance.put('tables', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            }
        })
            .then((res) => {
                if (res.status === 200) {
                    navigate("/restaurants/" + data.name === "" ? prev.name : data.name)
                }
                else console.error(res.data);
            })
            .catch(err => console.error(err))
            .finally(() => setIsLoading(false))
    }
    if (!searchParams.get("restaurantName") || searchParams.get("restaurantName") === "")
        return (
            <div className="w-full h-[95vh] text-center place-content-center">
                <Helmet>
                    <title>Błąd</title>
                </Helmet>
                <h1 className="text-3xl font-semibold mb-4">Błąd</h1>
                <h3>Aby dostać się na tą stronę, musisz najpierw dodać restaurację</h3>
            </div>
        );

    return (
        <div className="relative h-[93dvh] flex flex-col">
            <Helmet>
                <title>Edycja Stołu | MTM Project</title>
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
                <BreadcrumbItem key="images" isCurrent={tab === "images"}>
                    {translations.images}
                </BreadcrumbItem>
            </Breadcrumbs>

            {tab === "basic" && <Basic data={data} setData={setData} restaurantCurr={searchParams.get("currency")} />}
            {tab === "more" && <More data={data} setData={setData} />}
            {tab === "images" && <Images data={data} setData={setData} />}
            <div className="z-10 fixed mx-auto bottom-16 left-2 right-2 sm:left-12 sm:right-12 xl:left-96 xl:right-96 md:left-36 md:right-36 lg:left-64 lg:right-64 text-lg font-medium">
                <Button
                    isLoading={isLoading}
                    radius="full"
                    className='w-full bg-gradient-to-r from-gradient-zielony to-gradient-bezowy'
                    onPress={() => {
                        switch (tab) {
                            case "basic":
                                handleTabChange("more");
                                break;
                            case "more":
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
        </div>
    );
}