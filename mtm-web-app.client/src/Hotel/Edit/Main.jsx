import { useOutletContext, Outlet } from 'react-router-dom'
import { ProtectedRoute } from '../../Helpers'

export default () => {
    const { curr, isDarkMode, user, loginModal, registerModal } = useOutletContext();
    return (
        <ProtectedRoute user={user} loginModal={loginModal} registerModal={registerModal} >
            <Outlet context={{ curr, isDarkMode }} />
        </ProtectedRoute >
    );
}