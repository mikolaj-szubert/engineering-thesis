import React from 'react'
import { Button, Select, SelectItem, TimeInput } from '@nextui-org/react'
import { translations } from '../../lang'
import { parseTime } from '@internationalized/date'

export default ({ data, setData, openDays, setOpenDays }) => {
    const [inTime, setInTime] = React.useState(parseTime('10:00'));
    const [outTime, setOutTime] = React.useState(parseTime('22:00'));
    const [form, setForm] = React.useState({
        dayOfWeek: "",
        openingTime: "10:00",
        closingTime: "22:00",
    });

    const allFacilities = [
        'Polish',
        'Italian',
        'Chinese',
        'Indian',
        'Mexican',
        'French',
        'Japanese',
        'Thai',
        'Spanish',
        'Greek',
        'American',
        'Moroccan',
        'Lebanese',
        'Brazilian',
        'Turkish',
        'Russian',
        'Indonesian',
        'Vietnamese',
        'Korean',
        'Ethiopian',
        'Filipino',
        'Malaysian',
        'Scandinavian',
        'German',
        'Dutch',
        'British',
        'South_African',
        'Pakistani',
        'Peruvian',
        'Australian',
        'Argentine',
        'Egyptian',
        'Hungarian',
        'Irish',
        'Caribbean',
        'Bangladeshi',
        'Ukrainian',
        'Jewish',
        'Basque',
        'Finnish',
        'Chilean',
        'Afghan',
        'Tibetan',
        'Cambodian',
        'Singaporean',
        'Sri_Lankan',
        'Icelandic',
        'Scottish',
        'Maltese',
        'Belarusian',
        'Welsh',
        'Zambian',
        'Omani',
        'Kuwaiti',
        'Syrian',
        'Albanian',
        'Bantu',
        'Hawaiian',
        'Uzbek',
        'Azerbaijani',
        'Georgian',
        'Estonian',
        'Latvian',
        'Lithuanian',
        'Bhutanese',
        'Samoan',
        'Tongan',
        'Fijian',
        'Marshallese',
        'Papua_New_Guinean',
        'Micronesian',
        'Greenlandic',
        'Andorran',
        'Surinamese',
        'Guyanese',
        'Trinidadian',
        'Mauritian',
        'Seychellois',
        'Tatar',
        'Chuvash',
        'Yakut',
        'Buryat',
        'Breton',
        'Corsican',
        'Catalan',
        'Walloon',
        'Aragonese',
        'Galician',
        'Moldovan',
        'Panamanian',
        'Salvadoran',
        'Nicaraguan',
        'Costa_Rican',
        'Dominican',
        'Paraguayan',
        'Uruguayan',
        'Ecuadorean',
        'Bolivian',
        'Venezuelan',
        'Haitian',
        'Jamaican',
        'Northern_Irish',
        'Turkish_Cypriot',
        'Greek_Cypriot'
    ];

    const changeInTime = (e) => {
        let time = e.toString().split(":");
        time.pop();
        time = time.join(":");
        setInTime(e);
        setForm((prev) =>
        ({
            ...prev,
            openingTime: time
        }));
    };

    const changeOutTime = (e) => {
        let time = e.toString().split(":");
        time.pop();
        time = time.join(":");
        setOutTime(e);
        setForm((prev) =>
        ({
            ...prev,
            closingTime: time
        }));
    };

    const handleFacilitiesChange = (e) => {
        const arr = e.target.value.split(",");
        setData({
            ...data,
            [e.target.name]: arr.filter(item => item !== '')
        });
    };

    const handleOpenDayChange = (e) => {
        const { name, value } = e.target;
        setForm((prev) => ({ ...prev, [name]: value }));
    };

    const addOpenDay = (newDay) => {
        if(form.dayOfWeek !== '')
            setOpenDays((prev) => {
                if (prev.some((day) => day.dayOfWeek === newDay.dayOfWeek)) {
                    return prev;
                }
                return [...prev, newDay];
            });
    };

    React.useEffect(() => {
        setOpenDays(prev => {
            if(translations.getLanguage() === "pl") return prev.sort((a, b) => ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].indexOf(a.dayOfWeek) - ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].indexOf(b.dayOfWeek));
            else return prev.sort((a, b) => ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].indexOf(a.dayOfWeek) - ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].indexOf(b.dayOfWeek));
        });
        const result = translations.getLanguage() === "pl" ? ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].filter(day => !openDays.map((item) => item.dayOfWeek).includes(day)) : ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].filter(day => !openDays.map((item) => item.dayOfWeek).includes(day));
        setForm({
            dayOfWeek: result.length > 0 ? result[0] : "",
            openingTime: "10:00",
            closingTime: "22:00",
        });
    }, [openDays]);

    const removeOpenDay = (dayOfWeek) => {
        setOpenDays((prev) => prev.filter((day) => day.dayOfWeek !== dayOfWeek));
    };

    return (
        <div className="w-full pt-24 px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96">
            <Select
                className="mb-4"
                classNames={{
                    listbox: "text-black dark:text-white"
                }}
                name="cusines"
                selectionMode="multiple"
                labelPlacement="inside"
                label={translations.cuisines}
                placeholder={translations.selectCuisines}
                selectedKeys={data.cusines}
                onChange={handleFacilitiesChange}
            >
                {allFacilities.map((facility) => (
                    <SelectItem key={facility}>
                        {translations[facility]}
                    </SelectItem>
                ))}
            </Select>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
                <Select
                    className="min-w-[150px]"
                    classNames={{
                        listbox: "text-black dark:text-white"
                    }}
                    name="dayOfWeek"
                    label={translations.dayOfWeek}
                    placeholder={translations.selectDayOfWeek}
                    selectedKeys={[form.dayOfWeek]}
                    labelPlacement="inside"
                    onChange={handleOpenDayChange}
                    disabledKeys={openDays.map((item) => item.dayOfWeek)}
                >
                    {(translations.getLanguage() === "pl" ? ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"] : ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]).map(day => (
                        <SelectItem key={day}>
                            {translations[day]}
                        </SelectItem>
                    ))}
                </Select>
                <TimeInput
                    className="min-w-[150px]"
                    hideTimeZone
                    name="openingTime"
                    onChange={changeInTime}
                    value={inTime}
                    label={translations.checkIn}
                />
                <TimeInput
                    className="min-w-[150px]"
                    hideTimeZone
                    name="closingTime"
                    onChange={changeOutTime}
                    value={outTime}
                    label={translations.checkOut}
                />
                <Button
                    className="min-w-[150px] h-full"
                    onPress={() => addOpenDay(form)}
                >
                    Add
                </Button>
            </div>
            {openDays.map((item, index) => (
                <p className="cursor-pointer w-fit hover:line-through" onClick={() => { removeOpenDay(item.dayOfWeek) }} key={index}>{translations[item.dayOfWeek]} {item.openingTime} - {item.closingTime}</p>
            ))}
        </div>
    );
}