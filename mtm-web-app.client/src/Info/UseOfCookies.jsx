import Footer from '../Footer.jsx';
import { Helmet } from 'react-helmet'

export default function UseOfCookies() {
    return (
        <div>
            <Helmet>
                <title>Korzystanie z Plików Cookie | MTM Project</title>
            </Helmet>
            <div className="mx-8 sm:mx-16 md:mx-32 lg:mx-64 xl:mx-[28dvw] text-justify text-black dark:text-white">
                <h1 className="text-3xl my-4 font-bold text-center">Korzystanie z Plików Cookie</h1>

                <h3 className="text-lg mt-8 mb-2 font-bold">1. Czym są pliki cookie?</h3>
                <h5 className="text-md mb-4">
                Pliki cookie to niewielkie pliki tekstowe zapisywane na Twoim urządzeniu (komputerze, tablecie, telefonie) podczas korzystania z naszej strony internetowej. Pliki cookie mogą być odczytywane zarówno przez naszą stronę, jak i przez podmioty trzecie, które świadczą nam różnorodne usługi.
                </h5 >

                <h3 className="text-lg mt-8 mb-2 font-bold">2. Jakie rodzaje plików cookie wykorzystujemy?</h3 >
                <h5 className="text-md mb-4">Nasza strona korzysta z różnych rodzajów plików cookie, aby zapewnić jej prawidłowe funkcjonowanie oraz ulepszać Twoje doświadczenie użytkownika. Możemy korzystać z następujących typów plików cookie:
                    <ul className="ml-4 text-md list-disc">
                        <li>Niezbędne pliki cookie: Są to pliki niezbędne do funkcjonowania naszej strony, umożliwiające m.in. logowanie się i dokonywanie rezerwacji. Bez tych plików strona nie będzie działała poprawnie.</li>
                        <li>Analityczne pliki cookie: Służą do zbierania informacji o sposobie korzystania z naszej strony, aby pomóc nam ją ulepszać (np. poprzez analizę odwiedzin, kliknięć i ścieżek nawigacji).</li>
                        <li>Funkcjonalne pliki cookie: Pozwalają na zapamiętywanie Twoich preferencji (np. wybór języka) oraz ustawień, co poprawia Twoje doświadczenie korzystania z naszej strony.</li>
                        <li>Reklamowe pliki cookie: Używane do wyświetlania reklam dostosowanych do Twoich zainteresowań oraz śledzenia efektywności kampanii reklamowych.</li>
                    </ul>
                </h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">3. Pliki cookie podmiotów trzecich</h3>
                <h5 className="text-md mb-4">
                    Na naszej stronie mogą znajdować się pliki cookie pochodzące od podmiotów trzecich, takich jak dostawcy usług analitycznych (np. Google Analytics) lub partnerzy reklamowi. Te pliki cookie mogą zbierać informacje o Twojej aktywności na naszej stronie oraz na innych stronach internetowych.
                </h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">4. Jak zarządzać plikami cookie?</h3 >
                <h5 className="text-md mb-4">Możesz zarządzać ustawieniami plików cookie bezpośrednio w swojej przeglądarce internetowej. Większość przeglądarek internetowych umożliwia:
                    <ul className="ml-4 list-disc">
                        <li>Akceptowanie wszystkich plików cookie.</li>
                        <li>Blokowanie wszystkich plików cookie.</li>
                        <li>Usuwanie plików cookie.</li>
                        <li>Zarządzanie ustawieniami plików cookie dla poszczególnych witryn.</li>
                    </ul>
                    Aby dowiedzieć się więcej o zarządzaniu plikami cookie w swojej przeglądarce, odwiedź sekcję pomocy w przeglądarce lub stronę internetową dostawcy przeglądarki.
                </h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">5. Zmiany w korzystaniu z plików cookie</h3 >
                <h5 className="text-md mb-4">
                    Zastrzegamy sobie prawo do wprowadzania zmian w zasadach korzystania z plików cookie. Wszelkie zmiany będą publikowane na naszej stronie, a użytkownicy zostaną o nich poinformowani w odpowiedni sposób.
                </h5>
            </div>
            <Footer />
        </div>);
}