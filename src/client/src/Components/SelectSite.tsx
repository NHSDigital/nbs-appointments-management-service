import { Site } from "../Types/Site"
import { NhsFooter } from "./NhsFooter"
import { NhsHeader } from "./NhsHeader"
import { QuickSetupLink } from "./SiteSetup"

type SelectSiteProps = {
    sites: Site[]
    selectSite: (site: Site) => void;
}

export const SelectSite = ({sites, selectSite} : SelectSiteProps) => {
    return(
        <>
            <NhsHeader navLinks={[]} />
            <h3 className="nhsuk-heading-m">Please select a site</h3>
            <div className="nhsuk-width-container">
                <div className="nhsuk-inset-text">
                    {sites.map(s => (
                        <QuickSetupLink 
                        label={s.name} 
                        action={() => selectSite(s)}/>
                    ))}
                </div>
            </div>
            <NhsFooter/>
        </>
    )
}