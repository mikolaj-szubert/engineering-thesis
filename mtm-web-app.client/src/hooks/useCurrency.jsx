import { useState, useEffect } from 'react';

const useCurrency = () => {
    const [currency, setCurrency] = useState(localStorage.getItem('currency') || 'PLN');

    useEffect(() => {
        localStorage.setItem('currency', currency);
    }, [currency]);

    return { currency, setCurrency };
};

export default useCurrency;