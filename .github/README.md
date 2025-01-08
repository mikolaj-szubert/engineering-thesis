# MTM Web App
### MTM Project application for website.
Backend Stack  
[![Static Badge](https://img.shields.io/badge/v8.0-512BD4?logo=dotnet&logoColor=512BD4&labelColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) [![Static Badge](https://img.shields.io/badge/OpenAPI-gray?logo=openapiinitiative&logoColor=6BA539&labelColor=white&color=6BA539)](https://swagger.io/) [![Static Badge](https://img.shields.io/badge/JSON_Web_Token-gray?logo=jsonwebtokens&logoColor=black&labelColor=white&color=black)](https://jwt.io/)  

Frontend Stack  
[![Static Badge](https://img.shields.io/badge/Tailwind_CSS-06B6D4?logo=tailwindcss&labelColor=white)](https://tailwindcss.com/) [![Static Badge](https://img.shields.io/badge/Next_UI-black?logo=nextui&logoColor=black&labelColor=white)](https://nextui.org/) [![Static Badge](https://img.shields.io/badge/Framer-gray?logo=framer&logoColor=0055FF&labelColor=white&color=0055FF)](https://www.framer.com/) [![Static Badge](https://img.shields.io/badge/React-gray?logo=react&logoColor=61DAFB&labelColor=gray&color=61DAFB)](https://react.dev/) [![Static Badge](https://img.shields.io/badge/Vite-gray?logo=vite&logoColor=646CFF&labelColor=white&color=646CFF)](https://vitejs.dev/)

## Quick start guide
1. Modify ```appsettings.json``` file at MTM-Web-App.Server directory 
2. Install Entity Framework for project and create database by ```install.bat```
3. Run the application:
    - Dev mode (slower version with debugging): ```run-dev.bat```
    - Production mode (faster version without debugging): ```build-front.bat``` and then ```run-release.bat```

## Specification
### Documentation for technologies in use:
- [React](https://react.dev/reference/react)
- [Vite](https://vitejs.dev/guide/features.html)
- [Next UI](https://nextui.org/docs)
- [TailwindCSS](https://tailwindcss.com/docs)
- [Framer](https://www.framer.com/docs)
- [Leaflet](https://leafletjs.com/reference.html)
- [@react-oauth/google](https://www.npmjs.com/package/@react-oauth/google)
- [axios](https://axios-http.com/docs/intro)
- [blurhash-gradients](https://www.npmjs.com/package/blurhash-gradients)
- [JSON Web Token](https://jwt.io)
- [React Aria](https://github.com/adobe/react-spectrum#readme)
- [react-cookie](https://github.com/bendotcodes/cookies/tree/main/packages/react-cookie/#readme)
- [react-helmet](https://github.com/nfl/react-helmet#readme)
- [react-slideshow-image](https://react-slideshow-image.netlify.app)
- [react-toastify](https://github.com/fkhadra/react-toastify#readme)
- [ASP.NET Core API](https://learn.microsoft.com/en-us/aspnet/core)
- [Entity Framework](https://learn.microsoft.com/en-us/ef)
- [ipify API](https://www.ipify.org)
- [Frankfurter API](https://frankfurter.dev)
- [LocationIQ API](https://docs.locationiq.com/docs/introduction)

### Useful commands:
Run frontend in dev mode (```--host``` option allowing to enter frontend from mobile device turned on)  
```
npm run dev
```
Node module installation
```
npm i <nazwa_komponentu>
```
Node module deinstallation
```
npm r <nazwa_komponentu>
```