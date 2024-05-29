import React from "react"
import { DayOfWeek, WeekTemplate } from "../Types/Schedule";
import { When } from "../Components/When";
import { useTemplateService } from "../Services/TemplateService";
import { Link } from "react-router-dom";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { GettingStartedCallout } from "../Components/GettingStartedCallout";

export const TemplateListView = () => {
  const { site } = useSiteContext();
  const templateService = useTemplateService();
  const [status, setStatus] = React.useState<"loading" | "loaded">("loading");
  const [templates, setTemplates] = React.useState<WeekTemplate[]>([] as WeekTemplate[])

  const hasTemplates = () => {
    return templates && templates.length > 0;
  }

  React.useEffect(() => {
    templateService.getTemplates(site!.id).then(rsp => {
      setTemplates(rsp);
      setStatus("loaded");
    })
  }, [])

  const getOpeningDayDetails = (template: WeekTemplate) => {
    const allDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"] as DayOfWeek[]
    const openDays = template.items
      .filter(i => i.scheduleBlocks.length > 0)
      .flatMap(i => i.days)
      .sort((a, b) => allDays.indexOf(a) - allDays.indexOf(b));
    const closedDays = allDays.filter(d => !openDays.includes(d));
    let result = "";
    let sep = "";
    if (openDays.length > 0) {
      result += `Open | ${openDays.join(", ")}`
      sep = " - ";
    }
    if (closedDays.length > 0)
      result += `${sep}Closed | ${closedDays.join(", ")}`

    return result;
  }

  return (
    <>
        <When condition={status === "loaded" && !hasTemplates()}>
          <GettingStartedCallout />
        </When>
        <When condition={status === "loaded" && hasTemplates()}>
          <table className="nhsuk-table">
            <caption className="nhsuk-table__caption">
              Availability Template Management
              <span className="nhsuk-hint">Manage your current site availability templates</span>
            </caption>
            <thead className="nhsuk-table__head">
              <tr role="row">
                <th role="columnheader" className="" scope="col">
                  Name
                </th>
                <th role="columnheader" className="" scope="col">
                  Opening Days
                </th>
                <th role="columnheader" className="" scope="col">
                  Manage
                </th>
              </tr>
            </thead>
            <tbody>
              {templates.map(t => (
                <tr key={t.id}>
                  <td>{t.name}</td>
                  <td>{getOpeningDayDetails(t)}</td>
                  <td>
                    <Link to={`edit/${t.id}`}>
                      Edit
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div>
            <Link className="nhsuk-button--link" style={{ paddingLeft: "0px" }} to="/templates/edit">
              Create new template
            </Link>
          </div>
        </When>
    </>
  )
}