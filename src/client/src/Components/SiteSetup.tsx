import { useAuthContext } from "../ContextProviders/AuthContextProvider"
import { useSiteContext } from "../ContextProviders/SiteContextProvider"
import { Site } from "../Types/Site"
import { defaultServiceConfigurationTypes, ServiceConfiguration, SiteConfiguration } from "../Types/SiteConfiguration"
import { NhsFooter } from "./NhsFooter"
import { NhsHeader } from "./NhsHeader"
import { SiteDetails } from "./SiteDetails"
import { WarningCallout } from "./WarningCallout"
import { useRouter } from "next/navigation";

export const SiteSetup = ({ site }: { site: Site }) => {

    const { getUserEmail, signOut } = useAuthContext();

    const allServices = ["COVID:12_15", "COVID:16_17", "COVID:18_74", "COVID:75", "FLU:18_64", "FLU:65", "COVID_FLU:18_64", "COVID_FLU:65_74", "COVID_FLU:75"];
    const noCoAdmin = ["COVID:12_15", "COVID:16_17", "COVID:18_74", "COVID:75", "FLU:18_64", "FLU:65"];
    const covidOnly = ["COVID:12_15", "COVID:16_17", "COVID:18_74", "COVID:75"];
    const fluOnly = ["FLU:18_64", "FLU:65"];
    const { saveSiteConfiguration } = useSiteContext();
    const router = useRouter();

    const initialiseServices = (serviceTypes: string[]) => {
        const siteServices: ServiceConfiguration[] = defaultServiceConfigurationTypes.map(sc => ({
            code: sc.code,
            displayName: sc.displayName,
            duration: 5,
            enabled: serviceTypes.includes(sc.code)
        }));
        const siteConfiguration: SiteConfiguration = {
            site: site.id,
            informationForCitizen: "",
            serviceConfiguration: siteServices
        }
        saveSiteConfiguration(siteConfiguration);
        router.push("/templates");
    }

    return (
        <>
            <NhsHeader navLinks={[]} userEmail={getUserEmail()} signOut={signOut} />
            <div className="nhsuk-width-container-fluid">
                <main className="nhsuk-main-wrapper " id="maincontent" role="main">
                    <div className="nhsuk-grid-row">
                        <div className="nhsuk-grid-column-two-thirds">
                            <SiteDetails site={site} />
                            <WarningCallout title="Getting Started">
                                <p>Your site has not been setup for the appointments service. Choose one of the options below to get started.</p>
                                <p>You can change the service configuration later.</p>
                            </WarningCallout>
                            <div className="nhsuk-inset-text">
                                <QuickSetupLink
                                    label={"Covid and Flu with Co-Admin"}
                                    action={() => initialiseServices(allServices)} />
                                <QuickSetupLink
                                    label={"Covid and Flu"}
                                    action={() => initialiseServices(noCoAdmin)} />
                                <QuickSetupLink
                                    label={"Covid Only"}
                                    action={() => initialiseServices(covidOnly)} />
                                <QuickSetupLink
                                    label={"Flu Only"}
                                    action={() => initialiseServices(fluOnly)} />
                            </div>
                        </div>
                    </div>
                </main>
            </div>
            <NhsFooter />
        </>
    )
}

export const QuickSetupLink = ({ label, action }: { label: string, action: () => void }) => {
    return (
        <div className="nhsuk-action-link">
            <a className="nhsuk-action-link__link" href="#!" onClick={action}>
                <svg className="nhsuk-icon nhsuk-icon__arrow-right-circle" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" aria-hidden="true" width="36" height="36">
                    <path d="M0 0h24v24H0z" fill="none"></path>
                    <path d="M12 2a10 10 0 0 0-9.95 9h11.64L9.74 7.05a1 1 0 0 1 1.41-1.41l5.66 5.65a1 1 0 0 1 0 1.42l-5.66 5.65a1 1 0 0 1-1.41 0 1 1 0 0 1 0-1.41L13.69 13H2.05A10 10 0 1 0 12 2z"></path>
                </svg>
                <span className="nhsuk-action-link__text">{label}</span>
            </a>
        </div>
    )
}