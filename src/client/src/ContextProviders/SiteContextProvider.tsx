import React, { useEffect } from "react";
import { When } from "../Components/When";
import { SiteConfiguration } from "../Types/SiteConfiguration";
import { useSiteConfigurationService } from "../Services/SiteConfigurationService";
import { useAuthContext } from "./AuthContextProvider";
import { Site } from "../Types/Site";
import { SiteSetup } from "../Components/SiteSetup";

export interface ISiteContext {
    site: Site | null
    siteConfig: SiteConfiguration | null
    saveSiteConfiguration: (siteConfiguration: SiteConfiguration) => Promise<void>
}

export const SiteContext = React.createContext<ISiteContext | null>(null);

type SiteContextState = "loading" | "not-configured" | "not-found" | "ready";

export const SiteContextProvider = ({ children }: { children: React.ReactNode }) => {
    const [state, setState] = React.useState<SiteContextState>("loading");
    const [site, setSite] = React.useState<Site|null>(null);
    const [siteConfig, setSiteConfig] = React.useState<SiteConfiguration | null>(null)
    const { idToken } = useAuthContext();
    const { getSiteConfigurationForUser, setSiteConfiguration } = useSiteConfigurationService()

    const saveSiteConfiguration = (siteConfiguration:SiteConfiguration) => {
        return setSiteConfiguration(siteConfiguration).then(rsp => {
            setSiteConfig(siteConfiguration);
        });
    }

    useEffect(() => {
        if(siteConfig) {
            setState("ready")
        }
    }, [siteConfig])

    useEffect(() => {
        setSiteConfig(null);
        if(idToken) {
            getSiteConfigurationForUser().then(sc => {
                if(sc) {
                    setSite(sc.site)
                    if(sc.siteConfiguration) {
                        setSiteConfig(sc.siteConfiguration);
                    } else {
                        setState("not-configured");
                    }
                }
                else {
                    setState("not-found");
                }
            })
        }
    }, [idToken]);

    return (
        <SiteContext.Provider value={{site, siteConfig, saveSiteConfiguration}}>
            <When condition={state === "loading"}>Loading Configuration...</When>
            <When condition={state === "ready"}>{children}</When>
            <When condition={state === "not-configured"}>
                <SiteSetup site={site!} />
            </When>
            <When condition={state === "not-found"}>
                <div>We were unable to find you site. It may not be registered with our systems. You will need to contact our onboarding team.</div>
            </When>
        </SiteContext.Provider>
        );
}

export const useSiteContext = () => {
    const context = React.useContext(SiteContext);
    if (context) return context;

    throw Error("Location context was not registered");
  };