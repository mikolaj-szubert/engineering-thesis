import "./CustomStyles/home.css"
import "./CustomStyles/fonts.css"
import Footer from './Footer.jsx'
import Helpers from './Helpers'
import { createSearchParams, useNavigate, useSearchParams } from 'react-router-dom'
import { useState, useEffect, memo } from 'react'
import { Helmet } from "react-helmet"
import { toast } from 'react-toastify'
import { translations } from './lang'
import { blurhashAsGradients } from 'blurhash-gradients'

const BlurredButton = memo((props) => {
    const [backgroundImage, setBackgroundImage] = useState("");

    useEffect(() => {
        const img = new Image();
        img.src = props.img;
        img.onload = () => {
            setBackgroundImage(
                `linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url(${props.img})`
            );
        };
    }, [props.img]);

    return (
        <div
            className={`relative bg-center w-[100dvw] md:w-[300px] h-[30dvh] md:h-[150px] bg-[length:100dvw] md:bg-[length:300px] overflow-hidden rounded-lg mt-4 md:mx-[10px] md:my-[20px] float-left object-cover bg-no-repeat cursor-pointer will-change-transform transition-opacity duration-500 ${backgroundImage ? "opacity-100" : "opacity-0"
                }`}
            style={{
                backgroundImage: backgroundImage,
            }}
            onClick={() =>
                props.navigate({
                    pathname: props.pathname,
                    search: `?${createSearchParams({
                        [props.searchParamName]: props.searchParam,
                    })}`,
                })
            }
        >
            <a className="poppins-regular unselectable absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 text-white text-[36px] hover:text-white">
                {props.city}
            </a>
        </div>
    );
});

const Restaurant = memo(({ navigate, isDarkMode }) => {
    const [resSrc, setResSrc] = useState(null);
    const css = blurhashAsGradients('L1F$340000001n9t-U0000tM?d~p');

    useEffect(() => {
        if (!resSrc) {
            const restaurantImage = new Image();
            restaurantImage.src = '/restaurants.png';
            restaurantImage.onload = () => {
                setResSrc(restaurantImage.src);
            };
        }
    }, [resSrc]); // Używamy resSrc w zależności, żeby ładować obrazek tylko raz

    const backgroundStyle = isDarkMode
        ? { backgroundImage: `linear-gradient(rgba(0, 0, 0, .75), rgba(0, 0, 0, .75)), url(${resSrc})` }
        : { backgroundImage: `linear-gradient(rgba(255, 255, 255, .25), rgba(255, 255, 255, .25)), url(${resSrc})` };

    return (
        <div className="right cursor-pointer" onClick={() => navigate("./restaurants")}>
            <div
                style={resSrc ? backgroundStyle : css}
                className="img right-img dark:brightness-100 brightness-75 dark:hover:brightness-200 hover:brightness-100 transition-all duration-700"
            >
                <h1 className="relative pt-[50dvh] pl-[23dvw] text-4xl text-black dark:text-white font-['Poppins'] font-medium absolute right-12">{translations.restaurants}</h1>
            </div>
        </div>
    );
});

const Hotel = memo(({ navigate, isDarkMode }) => {
    const [src, setSrc] = useState(null);
    const css = blurhashAsGradients('L8E{U$00~W00,+%g-pMw00yYT1xs');

    useEffect(() => {
        if (!src) {
            const hotelImage = new Image();
            hotelImage.src = '/hotels.png';
            hotelImage.onload = () => {
                setSrc(hotelImage.src);
            };
        }
    }, [src]);

    const backgroundStyle = isDarkMode
        ? { backgroundImage: `linear-gradient(rgba(0, 0, 0, .75), rgba(0, 0, 0, .75)), url(${src})` }
        : { backgroundImage: `linear-gradient(rgba(255, 255, 255, .25), rgba(255, 255, 255, .25)), url(${src})` };

    return (
        <div className="left cursor-pointer" onClick={() => navigate("./hotels")}>
            <div
                style={src ? backgroundStyle : css}
                className="img left-img dark:brightness-100 brightness-75 dark:hover:brightness-200 hover:brightness-100 transition-all duration-700"
            >
                <h1 className="pt-[50dvh] pl-[30dvw] text-4xl text-black dark:text-white font-['Poppins'] font-medium z-20 blur-none">
                    {translations.hotels}
                </h1>
            </div>
        </div>
    );
});

function Home({ onLogin, isDarkMode }) {
    const navigate = useNavigate();
    const [searchParams,] = useSearchParams();

    const checkOtp = async () => {
        const token = searchParams.get("token");
        if (token) {
            console.log(token);
            Helpers.instance.post('auth/check-otp', {
                code: token
            }).then((res) => {
                if (res.status === 200) {
                    toast.success(res.data, { autoClose: false });
                    const claims = Helpers.getClaimsFromToken(res.data);
                    onLogin(claims);
                }
                else {
                    toast.error(res.data, { autoClose: false });
                }
            }).catch((err) => {
                console.error(err);
            });
        }
    };

    useEffect(() => {
        checkOtp();
    }, []);

    return (
        <div className="main">
            <Helmet>
                <title>{translations.title.replace("{0}", translations.homePage)}</title>
            </Helmet>
            {/*Mobile*/}
            <div className="md:hidden h-[95dvh] w-screen">
                <div onClick={() => navigate("./hotels")} className="h-1/2 bg-hotels-image dark:bg-hotels-image-dark bg-center bg-cover relative">
                    <span className="text-black dark:text-white font-medium text-3xl absolute w-full top-[23vh] text-center">
                        {translations.hotels}
                    </span>
                </div>
                <div onClick={() => navigate("./restaurants")} className="h-1/2 bg-restaurants-image dark:bg-restaurants-image-dark bg-center bg-cover relative">
                    <span className="text-black dark:text-white font-medium text-3xl absolute w-full top-[23vh] text-center">
                        {translations.restaurants}
                    </span>
                </div>
            </div>
            {/*Desktop*/}
            <div className="view hidden md:flex">
                <Hotel navigate={navigate} isDarkMode={isDarkMode} />
                <Restaurant navigate={navigate} isDarkMode={isDarkMode} />
            </div>
            <div className="content">
                <BlurredButton pathname="/hotels" searchParamName="city" searchParam="Poznan" img="./poznan.png" city="Poznań" navigate={navigate} />
                <BlurredButton pathname="/hotels" searchParamName="city" searchParam="Warsaw" img="./warszawa.png" city="Warszawa" navigate={navigate} />
                <BlurredButton pathname="/hotels" searchParamName="city" searchParam="Gdansk" img="./gdansk.png" city="Gdańsk" navigate={navigate} />
                <BlurredButton pathname="/hotels" searchParamName="city" searchParam="Wroclaw" img="./wroclaw.png" city="Wrocław" navigate={navigate} />
            </div>
            <hr className="hidden md:block border-black dark:border-white" style={{ margin: "0 15vw" }} />
            <div className="content">
                <div className="md:hidden block poppins-regular text-center text-3xl mt-24 text-black dark:text-white" >{translations.reserveByType}</div>
                <BlurredButton pathname="/hotels" searchParamName="types" searchParam="Hotel" img="./hotel.png" city={translations.hotels} navigate={navigate} />
                <BlurredButton pathname="/hotels" searchParamName="types" searchParam="Apartament" img="./apartament.png" city={translations.apartment} navigate={navigate} />
                <BlurredButton pathname="/hotels" searchParamName="types" searchParam="House" img="./home.png" city={translations.house} navigate={navigate} />
                <div className="hidden md:block poppins-regular text-black dark:text-white" style={{ float: "left", width: "300px", height: "150px", margin: "20px 10px", fontSize: "30px", textAlign: "right" }}>{translations.reserveByType}</div>
            </div>
            <hr className="hidden md:block border-black dark:border-white" style={{ margin: "0 15vw" }} />
            <div className="content">
                <div className="poppins-regular text-black dark:text-white md:float-left md:w-[300px] md:h-[150px] mt-24 md:my-[20px] md:mx-[10px] text-center md:text-left text-3xl">{translations.recommendedCusines}</div>
                <BlurredButton pathname="/restaurants" searchParamName="cusines" searchParam="Chinese" img="./chinese.png" city={translations.Chinese} navigate={navigate} />
                <BlurredButton pathname="/restaurants" searchParamName="cusines" searchParam="American" img="./american.png" city={translations.American} navigate={navigate} />
                <BlurredButton pathname="/restaurants" searchParamName="cusines" searchParam="Italian" img="./italian.png" city={translations.Italian} navigate={navigate} />
            </div>
            <Footer />
        </div>
    );
}

export default memo(Home);
