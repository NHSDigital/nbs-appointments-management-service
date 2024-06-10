import React from "react";
import dayjs from "dayjs";
import { useTemplateService } from "../Services/TemplateService";
import { TemplateAssignment, WeekTemplate, ErrorResponse } from "../Types";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { When, GettingStartedCallout } from "../Components";

type ScheduleEditorProps = {
    getTemplates: () => Promise<WeekTemplate[]>
    getAssignments: () => Promise<TemplateAssignment[]>
    saveAssignments: (assignments: TemplateAssignment[]) => Promise<Response>
}

export const ScheduleEditorCtx = () => {
    const { site } = useSiteContext();
    const templateService = useTemplateService();
    return <ScheduleEditor
        getTemplates={() => templateService.getTemplates(site?.id!)}
        getAssignments={() => templateService.getAssignments(site?.id!)}
        saveAssignments={assignments => templateService.saveAssignments(site?.id!, assignments)}
    />
}

export const ScheduleEditor = ({getTemplates, getAssignments, saveAssignments}: ScheduleEditorProps) => {
    const [status, setStatus] = React.useState<"loading" | "loaded">("loading");
    const [errors, setErrors] = React.useState<ErrorResponse[]>([] as ErrorResponse[]);
    const [templates, setTemplates] = React.useState<WeekTemplate[]>([] as WeekTemplate[])
    const [assignments, setAssignments] = React.useState<TemplateAssignment[]>([] as TemplateAssignment[])
    const [confirmed, setConfirmed] = React.useState<boolean>(false);

    React.useEffect(() => {
        setStatus("loading");
        getTemplates().then(rsp => {
            setTemplates(rsp);
            getAssignments().then(rsp => {
                rsp.sort((a, b) => a.from > b.from ? 1 : -1)
                setAssignments(rsp);
                setStatus("loaded");
            })
        })
    }, [errors])

    const addAvailability = () => {
        const fromDate = assignments.length > 0 ? dayjs(assignments.slice(-1)[0].until, "YYYY-MM-DD").add(1, "day") : dayjs();
        const newAvailability = {
            from: fromDate.format("YYYY-MM-DD"),
            until: fromDate.add(1, 'week').format("YYYY-MM-DD"),
            templateId: templates[0].id
        };
        setAssignments([...assignments, newAvailability]);
    }

    const updateAssignment = (index: number, newAssignment: TemplateAssignment) => {
        const newAssignments = assignments.map(
            (item, idx) => idx === index ? newAssignment : item
        );
        setAssignments(newAssignments);
    }

    const removeAssignment = (index: number) => {
        const newAssignments = [...assignments];
        newAssignments.splice(index, 1);
        setAssignments(newAssignments);
    }

    const confirm = () => {
        setErrors([]);
        saveAssignments(assignments).then(rsp => {
            if (rsp.status === 200) {
                setConfirmed(true);
            } else if (rsp.status === 400) {
                rsp.json().then(j => setErrors(j))
            } else {
                setErrors([{ message: "There was a problem saving your assignments.", property: "" }])
            }
        })
        // todo confirmation / error handling
    }

    const cancel = () => {
        setErrors([]);
    }

    return (
        <>
            <When condition={status === "loading"} >
                <span>Loading...</span>
            </When>
            <When condition={status === "loaded" && templates.length === 0}>
                <GettingStartedCallout />
            </When>
            <When condition={errors.length > 0}>
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
                            {errors.map(err => (
                                <li>
                                    <a href="#!">{err.message}</a>
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>
            </When>
            <When condition={status === "loaded" && templates.length > 0}>
                <When condition={assignments?.length < 1}>
                    <p>There is no scheduled availability for the current site</p>
                </When>
                <When condition={assignments?.length > 0} >
                    <div className="nhsuk-grid-row">
                        <div className="nhsuk-grid-column-one-half">
                            <table className="nhsuk-table" style={{ marginBottom: "10px" }}>
                                <caption className="nhsuk-table__caption">
                                    Site Schedule Management
                                    <div className="nhsuk-hint">Assign templates to date ranges to schedule availability</div>
                                </caption>
                                <thead className="nhsuk-table__head">
                                    <tr role="row">
                                        <th role="columnheader" scope="col">
                                            Date from
                                        </th>
                                        <th role="columnheader" scope="col">
                                            Date to
                                        </th>
                                        <th role="columnheader" scope="col">
                                            Template
                                        </th>
                                        <th>&nbsp;</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {
                                        assignments.map((a, i) => (
                                            <TemplateAssignmentRow
                                                key={i}
                                                assignment={a}
                                                templateItems={templates}
                                                onAssignmentChanged={assignment => updateAssignment(i, assignment)}
                                                onRemoveAssignment={() => removeAssignment(i)} />
                                        ))
                                    }
                                </tbody>
                            </table>
                        </div>
                    </div>
                </When>
                <button style={{ marginBottom: "16px" }}
                    className="nhsuk-button--link"
                    type="button"
                    onClick={addAvailability}>
                    Add availability
                </button>
                <div style={{ display: "flex", marginTop: "16px" }}>
                    <button
                        type="button"
                        className="nhsuk-button"
                        onClick={confirm}>
                        Confirm availability
                    </button>
                    <button
                        style={{ marginLeft: "24px" }}
                        type="button"
                        className="nhsuk-button nhsuk-button--secondary"
                        onClick={cancel}>
                        Cancel
                    </button>
                    <When condition={confirmed}>
                        <p style={{ display: "flex", alignItems: "center", paddingLeft: "15px" }}>
                            <svg className="nhsuk-icon nhsuk-icon__tick" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" aria-hidden="true" width="34" height="34">
                                <path strokeWidth="4" strokeLinecap="round" d="M18.4 7.8l-8.5 8.4L5.6 12" stroke="#007f3b"></path>
                            </svg>
                            Schedule confirmed
                            <button style={{ padding: "0 0 0 5px" }} className="nhsuk-button--link" type="button" onClick={() => setConfirmed(false)}>dismiss</button>
                        </p>
                    </When>
                </div>
            </When>
        </>
    )
}

type TemplateAssignmentRowProps = {
    templateItems: { name: string, id: string }[]
    assignment: TemplateAssignment
    onAssignmentChanged: (updatedAssignment: TemplateAssignment) => void;
    onRemoveAssignment: () => void;
}

export const TemplateAssignmentRow = ({ templateItems, assignment, onAssignmentChanged, onRemoveAssignment }: TemplateAssignmentRowProps) => {
    return (
        <tr role="row" className="nhsuk-table__row">
            <td className={`nhsuk-table__cell`}>
                <div className="nhsuk-date-input__item">
                    <input
                        type="date"
                        onChange={e => onAssignmentChanged({...assignment, from: e.target.value})}
                        value={assignment.from}
                        className="nhsuk-input nhsuk-date-input nhsuk-input--width-8"
                        aria-label="enter from date"
                    />
                </div>
            </td>
            <td className="nhsuk-table__cell ">
                <div className="nhsuk-date-input__item">
                    <input
                        type="date"
                        onChange={e => onAssignmentChanged({...assignment, until: e.target.value})}
                        value={assignment.until}
                        className="nhsuk-input nhsuk-date-input nhsuk-input--width-8"
                        aria-label="enter until date"
                    />
                </div>
            </td>
            <td className="nhsuk-table__cell ">
                <select
                    className="nhsuk-select"
                    value={assignment.templateId}
                    onChange={e => onAssignmentChanged({...assignment, templateId: e.target.value})}>
                    {
                        templateItems.map(t => (
                            <option key={t.id} value={t.id}>{t.name}</option>
                        ))
                    }
                </select>
            </td>
            <td className="nhsuk-table__cell ">
                <button
                    className="nhsuk-button--link"
                    type="button"
                    onClick={onRemoveAssignment}>
                    Remove
                </button>
            </td>
        </tr>
    )
}