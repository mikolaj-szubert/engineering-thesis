import React from "react";
import {
    useLocation,
    Outlet,
    Link,
    useNavigate,
    ScrollRestoration
} from "react-router-dom";
import {
    Avatar,
    Button,
    Divider,
    Dropdown,
    DropdownTrigger,
    DropdownMenu,
    DropdownItem,
    Link as NextLink,
    Navbar,
    NavbarBrand,
    NavbarMenuToggle,
    NavbarMenuItem,
    NavbarMenu,
    NavbarContent,
    NavbarItem,
    Switch,
    useDisclosure,
} from "@nextui-org/react";
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import LoginModal from './User/Login'
import RegisterModal from './User/Register'
import { MoonIcon } from "./Icons/MoonIcon";
import { SunIcon } from "./Icons/SunIcon";
import { translations } from './lang';
import "./CustomStyles/navbar.css";
import "./CustomStyles/fonts.css";
import { isMobile } from 'react-device-detect';

const UserSection = React.memo(({ user, registerModal, loginModal, onLogin, onLogout, navigate, isDarkMode }) => {
    const changeModal = () => {
        if (registerModal.isOpen) {
            registerModal.onClose();
            loginModal.onOpen();
        }
        else {
            loginModal.onClose();
            registerModal.onOpen();
        }
    }
    if (user) {
        return (
            <Dropdown
                placement="bottom-end"
                backdrop="blur"
                radius="sm"
                classNames={{
                    base: "before:bg-background",
                    content: "p-0 border-1 rounded-none bg-white dark:bg-black",
                }}
            >
                <DropdownTrigger>
                    <Avatar color="primary" size="sm" showFallback isBordered className="aspect-square transition-transform cursor-pointer" src={user.picture === "True" ? "/api/images/user" : user.picture} />
                </DropdownTrigger>
                <DropdownMenu
                    variant="faded"
                    color="primary"
                    autoFocus="last"
                    aria-label="Elementy rozwijanego menu"
                    className="p-3 bg-white dark:bg-black"
                    itemClasses={{
                        base: [
                            "bg-white",
                            "dark:bg-black",
                            "rounded-md",
                            "text-black",
                            "dark:text-default-500",
                            "transition-opacity",
                            "dark:data-[hover=true]:text-white",
                            "data-[hover=true]:bg-gray-200",
                            "dark:data-[hover=true]:bg-black",
                            "dark:data-[hover=true]:bg-black",
                            "data-[selectable=true]:focus:bg-black",
                            "data-[pressed=true]:opacity-70",
                            "data-[focus-visible=true]:ring-default-500",
                        ],
                        content: "border-none",
                    }}
                >
                    <DropdownItem onPress={() => navigate("account")} key="account">{translations.account}</DropdownItem>
                    <DropdownItem onPress={() => navigate("reservations")} key="reservations">{translations.reservations}</DropdownItem>
                    {user.Owner && user.Owner.includes("h") ? <DropdownItem onPress={() => navigate("hotels/reservations")} key="hotels">{translations.yourHotelRes}</DropdownItem> : null}
                    {user.Owner && user.Owner.includes("r") ? <DropdownItem onPress={() => navigate("restaurants/reservations")} key="restaurants">{translations.yourRestaurantRes}</DropdownItem> : null}
                    <DropdownItem onPress={() => onLogout()} key="logout" className="text-danger logout" color="danger">{translations.logout}</DropdownItem>
                </DropdownMenu>
            </Dropdown>
        );
    }
    else return (<>
            <NextLink onPress={registerModal.onOpen} key="register" className="text-white hover:text-gray-200 dark:hover:text-teal-500 font-semibold select-none cursor-pointer mx-2">{translations.register}</NextLink>
            <RegisterModal onLogin={onLogin} loginModal={loginModal} registerModal={registerModal} changeModal={changeModal} isDarkMode={isDarkMode} />
            <NextLink onPress={loginModal.onOpen} key="login" className="text-white hover:text-gray-200 dark:hover:text-teal-500 font-semibold select-none cursor-pointer mx-2">{translations.login}</NextLink>
            <LoginModal onLogin={onLogin} loginModal={loginModal} registerModal={registerModal} changeModal={changeModal} isDarkMode={isDarkMode} />
    </>);
});

const CurrencySelector = ({ curr, setCurr }) => (
    <Dropdown
        placement="bottom-end"
        backdrop="blur"
        radius="sm"
        classNames={{
            base: "before:bg-background",
            content: "p-0 border-1 rounded-none bg-white dark:bg-black",
        }}
    >
        <DropdownTrigger>
            <Button size="sm" color="primary" variant="ghost" className="transition-transform cursor-pointer">
                {curr}
            </Button>
        </DropdownTrigger>
        <DropdownMenu
            variant="faded"
            color="primary"
            autoFocus="last"
            aria-label="Elementy rozwijanego menu"
            className="p-3 bg-white dark:bg-black"
            itemClasses={{
                base: [
                    "bg-white",
                    "dark:bg-black",
                    "rounded-md",
                    "text-black",
                    "dark:text-default-500",
                    "transition-opacity",
                    "dark:data-[hover=true]:text-white",
                    "data-[hover=true]:bg-gray-200",
                    "dark:data-[hover=true]:bg-black",
                    "dark:data-[hover=true]:bg-black",
                    "data-[selectable=true]:focus:bg-black",
                    "data-[pressed=true]:opacity-70",
                    "data-[focus-visible=true]:ring-default-500",
                ],
                content: "border-none",
            }}
        >
            <DropdownItem onPress={() => setCurr("PLN")} key="PLN">{translations.pln}</DropdownItem>
            <DropdownItem onPress={() => setCurr("GBP")} key="GBP">{translations.gbp}</DropdownItem>
            <DropdownItem onPress={() => setCurr("EUR")} key="EUR">{translations.eur}</DropdownItem>
            <DropdownItem onPress={() => setCurr("USD")} key="USD">{translations.usd}</DropdownItem>
            <DropdownItem onPress={() => setCurr("CAD")} key="CAD">{translations.cad}</DropdownItem>
            <DropdownItem onPress={() => setCurr("AUD")} key="AUD">{translations.aud}</DropdownItem>
            <DropdownItem onPress={() => setCurr("JPY")} key="JPY">{translations.jpy}</DropdownItem>
            <DropdownItem onPress={() => setCurr("INR")} key="INR">{translations.inr}</DropdownItem>
            <DropdownItem onPress={() => setCurr("NZD")} key="NZD">{translations.nzd}</DropdownItem>
            <DropdownItem onPress={() => setCurr("CHF")} key="CHF">{translations.chf}</DropdownItem>
        </DropdownMenu>
    </Dropdown>
);

const LanguageSelector = React.memo(({ language, setLanguage }) => (
    <Dropdown
        placement="bottom-end"
        backdrop="blur"
        radius="sm"
        classNames={{
            base: "before:bg-background",
            content: "p-0 border-1 rounded-none bg-white dark:bg-black",
        }}
    >
        <DropdownTrigger>
            <Avatar color="primary" size="sm" showFallback isBordered className="transition-transform cursor-pointer object-fill" src={`https://flagcdn.com/${language.slice(0, 2) == 'en' ? 'gb' : 'pl'}.svg`} />
        </DropdownTrigger>
        <DropdownMenu
            variant="faded"
            color="primary"
            autoFocus="last"
            aria-label="Elementy rozwijanego menu"
            className="p-3 bg-white dark:bg-black"
            itemClasses={{
                base: [
                    "bg-white",
                    "dark:bg-black",
                    "rounded-md",
                    "text-black",
                    "dark:text-default-500",
                    "transition-opacity",
                    "dark:data-[hover=true]:text-white",
                    "data-[hover=true]:bg-gray-200",
                    "dark:data-[hover=true]:bg-black",
                    "dark:data-[hover=true]:bg-black",
                    "data-[selectable=true]:focus:bg-black",
                    "data-[pressed=true]:opacity-70",
                    "data-[focus-visible=true]:ring-default-500",
                ],
                content: "border-none",
            }}
        >
            <DropdownItem onPress={() => setLanguage("pl")} key="pl" startContent={<Avatar color="primary" size="sm" showFallback className="transition-transform cursor-pointer" src='https://flagcdn.com/pl.svg' />}>
                {translations.polish}
            </DropdownItem>
            <DropdownItem onPress={() => setLanguage("en")} key="en" startContent={<Avatar color="primary" size="sm" showFallback className="transition-transform cursor-pointer" src='https://flagcdn.com/gb.svg' />}>
                {translations.english}
            </DropdownItem>
        </DropdownMenu>
    </Dropdown>
));

const MobileUserSection = React.memo(({ setIsMenuOpen, user, registerModal, loginModal, onLogin, onLogout, navigate, isDarkMode }) => {
    const mobileChangeModal = () => {
        setIsMenuOpen(false);
        setTimeout(() => {
            if (registerModal.isOpen) {
                registerModal.onClose();
                loginModal.onOpen();
            }
            else {
                loginModal.onClose();
                registerModal.onOpen();
            }
        }, 300);
    }
    if (user) return (
        <NavbarItem>
            <div className="flex items-left gap-2">
                <Avatar
                    size="sm"
                    showFallback
                    isBordered
                    className="transition-transform ml-1"
                    src={user.picture}
                />
                <span className="text-base dark:text-white text-black mt-1 ml-1">{user.name}</span>
            </div>
            <p className="my-2 mt-4 text-base cursor-pointer" onClick={() => navigate("account")} key="account">{translations.account}</p>
            <p className="my-2 text-base cursor-pointer" onClick={() => navigate("reservations")} key="reservations">{translations.reservations}</p>
            {user.Owner && user.Owner.includes("h") ? <p className="my-2 text-base cursor-pointer" onClick={() => navigate("hotels/reservations")} key="hotels">{translations.yourHotelRes}</p> : null}
            {user.Owner && user.Owner.includes("r") ? <p className="my-2 text-base cursor-pointer" onClick={() => navigate("restaurants/reservations")} key="restaurants">{translations.yourRestaurantRes}</p> : null}
            <p className="my-2 text-base cursor-pointer" onClick={() => onLogout()} key="logout">{translations.logout}</p>
        </NavbarItem>
    );
    else return (<>
        <NextLink onPress={() => { setIsMenuOpen(false); loginModal.onOpen(); }} key="login" className="hover:text-gray-200 dark:hover:text-teal-500 font-semibold select-none cursor-pointer">{translations.login}</NextLink>
        <LoginModal onLogin={onLogin} loginModal={loginModal} registerModal={registerModal} changeModal={mobileChangeModal} isDarkMode={isDarkMode} />
        <NextLink onPress={() => { setIsMenuOpen(false); registerModal.onOpen(); }} key="register" className="hover:text-gray-200 dark:hover:text-teal-500 font-semibold select-none cursor-pointer">{translations.register}</NextLink>
        <RegisterModal onLogin={onLogin} loginModal={loginModal} registerModal={registerModal} changeModal={mobileChangeModal} isDarkMode={isDarkMode} />
    </>);
});

const Icon = React.memo(({ isDarkMode }) => {
    if (isDarkMode === true) return <MoonIcon />;
    else return <SunIcon className="text-black" />;
});

const renderNavbarItem = (path, currentPath, label) => (
    <NavbarItem isActive={(path.length > 1 && currentPath.startsWith(path)) || (path.length === 1 && currentPath === path)}>
        <Link
            className={`${(path.length > 1 && currentPath.startsWith(path)) || (path.length === 1 && currentPath === path) ? "underline underline-offset-4 decoration-teal-700 decoration-2" : "no-underline"
                } text-white hover:text-gray-200 dark:hover:text-teal-500 font-semibold select-none`}
            to={path}
        >
            {label}
        </Link>
    </NavbarItem>
);

const renderNavbarMenuItem = (label, currentPath, path, mobileRedirect) => (
    <NavbarMenuItem>
        <NextLink className={`${(path.length > 1 && currentPath.startsWith(path)) || (path.length === 1 && currentPath === path) ? "underline underline-offset-4 decoration-teal-700 decoration-2" : "no-underline"} hover:text-teal-500 text-black dark:text-white select-none w-full`} onPress={() => mobileRedirect(path)} >
            {label}
        </NextLink>
    </NavbarMenuItem>
);

export default function App({ isDarkMode, setIsDarkMode, user, onLogin, onLogout, curr, setCurr, language, setLanguage }) {
    const [isMenuOpen, setIsMenuOpen] = React.useReducer((current) => !current, false); //menu nawigacji mobilnej
    const navigate = useNavigate();
    const loginModal = useDisclosure();
    const registerModal = useDisclosure();

    const { pathname } = useLocation(); //ścieżka

    const mobileRedirect = (where) => {
        setIsMenuOpen(false);
        setTimeout(() => {
            navigate(where);
        }, 300);
    }

    if (pathname === '/btzdvmgnpvorthm') {
        console.log("TODO");
        return <Outlet />;
    }

    return (
        <div className="h-[100dvh] m-0">
            <Navbar
                maxWidth="full"
                height="2.5rem"
                className="h-10 max-h-10 bg-gradient-to-r from-gradient-zielony to-gradient-bezowy dark:from-black dark:to-black z-50 nav"
                classNames={{
                    base: "h-10"
                }}
                isMenuOpen={isMenuOpen}
                onMenuOpenChange={setIsMenuOpen}
            >
                {/* Mobile */}
                <NavbarContent className="md:hidden pr-0" justify="start">
                    <NavbarMenuToggle aria-label={isMenuOpen ? "Zamknij menu" : "Otwórz menu"} />
                </NavbarContent>

                <NavbarContent className="md:hidden" justify="center">
                    <NavbarBrand onClick={() => navigate("/")}>
                        <h1 className="text-2xl text-white poppins-black select-none">{translations.mtm}</h1>
                    </NavbarBrand>
                </NavbarContent>
                <NavbarContent className="md:hidden" justify="end" />
                {/* Mobile end */}

                <NavbarContent className="hidden md:flex gap-4" justify="center">
                    <NavbarBrand>
                        <Link
                            className={`${pathname === "/" ? "underline underline-offset-4 decoration-teal-700 decoration-2" : "no-underline"
                                } text-2xl text-white hover:text-gray-200 dark:hover:text-teal-500 cursor-pointer poppins-black cursor-default select-none`}
                            to="/"
                        >
                            {translations.mtm}
                        </Link>
                    </NavbarBrand>
                    {/*{renderNavbarItem("/", pathname, translations.homePage)}*/}
                    {renderNavbarItem("/hotels", pathname, translations.hotels)}
                    {renderNavbarItem("/restaurants", pathname, translations.restaurants)}
                    {renderNavbarItem("/map", pathname, translations.map)}
                </NavbarContent>
                <NavbarContent className="hidden md:flex" justify="end">
                    <NavbarItem>
                        <Switch
                            defaultSelected={true}
                            isSelected={isDarkMode}
                            onValueChange={setIsDarkMode}
                            size="sm"
                            color="primary"
                            thumbIcon={<Icon isDarkMode={isDarkMode} />}
                        />
                    </NavbarItem>
                    <NavbarItem>
                        <LanguageSelector setLanguage={setLanguage} language={language} />
                    </NavbarItem>
                    <NavbarItem>
                        <CurrencySelector curr={curr} setCurr={setCurr} />
                    </NavbarItem>
                    <NavbarItem>
                        <UserSection user={user} registerModal={registerModal} loginModal={loginModal} onLogin={onLogin} onLogout={onLogout} navigate={navigate} isDarkMode={isDarkMode} />
                    </NavbarItem>
                </NavbarContent>

                {/* Menu dla urządzeń mobilnych */}
                <NavbarMenu
                    className="bg-white dark:bg-black"
                >
                    {renderNavbarMenuItem(translations.homePage, pathname, "/", mobileRedirect)}
                    {renderNavbarMenuItem(translations.hotels, pathname, "/hotels", mobileRedirect)}
                    {renderNavbarMenuItem(translations.restaurants, pathname, "/restaurants", mobileRedirect)}
                    {renderNavbarMenuItem(translations.map, pathname, "/map", mobileRedirect)}
                    <Divider className="my-4 text-black" />
                    <MobileUserSection setIsMenuOpen={setIsMenuOpen} user={user} registerModal={registerModal} loginModal={loginModal} onLogin={onLogin} onLogout={onLogout} navigate={mobileRedirect} isDarkMode={isDarkMode} />
                    <Divider className="my-4 text-black" />
                    <NavbarMenuItem className="text-black dark:text-white select-none my-1">
                        <NextLink className={`no-underline text-black dark:text-white select-none`} onPress={() => setIsDarkMode(!isDarkMode)}>
                            <Switch
                                defaultSelected={true}
                                isSelected={isDarkMode}
                                onValueChange={setIsDarkMode}
                                size="sm"
                                color="primary"
                                thumbIcon={<Icon isDarkMode={isDarkMode} />}
                                className="mr-2"
                            />
                            {translations.changeTheme}
                        </NextLink>
                    </NavbarMenuItem>
                    <NavbarMenuItem className="text-black dark:text-white select-none my-1">
                        <LanguageSelector setLanguage={setLanguage} language={language} />
                    </NavbarMenuItem>
                    <NavbarMenuItem className="text-black dark:text-white select-none my-1">
                        <CurrencySelector curr={curr} setCurr={setCurr} />
                    </NavbarMenuItem>
                </NavbarMenu>
                {/*Menu dla urządzeń mobilnych*/}
            </Navbar>

            <ToastContainer
                position={isMobile ? "bottom-center" : "bottom-right"}
                newestOnTop={false}
                closeOnClick
                rtl={false}
                pauseOnFocusLoss
                draggable
                pauseOnHover
                theme={isDarkMode ? "dark" : "light"}
                limit={3}
                transition: Bounce
            />

            <Outlet className="max-h-[calc(100%-2.5rem)]" context={{ onLogin, onLogout, isDarkMode, user, loginModal, registerModal, curr }} />

            <ScrollRestoration
                getKey={(location,) => {
                    const paths = ["/hotels", "/restaurants"];
                    return paths.some((path) => location.pathname.startsWith(path)) ? location.pathname : location.key;
                }}
            />

        </div>
    );
}