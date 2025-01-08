import { FacebookLogo } from './Icons/FacebookLogo.jsx';
import { InstagramLogo } from './Icons/InstagramLogo.jsx';
import { Button, Link } from "@nextui-org/react";
import { useLocation, useOutletContext } from "react-router-dom";
import { translations } from './lang';

export default function Footer() {
    const { pathname } = useLocation();
    const { isDarkMode } = useOutletContext();

    return (
        <footer className="w-full border-t p-4 bg-white dark:bg-black">
            <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center w-full">
                <div className="text-black dark:text-white lg:w-1/2 w-full mb-4 lg:mb-0">
                    {translations.formatString(translations.questionsContactUs, "8:00", "15:00", "765 890 432")}
                </div>
                <div className="grid grid-cols-2 lg:grid-cols-3 gap-4 lg:w-1/2">
                    <Link
                        className={`${pathname === "/about"
                                ? "underline underline-offset-4 decoration-teal-500"
                                : "no-underline"
                            } text-teal-500 hover:text-teal-700 select-none`}
                        href="/about"
                    >
                        {translations.about}
                    </Link>
                    <Link
                        className={`${pathname === "/tos"
                                ? "underline underline-offset-4 decoration-teal-500"
                                : "no-underline"
                            } text-teal-500 hover:text-teal-700 select-none`}
                        href="/tos"
                    >
                        {translations.tos}
                    </Link>
                    <Link
                        className={`${pathname === "/privacy"
                                ? "underline underline-offset-4 decoration-teal-500"
                                : "no-underline"
                            } text-teal-500 hover:text-teal-700 select-none`}
                        href="/privacy"
                    >
                        {translations.privacyPolicy}
                    </Link>
                    <Link
                        className={`${pathname === "/cookies"
                                ? "underline underline-offset-4 decoration-teal-500"
                                : "no-underline"
                            } text-teal-500 hover:text-teal-700 select-none`}
                        href="/cookies"
                    >
                        {translations.useOfCookies}
                    </Link>
                    <Link
                        className={`${pathname === "/partnership"
                                ? "underline underline-offset-4 decoration-teal-500"
                                : "no-underline"
                            } text-teal-500 hover:text-teal-700 select-none`}
                        href="/partnership"
                    >
                        {translations.colaboration}
                    </Link>
                    <Link
                        className={`${pathname === "/app"
                            ? "underline underline-offset-4 decoration-teal-500"
                            : "no-underline"
                            } text-teal-500 hover:text-teal-700 select-none`}
                        href="/app"

                    >
                        {translations.app}
                    </Link>
                </div>
                <div className="flex items-center mt-4 lg:mt-0">
                    <Button
                        isIconOnly
                        className="dark:bg-black bg-white"
                        radius="full"
                        color="gradient"
                        aria-label="Facebook"
                        onPress={() => {
                            window.open("https://fb.me/", "_blank").focus();
                        }}
                    >
                        <FacebookLogo color={isDarkMode ? "#ffffff" : "#000000"} />
                    </Button>
                    <Button
                        isIconOnly
                        className="dark:bg-black bg-white ml-2"
                        aria-label="Instagram"
                        onPress={() => {
                            window.open("https://www.instagram.com/", "_blank").focus();
                        }}
                    >
                        <InstagramLogo color={isDarkMode ? "#ffffff" : "#000000"} />
                    </Button>
                </div>
            </div>
        </footer>
    );
}