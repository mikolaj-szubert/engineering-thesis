import React, { useState } from 'react';
import { Modal, ModalBody, ModalHeader, ModalContent, Button, Input, Link } from '@nextui-org/react';
import GoogleLogin from './GoogleLogin'
import { EyeFilledIcon } from '../Icons/EyeFilledIcon';
import { EyeSlashFilledIcon } from '../Icons/EyeSlashFilledIcon';
import { instance, getClaimsFromToken } from '../Helpers'
import Or from '../Or'
import { translations } from '../lang'

const LoginModal = (props) => {
    const [isResetPassword, setIsResetPassword] = useState(false);
    const [step, setStep] = useState(1);
    const [canProceed, setCanProceed] = useState(false);
    const [isVisible, setIsVisible] = useState(false);
    const [isVisible1, setIsVisible1] = useState(false);
    const [loading, setLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    const [btnError, setBtnError] = useState();
    const [formData, setFormData] = useState({
        email: '',
        password: '',
        code: '',
        newPassword: '',
        confirmNewPassword: ''
    });
    const [formValidation, setFormValidation] = useState({
        email: false,
        password: false,
        code: false,
        newPassword: false,
        confirmNewPassword: false
    });
    const [errorMessage, setErrorMessage] = useState({
        email: translations.giveValidEmail,
        code: translations.giveValidCode,
        password: translations.givePassword,
        newPassword: translations.giveValidPassword,
        confirmNewPassword: translations.giveValidConfirmPassword,
    });
    const [passwordHints, setPasswordHints] = useState({
        uppercase: false,
        lowercase: false,
        specialCharacter: false,
        digit: false,
        length: false
    });

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handleRecoveryKeyDown = (e) => { if (e.key === 'Enter') handleNextStep() };
    const handleRecoveryEndKeyDown = (e) => { if (e.key === 'Enter') handleResetPassword() };
    const handleLoginKeyDown = (e) => { if (e.key === 'Enter') handleLogin() };

    const toggleVisibility = () => setIsVisible((isVisible) => !isVisible);
    const toggleVisibility1 = () => setIsVisible1((isVisible) => !isVisible);
    const validateEmail = (value) => value.match(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/i);
    const validatePassword = (value) => value.match(/^(?=.*[a-zążźśęćłóń])(?=.*[A-ZĄŻŹĆŃÓĘŚŁ])(?=.*\d)(?=.*[^A-Za-z0-9ĄŻŹĆŃÓĘŚŁążźśęćłóń ]).{8,}$/gmu);

    React.useMemo(() => {
        setIsError(false);
        if (validateEmail(formData.email)) setFormValidation({
            ...formValidation,
            email: false,
        })
    }, [formData.email]);

    React.useMemo(() => {
        setIsError(false);
        setFormValidation({
            ...formValidation,
            password: false,
        });
        setErrorMessage({
            ...errorMessage,
            password: translations.giveValidPassword,
        });
    }, [formData.password]);

    React.useMemo(() => {
        setIsError(false);
        setFormValidation({
            ...formValidation,
            code: false
        });
    }, [formData.code]);

    React.useMemo(() => {
        if (formData.newPassword !== '') setFormValidation({
            ...formValidation,
            newPassword: false
        });
        setPasswordHints({
            uppercase: !formData.newPassword.match(/[A-ZĄŻŹĆŃÓĘŚŁ]/),
            lowercase: !formData.newPassword.match(/[a-zążźśęćłóń]/),
            specialCharacter: !formData.newPassword.match(/[^A-Za-z0-9ĄŻŹĆŃÓĘŚŁążźśęćłóń ]/u),
            digit: !formData.newPassword.match(/\d/),
            length: !formData.newPassword.match(/^.{8,}$/)
        });
    }, [formData.newPassword]);

    React.useMemo(() => {
        if (formData.confirmNewPassword !== '') setFormValidation({
            ...formValidation,
            confirmNewPassword: false
        });
        if (formData.confirmNewPassword !== formData.newPassword) setFormValidation({
            ...formValidation,
            confirmNewPassword: true
        });
    }, [formData.confirmNewPassword]);

    React.useEffect(() => {
        setCanProceed(!Object.values(formValidation).includes(true));
    }, [formValidation]);

    const handleForgotPassword = () => {
        setFormData({
            email: '',
            password: '',
            code: '',
            newPassword: '',
            confirmNewPassword: ''
        });
        setFormValidation({
            email: false,
            password: false,
            code: false,
            newPassword: false,
            confirmNewPassword: false
        });
        setIsError(false)
        setIsVisible(false);
        setIsVisible1(false);
        setIsResetPassword(true);
        setStep(1);
    };

    const handleClose = () => {
        setStep(1);
        setIsError(false);
        setFormData({
            email: '',
            password: '',
            code: '',
            newPassword: '',
            confirmNewPassword: ''
        });
        setFormValidation({
            email: false,
            password: false,
            code: false,
            newPassword: false,
            confirmNewPassword: false
        });
        setIsVisible(false);
        setIsVisible1(false);
        setIsResetPassword(false);
        props.loginModal.onClose();
    }

    const handleLogin = () => {
        if (!canProceed) return;
        const isEmailValid = formData.email !== '' && validateEmail(formData.email);
        const isPasswordValid = formData.password !== '';

        setFormValidation({
            ...formValidation,
            email: !isEmailValid,
            password: !isPasswordValid
        })

        if (isEmailValid && isPasswordValid) {
            setLoading(true);
            instance.post("auth/login", { email: formData.email, password: formData.password })
                .then((res) => {
                    if (res.status === 200) {
                        props.onLogin(getClaimsFromToken(res.data));
                        setIsError(false);
                        handleClose();
                    }
                    else {
                        setBtnError(res.data);
                        setIsError(true);
                    }
                })
                .catch((err) => {
                    console.error(err);
                })
                .finally(() => {
                    setLoading(false);
                });
        }
    };

    const sendVerificationCode = (email) => {
        instance.post('auth/send-otp', { email });
    }

    const handleNextStep = () => {
        if (step === 1 && isResetPassword) {
            const isEmailValid = formData.email !== '' && validateEmail(formData.email);

            setFormValidation({
                ...formValidation,
                email: !isEmailValid
            })

            if (isEmailValid) {
                sendVerificationCode(formData.email);
                setStep(2);
            }
        } else if (step === 2 && isResetPassword) {
            const isCodeValid = formData.code !== '' && formData.code.length === 6;

            setFormValidation({
                ...formValidation,
                code: !isCodeValid
            })

            if (isCodeValid) {
                setLoading(true);
                instance.post("auth/check-otp", { email: formData.email, code: formData.code })
                    .then((res) => {
                        if (res.status === 200) {
                            setIsError(false);
                            setStep(3);
                        }
                        else {
                            setBtnError(res.data);
                            setIsError(true);
                        }
                    })
                    .catch((err) => {
                        console.log(err);
                    })
                    .finally(() => {
                        setLoading(false);
                    });
            }
        }
    };

    const handleBack = () => {
        setIsError(false);
        if (step === 1) {
            setFormData({
                email: '',
                password: '',
                code: '',
                newPassword: '',
                confirmNewPassword: ''
            });
            setFormValidation({
                email: false,
                password: false,
                code: false,
                newPassword: false,
                confirmNewPassword: false
            });
            setIsVisible(false);
            setIsVisible1(false);
            setIsResetPassword(false);
        }
        else if (step === 3) {
            setFormData({
                ...formData,
                code: '',
            });
            setFormValidation({
                ...formValidation,
                code: false,
            });
        }
        setStep((step) => step - 1);
    };

    const handleResetPassword = () => {
        const isPasswordValid = formData.newPassword !== '' && validatePassword(formData.newPassword);
        const isConfirmPasswordValid = formData.confirmNewPassword !== '' && formData.newPassword === formData.confirmNewPassword;

        setFormValidation({
            ...formValidation,
            newPassword: !isPasswordValid,
            confirmNewPassword: !isConfirmPasswordValid
        });

        if (isPasswordValid && isConfirmPasswordValid) {
            setLoading(true);
            instance.put("account/manage/update", { password: formData.newPassword })
                .then((res) => {
                    if (res.status === 200) {
                        setIsError(false);
                        setFormData({
                            email: '',
                            password: '',
                            code: '',
                            newPassword: '',
                            confirmNewPassword: ''
                        });
                        setFormValidation({
                            email: false,
                            password: false,
                            code: false,
                            newPassword: false,
                            confirmNewPassword: false
                        });
                        setIsVisible(false);
                        setIsVisible1(false);
                        setIsResetPassword(false);
                        setIsResetPassword(false);
                    }
                    else {
                        setBtnError(res.data);
                        setIsError(true);
                    }
                })
                .catch((err) => {
                    console.error(err);
                })
                .finally(() => {
                    setLoading(false);
                });
        }
    };

    return (
        <Modal
            isDismissable={false}
            isOpen={props.loginModal.isOpen}
            onOpenChange={props.loginModal.onOpenChange}
            onClose={handleClose}
            backdrop="opaque"
            size="xl"
            placement="center"
            className="bg-white dark:bg-black p-0"
            hideCloseButton={true}
            classNames={{
                backdrop: "bg-gradient-to-t from-zinc-900 to-zinc-900/10 backdrop-opacity-20"
            }}
        >
            <ModalContent>
                <ModalHeader>
                    <Button isIconOnly={true} className="absolute t-2 right-2 z-10 bg-transparent text-2xl dark:text-white text-black" radius="full" tabIndex={0} onPress={handleClose}>✖</Button>
                    <span>{isResetPassword ? `${translations.resetingPasswordStep} ${step}` : translations.signIn}</span>
                </ModalHeader>
                <ModalBody>
                    {!isResetPassword && (
                        <form onSubmit={(e) => e.preventDefault()}>
                            <GoogleLogin modalToCloseOnLogin={props.loginModal} tabIndex={3} onLogin={props.onLogin} />
                            <Or />
                            <Input
                                autoFocus
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
                                className="my-4 w-full text-black dark:text-white"
                                isInvalid={formValidation.email}
                                errorMessage={errorMessage.email}
                            />
                            <Input
                                tabIndex={5}
                                variant="underlined"
                                radius="full"
                                name="password"
                                autoComplete="current-password"
                                label={translations.password}
                                placeholder={translations.typePassword}
                                endContent={
                                    <button className="focus:outline-none" type="button" onClick={toggleVisibility} aria-label={translations.changePasswordVisibilty}>
                                        {isVisible ? (
                                            <EyeFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                                        ) : (
                                            <EyeSlashFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                                        )}
                                    </button>
                                }
                                type={isVisible ? "text" : "password"}
                                value={formData.password}
                                onChange={handleChange}
                                className="my-4 w-full"
                                isInvalid={formValidation.password}
                                errorMessage={errorMessage.password}
                                onKeyDown={handleLoginKeyDown}
                            />
                            <Link tabIndex={6} className="cursor-pointer" onPress={handleForgotPassword}>{translations.forgottenPassword}</Link>
                            <Button onPress={handleLogin} isLoading={loading} tabIndex={7} isDisabled={isError} className={`text-white my-4 text-lg font-medium ${!isError ? "bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" : "bg-red-800"}`} radius="full" variant="solid" fullWidth="true">
                                {isError ? btnError : translations.login}
                            </Button>
                            <h4 className="poppins-regular text-black dark:text-white text-center mt-10">{translations.dontHaveAccount} <Link tabIndex={8} className="cursor-pointer bg-gradient-to-r from-gradient-zielony to-gradient-bezowy hover:from-gradient-zielony hover:to-gradient-bezowy inline-block text-transparent hover:text-transparent bg-clip-text" onPress={() => { props.changeModal(); handleClose() }}>{translations.register}</Link></h4>
                        </form>
                    )}

                    {isResetPassword && step === 1 && (
                        <form onSubmit={(e) => e.preventDefault()}>
                            <Input
                                autoFocus
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
                                className="my-4 w-full text-black dark:text-white"
                                isInvalid={formValidation.email}
                                errorMessage={errorMessage.email}
                                onKeyDown={handleRecoveryKeyDown}
                            />
                            <Button tabIndex={5} radius="full" variant="solid" fullWidth="true" className="text-white my-2 text-lg font-medium bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" onPress={handleNextStep}>
                                {translations.sendCode}
                            </Button>
                            <Button tabIndex={6} radius="full" variant="solid" fullWidth="true" className="text-white my-2 text-lg font-medium" onPress={handleBack}>
                                {translations.back}
                            </Button>
                        </form>
                    )}

                    {isResetPassword && step === 2 && (
                        <form onSubmit={(e) => e.preventDefault()}>
                            <h5 className="text-black dark:text-white text-center">{translations.typeCodeSentToEmail} {formData.email}</h5>
                            <Input
                                autoFocus
                                tabIndex={4}
                                name="code"
                                variant="underlined"
                                radius="full"
                                autoComplete="one-time-code"
                                label={translations.verificationCode}
                                placeholder={translations.typeCode}
                                value={formData.code}
                                onChange={handleChange}
                                isInvalid={formValidation.code}
                                errorMessage={errorMessage.code}
                                onKeyDown={handleRecoveryKeyDown}
                            />
                            <Button tabIndex={5} radius="full" variant="solid" fullWidth="true" isLoading={loading} isDisabled={isError} className={`text-white my-2 text-lg font-medium ${!isError ? "bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" : "bg-red-800"}`} onPress={handleNextStep}>
                                {isError ? btnError : translations.next}
                            </Button>
                            <Button tabIndex={6} radius="full" variant="solid" fullWidth="true" className="text-white my-2 text-lg font-medium" onPress={handleBack}>
                                {translations.back}
                            </Button>
                        </form>
                    )}

                    {isResetPassword && step === 3 && (
                        <form onSubmit={(e) => e.preventDefault()}>
                            <Input
                                autoFocus
                                tabIndex={4}
                                variant="underlined"
                                radius="full"
                                name="newPassword"
                                autoComplete="off"
                                label={translations.password}
                                placeholder={translations.typePassword}
                                endContent={
                                    <button tabIndex={5} className="focus:outline-none" type="button" onClick={toggleVisibility} aria-label={translations.changePasswordVisibilty}>
                                        {isVisible ? (
                                            <EyeFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                                        ) : (
                                            <EyeSlashFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                                        )}
                                    </button>
                                }
                                type={isVisible ? "text" : "password"}
                                value={formData.newPassword}
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
                                tabIndex={6}
                                variant="underlined"
                                radius="full"
                                name="confirmNewPassword"
                                autoComplete="off"
                                label={translations.confirmPassword}
                                placeholder={translations.repeatPassword}
                                endContent={
                                    <button tabIndex={7} className="focus:outline-none" type="button" onClick={toggleVisibility1} aria-label={translations.changePasswordVisibilty}>
                                        {isVisible1 ? (
                                            <EyeFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                                        ) : (
                                            <EyeSlashFilledIcon className="text-2xl text-default-400 pointer-events-none" />
                                        )}
                                    </button>
                                }
                                type={isVisible1 ? "text" : "password"}
                                value={formData.confirmNewPassword}
                                onChange={handleChange}
                                onKeyDown={handleRecoveryEndKeyDown}
                                className="my-4 w-full"
                                isInvalid={formValidation.confirmNewPassword}
                                errorMessage={errorMessage.confirmNewPassword}
                            />
                            <Button tabIndex={8} radius="full" variant="solid" fullWidth="true" isLoading={loading} isDisabled={isError} className={`text-white my-2 text-lg font-medium ${!isError ? "bg-gradient-to-r from-gradient-zielony to-gradient-bezowy" : "bg-red-800"}`} onPress={handleResetPassword}>
                                {isError ? btnError : translations.resetPassword}
                            </Button>
                            <Button tabIndex={9} radius="full" variant="solid" fullWidth="true" className="text-white my-2 text-lg font-medium" onPress={handleBack}>
                                {translations.back}
                            </Button>
                        </form>
                    )}
                </ModalBody>
            </ModalContent>
        </Modal>
    );
};

export default LoginModal;