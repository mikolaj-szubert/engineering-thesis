//ekran błędu 401 - unauthorized
import { Link } from '@nextui-org/react'
import { translations } from './lang';
export default (props) => {
    return (
        <div className="w-full h-[95vh] text-center place-content-center dark:text-white text-black">
            <h1 className="text-3xl font-semibold mb-4">{translations.formatString(translations.error, "401")}</h1>
            <h3>Strona, na którą próbujesz się dostać przeznaczona jest dla zalogowanych użytkowników.</h3>
            <Link className="cursor-pointer" onPress={props.loginModal.onOpen}>Zaloguj się</Link> bądź <Link className="cursor-pointer" onPress={props.registerModal.onOpen}>zarejestruj</Link>.
        </div>
    );
}