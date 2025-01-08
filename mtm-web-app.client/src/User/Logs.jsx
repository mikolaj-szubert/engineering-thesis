import React from 'react'
import { instance } from '../Helpers'
import { translations } from '../lang'
import { Spinner, Divider } from '@nextui-org/react'
import { DateFormatter } from '@internationalized/date'
import { useLocale } from 'react-aria'

export default () => {
    const [data, setData] = React.useState(null);
    const { locale } = useLocale();
    const fetchLogs = async () => {
        instance.get('account/manage/logs')
            .then((res) => {
                setData(res.data);
            })
            .catch((err) => console.error(err));
    }
    React.useEffect(() => {
        fetchLogs();
    }, []);
    function parseAndFormatDateTime(input, locale) {
        let utcDate;
        if (input.includes('.')) {
            const [datePart, timePart] = input.split(' ');
            const [day, month, year] = datePart.split('.').map(Number);
            const [hours, minutes, seconds] = timePart.split(':').map(Number);
            utcDate = new Date(Date.UTC(year, month - 1, day, hours, minutes, seconds));
        } else if (input.includes('/')) {
            utcDate = new Date(input + ' UTC');
        } else {
            throw new Error('Nieobsługiwany format daty.');
        }
        const formatter = new DateFormatter(locale, {
            dateStyle: 'short',
            timeStyle: 'short',
        });
        return formatter.format(utcDate);
    }

    return (
        data === null ?
            <Spinner className="w-full h-[95dvh]" label="Ładowanie..." />
            : typeof data === "object" ?
                data.map((item, index) => (
                    <div className="mx-4" key={index}>
                        <div className="my-8">
                            <h3 className="text-xl font-semibold">{item.logName.split(" ").map((itm) => (
                                translations[itm] + " "
                            ))}</h3>
                            <h3 className="text-base">{item.address} {item.location}</h3>
                            <h5 className="text-xs text-gray-300">{parseAndFormatDateTime(item.time, locale)}</h5>
                        </div>
                        <Divider />
                    </div>
                ))
                : <p className="m-12">{data}</p>
    );
}