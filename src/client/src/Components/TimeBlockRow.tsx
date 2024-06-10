import { Session } from "../Types/Schedule";
import { ServiceConfiguration, ValidationError } from "../Types";
import { ServiceSelector } from "./ServiceSelector";
import { When } from "./When";

interface TimeBlockRowProps {
    block: Session
    error?: ValidationError,
    uniqueId: string,
    enableAutoFocus: boolean
    handleTimeChange: (session: Session) => void,
    handleRemove: () => void,
    handleBlur: () => void,
    handleEnterKey: () => void,
    handleServiceChange: (event: React.ChangeEvent<HTMLInputElement>) => void
    handleSelectAllChange: (event: React.ChangeEvent<HTMLInputElement>, serviceConfiguration:ServiceConfiguration[]) => void
}

export const TimeBlockRow = ({ block, error, uniqueId, enableAutoFocus, handleTimeChange, handleBlur, handleRemove, handleEnterKey, handleServiceChange, handleSelectAllChange }: TimeBlockRowProps) => {
    return (
        <>
            <When condition={!!error}>
                <tr>
                    <th colSpan={4} className="nhsuk-form-group--error">
                            <span className="nhsuk-error-message nhsuk-u-margin-bottom-0">
                                <span className="nhsuk-u-visually-hidden">Error:</span>{" "}
                                {error?.message}
                            </span>
                    </th>
                </tr>
            </When>
            <tr role="row" className="nhsuk-table__row">
                <td className={`nhsuk-table__cell ${!!error ? "nhsuk-form-group--error" : ""}`}>
                    <div className="nhsuk-date-input__item">
                        <input
                            id={uniqueId + "_start"}
                            type="time"
                            className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${error?.field?.includes("start") ? "nhsuk-input--error" : ""}`}
                            value={block.from}
                            onChange={e => handleTimeChange({...block, from: e.target.value})}
                            onBlur={handleBlur}
                            onKeyDown={e => {
                                if (e.key === "Enter") {
                                    handleEnterKey();
                                }
                            }}
                            aria-label="enter start time"
                            autoFocus={enableAutoFocus}
                        />
                    </div>
                </td>
                <td className="nhsuk-table__cell ">
                    <div className="nhsuk-date-input__item">
                        <input
                            id={uniqueId + "_end"}
                            type="time"
                            className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${error?.field?.includes("end") ? "nhsuk-input--error" : ""}`}
                            value={block.until}
                            onChange={e => handleTimeChange({...block, until: e.target.value})}
                            onBlur={handleBlur}
                            onKeyDown={e => {
                                if (e.key === "Enter") {
                                    handleEnterKey();
                                }
                            }}
                            aria-label="enter end time"
                        />
                    </div>
                </td>
                <td className="nhsuk-table__cell ">
                    <ServiceSelector checkedServices={block.services} uniqueId={uniqueId} handleChange={handleServiceChange} handleSelectAllChange={handleSelectAllChange} hasError={error?.field === "services"} />
                </td>
                <td className="nhsuk-table__cell">
                    <button
                        className="nhsuk-button--link"
                        type="button"
                        onClick={handleRemove}>
                        Remove
                    </button>
                </td>
            </tr>
        </>
    )
}