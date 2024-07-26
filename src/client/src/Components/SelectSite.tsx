import { useAuthContext } from "../ContextProviders/AuthContextProvider"
import { Site } from "../Types/Site"
import { NhsFooter } from "./NhsFooter"
import { NhsHeader } from "./NhsHeader"
import { QuickSetupLink } from "./SiteSetup"

type SelectSiteProps = {
    sites: Site[]
    selectSite: (site: Site) => void;
}

export const SelectSite = ({sites, selectSite} : SelectSiteProps) => {
    const {getUserEmail, signOut} = useAuthContext()
    return(
        <>
            <NhsHeader navLinks={[]} userEmail={getUserEmail()} signOut={signOut} />
            <div className="nhsuk-width-container">
                <main className="nhsuk-main-wrapper " id="maincontent" role="main">
                    <div className="nhsuk-grid-row">
                        <div className="nhsuk-grid-column-full">
                            <h2>Please select a site</h2>
                            <div className="nhsuk-inset-text">
                        {sites.map(s => (
                            <QuickSetupLink 
                            label={s.name} 
                            action={() => selectSite(s)}/>
                        ))}
                    </div>
                        </div>
                    </div>
                </main>
            </div>
            <NhsFooter/>
        </>
    )
}