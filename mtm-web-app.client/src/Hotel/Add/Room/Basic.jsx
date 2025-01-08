import { Input, Textarea } from '@nextui-org/react'
import { translations } from '../../../lang'
import { useSearchParams } from 'react-router-dom'

export default ({ data, setData }) => {
    const [searchParams, ] = useSearchParams();
    const handleChange = (e) => {
        setData({
            ...data,
            [e.target.name]: e.target.value
        });
    };

    const handleFloatChange = (e) => {
        const a = isNaN(parseFloat(e.target.value)) ? '' : parseFloat(e.target.value);
        setData({
            ...data,
            [e.target.name]: a !== '' ? Math.round(a * 100) / 100 : a
        });
    };

    const handleIntChange = (e) => {
        const a = isNaN(parseInt(e.target.value)) ? '' : parseInt(e.target.value);
        setData({
            ...data,
            [e.target.name]: a
        });
    };

    return (
        <div className="w-full pt-24 px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96">
            <Input
                autocomplete={false}
                type="text"
                name="name"
                value={data.name}
                onChange={handleChange}
                label={translations.roomName}
                placeholder={translations.typeRoomName} />
            <Textarea
                autocomplete={false}
                className="my-4"
                name="description"
                value={data.description}
                onChange={handleChange}
                maxRows={5}
                minRows={5}
                placeholder={translations.typeRoomDescription} />
            <Input
                autocomplete={false}
                className="my-4"
                type="number"
                name="price"
                min="0,01"
                value={data.price}
                onChange={handleFloatChange}
                label={translations.roomPrice}
                placeholder={translations.typeRoomPrice}
                endContent={
                    <span className="pointer-events-none text-default-400 text-small">{searchParams.get("currency")}</span>
                } />
            <Input
                autocomplete={false}
                min="1"
                className="my-4"
                type="number"
                name="numberOfGivenRooms"
                value={data.numberOfGivenRooms}
                onChange={handleIntChange}
                label={translations.numberOfGivenRooms}
                placeholder={translations.typeNumberOfGivenRooms} />
        </div>
    );
}