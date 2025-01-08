import { useOutletContext, Outlet } from 'react-router-dom'
import { ProtectedRoute } from '../../Helpers'

export default () => {
    const { curr, isDarkMode, user, loginModal, registerModal, onLogin } = useOutletContext();
    return <ProtectedRoute user={user} loginModal={loginModal} registerModal={registerModal} ><Outlet context={{ curr, isDarkMode, user, onLogin }} /></ProtectedRoute >;
}