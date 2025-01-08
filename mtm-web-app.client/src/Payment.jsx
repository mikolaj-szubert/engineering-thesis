import { useEffect, useState } from 'react'
import { Helmet } from "react-helmet"
import { useSearchParams, useNavigate } from 'react-router-dom'
import { instance } from './Helpers'
import { translations } from './lang'
import { parseDate } from '@internationalized/date'
import './CustomStyles/payment.css'

const CardDetails = ({ setIsReadyToProceed }) => {
    const [num, setNum] = useState('');
    const [expM, setExpM] = useState('');
    const [expY, setExpY] = useState('');
    const [cvv, setCvv] = useState('');
    const [name, setName] = useState('');
    const [payByCash, setPayByCash] = useState(false);
    const handleNumChange = (e) => {
        if (!isNaN(e.target.value) && e.target.value.length <= 16) {
            setNum(e.target.value);
        }
    }
    const handleExpM = (e) => {
        const val = e.target.value;
        if (val === "" || !isNaN(val) && val.length <= 2 && parseInt(val) <= 12)
            setExpM(val);
    }
    const handleExpY = (e) => {
        const val = e.target.value;
        const d = new Date();
        const year = d.getFullYear();
        if (val === "" || !isNaN(val) && val.length <= 4)
            if (val.length != 4 || parseInt(val) >= year)
                setExpY(val);
    }
    const handleCVV = (e) => {
        const val = e.target.value;
        if (val === "" || !isNaN(val) && val.length <= 3)
            setCvv(val);
    }
    useEffect(() => {
        const canProceed = (num.length === 16 && expM.length === 2 && expY.length === 4 && cvv.length === 3 && name.length > 0) || payByCash;
        setIsReadyToProceed(canProceed);
    }, [num, expM, expY, cvv, name, payByCash]);
    return (
        <div className="bg-white">
            <span className="text-sm block w-full mt-2">Card Number</span>
            <input className={`${payByCash ? "bg-gray-300" : "bg-white"} border block w-full rounded-md`} autoComplete="cc-number" type="text" value={num} onChange={handleNumChange} disabled={payByCash === true ? true : false} />
            <span className="text-sm block w-full mt-2">MM/YYYY</span>
            <input className={`${payByCash ? "bg-gray-300" : "bg-white"} w-[49%] border rounded-md text-right`} autoComplete="cc-exp-month" type="text" value={expM} onChange={handleExpM} disabled={payByCash === true ? true : false} />
            <span className="inline-block text-sm bg-white w-[2%]">/</span>
            <input className={`${payByCash ? "bg-gray-300" : "bg-white"} w-[49%] border rounded-md`} autoComplete="cc-exp-year" type="text" value={expY} onChange={handleExpY} disabled={payByCash === true ? true : false} />
            <span className="text-sm block w-full mt-2">CVV</span>
            <input className={`${payByCash ? "bg-gray-300" : "bg-white"} border block w-full rounded-md`} autoComplete="cc-csc" type="text" value={cvv} onChange={handleCVV} disabled={payByCash === true ? true : false} />
            <span className="text-sm block w-full mt-2">Full Name</span>
            <input className={`${payByCash ? "bg-gray-300" : "bg-white"} border block w-full rounded-md`} autoComplete="cc-name" type="text" value={name} onChange={e => setName(e.target.value)} disabled={payByCash === true ? true : false} />
            <span className="text-sm block w-full mt-2">or</span>
            <label className="text-sm mt-2">Pay by cash <input className="appearance-none -mb-1 ml-1 bg-white border border-gray-400 rounded-md w-5 h-5 checked:bg-blue-500 checked:border-blue-500 relative checked:after:content-['✓'] checked:after:text-white checked:after:absolute checked:after:left-1 checked:after:text-sm" autoComplete="off" type="checkbox" checked={payByCash} onChange={() => setPayByCash(prev => !prev)} /></label>
        </div>
    );
}

const Error = () => <>ERROR</>;

const RedirectHotel = ({ resNum }) => {
    const [timeLeft, setTimeLeft] = useState(5);
    const navigate = useNavigate();

    useEffect(() => {
        if (timeLeft === 0) {
            window.open(`/api/reservations/hotel/pdf?reservationNum=${resNum}`);
            navigate("/reservations/hotels");
            return;
        }
        const intervalId = setInterval(() => {
            setTimeLeft(timeLeft - 1);
        }, 1000);

        return () => clearInterval(intervalId);
    }, [timeLeft]);

    return <div>
        <p>RESERVED</p>
        <p>Redirecting to reservation nr {resNum}: {timeLeft}s</p>
    </div>
}

const RedirectRestaurant = ({ resNum }) => {
    const [timeLeft, setTimeLeft] = useState(5);
    const navigate = useNavigate();

    useEffect(() => {
        if (timeLeft === 0) {
            setTimeLeft(0);
            window.open(`/api/reservations/restaurant/pdf?reservationNum=${resNum}`);
            navigate("/reservations/restaurants");
            return;
        }
        const intervalId = setInterval(() => {
            setTimeLeft(timeLeft - 1);
        }, 1000);

        return () => clearInterval(intervalId);
    }, [timeLeft]);

    return <div>
        <p>RESERVED</p>
        <p>Redirecting to reservation nr {resNum}: {timeLeft}s</p>
    </div>
}

const Hotel = ({ hotel, room, lang, curr, start, end }) => {
    const [data, setData] = useState();
    const [txt, setTxt] = useState("");
    const [reservationNumber, setReservationNumber] = useState();
    const [err, setErr] = useState(null);
    const [isReserved, setReserved] = useState(null);
    const [isReadyToProceed, setIsReadyToProceed] = useState(false);

    const fetchRoom = async () => {
        instance.get("hotels/" + hotel + "?currency=" + curr, { headers: {'Accept-Language':lang} })
            .then((res) => {
                if (res.status === 200) {
                    const result = res.data.rooms.find(o => o.name === room);
                    if (result) {
                        setErr(false);
                        setData(result);
                    }
                    else {
                        setErr(true);
                    }
                }
                else {
                    setErr(true);
                }
            })
            .catch((err) => { console.error(err); setErr(true); })
    }

    const reserve = () => {
        instance.post("reservations/hotel", {
            startDate: start,
            endDate: end,
            hotelName: hotel,
            roomName: room,
            notes: txt === "" ? null : txt
        }, { headers: { 'Accept-Language': lang } })
            .then((res) => {
                if (res.status === 200) {
                    setReservationNumber(res.data.number)
                    setReserved(true);
                }
                else {
                    setReserved(false);
                }
            })
            .catch((err) => { console.error(err); setReserved(false); })
    }

    useEffect(() => {
        fetchRoom()
    }, []);

    if (isReserved === true) {
        return <RedirectHotel resNum={reservationNumber} />
    }

    if (err !== null && err === false) {
        return (
            <div className="w-4/5 mx-auto pt-8">
                <p className="text-lg font-semibold">Your reservation:</p>
                <p>{hotel}, {room}</p>
                <p>Price: {parseFloat(data.price.replace(",", ".")) * parseDate(end).compare(parseDate(start))} {curr}</p>
                <p>Start: {start}</p>
                <p>End: {end}</p>
                <CardDetails setIsReadyToProceed={setIsReadyToProceed} />
                <textarea value={txt} onChange={(e) => { setTxt(e.target.value) }} placeholder="Reservation notes" className="bg-white border-1 rounded-md w-full my-4 h-[20dvh]" /><br />
                <button onClick={() => reserve()} disabled={!isReadyToProceed} className={`${isReadyToProceed ? "bg-sky-700 text-white" : "bg-gray-300 text-black"} rounded-full px-4 py-2 w-full`} as="button">Reserve</button>
            </div>
        );
    }

    if ((err !== null && err === true) || isReserved === false) {
        return <Error />;
    }

    return <>LOADING</>;
}

const Restaurant = ({ restaurant, table, lang, curr, date }) => {
    const [data, setData] = useState();
    const [txt, setTxt] = useState("");
    const [reservationNumber, setReservationNumber] = useState();
    const [err, setErr] = useState(null);
    const [isReserved, setReserved] = useState(null);
    const [isReadyToProceed, setIsReadyToProceed] = useState(false);

    const fetchRoom = async () => {
        instance.get("restaurants/" + restaurant + "?currency=" + curr, { headers: { 'Accept-Language': lang } })
            .then((res) => {
                if (res.status === 200) {
                    const result = res.data.tables.find(o => o.name === table);
                    if (result) {
                        setErr(false);
                        setData(result);
                    }
                    else {
                        setErr(true);
                    }
                }
                else {
                    setErr(true);
                }
            })
            .catch((err) => { console.error(err); setErr(true); })
    }

    const reserve = () => {
        instance.post("reservations/restaurant", {
            date: date,
            restaurantName: restaurant,
            tableName: table,
            notes: txt === "" ? null : txt
        }, { headers: { 'Accept-Language': lang } })
            .then((res) => {
                if (res.status === 200) {
                    setReservationNumber(res.data.number)
                    setReserved(true);
                }
                else {
                    setReserved(false);
                }
            })
            .catch((err) => { console.error(err); setReserved(false); })
    }

    useEffect(() => {
        fetchRoom()
    }, []);

    if (isReserved === true) {
        return <RedirectRestaurant resNum={reservationNumber} />
    }

    if (err !== null && err === false) {
        return (
            <div className="w-4/5 mx-auto pt-8">
                <p className="text-lg font-semibold">Your reservation:</p>
                <p>{restaurant}, {table}</p>
                <p>Price: {data.price}</p>
                <p>Date: {date}</p>
                <CardDetails setIsReadyToProceed={setIsReadyToProceed} />
                <textarea value={txt} onChange={(e) => { setTxt(e.target.value) }} placeholder="Uwagi do rezerwacji" className="bg-white border-1 rounded-md w-full my-4 h-[20dvh]" /><br />
                <button onClick={() => reserve()} disabled={!isReadyToProceed} className={`${isReadyToProceed ? "bg-sky-700 text-white" : "bg-gray-300 text-black"} rounded-full px-4 py-2 w-full`} as="button">Reserve</button>
            </div>
        );
    }

    if ((err !== null && err === true) || isReserved === false) {
        return <Error />;
    }

    return <>LOADING</>;
}

export default function Payment() {
    const [searchParams,] = useSearchParams();

    const lang = translations.getLanguage()
    const curr = searchParams.get("currency");
    const hotel = searchParams.get("hotel");
    const room = searchParams.get("room");
    const restaurant = searchParams.get("restaurant");
    const table = searchParams.get("table");
    const startDate = searchParams.get("start");
    const endDate = searchParams.get("end");
    const date = searchParams.get("date");

    const isCurrValid = curr && (curr === "PLN" || curr === "GBP" || curr === "EUR" || curr === "USD" || curr === "CAD" || curr === "AUD" || curr === "JPY" || curr === "INR" || curr === "NZD" || curr === "CHF");
    const isHotel = hotel && hotel !== "" && room && room !== "" && startDate && startDate !== "" && endDate && endDate !== "" && isCurrValid;
    const isRestaurant = restaurant && restaurant !== "" && table && table !== "" && date && date !== "" && isCurrValid;

    return(
        <div className="w-full h-[95vh] text-center place-content-center">
            <Helmet>
                <title>Payment</title>
            </Helmet>
            <div className="bg-white h-screen w-screen text-black">
                {isHotel && !isRestaurant ?
                    <Hotel hotel={hotel} room={room} lang={lang} curr={curr} start={startDate} end={endDate} />
                : isRestaurant && !isHotel ?
                        <Restaurant restaurant={restaurant} table={table} lang={lang} curr={curr} date={date} />
                    :
                    <Error />
                }
            </div>
        </div>
    )
}