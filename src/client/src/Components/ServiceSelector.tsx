import React from "react";
import "./ServiceSelector.css"
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { ServiceConfiguration } from "../Types";
import { When } from "./When";

type ServiceSelectorProps = {
    checkedServices: string[],
    uniqueId: string,
    handleChange: (event: React.ChangeEvent<HTMLInputElement>) => void
    hasError: boolean
    handleSelectAllChange: (event: React.ChangeEvent<HTMLInputElement>, serviceConfiguration: ServiceConfiguration[]) => void
}

export const ServiceSelector = ({ checkedServices, uniqueId, handleChange, hasError, handleSelectAllChange }: ServiceSelectorProps) => {

    const { siteConfig } = useSiteContext();

    const [isVisible, setIsVisible] = React.useState(false);
    const containerRef = React.useRef<HTMLDivElement>(null);

    const allEnabledServicesChecked = () => {
        let enabledServices = siteConfig?.serviceConfiguration.filter(x => x.enabled);
        return enabledServices?.length === checkedServices?.length;
    }

    React.useEffect(() => {
        const handleEventOutside = (event: Event) => {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsVisible(false);
            }
        }
        document.addEventListener("mousedown", handleEventOutside);
        document.addEventListener("focusin", handleEventOutside);
        return () => {
            document.removeEventListener("mousedown", handleEventOutside);
            document.removeEventListener("focusin", handleEventOutside);
        };
    }, [containerRef]);


    // TODO look at optimising the rendering for this
    const serviceString = React.useMemo(() => {
        let serviceString = "", latestEnd: string, latestType: string;
        const names = siteConfig?.serviceConfiguration.filter(configType => checkedServices.includes(configType.code)).map(x => x.displayName);
        names?.sort((a, b) => {
            const regex = /(^.*?(?=\d))(\d*)/;
            /* eslint-disable @typescript-eslint/no-unused-vars */
            const [_a, aType, aLowerRange] = a.match(regex)!;
            const [_b, bType, bLowerRange] = b.match(regex)!;
            /* eslint-enable @typescript-eslint/no-unused-vars */
            if (aType > bType) return 1;
            if (aType < bType) return -1;
            return +aLowerRange < +bLowerRange ? -1 : 1;
        }).forEach(dN => {
            let [fullMatch, type, start, end] = dN.match(/(^.*?(?=\d))(\d*)[-]?(\d*)?/)!;
            if (latestType === type) {
                if ((+latestEnd + 1) === +start) {
                    const endOrPlus = end ? end : start + "+"
                    serviceString = serviceString.replace(new RegExp(latestEnd + "$"), endOrPlus);
                } else {
                    serviceString += end ? `, ${start}-${end}` : `, ${start}+`
                }
            } else {
                serviceString += `${latestType ? " | " : ""}${fullMatch}${!end ? "+" : ""}`
            }
            latestEnd = end;
            latestType = type;
        });
        return serviceString
    }, [siteConfig?.serviceConfiguration, checkedServices]);

    const filteredServiceConfig = siteConfig?.serviceConfiguration.filter(s => checkedServices.includes(s.code) || s.enabled) ?? [];


    return <div className={`dropdown-check-list ${isVisible ? "visible" : ""}`} tabIndex={0} onFocus={() => setIsVisible(true)} ref={containerRef}>
        <span className={`anchor ${hasError ? "error" : ""}`}>
            {serviceString ? serviceString : "Select a service"}
        </span>
        <ul className="items">
            <When condition={filteredServiceConfig.length > 1}>
                <li className="nhsuk-checkboxes__item">
                    <input
                        type="checkbox"
                        id={`selectAllCheckbox_${uniqueId}`}
                        value="0"
                        className="nhsuk-checkboxes__input"
                        onChange={(e) => handleSelectAllChange(e, siteConfig?.serviceConfiguration!)}
                        checked={allEnabledServicesChecked()}
                    />
                    <label htmlFor={`selectAllCheckbox_${uniqueId}`} className="nhsuk-checkboxes__label">Select all</label>
                </li>
            </When>
            {filteredServiceConfig.map(service => {
                const key = `${service.code}_${uniqueId}`;
                return <li key={key} className="nhsuk-checkboxes__item">
                    <input
                        type="checkbox"
                        id={key}
                        value={service.code} name={service.code}
                        checked={checkedServices.includes(service.code)}
                        onChange={handleChange}
                        className="nhsuk-checkboxes__input"
                        // either this to disable or use filteredServiceConfig above to remove checked disabled services
                        //disabled={!checkedServices.includes(service.code) && !service.enabled}
                    />
                    <label htmlFor={key} className="nhsuk-checkboxes__label">{service.displayName}</label>
                </li>
            })
            }
        </ul>
    </div>
}