import Footer from '../Footer.jsx';
import { Helmet } from 'react-helmet'

export default function TermsOfUse() {
    return (
        <div>
            <Helmet>
                <title>Polityka Prywatności | MTM Project</title>
            </Helmet>
            <div className="mx-8 sm:mx-16 md:mx-32 lg:mx-64 xl:mx-[28dvw] text-justify text-black dark:text-white">
                <h1 className="text-3xl my-4 font-bold text-center">Polityka Prywatności</h1>

                <h3 className="text-lg mt-8 mb-2 font-bold">1. Wprowadzenie</h3>
                <h5 className="text-md mb-4">Niniejsza Polityka Prywatności określa zasady przetwarzania i ochrony danych osobowych użytkowników korzystających z naszej strony internetowej zajmującej się rezerwacją hoteli i restauracji. Dokładamy wszelkich starań, aby Twoje dane były przetwarzane w sposób zgodny z obowiązującymi przepisami prawa, w tym z ogólnym rozporządzeniem o ochronie danych osobowych (RODO).</h5 >

                <h3 className="text-lg mt-8 mb-2 font-bold">2. Administrator Danych Osobowych</h3 >
                <h5 className="text-md mb-4">Administratorem Twoich danych osobowych jest [Nazwa Firmy], z siedzibą w [Adres]. Możesz się z nami skontaktować za pośrednictwem adresu e-mail: [Adres e-mail kontaktowy].</h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">3. Jakie Dane Zbieramy?</h3>
                <h5 className="text-md mb-4">Podczas korzystania z naszej strony możemy zbierać następujące dane osobowe:
                    <ul className="ml-4 text-md list-disc">
                        <li>Dane rejestracyjne: Imię, nazwisko, adres e-mail, numer telefonu, dane logowania.</li>
                        <li>Dane dotyczące rezerwacji: Informacje o dokonanych rezerwacjach, w tym preferencje dotyczące hotelu/restauracji, liczba gości, daty rezerwacji.</li>
                        <li>Dane płatnicze: W przypadku opłacenia rezerwacji za pośrednictwem naszej strony możemy zbierać informacje dotyczące metod płatności.</li>
                        <li>Dane techniczne: Adres IP, rodzaj przeglądarki internetowej, informacje o urządzeniu i systemie operacyjnym, pliki cookies.</li>
                    </ul>
                </h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">4. Cele Przetwarzania Danych</h3 >
                <h5 className="text-md mb-4">Zbieramy i przetwarzamy Twoje dane osobowe w następujących celach:
                    <ul className="ml-4 text-md list-disc">
                        <li>Rezerwacje: W celu umożliwienia dokonania rezerwacji hotelowych i restauracyjnych.</li>
                        <li>Komunikacja: Aby kontaktować się z Tobą w sprawach związanych z rezerwacjami, zmianami w rezerwacjach, promocjami oraz wsparciem technicznym.</li>
                        <li>Płatności: Do przetwarzania transakcji finansowych związanych z rezerwacjami.</li>
                        <li>Personalizacja: Aby dostosować nasze usługi do Twoich preferencji.</li>
                        <li>Poprawa naszych usług: Aby analizować sposób korzystania z naszej strony, co pozwala nam ją ulepszać.</li>
                    </ul>
                </h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">5. Podstawa Prawna Przetwarzania</h3 >
                <h5 className="text-md mb-4">Przetwarzamy Twoje dane osobowe na następujących podstawach prawnych:
                    <ul className="ml-4 text-md list-disc">
                        <li>Zgoda: Jeśli wyraziłeś zgodę na przetwarzanie danych np. w celu otrzymywania materiałów marketingowych.</li>
                        <li>Wykonanie umowy: W celu realizacji rezerwacji hotelowej lub restauracyjnej.</li>
                        <li>Obowiązki prawne: Aby spełnić wymagania prawne dotyczące np. księgowości lub przechowywania danych.</li>
                        <li>Prawnie uzasadnione interesy: W celu poprawy jakości naszych usług, zapewnienia bezpieczeństwa strony oraz marketingu bezpośredniego (np. wysyłka newslettera).</li>
                    </ul>
                </h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">6. Udostępnianie Danych Osobowych</h3 >
                <h5 className="text-md mb-4">Twoje dane mogą być udostępniane następującym podmiotom:
                    <ul className="ml-4 text-md list-disc">
                        <li>Dostawcom usług: Hotelom i restauracjom, z którymi współpracujemy, w celu realizacji Twojej rezerwacji.</li>
                        <li>Dostawcom płatności: W celu realizacji transakcji płatniczych.</li>
                        <li>Zewnętrznym dostawcom usług IT: W zakresie hostingu, zarządzania stroną internetową i jej zabezpieczeń.</li>
                        <li>Organy państwowe: Jeśli wymaga tego prawo (np. organy podatkowe, organy ścigania).</li>
                    </ul>
                </h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">7. Okres Przechowywania Danych</h3 >
                <h5 className="text-md mb-4">Twoje dane osobowe będą przechowywane przez okres niezbędny do realizacji celów, dla których zostały zebrane, lub dłużej, jeśli wymaga tego prawo. Dane dotyczące rezerwacji i płatności będą przechowywane przez okres wymagany przepisami dotyczącymi księgowości i podatków.</h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">8. Prawa Użytkownika</h3 >
                <h5 className="text-md mb-4">Masz prawo do:
                    <ul className="ml-4 text-md list-disc">
                        <li>Dostępu do danych: Możesz zażądać informacji o tym, jakie dane przetwarzamy oraz w jakim celu.</li>
                        <li>Sprostowania danych: Możesz zażądać poprawienia błędnych lub nieaktualnych danych osobowych.</li>
                        <li>Usunięcia danych: Masz prawo zażądać usunięcia Twoich danych, jeśli nie są już potrzebne do realizacji celów, dla których zostały zebrane, lub jeśli cofniesz swoją zgodę na przetwarzanie.</li>
                        <li>Ograniczenia przetwarzania: Możesz zażądać ograniczenia przetwarzania Twoich danych w określonych sytuacjach.</li>
                        <li>Przenoszenia danych: Możesz zażądać przesłania danych w formacie umożliwiającym ich przekazanie innemu podmiotowi.</li>
                        <li>Sprzeciwu wobec przetwarzania: Możesz zgłosić sprzeciw wobec przetwarzania Twoich danych w celach marketingowych.</li>
                        <li>Aby skorzystać z tych praw, skontaktuj się z nami pod adresem: <a href="mailto:privacy@mtm-project.uk">privacy@mtm-project.uk</a>.</li >
                    </ul>
                </h5>
                <h3 className="text-lg mt-8 mb-2 font-bold">9. Pliki Cookies</h3 >
                <h5 className="text-md mb-4">Nasza strona używa plików cookies, które umożliwiają nam analizowanie ruchu na stronie oraz poprawę jej funkcjonowania. Możesz zarządzać plikami cookies w ustawieniach swojej przeglądarki.</h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">10. Zmiany w Polityce Prywatności</h3 >
                <h5 className="text-md mb-4">Zastrzegamy sobie prawo do wprowadzania zmian w niniejszej Polityce Prywatności. Wszelkie zmiany będą publikowane na naszej stronie, a użytkownicy zostaną o nich powiadomieni w odpowiedni sposób.</h5>

                <h3 className="text-lg mt-8 mb-2 font-bold">11. Kontakt</h3 >
                <h5 className="text-md mb-4">Jeśli masz jakiekolwiek pytania dotyczące niniejszej Polityki Prywatności lub chcesz skorzystać z przysługujących Ci praw, skontaktuj się z nami pod adresem: <a href="mailto:privacy@mtm-project.uk">privacy@mtm-project.uk</a>.</h5>
            </div>
            <Footer />
        </div>);
}