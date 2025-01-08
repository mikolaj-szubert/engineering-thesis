/** @type {import('tailwindcss').Config} */
import { nextui } from "@nextui-org/react";
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
        "./node_modules/@nextui-org/theme/dist/**/*.{js,ts,jsx,tsx}"
    ],
    theme: {
        fontFamily: {
            'sans' : '"Poppins", Arial, sans-serif',
        },
        extend: {
            backgroundImage: {
                'register-image': "linear-gradient(90deg, rgba(255,255,255,1) 0%, rgba(0,0,0,0) 100%), url('/register.png')",
                'register-image-dark': "linear-gradient(90deg, rgba(0,0,0,1) 0%, rgba(255,255,255,0) 100%), url('/register.png')",
            },
            colors: {
                'gradient-zielony': '#00B091',
                'gradient-bezowy': '#DABC71',
                'beige': '#D7BD7C',
                'bottle-green': '#0F3B3A',
                'inverse-zielony': '#FF4F6E',
                'inverse-bezowy': '#25438E',
            },
},
    },
    darkMode: 'selector',
    plugins: [
        nextui({
            themes: {
                dark: {
                    colors: {
                        primary: {
                            DEFAULT: "#00B091",
                            foreground: "#000000",
                        },
                        secondary: {
                            DEFAULT: "#006FEE",
                            foreground: "#000000",
                        },
                    },
                },
                light: {
                    colors: {
                        primary: {
                            DEFAULT: "#00B091",
                            foreground: "#ffffff",
                        },
                        secondary: {
                            DEFAULT: "#006FEE",
                            foreground: "#ffffff",
                        },
                    }
                }
            },
        })
    ],
}