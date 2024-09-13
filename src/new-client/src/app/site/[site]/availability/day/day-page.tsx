'use client';
import dayjs from 'dayjs';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import { AvailabilityBlock, Site } from '@types';
import { When } from '@components/when';
import CheckboxSelector from '@components/checkbox-selector';
import {
  calculateNumberOfAppointments,
  conflictsWith,
  isWithin,
  services,
  summariseServices,
  timeAsInt,
  timeSort,
} from '@services/availabilityService';
import { DaySummary } from '@components/day-summary';
import { useAvailability } from '@hooks/useAvailability';
import { Pagination } from '@components/nhsuk-frontend';
import { formatDateForUrl, parseDate } from '@services/timeService';

type Errors = {
  time?: string;
  services?: string;
  break?: string;
};

const sessionHolderOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
const appointmentLengthOptions = [4, 5, 6, 10, 12, 15];

type DayViewProps = {
  referenceDate: string;
  site: Site;
};

const DayViewPage = ({ referenceDate, site }: DayViewProps) => {
  const parsedDate = parseDate(referenceDate);
  const { blocks, saveBlock, removeBlock } = useAvailability();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();

  const day = dayjs(parsedDate);

  const [conflictBlock, setConflictBlock] = React.useState<
    string | undefined
  >();
  const [errors, setErrors] = React.useState<Errors>({});
  const [startTime, setStartTime] = React.useState('09:00');
  const [endTime, setEndTime] = React.useState('12:00');
  const [sessionHolders, setSessionHolders] = React.useState(1);
  const [appointmentLength, setAppointmentLength] = React.useState(5);
  const [selectedServices, setSelectedServices] = React.useState<string[]>([]);
  const [targetBlock, setTargetBlock] =
    React.useState<AvailabilityBlock | null>(null);
  const [showUnsavedChangesMessage, setShowUnsavedChangesMessage] =
    React.useState(false);

  const hasErrors = React.useMemo(
    () =>
      errors.time !== undefined ||
      errors.services !== undefined ||
      errors.break !== undefined,
    [errors],
  );

  const addBreak = () => {
    if (checkForUnsavedChanges()) {
      setShowUnsavedChangesMessage(true);
    } else {
      setShowUnsavedChangesMessage(false);
      setTargetBlock({
        day,
        start: '09:00',
        end: '10:00',
        appointmentLength: 5,
        sessionHolders: 0,
        services: [],
        isBreak: true,
        isPreview: true,
      });
    }
  };

  const addSession = () => {
    if (checkForUnsavedChanges()) {
      setShowUnsavedChangesMessage(true);
    } else {
      setShowUnsavedChangesMessage(false);
      setTargetBlock({
        day,
        start: '09:00',
        end: '10:00',
        appointmentLength: 5,
        sessionHolders: 1,
        services: [],
        isPreview: true,
        isBreak: false,
      });
    }
  };

  const cancelChanges = () => {
    setTargetBlock(null);
    setShowUnsavedChangesMessage(false);
    setErrors({});
  };

  const backToWeek = () => {
    const weekNumber = Math.floor(day.diff(dayjs('2024-01-01'), 'day') / 7) + 1;
    const params = new URLSearchParams(searchParams);
    params.delete('date');
    params.delete('block');
    params.set('wn', weekNumber.toString());

    replace(`${pathname.replace('/session', '')}?${params.toString()}`);
  };

  const checkForUnsavedChanges = () =>
    targetBlock &&
    (targetBlock?.isPreview ||
      targetBlock?.start != startTime ||
      targetBlock?.end != endTime ||
      targetBlock.sessionHolders != sessionHolders ||
      targetBlock?.services != selectedServices);

  const validate = () => {
    const err = {} as Errors;
    const st = timeAsInt(startTime);
    const et = timeAsInt(endTime);

    if (et !== 0 && et <= st) {
      err.time = 'The start time must be earlier than the end time.';
    } else if (conflictBlock) {
      const hit = blocks.find(b => b.start === conflictBlock);
      if (hit)
        err.time = `A conflicting session already exists between ${hit.start} and ${hit.end}`;
    }

    if (selectedServices.length == 0 && !targetBlock?.isBreak) {
      err.services = 'You must select at least one service.';
    }

    if (targetBlock?.isBreak) {
      if (
        blocks.filter(b => !b.isBreak && isWithin(targetBlock, b)).length == 0
      ) {
        err.break = 'Breaks must exist within a session.';
      }
    }

    setErrors(err);
    return (
      err.time === undefined &&
      err.services === undefined &&
      err.break === undefined
    );
  };

  const save = () => {
    if (validate()) {
      saveBlock(
        {
          day,
          start: startTime,
          end: endTime,
          appointmentLength,
          sessionHolders,
          services: selectedServices,
          isBreak: targetBlock?.isBreak,
        },
        targetBlock !== null && !targetBlock.isPreview
          ? targetBlock
          : undefined,
      );
      setShowUnsavedChangesMessage(false);
      setTargetBlock(null);
    }
  };

  const edit = (bl: AvailabilityBlock) => {
    if (checkForUnsavedChanges()) {
      setShowUnsavedChangesMessage(true);
    } else {
      setShowUnsavedChangesMessage(false);
      setTargetBlock(bl);
    }
  };

  const remove = (bl: AvailabilityBlock) => {
    if (checkForUnsavedChanges()) {
      setShowUnsavedChangesMessage(true);
    } else {
      setShowUnsavedChangesMessage(false);
      removeBlock(bl);
    }
  };

  const dayBlocks = React.useMemo(() => {
    return blocks.filter(b => b.day.isSame(day)).toSorted(timeSort);
  }, [blocks, day]);

  const editAction = {
    title: 'Edit',
    action: edit,
    test: (b: AvailabilityBlock) => !b.isPreview,
  };

  const removeAction = {
    title: 'Delete',
    action: remove,
    test: (b: AvailabilityBlock) => !b.isPreview,
  };

  const targetBlockAppointments = React.useMemo(
    () =>
      calculateNumberOfAppointments(
        {
          day,
          start: startTime,
          end: endTime,
          appointmentLength,
          services: [],
          sessionHolders,
          isBreak: false,
        },
        blocks,
      ),
    [startTime, endTime, appointmentLength, sessionHolders, day, blocks],
  );

  React.useEffect(() => {
    if (targetBlock) {
      setStartTime(targetBlock.start);
      setEndTime(targetBlock.end);
      setSessionHolders(targetBlock.sessionHolders);
      setSelectedServices(targetBlock.services);
      setAppointmentLength(targetBlock.appointmentLength);
    }
  }, [targetBlock]);

  React.useEffect(() => {
    const test = { start: startTime, end: endTime };
    const hit = dayBlocks.find(
      b => b.isBreak === targetBlock?.isBreak && conflictsWith(b, test),
    );
    setConflictBlock(hit?.start);
  }, [startTime, endTime, targetBlock, blocks, dayBlocks]);

  const yesterday = parsedDate.subtract(1, 'day');
  const tomorrow = parsedDate.add(1, 'day');

  return (
    <>
      <Pagination
        previous={{
          title: yesterday.format('DD MMMM YYYY'),
          href: `/site/${site.id}/availability/day?date=${formatDateForUrl(yesterday)}`,
        }}
        next={{
          title: tomorrow.format('DD MMMM YYYY'),
          href: `/site/${site.id}/availability/day?date=${formatDateForUrl(tomorrow)}`,
        }}
      />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h2>
            <span>
              <span className="nhsuk-caption-xl nhsuk-caption--bottom">
                <span className="nhsuk-u-visually-hidden">-</span>
                Manage sessions for this day
              </span>
            </span>
          </h2>
        </div>
      </div>
      <div style={{ display: 'flex', justifyContent: 'flex-start' }}>
        <div>
          <div className="nhsuk-card nhsuk-card">
            <div className="nhsuk-card__content nhsuk-card__content--primary">
              <h2 className="nhsuk-card__heading nhsuk-heading-m">
                Day Preview
              </h2>
              <DaySummary
                blocks={dayBlocks}
                showBreaks={true}
                hasError={b =>
                  errors.time !== undefined &&
                  b.start === conflictBlock &&
                  !b.isPreview
                }
                primaryAction={editAction}
                secondaryAction={removeAction}
              />
              <a href="#" onClick={() => addSession()}>
                Add a session
              </a>
              <a
                href="#"
                onClick={() => addBreak()}
                style={{ marginLeft: '20px' }}
              >
                Add a break
              </a>
              <a
                href="#"
                onClick={() => backToWeek()}
                style={{ marginLeft: '20px' }}
              >
                Back to week view
              </a>
            </div>
          </div>
          <div>
            <When condition={targetBlock !== null}>
              <div className="nhsuk-card nhsuk-card">
                <div className="nhsuk-card__content nhsuk-card__content--primary">
                  <h2 className="nhsuk-card__heading nhsuk-heading-m">
                    Edit session details
                  </h2>
                  <table>
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
                        <th>Max Concurrent Appts.</th>
                        <th>Appointment Length</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr role="row" className="nhsuk-table__row">
                        <td className="nhsuk-table__cell">
                          <div className="nhsuk-date-input__item">
                            <input
                              type="time"
                              className="nhsuk-input nhsuk-date-input nhsuk-input--width-5"
                              value={startTime}
                              onChange={e => setStartTime(e.target.value)}
                              aria-label="enter start time"
                            />
                          </div>
                        </td>
                        <td className="nhsuk-table__cell ">
                          <div className="nhsuk-date-input__item">
                            <input
                              type="time"
                              className="nhsuk-input nhsuk-date-input nhsuk-input--width-5"
                              value={endTime}
                              onChange={e => setEndTime(e.target.value)}
                              aria-label="enter end time"
                            />
                          </div>
                        </td>
                        <td className="nhsuk-table__cell ">
                          <When condition={!targetBlock?.isBreak}>
                            <CheckboxSelector
                              defaultMessage="Select services for the clinic"
                              options={services}
                              defaultOptions={targetBlock?.services}
                              summarise={opts =>
                                summariseServices(
                                  opts,
                                  'Select services for the clinic',
                                )
                              }
                              onChange={opts => setSelectedServices(opts)}
                            />
                          </When>
                          <When condition={targetBlock?.isBreak ?? false}>
                            Break period
                          </When>
                        </td>
                        <td className="nhsuk-table__cell">
                          <When condition={!targetBlock?.isBreak}>
                            <select
                              key="b"
                              className="nhsuk-select"
                              name="max-simul"
                              style={{ width: '120px' }}
                              defaultValue={sessionHolders}
                              onChange={e =>
                                setSessionHolders(parseInt(e.target.value))
                              }
                            >
                              {sessionHolderOptions.map(n => (
                                <option key={n} value={n}>
                                  {n}
                                </option>
                              ))}
                            </select>
                          </When>
                        </td>
                        <td className="nhsuk-table__cell">
                          <When condition={!targetBlock?.isBreak}>
                            <div>
                              <select
                                key="c"
                                className="nhsuk-select"
                                name="max-simul"
                                style={{ width: '64px' }}
                                defaultValue={appointmentLength}
                                onChange={e =>
                                  setAppointmentLength(parseInt(e.target.value))
                                }
                              >
                                {appointmentLengthOptions.map(n => (
                                  <option key={n} value={n}>
                                    {n}
                                  </option>
                                ))}
                              </select>
                              <span style={{ marginLeft: '32px' }}>
                                <b>{targetBlockAppointments}</b> appointments
                              </span>
                            </div>
                          </When>
                        </td>
                        <td className="nhsuk-table__cell">
                          <a href="#" onClick={() => save()}>
                            Save
                          </a>
                          <a
                            href="#"
                            onClick={() => cancelChanges()}
                            style={{ marginLeft: '20px' }}
                          >
                            Cancel
                          </a>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </When>
          </div>
          <When condition={hasErrors}>
            <div
              className="nhsuk-error-summary"
              aria-labelledby="error-summary-title"
              role="alert"
            >
              <h2
                className="nhsuk-error-summary__title"
                id="error-summary-title"
              >
                There are problems with the clinic settings
              </h2>
              <When condition={errors.time !== undefined}>
                <div className="nhsuk-error-summary__body">
                  <p>{errors.time}</p>
                </div>
              </When>
              <When condition={errors.services !== undefined}>
                <div className="nhsuk-error-summary__body">
                  <p>{errors.services}</p>
                </div>
              </When>
              <When condition={errors.break !== undefined}>
                <div className="nhsuk-error-summary__body">
                  <p>{errors.break}</p>
                </div>
              </When>
            </div>
          </When>
          <When condition={showUnsavedChangesMessage}>
            <div
              className="nhsuk-error-summary"
              aria-labelledby="error-summary-title"
              role="alert"
            >
              <h2
                className="nhsuk-error-summary__title"
                id="error-summary-title"
              >
                You have unsaved changes
              </h2>
              <div className="nhsuk-error-summary__body">
                <p>
                  Please save or cancel your current edits before continuing
                </p>
              </div>
            </div>
          </When>
        </div>
      </div>
    </>
  );
};

export default DayViewPage;
