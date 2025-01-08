import React from 'react'
import { useOutletContext } from 'react-router-dom'
import { instance } from '../Helpers'
import { translations } from '../lang'
import { Button, Input, Avatar } from '@nextui-org/react'
import { EyeFilledIcon } from "../Icons/EyeFilledIcon"
import { EyeSlashFilledIcon } from "../Icons/EyeSlashFilledIcon"
import { toast } from 'react-toastify'

export default () => {
    const [selectedFile, setSelectedFile] = React.useState(null);
    const [isLoading, setIsLoading] = React.useState(false);
    const [deleteImage, setDeleteImage] = React.useState(false);
    const [responseData, setResponseData] = React.useState('');
    const [isVisible1, setIsVisible1] = React.useState(false);
    const [isVisible2, setIsVisible2] = React.useState(false);
    const { onLogin, user } = useOutletContext();
    const [picture, setPicture] = React.useState(user.picture !== "True" ? user.picture : "/api/images/user");
    const [value, setValue] = React.useState({
        email: user.email || '',
        name: user.name || '',
        password: '',
        confirmPassword: ''
    });
    //obsługa błędów
    const [errorMessage, _] = React.useState({
        email: translations.giveValidEmail,
        password: translations.giveValidPassword,
        confirmPassword: translations.giveValidConfirmPassword,
    });
    const [formValidation, setFormValidation] = React.useState({
        email: false,
        password: false,
        confirmPassword: false,
    });
    //wskazówki do hasła
    const [passwordHints, setPasswordHints] = React.useState({
        uppercase: false,
        lowercase: false,
        specialCharacter: false,
        digit: false,
        length: false
    });

    const fileInputRef = React.useRef(null);

    //sprawdza czy użytkownik ma zdjęcie profilowe
    React.useEffect(() => {
        instance.get(picture.split('api')[1]).then(res => {
            if (res.status !== 200) setPicture(null);
        });
    }, []);

    const toggleVisibility1 = () => setIsVisible1(!isVisible1);
    const toggleVisibility2 = () => setIsVisible2(!isVisible2);

    const updateBtnClick = async () => {
        setIsLoading(true);
        if (selectedFile !== null) {
            const formData = new FormData();
            formData.append('file', selectedFile);
            await instance.post('images/user', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            }).then((res) => {
                if (res.status === 200) {
                    setResponseData(res.data);
                    onLogin({ ...user, picture: URL.createObjectURL(selectedFile) })
                }
            }).catch(err => console.error(err));
        }

        if (deleteImage === true) {
            instance.delete('images/user')
                .then(res => {
                    if (res.status === 200) {
                        setResponseData(res.data);
                    }
                })
                .catch(err => console.error(err));
        }

        const email = value.email !== '' && value.email !== user.email ? value.email : null;
        const name = value.name !== '' && value.name !== user.name ? value.name : null;
        const password = value.password !== '' && validatePassword(value.password) ? value.password : null;
        const confirmPassword = value.confirmPassword !== '' ? value.confirmPassword : null;
        if (email !== null || name !== null || (password !== null && confirmPassword !== null)) {
            await instance.put('account/manage/update', { email, name, password })
                .then((res) => {
                    setResponseData(res.data);
                    onLogin({ ...user, email: email, name: name })
                })
                .catch((err) => console.error(err));
        }
        setIsLoading(false);
    }

    //validacja emaila i hasła
    const validateEmail = (value) => value.match(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/i);
    const validatePassword = (value) => value.match(/^(?=.*[a-zążźśęćłóń])(?=.*[A-ZĄŻŹĆŃÓĘŚŁ])(?=.*\d)(?=.*[^A-Za-z0-9ĄŻŹĆŃÓĘŚŁążźśęćłóń ]).{8,}$/gmu);

    const handleDeleteImage = () => {
        setSelectedFile(null);
        if (typeof picture === "string") {
            setDeleteImage(true);
            setPicture(null);
        }
    }

    //sprawdzanie maila
    React.useMemo(() => {
        if (value.email === "") return;
        setFormValidation({
            ...value,
            email: validateEmail(value.email) ? false : true
        });
    }, [value.email]);

    //sprawdzanie hasła
    React.useMemo(() => {
        if (value.password !== '')
            setFormValidation({ ...value, password: false });
        setPasswordHints({
            uppercase: !value.password.match(/[A-ZĄŻŹĆŃÓĘŚŁ]/),
            lowercase: !value.password.match(/[a-zążźśęćłóń]/),
            specialCharacter: !value.password.match(/[^A-Za-z0-9ĄŻŹĆŃÓĘŚŁążźśęćłóń ]/u),
            digit: !value.password.match(/\d/),
            length: !value.password.match(/^.{8,}$/)
        })
    }, [value.password]);

    const handleAvatarClick = () => {
        fileInputRef.current.click();
    };

    const handleFileChange = (event) => {
        const file = event.target.files[0];
        if (file && (file.type === 'image/png' || file.type === 'image/jpeg')) {
            setSelectedFile(file);
            setDeleteImage(false);
        } else if (file && (file.type !== 'image/png' && file.type !== 'image/jpeg')) {
            toast.error('Proszę wybrać plik w formacie PNG lub JPG.');
        }
    };

    return (
        <form onSubmit={(e) => e.preventDefault()} method="POST" className="px-4 md:px-24 py-24">
            <div className="relative mx-auto mb-12 h-48 w-48">
                <Avatar
                    src={selectedFile ? URL.createObjectURL(selectedFile) : picture}
                    showFallback
                    isBordered
                    className="h-full w-full rounded-full cursor-pointer"
                    onClick={handleAvatarClick}
                />
                {(picture || selectedFile) && (
                    <Button
                        radius="full"
                        className="absolute text-lg bottom-4 right-4 text-red-500 bg-black dark:bg-white translate-x-1/4 translate-y-1/4"
                        onPress={handleDeleteImage}
                        isIconOnly
                    >
                        ✖
                    </Button>
                )}
            </div>
            <Input
                type="file"
                accept="image/png, image/jpeg"
                ref={fileInputRef}
                className="hidden"
                onChange={handleFileChange}
            />
            <Input
                autoFocus
                label="Name"
                variant="underlined"
                placeholder="Enter your name"
                type="text"
                value={value.name}
                onChange={(e) => {
                    setValue({
                        ...value,
                        name: e.target.value,
                    })
                }}
            />
            <Input
                label="Email"
                variant="underlined"
                placeholder="Enter your email"
                type="text"
                value={value.email}
                onChange={(e) => {
                    setValue({
                        ...value,
                        email: e.target.value,
                    })
                }}
            />
            <Input
                variant="underlined"
                radius="full"
                autoComplete="off"
                label={translations.password}
                placeholder={translations.typePassword}
                endContent={
                    <button className="focus:outline-none" type="button" onClick={toggleVisibility1} aria-label={translations.changePasswordVisibilty}>
                        {isVisible1 ? (
                            <EyeFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                        ) : (
                            <EyeSlashFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                        )}
                    </button>
                }
                type={isVisible1 ? "text" : "password"}
                value={value.password}
                onChange={(e) => {
                    setValue({
                        ...value,
                        password: e.target.value,
                    })
                }}
                isInvalid={formValidation.password}
                errorMessage={errorMessage.password}
            />
            <div className="my-4">
                <span>{translations.passwordMustContain}</span><br />
                <span className={`${passwordHints.uppercase ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.uppercase ? "❌" : "✅"} {translations.uppercase}</span><br />
                <span className={`${passwordHints.lowercase ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.lowercase ? "❌" : "✅"} {translations.lowercase}</span><br />
                <span className={`${passwordHints.digit ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.digit ? "❌" : "✅"} {translations.digit}</span><br />
                <span className={`${passwordHints.specialCharacter ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.specialCharacter ? "❌" : "✅"} {translations.specialChar}</span><br />
                <span className={`${passwordHints.length ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.length ? "❌" : "✅"} {translations.length}</span>
            </div>
            <Input
                variant="underlined"
                radius="full"
                autoComplete="off"
                label={translations.confirmPassword}
                placeholder={translations.repeatPassword}
                endContent={
                    <button className="focus:outline-none" type="button" onClick={toggleVisibility2} aria-label={translations.changePasswordVisibilty}>
                        {isVisible2 ? (
                            <EyeFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                        ) : (
                            <EyeSlashFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                        )}
                    </button>
                }
                type={isVisible2 ? "text" : "password"}
                value={value.confirmPassword}
                onChange={(e) => {
                    setValue({
                        ...value,
                        confirmPassword: e.target.value,
                    })
                }}
                isInvalid={formValidation.confirmPassword}
                errorMessage={errorMessage.confirmPassword}
            />
            <Button
                isLoading={isLoading}
                onPress={updateBtnClick}
                className="mt-12 text-white text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy"
                radius="full"
                variant="solid"
                fullWidth="true"
            >
                Update
            </Button>
            <p>{responseData}</p>
        </form>
    );
}