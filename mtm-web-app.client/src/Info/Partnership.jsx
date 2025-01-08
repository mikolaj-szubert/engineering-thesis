import Footer from '../Footer.jsx'
import { NavLink } from 'react-router-dom'
import { Helmet } from 'react-helmet'

export default function Partnership() {
    return (
        <div>
            <Helmet>
                <title>Współpraca | MTM Project</title>
            </Helmet>
            <div className="mx-8 sm:mx-16 md:mx-32 lg:mx-64 xl:mx-[28dvw] text-justify text-black dark:text-white">
                <h1 className="text-3xl my-4 font-bold text-center">Dołącz do innowacji z MTM Project – razem zmieniamy rynek!</h1>

                <h5 className=" text-md mb-4">Witaj w MTM Project – miejscu, gdzie technologia łączy się z wygodą, a innowacyjne rozwiązania wyznaczają nowe standardy. Jesteśmy dynamicznie rozwijającą się firmą specjalizującą się w pośrednictwie w rezerwacji hoteli i stolików w restauracjach oraz w obszarze bezpiecznych płatności. Ale to, co nas wyróżnia, to nasze nieszablonowe podejście do tworzenia komfortowych, nowoczesnych rozwiązań. Dzięki zastosowaniu najnowszych technologii oraz niespotykanych dotąd opcji, w MTM Project oferujemy gościom wyjątkową wygodę, a partnerom – realną przewagę konkurencyjną.</h5 >

                <h5 className=" text-md mb-4">Wyobraź sobie, że jesteś w podróży – niezależnie, czy służbowo, czy prywatnie – i już od momentu rezerwacji aż po zameldowanie czujesz się pewnie, bezpiecznie i komfortowo. Dzięki naszym rozwiązaniom drzwi do hotelu otworzysz za pomocą skanu QR, a miejsce w ulubionej restauracji zarezerwujesz w kilka sekund, bez telefonów, bez stresu i bez kolejek. To więcej niż rezerwacja – to całkowicie nowe doświadczenie podróży i korzystania z usług, w którym to Ty jesteś w centrum, a reszta po prostu działa.</h5>

                <h5 className=" text-md mb-4">W MTM Project nie tylko tworzymy technologię. Tworzymy przyszłość rezerwacji, płatności i komfortu – z myślą o wszystkich, którzy cenią swój czas, lubią swobodę i chcą korzystać z najnowocześniejszych narzędzi na rynku. Wiemy, że innowacje mają sens tylko wtedy, gdy przynoszą realne korzyści, dlatego nieustannie udoskonalamy nasze rozwiązania, aby odpowiadać na zmieniające się potrzeby naszych użytkowników i partnerów.</h5>
                                
                <h3 className="text-md mt-8 mb-2 font-bold">Dlaczego warto współpracować z MTM Project?</h3>
                <ul className="ml-4 text-md list-disc">
                    <li>Innowacja i prostota: Nasze technologie eliminują zbędne kroki. Otwieranie drzwi hotelowych przez QR? Tak, z nami to możliwe!</li>
                    <li>Nowa jakość komfortu: Oferujemy rozwiązania, które po prostu działają – bez zbędnych formalności, kolejek i zamieszania.</li>
                    <li>Przewaga konkurencyjna: Współpraca z MTM Project to inwestycja w technologię, która wprowadza Twoją ofertę na zupełnie nowy poziom.</li>
                    <li>Elastyczność i bezpieczeństwo płatności: Obsługujemy transakcje szybko, intuicyjnie i bezpiecznie – zarówno dla gości, jak i właścicieli obiektów.</li>
                    <li>Odpowiedzialność i wsparcie: Każdy nasz partner to dla nas priorytet. Zapewniamy pełne wsparcie techniczne i doradcze, byś mógł skupić się na tym, co najważniejsze – rozwoju swojej działalności.</li>
                </ul>

                <h5 className="text-md my-4">Przekonaj się, jak łatwo możemy razem stworzyć coś wyjątkowego. Dodaj swój <NavLink className="text-gradient-zielony" to="/hotels/add">hotel</NavLink> lub <NavLink className="text-gradient-zielony" to="/restaurants/add">restaurację</NavLink> – wspólnie przeniesiemy branżę rezerwacji i płatności na wyższy poziom!</h5>
            </div>
            <Footer />
        </div>);
}