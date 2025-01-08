import { Input, Select, SelectItem, Textarea } from '@nextui-org/react'
import { translations } from '../../lang'

export default ({ data, setData }) => {
    const currencies = [
        'PLN', //Polish Zloty New
        'GBP', //Great Britain Pound
        'EUR', //Euro
        'USD', //United States Dollar
        'CAD', //Canadian dollar
        'AUD', //Australian Dollar
        'JPY', //Japanese Yen
        'INR', //Indian Rupee
        'NZD', //New Zealand Dollar
        'CHF', //Swiss Franc
    ]

    const handleChange = (e) => {
        setData({
            ...data,
            [e.target.name]: e.target.value
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
                label={translations.restaurantName}
                placeholder={translations.typeRestaurantName} />
            <Textarea
                autocomplete={false}
                className="my-4"
                name="description"
                value={data.description}
                onChange={handleChange}
                maxRows={5}
                minRows={5}
                label={translations.restaurantDesc}
                placeholder={translations.typeRestaurantDesc} />
            <Select
                name="currency"
                classNames={{
                    listbox: "text-black dark:text-white"
                }}
                labelPlacement="inside"
                label={translations.restaurantCurr}
                selectedKeys={[data.currency]}
                onChange={handleChange}
            >
                {currencies.map((currency) => (
                    <SelectItem key={currency}>
                        {currency}
                    </SelectItem>
                ))}
            </Select>
        </div>
    );
}