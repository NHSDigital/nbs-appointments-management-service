import React from "react";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { SiteConfiguration, defaultServiceConfigurationTypes } from "../Types";
import { When } from "../Components";

type EditSiteServicesProps = {
      siteConfiguration: SiteConfiguration | null,
      setSiteConfiguration: (siteConfiguration: SiteConfiguration) => Promise<void>
};

export const EditSiteServicesCtx = () => {
    const { siteConfig, saveSiteConfiguration } = useSiteContext();
    return ( <EditSiteServices siteConfiguration={siteConfig} setSiteConfiguration={saveSiteConfiguration} />)
}

export const EditSiteServices = ({ siteConfiguration, setSiteConfiguration }: EditSiteServicesProps) => {
      const defaultDuration = 10;
      const maxInfoForCitizenLength = 150;
      const [submitted, setSubmitted] = React.useState<boolean>(false);
      const [infoForCitizen, setInfoForCitizen] = React.useState("");

      // TODO: Prevent render whilst siteconfig loads on refresh, then set in
      // initial state instead of using an effect (useState's initial value used in subsequent renders)
      React.useEffect(() => {
            setInfoForCitizen(siteConfiguration?.informationForCitizen!);
      }, [siteConfiguration?.informationForCitizen])

      const getExistingDurationOrDefault = (serviceCode: string): number => {
            let service = siteConfiguration?.serviceConfiguration.find(x => x.code === serviceCode)
            if (service) {
                  return service.enabled ? service.duration : 0
            }
            return defaultDuration
      }

      const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
            event.preventDefault();
            const newServiceConfigs = defaultServiceConfigurationTypes.map(sc => {
                  const newDuration = event.currentTarget[sc.code]?.value;
                  sc.enabled = newDuration !== "0";
                  if (sc.enabled) {
                        sc.duration = newDuration
                  }
                  return sc;
            })
            const newSiteConfiguration: SiteConfiguration = {
                  siteId: siteConfiguration?.siteId!,
                  informationForCitizen: infoForCitizen,
                  serviceConfiguration: newServiceConfigs
            };
            setSiteConfiguration(newSiteConfiguration)
                  .then(() => {
                        setSubmitted(true);
                  });
      }

      const handleChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
            const value = event.target.value;
            if (value.length <= maxInfoForCitizenLength) {
                  setInfoForCitizen(value.replaceAll(/[^ a-zA-Z0-9-,.:]/g, ""));
            }
      }

      return (
            <div className="nhsuk-grid-row">
                  <div className="nhsuk-grid-column-one-half">
                        <form onSubmit={handleSubmit}>
                        <table>
                              <caption className="nhsuk-table__caption">
                                    Service Appointment Length Management
                                    <div className="nhsuk-hint">Set the appointment length for your current site available services</div>
                              </caption>
                              <thead className="nhsuk-table__head">
                                    <tr role="row">
                                          <th role="columnheader" scope="col">
                                          Service Type
                                          </th>
                                          <th role="columnheader" scope="col">
                                                Appointment Length
                                          </th>
                                    </tr>
                              </thead>
                              <tbody className="nhsuk-table__body">
                              {
                                    siteConfiguration == null ? <div>No services configured</div> :
                                          defaultServiceConfigurationTypes.map(x => {
                                                return <tr key={x.code} >
                                                      <td>{x.displayName}</td>
                                                      <td>
                                                            <select id={x.code} name={x.code} className="nhsuk-select" defaultValue={getExistingDurationOrDefault(x.code)}>
                                                      {
                                                             Array.from({length: 16}, (x, i) => i)
                                                                  .map(x => <option key={x} value={x}>{x === 0 ? "Off" : x.toString()} </option>)
                                                      }
                                                 </select></td>
                                                </tr>
                                          })
                              }
                              </tbody>
                        </table>
                              <div className="nhsuk-character-count">
                                    <div className="nhsuk-form-group">
                                          <h3>Information for Citizens</h3>
                                          <div className="nhsuk-hint" id="more-detail-hint">
                                                This can include directions, parking restrictions or any information not covered by site attributes. Letters, numbers and basic punctuation only. Special characters will automatically be removed.
                                          </div>
                                          <textarea className="nhsuk-textarea" value={infoForCitizen} onChange={handleChange} id="more-detail" name="more-detail" rows={3} aria-describedby="more-detail-hint"></textarea>
                                    </div>
                                    <div className="nhsuk-hint nhsuk-character-count__message" id="more-detail-info">
                                          You can enter up to {maxInfoForCitizenLength - (infoForCitizen?.length || 0)} characters
                                    </div>
                              </div>
                              <div style={{display:"flex"}}>
                                    <button className="nhsuk-button nhsuk-u-margin-0" type="submit">
                                          Confirm site configuration
                                    </button>
                                    <When condition={submitted}>
                                          <div style={{ display: "flex", alignItems: "center", paddingLeft: 15 }}>
                                                <svg className="nhsuk-icon nhsuk-icon__tick" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" aria-hidden="true" width="34" height="34">
                                                      <path strokeWidth="4" strokeLinecap="round" d="M18.4 7.8l-8.5 8.4L5.6 12" stroke="#007f3b"></path>
                                                </svg>
                                                Site configuration updated
                                          </div>
                                    </When>
                              </div>
                        </form>
                  </div>
            </div>
      )
};