import { useParams, useOutletContext, useNavigate, useSearchParams } from 'react-router-dom'
import React from 'react'
import { instance } from '../Helpers'
import Footer from '../Footer'
import { Button, Spinner, Modal, ModalContent, ModalBody, useDisclosure, Chip, Card, CardHeader, CardBody, DateRangePicker, Popover, PopoverContent, PopoverTrigger, Skeleton } from '@nextui-org/react'
import { Slide } from 'react-slideshow-image'
import 'react-slideshow-image/dist/styles.css'
import { translations } from '../lang'
import { Helmet } from 'react-helmet'
import { toast } from 'react-toastify'
import { today, getLocalTimeZone, parseDate } from "@internationalized/date";

const Gallery = ({ hotel, imageArr }) => {
    const { isOpen, onOpen, onClose } = useDisclosure();
    const [currentIndex, setCurrentIndex] = React.useState(0);

    const properties = {
        transitionDuration: 500,
        arrows: true,
        infinite: true,
        easing: "ease"
    };

    const propertiesFullScreen = {
        transitionDuration: 300,
        arrows: true,
        infinite: true,
        easing: "ease"
    };

    const setCurrIndex = (_, to) => setCurrentIndex(to);
    const isCenter = (index) => (currentIndex + 1 == imageArr.length ? 0 : currentIndex + 1) === index

    return (
        <div>
            <div className="m-auto w-11/12 h-[50dvh] mb-12">
                <Slide className="h-[50vh]" {...properties} indicators={true} slidesToShow={1} slidesToScroll={1} onStartChange={setCurrIndex} responsive={[{
                    breakpoint: 768,
                    settings: {
                        slidesToShow: 3,
                        slidesToScroll: 1
                    }
                }]}>
                    {imageArr.map((img, index) => (
                        <div onClick={isCenter(index) ? onOpen : null} key={index} className={`relative flex items-center justify-center bg-cover bg-center h-[50vh] cursor-pointer transition-all duration-500 ${isCenter(index) ? 'brightness-100' : 'md:brightness-50 scale-90'}`} style={{ backgroundImage: `url('/api/images/hotel/${hotel.name}/${img.url}')` }}>
                            <span className={img.desc === null ? 'hidden' : `absolute bottom-0 left-0 w-full bg-black bg-opacity-50 text-white text-center p-2`}>{img.desc}</span>
                        </div>
                    ))}
                </Slide>
            </div>
            <Modal
                size="full"
                isOpen={isOpen}
                onClose={onClose}
            >
                <ModalContent>
                    {(onClose) => (
                        <ModalBody>
                            <Slide className="h-[100dvh]" {...propertiesFullScreen} infinite={false} indicators={false} slidesToShow={1} slidesToScroll={1} onStartChange={setCurrIndex} defaultIndex={currentIndex + 1 === imageArr.length ? 0 : currentIndex + 1}>
                                {imageArr.map((img, index) => (
                                    <div key={index} className='relative flex items-center justify-center bg-contain bg-center bg-no-repeat h-[100dvh]' style={{ backgroundImage: `url('/api/images/hotel/${hotel.name}/${img.url}')` }}>
                                        <span className={img.desc === null ? 'hidden' : `absolute bottom-0 left-0 w-full bg-black bg-opacity-50 text-white text-center p-2`}>{img.desc}</span>
                                    </div>
                                ))}
                            </Slide>
                        </ModalBody>
                    )}
                </ModalContent>
            </Modal>
        </div>
    );
};

const Rooms = ({ id, hotel }) => {
    const navigate = useNavigate();
    const [rooms, setRooms] = React.useState(null);
    const [searchParams, setSearchParams] = useSearchParams();
    const { curr, user } = useOutletContext();
    const [currentDateSelection, setCurrentDateSelection] = React.useState(searchParams.get("startDate") !== null && searchParams.get("endDate") !== null ? {
        start: parseDate(searchParams.get("startDate")),
        end: parseDate(searchParams.get("endDate"))
    } : null);

    const fetchRooms = async () => {
        let dateRange = "";
        const base = "hotels/" + id + "/rooms" + "?currency=" + curr;
        if (currentDateSelection !== null) {
            dateRange = `&start=${currentDateSelection.start.toString()}&end=${currentDateSelection.end.toString()}`;
            const params = new URLSearchParams();
            params.set("startDate", currentDateSelection.start.toString());
            params.set("endDate", currentDateSelection.end.toString());
            setSearchParams(params);
        }
        instance.get(currentDateSelection !== null ? base + dateRange : base)
            .then(res => {
                setRooms(res.data === "" ? null : res.data);
            })
            .catch(err => console.error(err));
    }

    const deleteRoom = async (hotel, roomName) => {
        await instance.delete('rooms/' + hotel + '/' + roomName)
            .then(res => {
                if (res.status === 200) {
                    setRooms(prevRooms => prevRooms.filter(r => r.name !== roomName));
                }
            }).catch(err => console.error(err));
    }

    React.useEffect(() => {
        fetchRooms();
    }, [id]);

    return (
        <div className={`my-4 md:mx-48 ${rooms === null ? "h-[500px]" : null } h-max`}>
            <DateRangePicker
                label={translations.dateRange}
                className="my-4"
                classNames={{
                    content: "w-full",
                }}
                value={currentDateSelection}
                onChange={setCurrentDateSelection}
                minValue={today(getLocalTimeZone())}
            />
            <Button
                isDisabled={currentDateSelection === null}
                className="text-white w-full text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                onPress={fetchRooms}
            >
                {translations.confirm}
            </Button>
            {rooms !== null ?
                rooms.map((item, index) => (
                    <Card className="my-4" key={index}>
                        <CardHeader>
                            <h3 className="text-xl font-semibold">{item.name}</h3>
                        </CardHeader>
                        <CardBody>
                            {item.description ? <strong>{translations.description}:</strong> : null}
                            {item.description ? item.description.split("\n").map((e, index) => (
                                <div className="pt-2" key={index}>
                                    <h5>{e}</h5>
                                </div>
                            )) : null}
                            {item.facilities && item.facilities.length > 0 ?
                                <div className="mt-4">
                                    <strong>{translations.amenities}:</strong>
                                    <ul className="list-disc list-inside">
                                        {
                                            item.facilities.map((i, index) => (
                                                <li className="mx-1" color="primary" key={index} variant="shadow">{translations[i]}</li>
                                            ))
                                        }
                                    </ul>
                                </div>
                                :
                                null
                            }
                            <strong className="mt-4">{translations.spotsCount}:</strong>{item.personCount}
                            <strong className="mt-4">{translations.nightCost}:</strong>{item.price}
                            <Button
                                isDisabled={searchParams.get("startDate") === null || searchParams.get("endDate") === null}
                                className="text-white w-full my-2 bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                                onPress={() => navigate({
                                    pathname: '/payment',
                                    search: `?hotel=${hotel.name}&room=${item.name}&currency=${curr}&start=${currentDateSelection.start.toString()}&end=${currentDateSelection.end.toString()}`,
                                })}
                            >
                                {searchParams.get("startDate") === null || searchParams.get("endDate") === null ? translations.selectReservationDate : translations.reserve}
                            </Button>
                            {user && user.email && user.email === hotel.ownerEmail ?
                                <>
                                    <Button
                                        variant="ghost"
                                        className="w-full my-2"
                                        onPress={() => {
                                            const description = item.description ? encodeURIComponent(item.description) : " ";
                                            const facilities = encodeURIComponent(item.facilities.join(','));
                                            navigate({
                                                pathname: '/hotels/edit/rooms',
                                                search: `?hotelName=${hotel.name}&name=${item.name}&currency=${hotel.currency}&facilities=${facilities}&description=${description}&personCount=${item.personCount}`, //edycja pokoju
                                            })
                                        }}
                                    >
                                        {translations.Edit}
                                    </Button>
                                    <Popover key="opaque" showArrow backdrop="opaque" offset={10} placement="bottom">
                                        <PopoverTrigger>
                                            <Button className="capitalize" color="danger" variant="ghost">
                                                {translations.Delete}
                                            </Button>
                                        </PopoverTrigger>
                                        <PopoverContent className="md:w-[60dvw] w-[90dvw]">
                                            <div className="px-1 py-2 w-full">
                                                <p className="text-small font-bold text-center text-foreground">
                                                    {translations.areYouSure}
                                                </p>
                                                <div className="mt-2 w-full">
                                                    <Button
                                                        onPress={async () =>
                                                            await deleteRoom(hotel.name, item.name) //usuwanie pokoju
                                                        }
                                                        className="w-full"
                                                        variant="ghost"
                                                        color="danger"
                                                    >
                                                        {translations.Delete}
                                                    </Button>
                                                </div>
                                            </div>
                                        </PopoverContent>
                                    </Popover>
                                </>
                                : null
                            }
                        </CardBody>
                    </Card>
                ))
                :
                [...Array(3).keys()].map((_, index) => (
                    <Card className="my-4" key={index}>
                        <CardHeader>
                            <Skeleton className="rounded-lg w-2/5"><h3 className="text-xl font-semibold">nazwa</h3></Skeleton>
                        </CardHeader>
                        <CardBody>
                            <Skeleton className="rounded-lg w-2/5"><strong>Opis:</strong></Skeleton>
                            <div className="pt-2">
                                <Skeleton className="rounded-lg w-3/5"><h5>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</h5></Skeleton>
                                <Skeleton className="rounded-lg w-3/5"><h5>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</h5></Skeleton>
                                <Skeleton className="rounded-lg w-1/5"><h5></h5></Skeleton>
                                <Skeleton className="rounded-lg w-4/5"><h5>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</h5></Skeleton>
                            </div>
                            <div className="mt-4">
                                <Skeleton className="rounded-lg"><strong>Udogodnienia:</strong></Skeleton>
                                <ul className="list-disc list-inside">
                                    <Skeleton className="rounded-lg my-1 mx-1 w-3/4"><li className="mx-1" color="primary" variant="shadow">udogodnienia</li></Skeleton>
                                    <Skeleton className="rounded-lg my-1 mx-1 w-4/5"><li className="mx-1" color="primary" variant="shadow">udogodnienia 1</li></Skeleton>
                                    <Skeleton className="rounded-lg my-1 mx-1 w-1/5"><li className="mx-1" color="primary" variant="shadow">udogodnienia 2</li></Skeleton>
                                </ul>
                            </div>
                            <Skeleton className="w-full h-1/3 my-2 rounded-lg"><Button className="w-full my-2 rounded-lg" >Wybierz datę rezerwacji</Button></Skeleton>
                        </CardBody>
                    </Card>
                ))
            }
        </div>
    );
}

const Rating = ({ initialValue = 0, onChange }) => {
    const [value, setValue] = React.useState(initialValue);

    const handleClick = (index) => {
        const newValue = index + 1; // Gwiazdki są indeksowane od 0, więc dodajemy 1
        setValue(newValue);
        if (onChange) {
            onChange(newValue); // Wywołaj funkcję przekazaną jako `onChange`, jeśli istnieje
        }
    };

    const stars = Array.from({ length: 5 }, (_, index) => {
        const fillLevel = Math.min(Math.max(value - index, 0), 1);
        return fillLevel;
    });

    return (
        <div className="flex space-x-1">
            {stars.map((fill, index) => (
                <div
                    key={index}
                    className="relative w-6 h-6 text-yellow-400 cursor-pointer"
                    onClick={() => handleClick(index)}
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
                    {/* Wypełniona gwiazdka */}
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

export default () => {
    const { user } = useOutletContext();
    const navigate = useNavigate();
    const [hotel, setHotel] = React.useState(null);
    const { id } = useParams(); // id jest tutaj nazwą restauracji, wpisując localhost:port/hotels/Jadłodajnia-Ździsia id przyjmie wartość "Jadłodajnia-Ździsia"
    const [imageArr, setImageArr] = React.useState([]);
    const [isWide, setIsWide] = React.useState(window.innerWidth > 847);

    const fetchHotel = async () => {
        instance.get('hotels/' + id)
            .then((res) => {
                setHotel(res.data);
            })
            .catch((err) => {
                console.error(err)
            })
    };

    React.useEffect(() => {
        if (hotel && hotel.images) {
            if (isWide) {
                setImageArr(() => {
                    const newImageArr = [];
                    hotel.images.forEach(image => {
                        const dict = {};
                        dict['url'] = image;
                        dict['desc'] = null;
                        newImageArr.push(dict);
                    });
                    if(hotel.rooms)
                        hotel.rooms.forEach(room => {
                            room.images.forEach(image => {
                                const dict = {};
                                dict['url'] = image;
                                dict['desc'] = room.name;
                                newImageArr.push(dict);
                            });
                        });
                    const last = newImageArr.pop();
                    newImageArr.unshift(last);
                    return newImageArr;
                });
            }
            else {
                setImageArr(() => {
                    const newImageArr = [];
                    hotel.images.forEach(image => {
                        const dict = {};
                        dict['url'] = image;
                        dict['desc'] = null;
                        newImageArr.push(dict);
                    });
                    hotel.rooms.forEach(room => {
                        room.images.forEach(image => {
                            const dict = {};
                            dict['url'] = image;
                            dict['desc'] = room.name;
                            newImageArr.push(dict);
                        });
                    });
                    return newImageArr;
                });
            }
        }
    }, [isWide, hotel]);

    const handleRatingChange = (value) => {
        if (hotel !== null && hotel.name) {
            instance.post('rating/hotel', {
                rating: value,
                objectName: hotel.name
            });
        }
    }

    const deleteHotel = async (hotel) => {
        await instance.delete('hotels/' + hotel)
            .then(res => {
                if (res.status === 200) {
                    navigate("/hotels")
                }
                else toast.error(res.data)
            }).catch(err => console.error(err));
    }

    React.useEffect(() => {
        const handleResize = () => {
            setIsWide(window.innerWidth > 847);
        };
        window.addEventListener('resize', handleResize);
        fetchHotel();

        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);
    return (
        <>
            {
                hotel !== null ?
                (
                    typeof hotel === "object" ?
                        <div>
                            <Helmet>
                                <title>{translations.formatString(translations.title, hotel.name)}</title>
                            </Helmet>
                            <Gallery hotel={hotel} imageArr={imageArr} />
                            <div className="md:m-4 my-4 md:mt-4 mt-16 place-items-center">
                                <h1 className="text-4xl text-black dark:text-white font-bold">{hotel.name}</h1>
                                <h2 className="text-sm text-gray-400">{hotel.address.city}, {hotel.address.road} {hotel.address.houseNumber} {hotel.address.postalCode}, {hotel.address.country}</h2>
                                <Rating initialValue={hotel.rating} onChange={handleRatingChange} />
                                <div className="place-self-center mx-4 py-4 select-none">
                                    {hotel.facilities.map((item, index) => (
                                        <Chip className="mx-1" color="primary" key={index} variant="shadow">{translations[item]}</Chip>
                                    ))}
                                </div>
                                {user && user.email && user.email === hotel.ownerEmail ?
                                    <div className="w-full md:px-44">
                                        <Button
                                            variant="ghost"
                                            className="w-full"
                                            onPress={() => {
                                                const description = encodeURIComponent(hotel.description);
                                                const facilities = encodeURIComponent(hotel.facilities.join(','));
                                                const address = encodeURIComponent(`${hotel.address.road} ${hotel.address.houseNumber} ${hotel.address.postalCode} ${hotel.address.city} ${hotel.address.country}`);

                                                navigate({
                                                    pathname: '/hotels/edit',
                                                    search: `?name=${encodeURIComponent(hotel.name)}&checkIn=${encodeURIComponent(hotel.checkIn)}&checkOut=${encodeURIComponent(hotel.checkOut)}&address=${address}&description=${description}&facilities=${facilities}&currency=${hotel.currency}`,
                                                });
                                            }}
                                        >
                                            {translations.Edit}
                                        </Button>
                                        <Popover className="w-full" key="opaque" showArrow backdrop="opaque" offset={10} placement="bottom">
                                            <PopoverTrigger>
                                                <Button className="capitalize w-full mt-2" color="danger" variant="ghost">
                                                    {translations.Delete}
                                                </Button>
                                            </PopoverTrigger>
                                            <PopoverContent className="md:w-[60dvw] w-[90dvw]">
                                                <div className="w-full px-1 py-2">
                                                    <p className="text-small font-bold text-center text-foreground">
                                                        {translations.areYouSure}
                                                    </p>
                                                    <div className="w-full mt-2">
                                                            <Button className="w-full" onPress={async () => await deleteHotel(hotel.name)} variant="ghost" color="danger">{translations.Delete}</Button>
                                                    </div>
                                                </div>
                                            </PopoverContent>
                                        </Popover>
                                        {user && user.email && user.email === hotel.ownerEmail ? <Button
                                            variant="ghost"
                                            color="primary"
                                            className="w-full my-2"
                                            onPress={() => navigate({
                                                pathname: '/hotels/add/rooms',
                                                search: `?hotelName=${hotel.name}&currency=${hotel.currency}`,
                                            })}
                                        >
                                            {translations.addRooms}
                                        </Button> : null}
                                    </div>
                                    : null
                                }
                            </div>
                            <div className="my-4 md:mx-48 text-black dark:text-white">
                                {hotel.description.split("\n").map((e, index) => (
                                    <div className="pt-2" key={index}>
                                        <h3>{e}</h3>
                                    </div>
                                ))}
                            </div>
                            <Rooms id={id} hotel={hotel} />
                        </div>
                        :
                        <div className="w-full h-[95vh] text-center place-content-center">
                            <Helmet>
                                <title>{translations.formatString(translations.title, translations.notFound)}</title>
                            </Helmet>
                            <h1 className="text-3xl font-semibold mb-4">{translations.formatString(translations.error, translations.notFound)}</h1>
                            <h3>{translations.pageYoureTryingToGetIntoDoesNotExist}</h3>
                        </div>
                )
                    : <Spinner className="w-full h-[95dvh]" label={translations.laoding} />
            }
            <Footer />
        </>
    )
}