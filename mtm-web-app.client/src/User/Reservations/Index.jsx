import { ProtectedRoute } from '../../Helpers'
import { translations } from '../../lang'
import { useOutletContext, NavLink, useLocation, useParams } from 'react-router-dom'
import { Helmet } from 'react-helmet'
import { Divider } from '@nextui-org/react'
import Footer from '../../Footer'
import Hotel from './Hotel'
import Restaurant from './Restaurant'

export default () => {
    const { onLogin, user, loginModal, registerModal, onLogout } = useOutletContext();
    const { pathname } = useLocation(); //œcie¿ka
    const { obj } = useParams(); //parametry dynamiczne
    return (
        <>
            <Helmet>
                <title>{translations.formatString(translations.title, "Rezerwacje")}</title>
            </Helmet>
            <ProtectedRoute user={user} loginModal={loginModal} registerModal={registerModal}>
                <div className="flex md:flex-row flex-col md:h-screen relative">
                    {/*MOBILE*/}
                    <div className="relative mt-2 block md:hidden w-full">
                        <div className="w-full flex justify-between space-x-4">
                            <NavLink
                                className={`block px-4 py-2 text-center text-white rounded-full w-1/2 border-2 border-transparent hover:brightness-90 hover:border-white ${pathname == '/reservations/hotels' || pathname === '/reservations'
                                    ? 'bg-gradient-zielony'
                                    : 'bg-bottle-green'
                                    }`}
                                to="/reservations/hotels"
                            >
                                {translations.hotels}
                            </NavLink>
                            <NavLink
                                className={`block px-4 py-2 text-center text-white rounded-full w-1/2 border-2 border-transparent hover:brightness-90 hover:border-white ${pathname == '/reservations/restaurants'
                                    ? 'bg-gradient-zielony'
                                    : 'bg-bottle-green'
                                    }`}
                                to="/reservations/restaurants"
                            >
                                {translations.restaurants}
                            </NavLink>
                        </div>
                    </div>
                    {/*MOBILE*/}
                    <div className="basis-1/5 relative hidden md:block">
                        <div className="fixed left-0 pt-4 w-1/5">
                            <NavLink
                                className={`block mx-4 px-4 py-2 text-center text-white rounded-full min-w-min max-w-full border-2 border-transparent hover:brightness-90 hover:border-white ${pathname == '/reservations/hotels' || pathname === '/reservations' ? 'bg-gradient-zielony' : 'bg-bottle-green'}`}
                                to="/reservations/hotels"
                            >
                                {translations.hotels}
                            </NavLink>
                            <Divider className="my-4" />
                            <NavLink
                                className={`block mx-4 px-4 py-2 text-center text-white rounded-full min-w-min max-w-full border-2 border-transparent hover:brightness-90 hover:border-white ${pathname == '/reservations/restaurants' ? 'bg-gradient-zielony' : 'bg-bottle-green'}`}
                                to="/reservations/restaurants"
                            >
                                {translations.restaurants}
                            </NavLink>
                        </div>
                    </div>
                    <Divider orientation="vertical" />
                    <div className="basis-4/5">
                        {obj === "restaurants" ?
                            <Restaurant />
                            :
                            <Hotel />
                        }
                    </div>
                </div>
                <Footer />
            </ProtectedRoute>
        </>
    );
}