import React from 'react'
import { Select, SelectItem, TimeInput } from '@nextui-org/react'
import { translations } from '../../lang'
import { parseTime } from "@internationalized/date";

export default ({ data, setData }) => {
    const [inTime, setInTime] = React.useState(parseTime(data.checkIn));
    const [outTime, setOutTime] = React.useState(parseTime(data.checkOut));
    const types = [
        'Hotel',
        'Hostel',
        'Apartment',
        'Penthouse',
        'House',
        'Willa',
        'Pension',
    ]
    const allFacilities = [
        'FreeParking',
        'PaidParking',
        'Elevator',
        'WheelchairAccessible',
        'Pool',
        'HotTub',
        'Sauna',
        'FitnessCenter',
        'Spa',
        'MassageService',
        'GameRoom',
        'Playground',
        'Library',
        'Garden',
        'Terrace',
        'BarbecueFacilities',
        'TennisCourt',
        'GolfCourse',
        'WaterSportsFacilities',
        'SkiStorage',
        'SkiInSkiOutAccess',
        'HikingTrails',
        'BicycleRental',
        'HorseRiding',
        'BowlingAlley',
        'AirportShuttle',
        'CarRental',
        'ElectricVehicleCharging',
        'Restaurant',
        'Bar',
        'BreakfastIncluded',
        'Kitchenette',
        'VendingMachine',
        'ConciergeService',
        'Housekeeping',
        'LaundryService',
        'BabysittingService',
        'LuggageStorage',
        'CurrencyExchange',
        'ATMOnSite',
        'TourDesk',
        'TicketService',
        'BusinessCenter',
        'MeetingRooms',
        'MultilingualStaff',
        'Fireplace',
        'SelfCheckIn',
        'EcoFriendly'
    ];

    const handleChange = (e) => {
        setData({
            ...data,
            [e.target.name]: e.target.value
        });
    };

    const handleFacilitiesChange = (e) => {
        const arr = e.target.value.split(",");
        setData({
            ...data,
            [e.target.name]: arr.filter(item => item !== '')
        });
    };

    const changeInTime = (e) => {
        let time = e.toString().split(":");
        time.pop();
        time = time.join(":");
        setInTime(e);
        setData({
            ...data,
            checkIn: time
        });
    };

    const changeOutTime = (e) => {
        let time = e.toString().split(":");
        time.pop();
        time = time.join(":");
        setOutTime(e);
        setData({
            ...data,
            checkOut: time
        });
    };

    return (
        <div className="w-full pt-24 px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96">
            <Select
                className="mb-4"
                classNames={{
                    listbox: "text-black dark:text-white"
                }}
                name="type"
                labelPlacement="inside"
                label={translations.type}
                placeholder={translations.selectType}
                selectedKeys={[data.type]}
                onChange={handleChange}
            >
                {types.map((type) => (
                    <SelectItem key={type}>
                        {type}
                    </SelectItem>
                ))}
            </Select>
            <Select
                className="mb-4"
                classNames={{
                    listbox: "text-black dark:text-white"
                }}
                name="facilities"
                selectionMode="multiple"
                labelPlacement="inside"
                label={translations.facilities}
                placeholder={translations.selectFacilities}
                selectedKeys={data.facilities}
                onChange={handleFacilitiesChange}
            >
                {allFacilities.map((facility) => (
                    <SelectItem key={facility}>
                        {translations[facility]}
                    </SelectItem>
                ))}
            </Select>
            <TimeInput
                hideTimeZone
                name="checkIn"
                onChange={changeInTime}
                value={inTime}
                className="mb-4"
                label={translations.checkIn} />
            <TimeInput
                hideTimeZone 
                name="checkOut"
                onChange={changeOutTime}
                value={outTime}
                className="mb-4"
                label={translations.checkOut} />
        </div>
    );
}