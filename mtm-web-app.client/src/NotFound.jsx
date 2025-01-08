//ekran błędu 404 - brak strony
import { Helmet } from "react-helmet";
import { translations } from './lang';
export default function NotFound() {
    return(
        <div className="w-full h-[95vh] text-center place-content-center">
            <Helmet>
                <title>{translations.formatString(translations.title, translations.notFound)}</title>
            </Helmet>
            <h1 className="text-3xl font-semibold mb-4">{translations.formatString(translations.error, translations.notFound)}</h1>
            <h3>{translations.pageYoureTryingToGetIntoDoesNotExist}</h3>
        </div>
    )
}