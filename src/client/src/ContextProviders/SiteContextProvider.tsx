import React, { useEffect } from "react";
import { When } from "../Components/When";
import { SiteConfiguration } from "../Types/SiteConfiguration";
import {useSiteConfigurationService} from "../Services/SiteConfigurationService";
import { useAuthContext } from "./AuthContextProvider";
import { Site } from "../Types/Site";
import { SiteSetup } from "../Components/SiteSetup";
import { SelectSite } from "../Components/SelectSite";
import {useUserService} from "../Services/UserService";

export interface ISiteContext {
    site: Site | null
    siteConfig: SiteConfiguration | null
    saveSiteConfiguration: (siteConfiguration: SiteConfiguration) => Promise<void>
    hasPermission: (permission: string) => boolean;
}

export const SiteContext = React.createContext<ISiteContext | null>(null);

type SiteContextState = "loading" | "select-site" | "not-configured" | "not-found" | "ready";

export const SiteContextProvider = ({ children }: { children: React.ReactNode }) => {
    const [state, setState] = React.useState<SiteContextState>("loading");
    const [site, setSite] = React.useState<Site|null>(null);
    const [availableSites, setAvailableSites] = React.useState<Site[]>([] as Site[])
    const [siteConfig, setSiteConfig] = React.useState<SiteConfiguration | null>(null)
    const { idToken } = useAuthContext();
    const { getSiteConfiguration, getSitesForUser, setSiteConfiguration } = useSiteConfigurationService()
    const { getUserPermissions } = useUserService();

    const saveSiteConfiguration = (siteConfiguration:SiteConfiguration) => {
        return setSiteConfiguration(siteConfiguration).then(rsp => {
            setSiteConfig(siteConfiguration);
        });
    }

    const hasPermission = (permission: string): boolean => {
        const storedPerms = localStorage.getItem("perms")
        const perms = storedPerms ? JSON.parse(storedPerms) : [];
        return perms.includes(permission);
    }

    useEffect(() => {
        if(siteConfig) {
            setState("ready")
        }
    }, [siteConfig])

    useEffect(() => {
        if(site) {
            getUserPermissions(site.id).then(rsp => 
                localStorage.setItem("perms", JSON.stringify(rsp.permissions)));   
        }
    }, [site])

    useEffect(() => {
        setSiteConfig(null);
        if(idToken !== null) {
            if(site === null) {
                getSitesForUser().then(sfu => {
                    if(sfu.length > 1) {
                        setAvailableSites(sfu)
                        setState("select-site")
                    } else if(sfu.length === 1) {
                        setSite(sfu[0])
                    } else {
                        setState("not-found");
                    }
                })
            }
            else {
                getSiteConfiguration(site!.id).then(sc => {
                    if(sc) {
                        setSiteConfig(sc);
                    }
                    else {
                        setState("not-configured");
                    }
                })
            }
        } else {
            setSite(null);
        }
    }, [idToken, site]);

    return (
        <SiteContext.Provider value={{site, siteConfig, saveSiteConfiguration, hasPermission}}>
            <When condition={state === "loading"}>Loading Configuration...</When>
            <When condition={state === "ready"}>{children}</When>
            <When condition={state === "not-configured"}>
                <SiteSetup site={site!} />
            </When>
            <When condition={state === "select-site"}>
                <SelectSite sites={availableSites} selectSite={s => setSite(s)} />
            </When>
            <When condition={state === "not-found"}>
                <div>We were unable to find your site. It may not be registered with our systems. You will need to contact our onboarding team.</div>
            </When>
        </SiteContext.Provider>
        );
}

export const useSiteContext = () => {
    const context = React.useContext(SiteContext);
    if (context) return context;

    throw Error("Location context was not registered");
  };