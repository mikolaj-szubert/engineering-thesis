import { Input, Select, SelectItem } from '@nextui-org/react'
import { translations } from '../../../lang'

export default ({ data, setData }) => {
    const types = [
        'Standard',
        'Double',
        'Twin',
        'Family',
        'Studio',
        'Superior',
        'Deluxe',
        'Suite',
        'JuniorSuite',
        'Penthouse',
        'Loft',
        'Dormitory',
        'JacuzziRoom',
        'PoolSuite',
        'Accessible',
        'Connecting',
        'Executive',
        'Business',
        'Villa'
    ]
    const allFacilities = [
        'FreeWiFi',
        'AirConditioning',
        'Heating',
        'NonSmoking',
        'Smoking',
        'PetFriendly',
        'Kitchen',
        'PrivateBathroom',
        'WashingMachine',
        'Dryer',
        'Iron',
        'HairDryer',
        'RoomService',
        'MiniBar',
        'CoffeeMaker',
        'InRoomSafe',
        'FlatScreenTV',
        'Balcony',
        'OceanView',
        'MountainView',
        'BlackoutCurtains',
        'PremiumBedding',
        'FreeHighSpeedInternet',
        'SatelliteTV',
        'StreamingServiceAccess',
        'SmartHomeFeatures',
        'USBChargingPorts'
    ];

    const handleChange = (e) => {
        setData({
            ...data,
            [e.target.name]: e.target.value
        });
    };

    const handleIntChange = (e) => {
        const a = isNaN(parseInt(e.target.value)) ? '' : parseInt(e.target.value);
        setData({
            ...data,
            [e.target.name]: a
        });
    };

    const handleFacilitiesChange = (e) => {
        const arr = e.target.value.split(",");
        setData({
            ...data,
            [e.target.name]: arr.filter(item => item !== '')
        });
    };

    return (
        <div className="w-full pt-24 px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96">
            <Select
                name="roomType"
                classNames={{
                    listbox: "text-black dark:text-white"
                }}
                labelPlacement="inside"
                label={translations.type}
                placeholder={translations.selectType}
                selectedKeys={[data.roomType]}
                onChange={handleChange}
            >
                {types.map((type) => (
                    <SelectItem key={type}>
                        {type}
                    </SelectItem>
                ))}
            </Select>
            <Select
                className="my-4"
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
            <Input
                autocomplete={false}
                min="1"
                type="number"
                name="personCount"
                value={data.personCount}
                onChange={handleIntChange}
                label={translations.maxPeople}
                placeholder={translations.typeMaxPeople} />
        </div>
    );
}