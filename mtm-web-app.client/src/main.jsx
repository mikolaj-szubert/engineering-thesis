import ReactDOM from 'react-dom/client'
import { NextUIProvider } from '@nextui-org/react'
import './index.css'
import { GoogleOAuthProvider } from '@react-oauth/google'
import { instance } from "./Helpers"
import axios from 'axios'
import App from './App'

(async function () {
    await axios("/ip?format=json")
        .then((res) => {
            axios.defaults.headers.common['X-IP-Address'] = res.data.ip;
            instance.defaults.headers.common['X-IP-Address'] = res.data.ip;
        });
}());

ReactDOM.createRoot(document.getElementById('root')).render(
    <NextUIProvider>
        <main className="bg-white dark:bg-black">
            <GoogleOAuthProvider clientId="922373443586-h3jetju6q6h11r4tf4f4sge5chkiebo6.apps.googleusercontent.com">
                <App />
            </GoogleOAuthProvider>
        </main>
    </NextUIProvider>
)