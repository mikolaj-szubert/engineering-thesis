import { useState, useEffect } from 'react'
import { instance } from '../Helpers'
import { Accordion, AccordionItem, Button, Divider } from '@nextui-org/react'
import { translations } from '../lang'
import { Helmet } from 'react-helmet'

export default () => {
    const [data, setData] = useState(null);
    const [err, setErr] = useState(null);
    const [isLoading, setIsLoading] = useState(null);
    useEffect(() => {
        instance.get('reservations/owned/restaurant')
            .then(res => {
                if (res.status === 204) setErr(translations.noReservations)
                else setData(res.data);
            })
            .catch(err => console.error(err));
    }, []);
    const deleteReservation = async (reservationNumber) => {
        setIsLoading(reservationNumber);
        const url = 'reservations/restaurant/' + reservationNumber;
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
    if (!data && !err)
        return <p className="w-full p-12 text-center text-lg">{translations.errorOccured}</p>;
    else if (!data && err)
        return <p className="w-full p-12 text-center text-lg">{err}</p>;
    else
        return (
            <>
                <Helmet>
                    <title>{translations.formatString(translations.title, "Rezerwacje w moich Hotelach")}</title>
                </Helmet>
                <Accordion variant="splitted" selectionBehavior="toggle" defaultExpandedKeys={["0"]} className="mt-2">
                    {data.map((item, index) => (
                        <AccordionItem key={index} aria-label={"Nr: " + item.reservationNumber} title={"Nr: " + item.reservationNumber}>
                            <Divider className="-mt-4 mb-2" />
                            <p className="dark:text-white text-black">{translations.restaurantName}: {item.restaurantName}</p>
                            <p className="dark:text-white text-black">{String(translations.table).charAt(0).toUpperCase() + String(translations.table).slice(1)}: {item.name}</p>
                            <p className="dark:text-white text-black">{translations.date}: {item.date}</p>
                            {item.notes && <p className="dark:text-white text-black">{translations.notes}: {item.notes}</p>}
                            <Button className="text-white rounded-full w-full my-2 bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" onPress={() => deleteReservation(item.reservationNumber)} isLoading={isLoading === item.reservationNumber}>{translations.cancel}</Button>
                        </AccordionItem>
                    ))}
                </Accordion>
            </>
        );
}