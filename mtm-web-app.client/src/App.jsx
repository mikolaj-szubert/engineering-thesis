import React, { Suspense, useEffect } from "react"
import { createBrowserRouter, RouterProvider, createRoutesFromElements, Route } from "react-router-dom"
import { CookiesProvider, useCookies } from "react-cookie"
import { Spinner } from "@nextui-org/react"
import Nav from './Navbar'
import Home from './Home'
const NotFound = React.lazy(() => import('./NotFound'));
const Payment = React.lazy(() => import('./Payment'));
import { toast } from 'react-toastify'
import { translations } from './lang'
import { instance, getClaimsFromToken } from "./Helpers"
import axios from 'axios'
import { I18nProvider } from "@react-aria/i18n"

// Custom Hooks
import useTheme from './hooks/useTheme';
import useLanguage from './hooks/useLanguage';
import useCurrency from './hooks/useCurrency';
import useOnline from './hooks/useOnline';

const ErrorBoundary = () =>
(
    <div className="w-full h-[95vh] text-center place-content-center dark:text-white text-black">
        <h1 className="text-3xl font-semibold mb-4">{translations.errorBoundary}</h1>
        <h3>{translations.sorry}</h3>
    </div>
);

export default function App() {
    const { isDarkMode, toggleDarkMode } = useTheme();
    const { language, handleLanguageChange } = useLanguage();
    const { currency, setCurrency } = useCurrency();
    const [cookies, setCookie, removeCookie] = useCookies(['user']);
    const online = useOnline();

    if (!online) toast.error("No internet connection.");

    useEffect(() => {
        instance.post('auth/refresh-token')
            .then(res => {
                if (res.status === 200) handleLogin(getClaimsFromToken(res.data));
            })
            .catch(console.error);
    }, []);

    const handleLogin = (user) => {
        setCookie('user', user, { path: '/', expires: new Date(8640000000000000) });
    };

    const handleLogout = async () => {
        try {
            const res = await toast.promise(
                axios.get("api/auth/logout"),
                {
                    pending: translations.logingOut,
                    success: translations.loggedOut,
                    error: translations.errorOccured
                },
                { autoClose: 2000 }
            );

            if (res.status === 200) {
                removeCookie('user');
            }
        } catch (err) {
            if (online) {
                removeCookie('user');
                console.error(err.message);
            } else {
                toast.error("No internet connection.");
            }
        }
    };

    const router = createBrowserRouter(
        createRoutesFromElements(
            <>
                <Route
                    path="/"
                    errorElement={<ErrorBoundary />}
                    element={
                        <Nav
                            language={language}
                            setLanguage={handleLanguageChange}
                            curr={currency}
                            setCurr={setCurrency}
                            user={cookies.user}
                            onLogin={handleLogin}
                            onLogout={handleLogout}
                            isDarkMode={isDarkMode}
                            setIsDarkMode={toggleDarkMode}
                        />
                    }>
                    <Route index errorElement={<ErrorBoundary />} element={<Home onLogin={handleLogin} isDarkMode={isDarkMode} />} />
                    <Route path="map" errorElement={<ErrorBoundary />} lazy={() => import('./Map').then(module => ({ Component: module.default }))} />
                    <Route path="hotels" errorElement={<ErrorBoundary />}>
                        <Route index errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Hotels').then(module => ({ Component: module.default }))} />
                        <Route path=":id" errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Hotel').then(module => ({ Component: module.default }))} />
                        <Route path="reservations" errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/MyReservations').then(module => ({ Component: module.default }))} />
                        <Route path="add" errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Add/Main').then(module => ({ Component: module.default }))} >
                            <Route index errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Add/Index').then(module => ({ Component: module.default }))} />
                            <Route path="rooms" errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Add/Room/Index').then(module => ({ Component: module.default }))} />
                        </Route>
                        <Route path="edit" errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Edit/Main').then(module => ({ Component: module.default }))} >
                            <Route index errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Edit/Index').then(module => ({ Component: module.default }))} />
                            <Route path="rooms" errorElement={<ErrorBoundary />} lazy={() => import('./Hotel/Edit/Room/Index').then(module => ({ Component: module.default }))} />
                        </Route>
                    </Route>
                    <Route path="restaurants" errorElement={<ErrorBoundary />}>
                        <Route index errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Restaurants').then(module => ({ Component: module.default }))} />
                        <Route path=":id" errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Restaurant').then(module => ({ Component: module.default }))} />
                        <Route path="reservations" errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/MyReservations').then(module => ({ Component: module.default }))} />
                        <Route path="add" errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Add/Main').then(module => ({ Component: module.default }))} >
                            <Route index errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Add/Index').then(module => ({ Component: module.default }))} />
                            <Route path="tables" errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Add/Table/Index').then(module => ({ Component: module.default }))} />
                        </Route>
                        <Route path="edit" errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Edit/Main').then(module => ({ Component: module.default }))} >
                            <Route index errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Edit/Index').then(module => ({ Component: module.default }))} />
                            <Route path="tables" errorElement={<ErrorBoundary />} lazy={() => import('./Restaurants/Edit/Table/Index').then(module => ({ Component: module.default }))} />
                        </Route>
                    </Route>
                    <Route path="tos" errorElement={<ErrorBoundary />} lazy={() => import('./Info/TermsOfUse').then(module => ({ Component: module.default }))} />
                    <Route path="privacy" errorElement={<ErrorBoundary />} lazy={() => import('./Info/Privacy').then(module => ({ Component: module.default }))} />
                    <Route path="cookies" errorElement={<ErrorBoundary />} lazy={() => import('./Info/UseOfCookies').then(module => ({ Component: module.default }))} />
                    <Route path="partnership" errorElement={<ErrorBoundary />} lazy={() => import('./Info/Partnership').then(module => ({ Component: module.default }))} />
                    <Route path="about" errorElement={<ErrorBoundary />} lazy={() => import('./Info/About').then(module => ({ Component: module.default }))} />
                    <Route path="account" errorElement={<ErrorBoundary />} lazy={() => import('./User/Account').then(module => ({ Component: module.default }))} >
                        <Route index errorElement={<ErrorBoundary />} lazy={() => import('./User/Edit').then(module => ({ Component: module.default }))} />
                        <Route path="edit" errorElement={<ErrorBoundary />} lazy={() => import('./User/Edit').then(module => ({ Component: module.default }))} />
                        <Route path="logs" errorElement={<ErrorBoundary />} lazy={() => import('./User/Logs').then(module => ({ Component: module.default }))} />
                        <Route path="delete" errorElement={<ErrorBoundary />} lazy={() => import('./User/Delete').then(module => ({ Component: module.default }))} />
                    </Route>
                    <Route path="reservations/:obj?" errorElement={<ErrorBoundary />} lazy={() => import('./User/Reservations/Index').then(module => ({ Component: module.default }))} />
                    <Route path="*" element={<NotFound />} />
                </Route>
                <Route path="/payment" element={<Payment />} errorElement={<ErrorBoundary />} />
            </>
        )
    );

    return (
        <I18nProvider locale={language}>
            <CookiesProvider>
                <Suspense fallback={<Spinner className="dark:bg-black bg-white w-full h-[95dvh]" label={translations.laoding} />}>
                    <RouterProvider router={router} />
                </Suspense>
            </CookiesProvider>
        </I18nProvider>
    );
}
