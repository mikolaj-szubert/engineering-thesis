@echo off
cd MTM-Web-App.Server
dotnet restore MTM_Web_App.Server.csproj
dotnet ef database update Init
cd ../mtm-web-app.client
npm i