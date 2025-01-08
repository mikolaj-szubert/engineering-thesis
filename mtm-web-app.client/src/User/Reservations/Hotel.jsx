import { useEffect, useState } from 'react'
import { instance } from '../../Helpers'
import { translations } from '../../lang'
import { Accordion, AccordionItem, Button, Divider, Spinner } from '@nextui-org/react'
import { NavLink } from 'react-router-dom'
import { toast } from 'react-toastify'

export default () => {
    const [data, setData] = useState(null);
    const [isLoading, setIsLoading] = useState(null);

    const fetchReservations = async () => {
        instance.get('reservations/hotel')
            .then((res) => {
                setData(res.data)
            })
            .catch(err => console.error(err));
    }

    const deleteReservation = async (reservationNumber) => {
        setIsLoading(reservationNumber);
        const url = 'reservations/hotel/' + reservationNumber;
        await instance.delete(url)
            .then((res) => {
                if (res.status === 200) {
                    setData(prev => prev.filter(i => i.reservationNumber !== reservationNumber));
                    toast.success(translations.reservationDeleted);
                }
                else {
                    toast.success(translations.deletingError);
                }
            })
            .catch(err => console.error(err))
            .finally(() => setIsLoading(null));
    }

    useEffect(() => {
        fetchReservations();
    }, []);

    if (data === null)
        return <Spinner className="w-full h-[95dvh]" label={translations.laoding} />;

    if (typeof data === "object" && data.length > 0) {
        return (
            <Accordion className="mt-2" variant="splitted" selectionBehavior="toggle" defaultExpandedKeys={["0"]}>
                {data.map((item, index) => (
                    <AccordionItem key={index} aria-label={item.hotelName} className="relative" title={<>{translations.ReservationNumber}: {item.reservationNumber}</>}>
                        <Divider className="-mt-2 mb-2" />
                        <a className="text-gradient-zielony" href={"/api/reservations/hotel/pdf?reservationNum=" + item.reservationNumber} target="_blank">{translations.seeReservation}</a>
                        <p className="dark:text-white text-black"><strong>{translations.hotelName}:</strong> <NavLink className="text-gradient-zielony text-base" to={"/hotels/" + item.hotelName}>{item.hotelName}</NavLink></p>
                        <p className="dark:text-white text-black"><strong>{translations.dateRange}:</strong> {item.from} - {item.to}</p>
                        <p className="dark:text-white text-black"><strong>{translations.numberOfPeople}:</strong> {item.peopleCount}</p>
                        <p className="dark:text-white text-black"><strong>{translations.total}:</strong> {item.summaryCost} {item.currency}</p>
                        <Button className="text-white rounded-full w-full my-2 bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" onPress={() => deleteReservation(item.reservationNumber)} isLoading={isLoading === item.reservationNumber}>{translations.cancel}</Button>
                    </AccordionItem>
                ))}
            </Accordion>
        )
    }

    else return <p className="m-12">{translations.noReservations}.</p>;
}