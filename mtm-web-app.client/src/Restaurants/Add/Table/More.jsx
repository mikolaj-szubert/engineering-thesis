import { Input } from '@nextui-org/react'
import { translations } from '../../../lang'

export default ({ data, setData }) => {

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
                min="1"
                type="number"
                name="personCount"
                value={data.personCount}
                onChange={handleIntChange}
                label={translations.maxPeople}
                placeholder={translations.typeMaxPeopleT} />
            <Input
                autocomplete={false}
                min="1"
                className="my-4"
                type="number"
                name="numberOfGivenTables"
                value={data.numberOfGivenTables}
                onChange={handleIntChange}
                label={translations.numberOfGivenTables}
                placeholder={translations.typeNumberOfGivenTables} />
        </div>
    );
}