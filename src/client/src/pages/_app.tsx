import "/node_modules/nhsuk-frontend/dist/nhsuk.css"
import '../index.css'
import type { AppProps } from "next/app";
import dynamic from "next/dynamic";
import { AppPage } from "src/Components/AppPage";
import { SiteContextProvider } from "src/ContextProviders/SiteContextProvider";
import Head from "next/head";

// dynamically render auth provider as it uses window object
const DynamicAuthCtxProvider = dynamic(
    () => import('src/ContextProviders/AuthContextProvider').then(mod => mod.AuthContextProvider),
    {ssr: false}
);

export default function App({ Component, pageProps }: AppProps) {
    return <>
        <Head>
            <meta name="viewport" content="width=device-width, initial-scale=1"/>
            <title>NHS Appointments Book</title>
        </Head>
        <DynamicAuthCtxProvider>
            <SiteContextProvider>
                <AppPage navLinks={[
                    { name: "Home", route: "/" },
                    { name: "Availability Scheduler", route: "/availability" },
                    { name: "Templates", route: "/templates" },
                    { name: "Edit Services", route: "/site" },
                    { name: "Daily Bookings", route: "/bookings" },
                    { name: "Calendar", route: "/calendar" }
                ]}>
                    <Component {...pageProps} />
                </AppPage>
            </SiteContextProvider>
        </DynamicAuthCtxProvider>
    </>
}