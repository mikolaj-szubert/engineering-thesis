import { useGoogleLogin } from '@react-oauth/google'
import { GoogleLogo } from '../Icons/GoogleLogo'
import { Button } from '@nextui-org/react';
import { instance, getClaimsFromToken } from '../Helpers'
import { translations } from '../lang'
import React from 'react'

export default ({ modalToCloseOnLogin, onLogin }) => {
    const [isLoading, setIsLoading] = React.useState(false);
    const login = useGoogleLogin({
        flow: 'auth-code',
        select_account: false,
        scope: "email profile openid",
        prompt: 'none',
        cancel_on_tap_outside: true,
        onSuccess: async (codeResponse) => {
            setIsLoading(true);
            await instance.post("auth/google-login", { token: codeResponse.code })
                .then((res) => {
                    if (res.status === 200) {
                        modalToCloseOnLogin.onClose();
                        onLogin(getClaimsFromToken(res.data));
                    }
                    else console.error(res.data);
                })
                .catch((err) => {
                    console.error(err.message);
                })
                .finally(() => setIsLoading(false));
        },
        onError: errorResponse => console.log(errorResponse),
    });
    return (
        <Button onPress={login} isLoading={isLoading} className="border-solid border-black border-1 dark:border-0 text-black bg-white p-2 text-base" radius="full" variant="solid" fullWidth="true" startContent={<GoogleLogo />}>
            {translations.continueUsingGoogle}
        </Button>
    );
}