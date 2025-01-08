import Footer from '../Footer.jsx';
import { Helmet } from 'react-helmet'

export default function About() {
    return (
        <div>
            <Helmet>
                <title>O Nas | MTM Project</title>
            </Helmet>
            <div className="mx-8 sm:mx-16 md:mx-32 lg:mx-64 xl:mx-[28dvw] text-justify text-black dark:text-white">
                <h1 className="text-3xl my-4 font-bold text-center">O Nas</h1>

                <h5 className=" text-md mb-4">Witamy w MTM Project – firmie, która zmienia oblicze branży turystycznej poprzez innowacyjne rozwiązania w zakresie rezerwacji hoteli i stolików w restauracjach oraz bezpiecznych płatności. Nasza misja to uprościć i ułatwić życie naszym klientom, łącząc technologię z wygodą i komfortem.</h5 >

                <h3 className="text-lg mt-8 mb-2 font-bold">Nasza historia</h3>
                <h5 className=" text-md mb-4">MTM Project powstało z pasji do podróży i odkrywania nowych miejsc. Widzieliśmy, jak wiele można poprawić w procesie rezerwacji oraz w korzystaniu z usług hotelowych i gastronomicznych. W odpowiedzi na te potrzeby stworzyliśmy platformę, która integruje nowoczesne technologie, aby zapewnić naszym klientom wyjątkowe doświadczenia. Wierzymy, że każdy zasługuje na komfort i swobodę podczas podróży.</h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">Zespół MTM Project</h3>
                <h5 className=" text-md mb-4">Nasz zespół to grupa pasjonatów z różnych dziedzin, którzy łączą swoje umiejętności i doświadczenia, aby dostarczać najlepsze usługi. Pracujemy z myślą o klientach, wsłuchując się w ich potrzeby i dostosowując nasze rozwiązania do ich oczekiwań.</h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">Dołącz do nas</h3>
                <h5 className=" text-md mb-4">Zachęcamy do odkrycia możliwości, jakie daje MTM Project. Jesteśmy otwarci na współpracę z obiektami hotelowymi, restauracjami oraz innymi partnerami, którzy pragną wprowadzać innowacje w swojej działalności. Razem możemy stworzyć coś wyjątkowego!</h5>

                <h3 className="text-md mt-8 mb-2 font-bold">Nasze wartości</h3>
                <ul className="ml-4 text-md list-disc">
                    <li>Innowacyjność: Nie boimy się wyzwań. Stale poszukujemy nowych rozwiązań, które będą odpowiadały na potrzeby naszych użytkowników. Dzięki technologii otwierania drzwi za pomocą kodu QR, redefiniujemy standardy gościnności.</li>
                    <li>Komfort: Dążymy do tego, aby każdy aspekt korzystania z naszych usług był prosty i intuicyjny. Od rezerwacji po płatności – zależy nam na tym, abyś mógł cieszyć się swoim czasem bez zbędnych komplikacji.</li>
                    <li>Zaufanie: Bezpieczeństwo naszych klientów jest dla nas priorytetem. Oferujemy sprawdzone i bezpieczne metody płatności, abyś mógł z pełnym spokojem korzystać z naszych usług.</li>
                </ul>

                <h5 className="text-md my-4">Przekonaj się, jak łatwo możemy razem stworzyć coś wyjątkowego. Dołącz do MTM Project – wspólnie przeniesiemy branżę rezerwacji i płatności na wyższy poziom!</h5>
            </div>
            <Footer />
        </div>);
}