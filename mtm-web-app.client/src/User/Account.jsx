import { ProtectedRoute } from '../Helpers'
import { translations } from '../lang'
import { useOutletContext, Outlet, NavLink, useLocation } from 'react-router-dom'
import { Helmet } from 'react-helmet'
import { Divider } from '@nextui-org/react'
import Footer from '../Footer'

export default () => {
    const { onLogin, user, loginModal, registerModal, onLogout } = useOutletContext();
    const { pathname } = useLocation(); //ścieżka
    return (
        <>
            <Helmet>
                <title>{translations.formatString(translations.title, translations.account)}</title>
            </Helmet>
            <ProtectedRoute user={user} loginModal={loginModal} registerModal={registerModal}>
                <div className="flex md:flex-row flex-col md:min-h-screen relative">
                    {/*MOBILE*/}
                    <div className="relative mt-2 block md:hidden w-full">
                        <div className="w-full flex justify-between space-x-4">
                            <NavLink
                                className={`block px-4 py-2 text-center text-white rounded-full flex-1 border-2 border-transparent hover:brightness-90 hover:border-white ${pathname === '/account/edit' || pathname === '/account'
                                        ? 'bg-gradient-zielony'
                                        : 'bg-bottle-green'
                                    }`}
                                to="edit"
                            >
                                {translations.Edit}
                            </NavLink>
                            <NavLink
                                className={`block px-4 py-2 text-center text-white rounded-full flex-1 border-2 border-transparent hover:brightness-90 hover:border-white ${pathname === '/account/logs'
                                    ? 'bg-gradient-zielony'
                                    : 'bg-bottle-green'
                                    }`}
                                to="logs"
                            >
                                {translations.Logs}
                            </NavLink>
                            <NavLink
                                className={`block px-4 py-2 text-center text-white rounded-full flex-1 border-2 border-transparent hover:brightness-90 hover:border-white ${pathname === '/account/delete'
                                        ? 'bg-gradient-zielony'
                                        : 'bg-bottle-green'
                                    }`}
                                to="delete"
                            >
                                {translations.Deleting}
                            </NavLink>
                        </div>
                    </div>
                    {/*MOBILE*/}
                    <div className="basis-1/5 relative hidden md:block">
                        <div className="fixed left-0 pt-4 w-1/5">
                            <NavLink
                                className={`block mx-4 px-4 py-2 text-center text-white rounded-full min-w-min max-w-full border-2 border-transparent hover:brightness-90 hover:border-white ${pathname === '/account/edit' || pathname === '/account' ? 'bg-gradient-zielony' : 'bg-bottle-green'}`}
                                to="edit"
                            >
                                {translations.Edit}
                            </NavLink>
                            <Divider className="my-4" />
                            <NavLink
                                className={`block mx-4 px-4 py-2 text-center text-white rounded-full min-w-min max-w-full border-2 border-transparent hover:brightness-90 hover:border-white ${pathname === '/account/logs' ? 'bg-gradient-zielony' : 'bg-bottle-green'}`}
                                to="logs"
                            >
                                {translations.Logs}
                            </NavLink>
                            <Divider className="my-4" />
                            <NavLink
                                className={`block mx-4 px-4 py-2 text-center text-white rounded-full min-w-min max-w-full border-2 border-transparent hover:brightness-90 hover:border-white ${pathname === '/account/delete' ? 'bg-gradient-zielony' : 'bg-bottle-green'}`}
                                to="delete"
                            >
                                {translations.Deleting}
                            </NavLink>
                        </div>
                    </div>
                    <Divider orientation="vertical" />
                    <div className="basis-4/5">
                        <Outlet context={{ onLogin, user, onLogout }} />
                    </div>
                </div>
                <Footer />
            </ProtectedRoute>
        </>
    );
}