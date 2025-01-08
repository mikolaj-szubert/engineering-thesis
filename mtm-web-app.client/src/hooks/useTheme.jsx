import { useState, useEffect } from "react";

const useTheme = () => {
    const [isDarkMode, setIsDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const toggleDarkMode = () => setIsDarkMode((prev) => !prev);

    useEffect(() => {
        const theme = isDarkMode ? "dark" : "light";
        localStorage.setItem("theme", theme);
        document.documentElement.classList.toggle("dark", isDarkMode);
    }, [isDarkMode]);

    return { isDarkMode, toggleDarkMode };
};

export default useTheme;