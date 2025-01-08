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
    const [cookies,, removeCookie] = useCookies(['addTable']);
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
        personCount: "",
        numberOfGivenTables: "",
        images: [],
    });

    React.useEffect(() => {
        if(cookies.addTable && cookies.addTable.name === searchParams.get("restaurantName")) {
            removeCookie('addTable');
        }
    },[]);

    const isReadyToProceed = (tab === "basic" && data.name !== "" && !isNaN(parseFloat(data.price)) && data.price > 0) ||
        (tab === "more" && data.personCount > 0 && !isNaN(parseInt(data.numberOfGivenTables)) && parseInt(data.numberOfGivenTables) > 0) ||
        (tab === "images" && data.images.length > 2);

    const handleTabChange = (arg) => {
        setTab(arg);
        setVisitedTabs(oldArray => [...oldArray, arg]);
    }

    const sendData = () => {
        setIsLoading(true);
        const formData = new FormData();
        formData.append('Name', data.name);
        formData.append('PersonCount', data.personCount);
        if(data.description !== "") formData.append('Description', data.description);
        formData.append('Price', data.price);
        formData.append('RestaurantName', searchParams.get("restaurantName"));
        formData.append('NumberOfGivenTables', data.numberOfGivenTables);
        data.images.forEach((file) => {
            formData.append(`Files`, file);
        });
        instance.post('tables', formData, {
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
                <title>Dodawanie Stołu | MTM Project</title>
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

            {tab === "basic" && <Basic data={data} setData={setData} restaurantCurr={searchParams.get("currency")} />}
            {tab === "more" && <More data={data} setData={setData} />}
            {tab === "images" && <Images data={data} setData={setData} />}
            <div className="z-10 fixed mx-auto bottom-16 left-2 right-2 sm:left-12 sm:right-12 xl:left-96 xl:right-96 md:left-36 md:right-36 lg:left-64 lg:right-64 text-lg font-medium">
                {!canAddMore ?
                    <Button
                        isLoading={isLoading}
                        isDisabled={!isReadyToProceed}
                        radius="full"
                        className='w-full bg-gradient-to-r from-gradient-zielony to-gradient-bezowy'
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
                            className="w-full bg-gradient-to-r from-gradient-zielony to-gradient-bezowy mb-4"
                            onPress={() => {
                                navigate(`/restaurants/${searchParams.get("restaurantName")}`);
                            }}
                        >
                            Zobacz swoją restaurację
                        </Button>
                        <Button
                            radius="full"
                            className='w-full'
                            onPress={() => {
                                setTab("basic")
                                setVisitedTabs(["basic"])
                                setData({
                                    name: "",
                                    description: "",
                                    price: "",
                                    personCount: "",
                                    numberOfGivenTables: "",
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