import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';
import viteCompression from 'vite-plugin-compression';

const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
        ? `${process.env.APPDATA}/ASP.NET/https`
        : `${process.env.HOME}/.aspnet/https`;
    
const certificateName = "mtm-web-app.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (
        0 !== child_process.spawnSync('dotnet', [
            'dev-certs',
            'https',
            '--export-path',
            certFilePath,
            '--format',
            'Pem',
            '--no-password',
        ], { stdio: 'inherit' }).status
    ) {
        throw new Error("Could not create certificate.");
    }
}

const httpsConfig = {
    key: fs.readFileSync(keyFilePath),
    cert: fs.readFileSync(certFilePath),
};

const target = env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
    : env.ASPNETCORE_URLS
        ? env.ASPNETCORE_URLS.split(';')[0]
        : 'https://localhost:7062';

export default defineConfig({
    build: {
        assetsInlineLimit: 1024,
        sourcemap: false,
        minify: 'terser',
        terserOptions: {
            compress: {
                drop_console: true,
                drop_debugger: true,
                ecma: 2020,
                module: true,
            },
            output: {
                comments: false,
            },
        },
    },
    plugins: [
        plugin(),
        viteCompression({ algorithm: 'brotliCompress' }),
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    server: {
        proxy: {
            '^/api/*': {
                target,
                secure: false,
            },
            '/ip': {
                target: 'https://api.ipify.org',
                changeOrigin: true,
                rewrite: (path) => path.replace(/^\/ip/, ''),
                secure: true,
            },
        },
        port: 5173,
        https: httpsConfig,
    },
});
