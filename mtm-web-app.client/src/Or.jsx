//ekran błędu 404 - brak strony
import React from 'react';
import { translations } from './lang';
export default ({ text = translations.or }) => (
    <div className="flex items-center my-4">
        <div className="flex-grow border-t border-gray-300"></div>
        <span className="mx-4 text-gray-500 font-semibold">{text}</span>
        <div className="flex-grow border-t border-gray-300"></div>
    </div>
);