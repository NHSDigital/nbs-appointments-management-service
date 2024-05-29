import { SiteDetails } from "../Components/SiteDetails";
import { WarningCallout } from "../Components/WarningCallout";
import { useSiteContext } from "../ContextProviders/SiteContextProvider"

export const HomePage = () => {
      const { site } = useSiteContext();
      return <>
            <div className="nhsuk-grid-row">
                  <div className="nhsuk-grid-column-one-half">
                        <SiteDetails site={site!} />
                        <WarningCallout title="New campaign alert">
                              <p>The Spring/Summer Covid campaign begins on 15th April</p>
                        </WarningCallout>
                  </div>
                  <div className="nhsuk-grid-column-one-half">
                  </div>
            </div>
      </>;
}