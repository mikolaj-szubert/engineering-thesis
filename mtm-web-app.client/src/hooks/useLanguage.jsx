import { useState, useEffect } from 'react';
import axios from 'axios';
import { instance } from '../Helpers';
import { translations } from '../lang';

const useLanguage = () => {
    const browserLocale = navigator.language || 'en';
    const [language, setLanguage] = useState(localStorage.getItem('language') || browserLocale.slice(0, 2));

    const handleLanguageChange = (newLang) => {
        translations.setLanguage(newLang);
        axios.defaults.headers.common['Accept-Language'] = newLang;
        instance.defaults.headers.common['Accept-Language'] = newLang;
        setLanguage(newLang);
        localStorage.setItem('language', newLang);
    };

    useEffect(() => {
        translations.setLanguage(language);
        axios.defaults.headers.common['Accept-Language'] = language;
        instance.defaults.headers.common['Accept-Language'] = language;
    }, [language]);

    return { language, handleLanguageChange };
};

export default useLanguage;