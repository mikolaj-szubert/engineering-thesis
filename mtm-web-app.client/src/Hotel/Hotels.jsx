import { instance } from '../Helpers'
import {
    Button,
    Checkbox,
    Input,
    Modal,
    ModalBody,
    ModalContent,
    ModalFooter,
    ModalHeader,
    Slider,
    useDisclosure,
    Card,
    CardHeader,
    CardBody,
    Image,
    Skeleton,
    DateRangePicker,
    ScrollShadow
} from '@nextui-org/react'
import React from 'react'
import { useSearchParams, useOutletContext, useNavigate } from 'react-router-dom'
import { translations } from '../lang'
import Footer from '../Footer';
import { today, getLocalTimeZone, parseDate } from "@internationalized/date";
import { Helmet } from 'react-helmet'

const StarProgressBar = ({ value }) => {
    const MAX_STARS = 5;

    // Tworzymy tablicę gwiazdek
    const stars = Array.from({ length: MAX_STARS }, (_, index) => {
        const fillLevel = Math.min(Math.max(value - index, 0), 1); // Obliczamy wypełnienie każdej gwiazdki
        return fillLevel;
    });

    return (
        <div className="flex space-x-1">
            {stars.map((fill, index) => (
                <div
                    key={index}
                    className="relative w-6 h-6 text-yellow-400"
                >
                    {/* Tło pustej gwiazdki */}
                    <svg
                        className="absolute inset-0"
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    >
                        <polygon
                            points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"
                            className="dark:text-gray-700 text-gray-400"
                            fill="currentColor"
                        />
                    </svg>
                    <svg
                        className="absolute inset-0"
                        xmlns="http://www.w3.org/2000/svg"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        style={{ clipPath: `inset(0 ${100 - fill * 100}% 0 0)` }}
                    >
                        <polygon
                            points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"
                            className="text-yellow-400"
                            fill="currentColor"
                        />
                    </svg>
                </div>
            ))}
        </div>
    );
};

const Hotel = React.memo(({ hotels }) => {
    const navigate = useNavigate();
    const [searchParams,] = useSearchParams();
    return (
        <div>
            {hotels === null ?
                [...Array(3).keys()].map((_, index) => (
                    <Card className="py-4 m-4 w-[324px] h-[359px] float-left" key={index} >
                        <Skeleton className="rounded-lg mx-4">
                            <div className="h-[200px] w-[300px]"></div>
                        </Skeleton>
                        <CardBody className="space-y-2">
                            <Skeleton className="rounded-lg h-[28px] w-full">
                                <div className="rounded-lg">nazwa</div>
                            </Skeleton>
                            <Skeleton className="rounded-lg h[-[16px] w-4/5">
                                <div className="rounded-lg">miejsce</div>
                            </Skeleton>
                            <Skeleton className="rounded-lg h-[19px] w-1/5">
                                <div className="rounded-lg">cena</div>
                            </Skeleton>
                            <Skeleton className="rounded-lg h-[24px] w-[130px]">
                                <div className="rounded-lg">ocena</div>
                            </Skeleton>
                        </CardBody>
                    </Card>
                ))
                : typeof hotels === "object" ? (
                    hotels.map((hotel) => (
                        <Card key={hotel.name} className="py-4 m-4 w-max float-left cursor-pointer">
                            <div onClick={() => navigate({
                                pathname: hotel.name,
                                search: searchParams.get("startDate") !== null && searchParams.get("endDate") !== null ? `?startDate=${searchParams.get("startDate")}&endDate=${searchParams.get("endDate")}`:null
                            })}>
                                <CardHeader className="overflow-visible py-2">
                                    <Image
                                        isZoomed
                                        alt="Card background"
                                        className="object-cover rounded-xl"
                                        src={hotel.image !== null ? '/api/images/hotel/' + hotel.name + "/" + hotel.image : "../broken.png"}
                                        width={300}
                                        height={200}
                                    />
                                </CardHeader>
                                <CardBody>
                                    <h4 className="font-bold text-large">{hotel.name}</h4>
                                    <p className="text-tiny uppercase font-bold">{hotel.city}, {hotel.country}</p>
                                    <small className="text-default-500">{hotel.minPrice}</small>
                                    <StarProgressBar value={hotel.rating} />
                                </CardBody>
                            </div>
                        </Card>
                    ))
                )
                :
                <p className="p-12">{hotels}</p>
            }
        </div>
    )
});

const Filters = React.memo(({ city, setCity, value, setValue, curr, maxVal, setRating, rating, setObjTypes, objTypes, setFacilities, facilities, currentDateSelection, setCurrentDateSelection, allTypes, allFacilities }) => {
    return (
        <div className="px-1 pt-2 pb-8 w-full items-center justify-center">
            <h3 className="my-4 text-xl font-semibold dark:text-white text-black">{translations.city}</h3>
            <Input
                type="text"
                value={city}
                onChange={(e) => setCity(e.target.value)}
                size="sm"
                placeholder={translations.typeCity}
                className="w-full"
            />
            <DateRangePicker
                selectorButtonPlacement="start"
                label={translations.dateRange}
                labelPlacement="outside"
                className="w-full dark:text-white text-black"
                classNames={{
                    label: "my-4 text-xl font-semibold",
                }}
                value={currentDateSelection}
                onChange={setCurrentDateSelection}
                minValue={today(getLocalTimeZone())}
            />
            <Slider
                label={translations.priceRange}
                step={1}
                maxValue={maxVal}
                minValue={0}
                value={value}
                onChange={setValue}
                formatOptions={{ style: "currency", currency: curr }}
                className="w-full dark:text-white text-black"
                classNames={{
                    label: "my-4 text-xl font-semibold",
                }}
            />
            <h3 className="my-4 text-xl font-semibold dark:text-white text-black">{translations.objType}</h3>
            {allTypes.map((type) => (
                <Checkbox
                    key={type}
                    isSelected={objTypes.includes(type)}
                    className="block"
                    onValueChange={(isSelected) => {
                        setObjTypes((prev) =>
                            isSelected
                                ? [...prev, type]
                                : prev.filter((f) => f !== type)
                        );
                    }}
                >
                    {type.charAt(0).toUpperCase() + type.slice(1)}
                </Checkbox>
            ))}
            <h3 className="my-4 text-xl font-semibold dark:text-white text-black">{translations.amenities}</h3>
            {allFacilities.map((facility) => (
                <Checkbox
                    key={facility}
                    isSelected={facilities.includes(facility)}
                    className="block"
                    onValueChange={(isSelected) => {
                        setFacilities((prev) =>
                            isSelected
                                ? [...prev, facility]
                                : prev.filter((f) => f !== facility)
                        );
                    }}
                >
                    {translations[facility]}
                </Checkbox>
            ))}
            <Slider
                showSteps
                label={translations.guestRating}
                step={1}
                maxValue={5}
                minValue={1}
                value={rating}
                onChange={setRating}
                className="w-full mt-4 dark:text-white text-black"
                classNames={{
                    label: "my-4 text-xl font-semibold",
                }}
            />
        </div>
    );
});

export default () => {
    const [maxVal, setMaxVal] = React.useState(10000);
    const { curr } = useOutletContext(); //currency selected by user
    const { isOpen, onOpen, onOpenChange } = useDisclosure(); //modal
    let [searchParams, setSearchParams] = useSearchParams();

    const allFacilities = [
        "FreeWiFi", "FreeParking", "PaidParking", "AirConditioning", "Heating",
        "Elevator", "WheelchairAccessible", "NonSmoking", "Smoking",
        "PetFriendly", "Kitchen", "PrivateBathroom", "WashingMachine", "Dryer",
        "Iron", "HairDryer", "Pool", "HotTub", "Sauna", "FitnessCenter", "Spa",
        "MassageService", "GameRoom", "Playground", "Library", "Garden", "Terrace",
        "BarbecueFacilities", "TennisCourt", "GolfCourse", "WaterSportsFacilities",
        "SkiStorage", "SkiInSkiOutAccess", "HikingTrails", "BicycleRental",
        "HorseRiding", "BowlingAlley", "AirportShuttle", "CarRental",
        "ElectricVehicleCharging", "RoomService", "MiniBar", "CoffeeMaker",
        "InRoomSafe", "FlatScreenTV", "Balcony", "OceanView", "MountainView",
        "BlackoutCurtains", "PremiumBedding", "Restaurant", "Bar",
        "BreakfastIncluded", "Kitchenette", "VendingMachine", "ConciergeService",
        "Housekeeping", "LaundryService", "BabysittingService", "LuggageStorage",
        "CurrencyExchange", "ATMOnSite", "TourDesk", "TicketService",
        "BusinessCenter", "MeetingRooms", "HypoallergenicRooms", "FamilyRooms",
        "FreeHighSpeedInternet", "SatelliteTV", "StreamingServiceAccess",
        "SmartHomeFeatures", "USBChargingPorts", "MultilingualStaff", "Fireplace",
        "SelfCheckIn", "EcoFriendly"
    ];
    const allTypes = [
        'Hotel',
        'Hostel',
        'Apartment',
        'Penthouse',
        'House',
        'Willa',
        'Pension'
    ];

    const [value, setValue] = React.useState([
        Number(searchParams.get("minPrice") || 0),
        Number(searchParams.get("maxPrice") || maxVal),
    ]);
    const [rating, setRating] = React.useState([
        Number(searchParams.get("minRating") || 1),
        Number(searchParams.get("maxRating") || 10)
    ]);
    const [hotels, setHotels] = React.useState(null); //api fetch results
    const [inputMinValue, setInputMinValue] = React.useState(value[0]);
    const [inputMaxValue, setInputMaxValue] = React.useState(value[1]);
    const [city, setCity] = React.useState(searchParams.get("city") || "");
    const [objTypes, setObjTypes] = React.useState(searchParams.get("types") !== null ? searchParams.get("types").split(",") : []);
    const [facilities, setFacilities] = React.useState(searchParams.get("facilities") !== null ? searchParams.get("facilities").split(",") : []);
    const [currentDateSelection, setCurrentDateSelection] = React.useState(searchParams.get("startDate") !== null && searchParams.get("endDate") !== null ? {
        start: parseDate(searchParams.get("startDate")),
        end: parseDate(searchParams.get("endDate"))
    } : null);

    const fetchHotels = async () => {
        const params = Object.fromEntries(searchParams);
        instance.get(Object.keys(params).length === 0 ? `hotels?currency=${curr}` : `hotels?currency=${curr}`, { params })
            .then(response => {
                setHotels(response.data.result);
                if (response.status === 200) {
                    setMaxVal(Math.ceil(response.data.maxPrice));
                }
            })
            .catch(error => console.error("Błąd podczas pobierania hoteli:", error))
    };

    React.useEffect(() => {
        fetchHotels();
    }, [searchParams]);

    const handleSubmitFilters = () => {
        const params = new URLSearchParams();

        if (value[0] !== 0 || value[1] !== maxVal) {
            params.set("minPrice", value[0]);
            params.set("maxPrice", value[1]);
        }

        if (currentDateSelection !== null) {
            params.set("startDate", currentDateSelection.start.toString());
            params.set("endDate", currentDateSelection.end.toString());
        }

        if (city !== "") params.set("city", city);

        if (facilities.length > 0) {
            params.set("facilities", facilities.join(","))
        }

        if (objTypes.length > 0) {
            params.set("types", objTypes.join(","))
        }

        if (rating[0] !== 1 || rating[1] !== 10) {
            params.set("minRating", rating[0]);
            params.set("maxRating", rating[1]);
        }

        setSearchParams(params);

        if (isOpen)
            onOpenChange(false);
    };

    return (
        <>
            <Helmet>
                <title>{translations.formatString(translations.title, "Hotele")}</title>
            </Helmet>
            <div className="flex flex-col md:flex-row h-[96dvh]">
                <ScrollShadow className="hidden md:block w-1/4 border-r border-white p-4 h-[95dvh] overflow-y-scroll overflow-x-hidden filters relative">
                    <h4 className="text-4xl text-black dark:text-white font-bold">{translations.filters}</h4>
                    <Filters
                        {...{ city, setCity, value, setValue, inputMinValue, setInputMinValue, inputMaxValue, setInputMaxValue, curr, maxVal, setRating, rating, setObjTypes, objTypes, setFacilities, facilities, currentDateSelection, setCurrentDateSelection, allTypes, allFacilities }}
                    />
                    <div className="sticky bottom-4 bg-transparent">
                        <Button
                            className="text-white w-full text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                            onPress={handleSubmitFilters}
                        >
                            {translations.confirm}
                        </Button>
                    </div>
                </ScrollShadow>
                <div className="w-full md:w-3/4">
                    <Button className="md:hidden block m-4 place-self-center w-4/5" color="primary" onPress={onOpen}>{translations.filters}</Button>
                    <Modal
                        isOpen={isOpen}
                        onOpenChange={onOpenChange}
                        hideCloseButton
                        isDismissable={false}
                        size="full"
                        className="md:hidden"
                    >
                        <ModalContent>
                            <ModalHeader>
                                <h4 className="mt-4 dark:text-white text-black text-4xl font-bold">{translations.filters}</h4>
                                <Button isIconOnly={true} className="absolute t-2 right-2 z-10 bg-transparent text-2xl dark:text-white text-black" radius="full" tabIndex={0} onPress={onOpenChange}>✖</Button>
                            </ModalHeader>
                            <ModalBody className="max-h-screen overflow-y-auto">
                                <Filters
                                    {...{ city, setCity, value, setValue, inputMinValue, setInputMinValue, inputMaxValue, setInputMaxValue, curr, maxVal, setRating, rating, setObjTypes, objTypes, setFacilities, facilities, currentDateSelection, setCurrentDateSelection, allTypes, allFacilities }}
                                />
                            </ModalBody>
                            <ModalFooter className="sticky bottom-0 bg-transparent p-4">
                                <Button
                                    className="text-white w-full text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                                    onPress={handleSubmitFilters}
                                >
                                    {translations.confirm}
                                </Button>
                            </ModalFooter>
                        </ModalContent>
                    </Modal>
                    <Hotel hotels={hotels} curr={curr} />
                </div>
            </div>
            <Footer />
        </>
    );
}
