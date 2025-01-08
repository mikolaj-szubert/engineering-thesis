import React from 'react'
import axios from 'axios'
import { jwtDecode } from 'jwt-decode'
import Unauthorized from './Unauthorized'

export const getClaimsFromToken = (token) => {
    try {
        return jwtDecode(token);
    } catch (error) {
        console.error("Failed to decode token", error);
        return null;
    }
};

export const instance = axios.create({
    withCredentials: true,
    baseURL: "/api",
    validateStatus: function (status) {
        return status < 500;
    }
});

export const ProtectedRoute = ({ children, user, loginModal, registerModal }) => {
    const [isAuthorized, setIsAuthorized] = React.useState(false);

    React.useEffect(() => {
        if (user) setIsAuthorized(true);
        else setIsAuthorized(false);
    }, [user]);
    if (isAuthorized && user) return children;
    return <Unauthorized loginModal={loginModal} registerModal={registerModal} />;
}

export default {
    instance,
    getClaimsFromToken,
    ProtectedRoute
}