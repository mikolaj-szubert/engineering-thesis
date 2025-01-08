import { Link } from 'react-router-dom';
import Footer from '../Footer.jsx';
import { Helmet } from 'react-helmet'

export default function TermsOfUse() {
    return (
        <div>
            <Helmet>
                <title>Zasady Użytkowania | MTM Project</title>
            </Helmet>
            <div className="mx-8 sm:mx-16 md:mx-32 lg:mx-64 xl:mx-[28dvw] text-justify text-black dark:text-white">
                <h1 className="text-3xl my-4 font-bold text-center">Zasady Użytkowania</h1>
                <h3 className="text-lg mt-8 mb-2 font-bold">1. Usługi</h3>
                <h5 className="text-md mb-4">Nasza strona umożliwia użytkownikom rezerwowanie miejsc w hotelach i restauracjach za pośrednictwem naszego systemu.Działamy jako pośrednik pomiędzy użytkownikami a dostawcami usług(hotelami i restauracjami).</h5 >
                <h3 className="text-lg mt-8 mb-2 font-bold">2. Rejestracja Konta</h3 >
                <h5 className="text-md mb-4">Aby móc dokonać rezerwacji, użytkownicy mogą być zobowiązani do założenia konta.Rejestracja wymaga podania prawdziwych danych osobowych, takich jak imię, nazwisko, adres e - mail, numer telefonu.Użytkownik jest odpowiedzialny za utrzymanie poufności danych swojego konta oraz za wszystkie działania wykonywane za jego pomocą.</h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">3. Rezerwacje i Płatności</h3>
                <h5 className="text-md mb-4">Rezerwacje: Każda rezerwacja dokonana za pośrednictwem naszej platformy podlega dostępności miejsc w danym hotelu lub restauracji. Użytkownicy są zobowiązani do upewnienia się, że wszystkie dane wprowadzone podczas rezerwacji są poprawne.
                <br/>Płatności: W przypadku, gdy strona wymaga opłacenia rezerwacji z góry, użytkownicy zobowiązani są do dokonania płatności zgodnie z warunkami płatności wskazanymi podczas procesu rezerwacji.
                <br/>Anulacje: Zasady anulowania rezerwacji różnią się w zależności od hotelu lub restauracji i są jasno przedstawione w momencie dokonywania rezerwacji.Użytkownicy są zobowiązani do zapoznania się z nimi przed potwierdzeniem rezerwacji.</h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">4. Obowiązki Użytkownika</h3 >
                <h5 className="text-md mb-4">Użytkownik zobowiązuje się do korzystania ze strony zgodnie z obowiązującymi przepisami prawa i dobrymi obyczajami.
                Zabronione jest korzystanie z naszej platformy w sposób, który może spowodować zakłócenie jej działania, uszkodzenie systemów komputerowych, lub naruszenie praw innych użytkowników.
                Użytkownik jest odpowiedzialny za wszelkie działania podejmowane w ramach jego konta.</h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">5. Odpowiedzialność</h3 >
                <h5 className="text-md mb-4">Strona działa jako pośrednik i nie ponosi odpowiedzialności za jakiekolwiek błędy lub opóźnienia wynikające z działania hoteli lub restauracji.
                Nie gwarantujemy, że usługi na naszej stronie będą dostępne bez przerwy i wolne od błędów.W przypadku problemów technicznych podejmiemy działania w celu jak najszybszego ich rozwiązania.</h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">6. Polityka Prywatności</h3 >
                <h5 className="text-md mb-4">Korzystanie z naszej strony jest objęte Polityką Prywatności, która określa sposób zbierania, przechowywania i wykorzystywania danych osobowych użytkowników. Zasady te można znaleźć <Link to="../privacy">tutaj</Link>.</h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">7. Zmiany w Zasadach</h3 >
                <h5 className="text-md mb-4">Zastrzegamy sobie prawo do wprowadzania zmian w niniejszych Zasadach Użytkowania w dowolnym czasie.Użytkownicy zostaną poinformowani o zmianach za pośrednictwem naszej strony lub drogą mailową.Korzystanie z platformy po wprowadzeniu zmian oznacza akceptację zmienionych zasad.</h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">8. Kontakt</h3 >
                <h5 className="text-md mb-4">W razie jakichkolwiek pytań dotyczących niniejszych Zasad Użytkowania, prosimy o kontakt za pośrednictwem adresu e - mail: <a href="mailto:contact@mtm-project.uk">contact@mtm-project.uk</a>.</h5>
            </div>
            <Footer />
        </div>);
}