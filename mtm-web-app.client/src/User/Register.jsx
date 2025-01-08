import React, { useState } from 'react';
import { Modal, ModalBody, ModalContent, Button, Input, Link, InputOtp } from '@nextui-org/react';
import GoogleLogin from './GoogleLogin'
import { useNavigate } from "react-router-dom";
import { EyeFilledIcon } from '../Icons/EyeFilledIcon';
import { EyeSlashFilledIcon } from '../Icons/EyeSlashFilledIcon';
import { instance, getClaimsFromToken } from '../Helpers'
import Or from '../Or'
import { translations } from '../lang'

export default (props) => {
    const navigate = useNavigate();
    const [step, setStep] = useState(1); // kroki rejestracji
    //obsługa danych w formularzu
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        code: '',
        password: '',
        confirmPassword: '',
    });
    const [loading, setLoading] = useState(false);
    //podgląd haseł
    const [isVisible1, setIsVisible1] = useState(false);
    const [isVisible2, setIsVisible2] = useState(false);
    //obsługa błędów
    const [errorMessage, setErrorMessage] = useState({
        name: translations.giveValidName,
        email: translations.giveValidEmail,
        code: translations.giveValidCode,
        password: translations.giveValidPassword,
        confirmPassword: translations.giveValidConfirmPassword,
    });
    const [formValidation, setFormValidation] = useState({
        name: false,
        email: false,
        password: false,
        confirmPassword: false,
        code: false
    });
    const [canProceed, setCanProceed] = useState(false);
    //wskazówki do hasła
    const [passwordHints, setPasswordHints] = useState({
        uppercase: false,
        lowercase: false,
        specialCharacter: false,
        digit: false,
        length: false
    });

    const toggleVisibility1 = () => setIsVisible1(!isVisible1);
    const toggleVisibility2 = () => setIsVisible2(!isVisible2);

    //validacja emaila i hasła
    const validateEmail = (value) => value.match(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/i);
    const validatePassword = (value) => value.match(/^(?=.*[a-zążźśęćłóń])(?=.*[A-ZĄŻŹĆŃÓĘŚŁ])(?=.*\d)(?=.*[^A-Za-z0-9ĄŻŹĆŃÓĘŚŁążźśęćłóń ]).{8,}$/gmu);

    const handleChange = (e) => {
        if ((e.target.name === "password" || e.target.name === "confirmPassword") && e.target.value.includes(" ")) {
            return;
        }
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handleKeyDown = (e) => { if (e.key === 'Enter') handleNext() };

    const redirect = (where) => {
        props.registerModal.onClose();
        setTimeout(() => {
            navigate(where);
        }, 300);
    }

    //sprawdzanie imienia i nazwiska
    React.useMemo(() => {
        if (formData.name !== '')
            setFormValidation({ ...formValidation, name: false });
    }, [formData.name]);

    //sprawdzanie maila
    React.useMemo(() => {
        if (formData.email === "") return;
        setFormValidation({
            ...formValidation,
            email: validateEmail(formData.email) ? false : true
        });
    }, [formData.email]);

    //sprawdzanie kodu weryfikacyjnego
    React.useMemo(() => {
        if (formData.code !== "") {
            setFormValidation({
                ...formValidation,
                code: false
            });
        }
    }, [formData.code]);

    //sprawdzanie hasła
    React.useMemo(() => {
        if (formData.password !== '')
            setFormValidation({ ...formValidation, password: false });
        setPasswordHints({
            uppercase: !formData.password.match(/[A-ZĄŻŹĆŃÓĘŚŁ]/),
            lowercase: !formData.password.match(/[a-zążźśęćłóń]/),
            specialCharacter: !formData.password.match(/[^A-Za-z0-9ĄŻŹĆŃÓĘŚŁążźśęćłóń ]/u),
            digit: !formData.password.match(/\d/),
            length: !formData.password.match(/^.{8,}$/)
        })
    }, [formData.password]);

    //sprawdzanie 2 hasła
    React.useMemo(() => {
        if (formData.confirmPassword !== '')
            setFormValidation({ ...formValidation, confirmPassword: false });
    }, [formData.confirmPassword]);

    //czyszczenie pól na wyjściu
    const handleClose = () => {
        setStep(1);
        setFormData({
            name: '',
            email: '',
            code: '',
            password: '',
            confirmPassword: ''
        });
        setFormValidation({
            name: false,
            email: false,
            code: false,
            password: false,
            confirmPassword: false
        });
        setIsVisible1(false);
        setIsVisible2(false);
        props.registerModal.onClose();
    }

    React.useEffect(() => {
        setCanProceed(!Object.values(formValidation).includes(true));
    }, [formValidation]);

    //nawigacja
    const handleNext = async () => {
        if (!canProceed) return;
        if (step === 1) {
            setStep(2);
        } else if (step === 2) {
            const isNameValid = formData.name !== '';
            const isEmailValid = formData.email !== '' || validateEmail(formData.email);
            setFormValidation({
                ...formValidation,
                name: !isNameValid,
                email: !isEmailValid
            });

            if (isNameValid && isEmailValid) {
                setLoading(true);
                await instance.get("auth/is-name-free/" + formData.email)
                    .then((res) => {
                        if (res.status === 200) {
                            setStep(3);
                            instance.post("auth/send-otp", {
                                email: formData.email,
                                name: formData.name
                            });
                        }
                        else if (res.status === 409) {
                            setFormValidation({
                                ...formValidation,
                                email: true
                            });
                            setErrorMessage({
                                ...errorMessage,
                                email: res.data
                            });
                        }
                    }).catch(err => {
                        console.error(err);
                    }).finally(() => {
                        setLoading(false);
                    });
            }
        }
        else if (step === 3) {
            const isValid = formData.code !== "" || formData.code.length === 6;
            setFormValidation({
                ...formValidation,
                code: !isValid
            });

            if (isValid) {
                setLoading(true);
                await instance.post("auth/check-otp", {
                    email: formData.email,
                    code: formData.code
                }).then((res) => {
                    if (res.status === 200) {
                        setStep(4);
                    }
                    else {
                        setFormValidation({
                            ...formValidation,
                            code: true
                        });
                        setErrorMessage({
                            ...errorMessage,
                            code: res.data
                        })
                    }
                }).catch(err => {
                    console.error(err);
                }).finally(() => {
                    setLoading(false);
                });
            }
        }
        else if (step === 4) {
            const isPwdValid = formData.password !== '' || validatePassword(formData.password);
            const isCPwdValid = formData.password === formData.confirmPassword;
            setFormValidation({
                ...formValidation,
                password: !isPwdValid,
                confirmPassword: !isCPwdValid
            });

            if (isPwdValid && isCPwdValid) {
                setLoading(true);
                await instance.post("auth/register", {
                    password: formData.password
                }).then((res) => {
                    if (res.status === 200) {
                        props.onLogin(getClaimsFromToken(res.data));
                        handleClose();
                    }
                    else {
                        console.error(res.data);
                    }
                }).catch(err => {
                    console.error(err);
                }).finally(() => {
                    setLoading(false);
                });
            }
        }
    };

    const handleBack = () => {
        if (step === 3) setFormData({
            ...formData,
            code: ''
        });
        setStep((step) => step - 1);
    };

    return (
        <Modal
            isDismissable={false}
            isOpen={props.registerModal.isOpen}
            onOpenChange={props.registerModal.onOpenChange}
            onClose={handleClose}
            backdrop="opaque"
            size="5xl"
            placement="center"
            className="bg-white dark:bg-black sm:h-5/6 p-0"
            hideCloseButton={true}
            classNames={{
                backdrop: "bg-gradient-to-t from-zinc-900 to-zinc-900/10 backdrop-opacity-20"
            }}
        >
            <ModalContent>
                <ModalBody className="p-0">
                    <div className="sm:grid sm:grid-cols-2 sm:gap-4 place-content-center sm:place-content-stretch h-full w-full m-0">
                        <Button isIconOnly={true} className="absolute t-2 right-2 z-10 bg-transparent text-2xl text-black dark:text-white sm:text-white" radius="full" tabIndex={0} onPress={handleClose}>✖</Button>
                        <div className="place-self-center w-full">
                            {step !== 1 && <Button className="absolute left-4 top-1 bg-transparent text-4xl font-black text-black dark:text-white" tabIndex={1} radius="full" onPress={handleBack} isIconOnly={true}>←</Button>}
                            <div className="p-4 sm:p-12 place-center w-full h-full">
                                {step === 1 && (
                                    <>
                                        <h2 className="poppins-bold text-center text-black dark:text-white" style={{ fontSize: "1.8em" }}>{translations.joinUs}</h2>

                                        <GoogleLogin modalToCloseOnLogin={props.registerModal} tabIndex={3} onLogin={props.onLogin} />

                                        <Or />
                                        <Button onPress={handleNext} tabIndex={4} className="text-white text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" radius="full" variant="solid" fullWidth="true">
                                            {translations.register}
                                        </Button>
                                        <h5 className="poppins-light text-black dark:text-white text-center text-sm mt-4">{translations.regulations} <Link tabIndex={5} className="poppins-light text-teal-500 hover:text-teal-700 text-sm cursor-pointer" onPress={() => { redirect("./tos") }}>{translations.tosAccusative}</Link>, <Link tabIndex={6} className="poppins-light text-teal-500 hover:text-teal-700 text-sm cursor-pointer" onPress={() => { redirect("./privacy") }}>{translations.privacyPolicyAccusative}</Link> {translations.and} <Link tabIndex={7} className="poppins-light text-teal-500 hover:text-teal-700 text-sm cursor-pointer" onPress={() => { redirect("./cookies") }}>{translations.useOfCookiesAccusative}</Link></h5>
                                        <h4 className="poppins-regular text-black dark:text-white text-center mt-10">{translations.alreadyHaveAnAccount} <Link tabIndex={8} className="cursor-pointer bg-gradient-to-r from-gradient-zielony to-gradient-bezowy hover:from-gradient-zielony hover:to-gradient-bezowy inline-block text-transparent hover:text-transparent bg-clip-text" onPress={props.changeModal}>{translations.login}</Link></h4>
                                    </>
                                )}
                                {step === 2 && (
                                    <form onSubmit={(e) => e.preventDefault()} method="POST" className="w-full">
                                        <Input
                                            autoFocus
                                            tabIndex={3}
                                            variant="underlined"
                                            radius="full"
                                            name="name"
                                            autoComplete="name"
                                            label={translations.name}
                                            placeholder={translations.typeName}
                                            value={formData.name}
                                            onChange={handleChange}
                                            className="my-4 w-full text-black dark:text-white"
                                            isInvalid={formValidation.name}
                                            errorMessage={errorMessage.name}
                                        />
                                        <Input
                                            tabIndex={4}
                                            variant="underlined"
                                            radius="full"
                                            name="email"
                                            autoComplete="email username"
                                            label={translations.email}
                                            type="email"
                                            placeholder={translations.typeEmail}
                                            value={formData.email}
                                            onChange={handleChange}
                                            onKeyDown={handleKeyDown}
                                            className="my-4 w-full text-black dark:text-white"
                                            isInvalid={formValidation.email}
                                            errorMessage={errorMessage.email}
                                        />
                                        <Button tabIndex={5} onPress={handleNext} isLoading={loading} radius="full" aria-label="Dalej" className="text-white text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy w-full">{translations.next}</Button>
                                    </form>
                                )}
                                {step === 3 && (
                                    <form onSubmit={(e) => e.preventDefault()} method="POST" className="w-full flex flex-col items-center justify-center">
                                        <h5 className="text-black dark:text-white text-center mb-4">
                                            {translations.typeCodeSentToEmail} {formData.email}
                                        </h5>
                                        <div className="w-full flex justify-center">
                                            <InputOtp
                                                autoFocus
                                                length={6}
                                                tabIndex={3}
                                                name="code"
                                                autoComplete="one-time-code"
                                                label={translations.verificationCode}
                                                placeholder={translations.typeCode}
                                                value={formData.code}
                                                onChange={handleChange}
                                                onKeyDown={handleKeyDown}
                                                className="flex justify-center gap-2" /* Kluczowe style dla pól OTP */
                                                isInvalid={formValidation.code}
                                                errorMessage={errorMessage.code}
                                            />
                                        </div>
                                        <Button
                                            tabIndex={4}
                                            onPress={handleNext}
                                            isLoading={loading}
                                            radius="full"
                                            aria-label="Dalej"
                                            className="text-white text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy w-full mt-4"
                                        >
                                            {translations.next}
                                        </Button>
                                    </form>
                                )}
                                {step === 4 && (
                                    <form onSubmit={(e) => e.preventDefault()} method="POST" className="w-full">
                                        <Input
                                            autoFocus
                                            tabIndex={3}
                                            variant="underlined"
                                            radius="full"
                                            name="password"
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
                                            value={formData.password}
                                            onChange={handleChange}
                                            className="my-4 w-full"
                                            isInvalid={formValidation.password}
                                            errorMessage={errorMessage.password}
                                        />
                                        <span>{translations.passwordMustContain}</span><br />
                                        <span className={`${passwordHints.uppercase ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.uppercase ? "❌" : "✅"} {translations.uppercase}</span><br />
                                        <span className={`${passwordHints.lowercase ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.lowercase ? "❌" : "✅"} {translations.lowercase}</span><br />
                                        <span className={`${passwordHints.digit ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.digit ? "❌" : "✅"} {translations.digit}</span><br />
                                        <span className={`${passwordHints.specialCharacter ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.specialCharacter ? "❌" : "✅"} {translations.specialChar}</span><br />
                                        <span className={`${passwordHints.length ? 'dark:text-white text-black' : 'text-[#00d26a]'}`}>{passwordHints.length ? "❌" : "✅"} {translations.length}</span>
                                        <Input
                                            tabIndex={4}
                                            variant="underlined"
                                            radius="full"
                                            name="confirmPassword"
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
                                            value={formData.confirmPassword}
                                            onChange={handleChange}
                                            onKeyDown={handleKeyDown}
                                            className="my-4 w-full"
                                            isInvalid={formValidation.confirmPassword}
                                            errorMessage={errorMessage.confirmPassword}
                                        />
                                        <Button onPress={handleNext} isLoading={loading} radius="full" className="text-white text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy w-full">{translations.register}</Button>
                                    </form>
                                )}
                            </div>
                        </div>
                        <div className="hidden sm:block bg-register-image dark:bg-register-image-dark bg-no-repeat bg-cover bg-center will-change-transform" />
                    </div>
                </ModalBody>
            </ModalContent>
        </Modal>
    );
};