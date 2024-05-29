import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { When } from "./When";

type SiteIndicatorProps = {
    title: string
}

export const SiteIndicator = ({title} : SiteIndicatorProps) => {
    const { site } = useSiteContext();

    return (
    <div className="site-indicator__container">
        <div className="site-indicator" aria-label="Location navigation">
            <div className="site-indicator__item">
                <span className="site-indicator__content">
                    <When condition={site !== null}>
                        {site?.name} {" > "}
                    </When>
                    {title}
                </span>
            </div>
        </div>
    </div>
)}
