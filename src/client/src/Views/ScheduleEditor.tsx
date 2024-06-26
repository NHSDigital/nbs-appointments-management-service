import React from "react";
import { useTemplateService } from "../Services/TemplateService";
import { TemplateAssignment, WeekTemplate } from "../Types/Schedule";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { When } from "../Components/When";
import dayjs from "dayjs";
import { GettingStartedCallout } from "../Components/GettingStartedCallout";
import { ErrorResponse } from "../Types/ErrorResponse";

export const ScheduleEditor = () => {
    const templateService = useTemplateService();
    const { site } = useSiteContext();

    const [status, setStatus] = React.useState<"loading" | "loaded">("loading");
    const [errors, setErrors] = React.useState<ErrorResponse[]>([] as ErrorResponse[]);
    const [templates, setTemplates] = React.useState<WeekTemplate[]>([] as WeekTemplate[])
    const [assignments, setAssignments] = React.useState<TemplateAssignment[]>([] as TemplateAssignment[])
    const [confirmed, setConfirmed] = React.useState<boolean>(false);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = () => {
        setStatus("loading");
        templateService.getTemplates(site!.id).then(rsp => {
            setTemplates(rsp);
            templateService.getAssignments(site!.id).then(rsp => {
                rsp.sort((a, b) => a.from > b.from ? 1 : -1)
                setAssignments(rsp);
                setStatus("loaded");
            })
        })
    }

    const fromDate = React.useMemo(() => {
        if (assignments.length > 0) {
            const from = dayjs(assignments.slice(-1)[0].until, "YYYY-MM-DD").add(1, "day")
            return from
        } else {
            return dayjs(new Date())
        }
    }, [assignments])

    const addAvailability = () => {
        const newAvailability = {
            from: fromDate.format("YYYY-MM-DD"),
            until: fromDate.add(7, 'day').format("YYYY-MM-DD"),
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
        newAssignments.sort((a, b) => a.from > b.from ? 1 : -1);
        setAssignments(newAssignments);
    }

    const confirm = () => {
        setErrors([] as ErrorResponse[]);
        templateService.saveAssignments(site!.id, assignments).then(rsp => {
            if (rsp.status === 200) {
                setConfirmed(true);
            } else if (rsp.status === 400) {
                rsp.json().then(j => setErrors(j))
            } else {
                loadData()
                setErrors([{ message: "There was a problem saving your assignments.", property: "" }])
            }

        })

        // todo confirmation / error handling

    }

    const cancel = () => {
        setErrors([] as ErrorResponse[]);
        loadData();
    }

    const hasAssignments = () => {
        return assignments && assignments.length > 0;
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
                <When condition={!hasAssignments()}>
                    <p>There is no scheduled availability for the current site</p>
                    <button className="nhsuk-button" type="button" onClick={addAvailability}>
                        Add availability
                    </button>
                </When>
                <When condition={hasAssignments()} >
                    <div className="nhsuk-grid-row">
                        <div className="nhsuk-grid-column-one-half">
                            <table className="nhsuk-table">
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
                                <tbody className="nhsuk-table__body">
                                    {
                                        assignments.map((a, i) => (
                                            <TemplateAssignmentRow
                                                key={i}
                                                assignment={a}
                                                templateItems={templates}
                                                onAssignmentChanged={u => updateAssignment(i, u)}
                                                onRemoveAssignment={() => removeAssignment(i)} />
                                        ))
                                    }
                                    <tr>
                                        <th>
                                            <button
                                                className="nhsuk-button--link"
                                                type="button"
                                                onClick={addAvailability}>
                                                Add availability
                                            </button>
                                        </th>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div className="nhsuk-navigation">
                        <button
                            type="button"
                            className="nhsuk-button nhsuk-u-margin-0"
                            onClick={confirm}>
                            Confirm availability
                        </button>
                        <button
                            type="button"
                            className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
                            onClick={cancel}>
                            Cancel
                        </button>
                        <When condition={confirmed}>
                            <div className="asa-button-message">
                                <svg className="nhsuk-icon nhsuk-icon__tick" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" aria-hidden="true" width="34" height="34">
                                    <path strokeWidth="4" strokeLinecap="round" d="M18.4 7.8l-8.5 8.4L5.6 12" stroke="#007f3b"></path>
                                </svg>
                                Schedule confirmed
                                <button className="nhsuk-back-link__link" type="button" onClick={() => setConfirmed(false)}>dismiss</button>
                            </div>
                        </When>
                    </div>
                </When>
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

    const updateFrom = (from: string) => {
        const updatedAssignment = { ...assignment, from }
        onAssignmentChanged(updatedAssignment);
    }

    const updateUntil = (until: string) => {
        const updatedAssignment = { ...assignment, until }
        onAssignmentChanged(updatedAssignment);
    }

    const updateTemplate = (templateId: string) => {
        const updatedAssignment = { ...assignment, templateId }
        onAssignmentChanged(updatedAssignment);
    }

    return (
        <tr role="row" className="nhsuk-table__row">
            <td className={`nhsuk-table__cell`}>
                <div className="nhsuk-date-input__item">
                    <input
                        type="date"
                        onChange={e => updateFrom(e.target.value)}
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
                        onChange={e => updateUntil(e.target.value)}
                        value={assignment.until}
                        className="nhsuk-input nhsuk-date-input nhsuk-input--width-8"
                        aria-label="enter until date"
                    />
                </div>
            </td>
            <td className="nhsuk-table__cell ">
                <select
                    className="nhsuk-select"
                    defaultValue={assignment.templateId}
                    value={assignment.templateId}
                    onChange={e => updateTemplate(e.target.value)}>
                    {
                        templateItems.map(t => (
                            <option key={t.id} selected={t.id === assignment.templateId} value={t.id}>{t.name} </option>
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