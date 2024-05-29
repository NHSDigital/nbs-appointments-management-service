import React from "react";
import { DayOfWeek, Session } from "../Types/Schedule";
import { TimeBlockRow } from "./TimeBlockRow";
import { ValidationError } from "../Types/ValidationError";
import { calculateNewSessionTime, parseTime } from "../utils";
import { When } from "./When";
import { ServiceConfiguration } from "../Types/SiteConfiguration";

interface TimeBlockProps {
    dayLabel: DayOfWeek,
    scheduleBlocks: Session[]
    setDayBlocks: (dayBlocks: Session[]) => void,
    setIsValid: (isValid: boolean) => void,
    copyToAllDays: (sourceDay: DayOfWeek) => void
}

export const TimeBlocks = ({ dayLabel, scheduleBlocks, setDayBlocks, setIsValid, copyToAllDays }: TimeBlockProps) => {

    const [validationErrors, setValidationErrors] = React.useState<ValidationError[]>([]);
    const [enableAutoFocus, setEnableAutoFocus] = React.useState(false);

    const handleTimeChange = (value: string, type: "start" | "end", i: number) => {
        if (scheduleBlocks) {
            if (type === "start") {
                scheduleBlocks[i].from = value;
            } else {
                scheduleBlocks[i].until = value;
            }
        }
        setDayBlocks([...scheduleBlocks]);
    }

    const handleServiceChange = (event: React.ChangeEvent<HTMLInputElement>, index: number) => {
        const services = [...scheduleBlocks[index].services];
        if (event.target.checked && !services.includes(event.target.value)) {
            services.push(event.target.value);
        } else {
            services.splice(services.indexOf(event.target.value), 1);
        }
        scheduleBlocks[index].services = services;
        setDayBlocks([...scheduleBlocks]);
    }

    const handleSelectAllChange = (event: React.ChangeEvent<HTMLInputElement>,  serviceConfiguration:ServiceConfiguration[], index: number) => {
      let allEnabledServiceCodes = serviceConfiguration
         .filter(x => x.enabled)
         .map(x => x.code);
      if (event.target.checked) {
         scheduleBlocks[index].services = allEnabledServiceCodes!;
      } else {
         scheduleBlocks[index].services = [];
      }
      setDayBlocks([...scheduleBlocks]);
    }

    const addBlock = () => {
        if (!canAddBlocks || validationErrors.length) {
            return;
        }
        !enableAutoFocus && setEnableAutoFocus(true);
        const lastBlock = scheduleBlocks[scheduleBlocks.length - 1]
        const newSession = calculateNewSessionTime(lastBlock?.until, {start: "09:00", end: "15:00"});
        setDayBlocks([...scheduleBlocks, { from: newSession.start, until: newSession.end, services: lastBlock?.services ?? [] }])
    }

    const removeBlock = (index: number) => {
        setDayBlocks([...scheduleBlocks.filter((_, i) => i !== index)]);
    }

    const sortBlocks = () => {
        // TODO directly compare arrays instead of checking order?
        // as this uses string comparison, using 24 to ensure sort order
        const replaceZeros = (session: Session) => session.until.startsWith("00") ? session.until.replace(/^\d{2}/, "24") : session.until; 
        const isInOrder = scheduleBlocks.every((s, i, arr) => {
            const lastItem = arr[i-1];
            if(lastItem){
                const lastUntilWithMidnight = replaceZeros(lastItem)
                const thisUntilWithMidnight = replaceZeros(s)
                return lastUntilWithMidnight < thisUntilWithMidnight;
            } else {
                return true;
            }
        });
        if(!isInOrder) {
            scheduleBlocks.sort((x, y) => {
                if(!x.from || !x.until) return 0;
                const xUntilWithMidnight = replaceZeros(x);
                const yUntilWithMidnight = replaceZeros(y);
                return  xUntilWithMidnight === yUntilWithMidnight ? 0 : xUntilWithMidnight < yUntilWithMidnight ? -1 : 1
            });
            setDayBlocks([...scheduleBlocks]);
        }
    }

    React.useEffect(() => {
        const validationErrors: ValidationError[] = [];
        scheduleBlocks.map(s => ({ from: parseTime(s.from), until: parseTime(s.until, true), services: s.services})).forEach((s, i, arr) => {
            if (!s.from || !s.until) {
                const message = `Please enter a valid ${!s.from ? "start" : "end"} time`;
                validationErrors.push({ index: i, message: message, field: !s.from ? "start" : "end" });
                return;
            } else {
                const diff = s.until.getTime() - s.from.getTime();
                // 1000 x 60 x 60 = number of ms in an hour
                if (diff > 0 && diff < 3600000) {
                    validationErrors.push({ index: i, message: "Time periods must be a minimum of 60 minutes", field:"start&end" });
                    return;
                }
                if (diff <= 0) {
                    validationErrors.push({ index: i, message: "End time must be after start time", field:"start&end" });
                    return;
                }
                const previousSession = i > 0 && arr[i - 1]?.until;
                if (previousSession && s.from.getTime() < previousSession.getTime()) {
                    validationErrors.push({ index: i, message: "Time periods cannot overlap", field:"start&end" });
                }
            }

            if (s.services.length === 0) {
                validationErrors.push(({ index: i, message: "At least one service is required", field: "services", }))
            }
        });
        setValidationErrors(validationErrors);
        setIsValid(validationErrors.length === 0);
    }, [scheduleBlocks, setIsValid])

    // memoised to prevent recalculating for all days
    const canAddBlocks = React.useMemo(() => {
        let totalTime: number = 0;
        const startOfDay = parseTime("00:00")!.getTime()
        scheduleBlocks.map(s => ({from: parseTime(s.from), until: parseTime(s.until, true), services:s.services})).forEach((block, i, arr) => {
            if (block.from && block.until) {
                // add time from start of day to first time
                if (i === 0) {
                    totalTime += block.from.getTime() - startOfDay;
                } else {  // add time between sessions
                    const lastUntil = arr[i - 1].until;
                    totalTime += lastUntil ? block.from.getTime() - lastUntil.getTime() : 0;
                }
                // sum of time accounted for by sessions
                totalTime += block.until.getTime() - block.from.getTime();
            }
        });
        return totalTime < (1000 * 60 * 60 * 24);
    }, [scheduleBlocks])

    return <table className="nhsuk-table">
        <caption className="nhsuk-table__caption">{dayLabel}</caption>
        <When condition={scheduleBlocks?.length > 0}>
            <thead className="nhsuk-table__head">
                <tr role="row">
                    <th role="columnheader" scope="col">
                        Start Time
                    </th>
                    <th role="columnheader" scope="col">
                        End Time
                    </th>
                    <th role="columnheader" scope="col">
                        Services
                    </th>
                    <th>Actions</th>
                </tr>
            </thead>
        </When>
        <tbody className="nhsuk-table__body">
            {scheduleBlocks.length ?
                scheduleBlocks.map((block, i) => {
                    return <TimeBlockRow
                        block={block}
                        handleTimeChange={(value, type) => handleTimeChange(value, type, i)}
                        handleRemove={() => removeBlock(i)}
                        handleBlur={sortBlocks}
                        handleEnterKey={addBlock}
                        error={validationErrors.find(x => x.index === i)}
                        key={`timeblockrow_${i}`}
                        uniqueId={dayLabel + i}
                        handleServiceChange={e => handleServiceChange(e, i)}
                        handleSelectAllChange={(e, serviceConfig) => handleSelectAllChange(e, serviceConfig, i)}
                        enableAutoFocus={enableAutoFocus}
                    />
                }) :
                <tr><td colSpan={4}>Currently no time blocks are assigned to this day.</td></tr>
            }
            <tr>
                <th>
                    <button
                        className="nhsuk-button--link"
                        type="button"
                        onClick={addBlock}
                        disabled={!canAddBlocks || validationErrors.length > 0}>
                        Add time period
                    </button>
                </th>
                <When condition={scheduleBlocks.length > 0}>
                    <th>
                        <button
                            className="nhsuk-button--link"
                            type="button"
                            onClick={() => {setDayBlocks([])}}>
                            Remove all
                        </button>
                    </th>
                </When>
                <When condition={dayLabel === "Monday" && scheduleBlocks.length > 0}>
                    <th>
                        <button
                            className="nhsuk-button--link"
                            type="button"
                            onClick={() => {copyToAllDays(dayLabel)}}
                            disabled={validationErrors.length > 0}>
                                Copy to all days
                        </button>
                    </th>
                </When>
            </tr>
        </tbody>
    </table>
}