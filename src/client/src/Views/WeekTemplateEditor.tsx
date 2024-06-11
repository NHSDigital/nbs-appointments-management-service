import React from "react";
import { DayOfWeek, ExplodedWeekTemplate, WeekDaySessionMap, WeekTemplate } from "../Types/Schedule";
import { When } from "../Components/When";
import { TimeBlocks } from "../Components/TimeBlocks";
import { useTemplateService } from "../Services/TemplateService";
import { Link, useParams } from "react-router-dom";
import { blocksEqual, cloneTemplate, templateToViewModel, viewModelToTemplate } from "../utils"
import { useSiteContext } from "../ContextProviders/SiteContextProvider";

let initialDays: WeekDaySessionMap = { Monday: [], Tuesday: [], Wednesday: [], Thursday: [], Friday: [], Saturday: [], Sunday: [] };
let initialTemplate: ExplodedWeekTemplate = {
    name: "New Weekly Schedule",
    site: "",
    id: "",
    days: initialDays
};

type WeekTemplateEditorProps = {
    templateId: string | undefined
    siteId: string
    getTemplates: () => Promise<WeekTemplate[]>
    saveTemplate: (template: WeekTemplate) => Promise<string>
}

export const WeekTemplateEditorCtx = () => {
    const { site } = useSiteContext();
    const { templateId } = useParams();
    const templateService = useTemplateService();

    return (<WeekTemplateEditor
        templateId={templateId}
        siteId={site!.id}
        getTemplates={() => templateService.getTemplates(site!.id)}
        saveTemplate={templateService.saveTemplate}
    />)
}

export const WeekTemplateEditor = ({ templateId, siteId, getTemplates, saveTemplate }: WeekTemplateEditorProps) => {
    const [status, setStatus] = React.useState<null | "loading" | "errored" | "confirmed">(null);
    const [originalTemplate, setOriginalTemplate] = React.useState<ExplodedWeekTemplate>(initialTemplate);
    const [weekTemplate, setWeekTemplate] = React.useState<ExplodedWeekTemplate>(initialTemplate);
    const [isValid, setIsValid] = React.useState(true);
    const errorText = React.useRef("");

    const copyToAllDays = (sourceDay: DayOfWeek) => {
        const newSchedule = { ...weekTemplate.days };
        const blocksToCopy = newSchedule[sourceDay];
        for (let sched in newSchedule) {
            newSchedule[sched as DayOfWeek] = blocksToCopy.map(x => ({ ...x }));
        }
        setWeekTemplate({ ...weekTemplate, days: newSchedule });
    }

    const save = () => {
        var payload = viewModelToTemplate(weekTemplate);
        payload.site = siteId;
        saveTemplate(payload).then(id => {
            setWeekTemplate({ ...weekTemplate, id})
            setStatus("confirmed");
        });
    }

    const handleNameChange = (e: any) => {
        setWeekTemplate({ ...weekTemplate, name: e.target.value })
    };

    React.useEffect(() => {
        if (templateId) {
            setStatus("loading");
            getTemplates().then(templates => {
                const template = templates.find(t => t.id === templateId);
                if (template) {
                    const transformedTemplate = templateToViewModel(template);
                    const clone = cloneTemplate(transformedTemplate);
                    setOriginalTemplate(clone)
                    setWeekTemplate(transformedTemplate);
                    setStatus(null);
                } else {
                    setStatus("errored");
                }
            })
        }
    }, [templateId])

    const hasChanged = React.useMemo(() => {
        const changed = Object.entries(weekTemplate.days).map(([dayLabel, blocks]) =>
            blocks && originalTemplate.days && blocksEqual(blocks, originalTemplate.days[dayLabel as DayOfWeek]));
        return originalTemplate.name !== weekTemplate.name || changed.some(x => !x);
    }, [weekTemplate, originalTemplate]);

    return (
            <div className="nhsuk-grid-row">
                <div className="nhsuk-grid-column-one-half">
                        <When condition={status === "errored"}>
                            <div
                                className="nhsuk-error-summary"
                                aria-labelledby="error-summary-title"
                                role="alert"
                                tabIndex={-1}
                            >
                                <h2 className="nhsuk-error-summary__title" id="error-summary-title">
                                    <span className="nhsuk-u-visually-hidden">Error:</span>
                                    There is a problem
                                </h2>
                                <div className="nhsuk-error-summary__body">
                                    {/* <p>There has been a server error, please try again</p> */}
                                    <ul className="nhsuk-list nhsuk-error-summary__list">
                                        <li>
                                            <a href="#!">{errorText.current}</a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </When>
                        <div className="nhsuk-form-group">
                            <label className="nhsuk-label">
                                Template name
                            </label>
                            <input className="nhsuk-input nhsuk-input--width-20" value={weekTemplate.name} type="text" onChange={handleNameChange} />
                        </div>
                        {
                            Object.entries(weekTemplate.days).map(([dayLabel, scheduleBlocks]) => {
                                return <TimeBlocks
                                    key={dayLabel}
                                    dayLabel={dayLabel as DayOfWeek}
                                    scheduleBlocks={scheduleBlocks}
                                    setDayBlocks={blocks => {
                                        const updatedDays = { ...weekTemplate.days, [dayLabel]: blocks }
                                        setWeekTemplate({ ...weekTemplate, days: updatedDays })
                                    }
                                    }
                                    setIsValid={setIsValid}
                                    copyToAllDays={copyToAllDays}
                                />
                            })
                        }
                        <div className="nhsuk-navigation">
                            <button
                                id="submit-schedule"
                                type="button"
                                className="nhsuk-button nhsuk-u-margin-bottom-0"
                                onClick={save}
                                disabled={!isValid || !hasChanged}
                            >
                                Confirm template
                            </button>
                            <Link to="/templates" className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0">
                                Cancel
                            </Link>
                            <When condition={status === "loading"}>
                                <div>Loading...</div>
                            </When>
                            <When condition={status === "confirmed"}>
                                <div className="asa-button-message">
                                    <svg className="nhsuk-icon nhsuk-icon__tick" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" aria-hidden="true" width="34" height="34">
                                        <path strokeWidth="4" strokeLinecap="round" d="M18.4 7.8l-8.5 8.4L5.6 12" stroke="#007f3b"></path>
                                    </svg>
                                    Schedule confirmed
                                    <button className="nhsuk-back-link__link" type="button" onClick={() => setStatus(null)}>dismiss</button>
                                </div>
                            </When>
                        </div>
                </div>
            </div>
    )
}