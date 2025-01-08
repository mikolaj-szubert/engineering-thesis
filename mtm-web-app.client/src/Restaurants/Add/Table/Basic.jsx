import { Input, Textarea } from '@nextui-org/react'
import { translations } from '../../../lang'

export default ({ data, setData, restaurantCurr }) => {

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

    return (
        <div className="w-full pt-24 px-2 sm:px-12 md:px-36 lg:px-64 xl:px-96">
            <Input
                autocomplete={false}
                type="text"
                name="name"
                value={data.name}
                onChange={handleChange}
                label={translations.tableName}
                placeholder={translations.typeTableName} />
            <Textarea
                autocomplete={false}
                className="my-4"
                name="description"
                value={data.description}
                onChange={handleChange}
                maxRows={5}
                minRows={5}
                placeholder={translations.typeTableDescription} />
            <Input
                autocomplete={false}
                className="my-4"
                type="number"
                name="price"
                min="0,01"
                value={data.price}
                onChange={handleFloatChange}
                label={translations.tablePrice}
                placeholder={translations.typeTablePrice}
                endContent={
                    <span className="pointer-events-none text-default-400 text-small">{restaurantCurr}</span>
                } />
        </div>
    );
}