'use client';
import dayjs from 'dayjs';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import { services } from '../../services';
import { DaySummary } from '../../day-summary';
import { useAvailability } from '../../blocks';
import { AvailabilityBlock } from '@types';
import { timeSort, timeAsInt, conflictsWith } from '../common';
import { When } from '@components/when';

type Errors = {
  time?: string;
};

const SessionPage = () => {
  const { blocks, saveBlock, removeBlock } = useAvailability();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();

  const date = searchParams.get('date');
  const day = dayjs(date);

  const [conflictBlock, setConflictBlock] = React.useState<
    string | undefined
  >();
  const [errors, setErrors] = React.useState<Errors>({});
  const [startTime, setStartTime] = React.useState('09:00');
  const [endTime, setEndTime] = React.useState('12:00');
  const [sessionHolders, setSessionHolders] = React.useState(1);
  const [selectedServices, setSelectedServices] = React.useState<string[]>([]);
  const [previewBlocks, setPreviewBlocks] = React.useState<AvailabilityBlock[]>(
    [],
  );
  const [targetBlock, setTargetBlock] =
    React.useState<AvailabilityBlock | null>(null);
  const [showUnsavedChangesMessage, setShowUnsavedChangesMessage] =
    React.useState(false);

  const toggleService = (svc: string) => {
    if (selectedServices.includes(svc))
      setSelectedServices(selectedServices.filter(s => s !== svc));
    else setSelectedServices([...selectedServices, svc]);
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
        sessionHolders: 1,
        services: [],
        isPreview: true,
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

    setErrors(err);
    return err.time === undefined;
  };

  const save = () => {
    if (validate()) {
      saveBlock(
        {
          day,
          start: startTime,
          end: endTime,
          sessionHolders,
          services: selectedServices,
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
    const timeFilter = targetBlock?.isPreview ? 'na' : targetBlock?.start;
    return blocks.filter(b => b.day.isSame(day) && b.start !== timeFilter);
  }, [blocks, day, targetBlock]);

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

  React.useEffect(() => {
    if (targetBlock) {
      setStartTime(targetBlock.start);
      setEndTime(targetBlock.end);
      setSessionHolders(targetBlock.sessionHolders);
      setSelectedServices(targetBlock.services);
    }
  }, [targetBlock]);

  React.useEffect(() => {
    const test = { start: startTime, end: endTime };
    const hit = dayBlocks.find(b => conflictsWith(b, test));
    setConflictBlock(hit?.start);
  }, [startTime, endTime, blocks]);

  React.useEffect(() => {
    const pbs: AvailabilityBlock[] = [
      ...dayBlocks.filter(b => b.day.isSame(day)),
    ];
    if (targetBlock) {
      pbs.push({
        day,
        start: startTime,
        end: endTime,
        sessionHolders: sessionHolders,
        services: selectedServices,
        isPreview: true,
      });
    }
    pbs.sort(timeSort);
    setPreviewBlocks(pbs);
  }, [
    blocks,
    targetBlock,
    startTime,
    endTime,
    selectedServices,
    sessionHolders,
  ]);

  return (
    <>
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h2>
            <span>
              {day.format('MMM DD dddd')}
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
          <When condition={targetBlock !== null}>
            <div
              className={`nhsuk-form-group ${errors.time ? 'nhsuk-form-group--error' : ''}`}
            >
              {errors.time && (
                <span className="nhsuk-error-message">
                  <span className="nhsuk-u-visually-hidden">Error:</span>{' '}
                  {errors.time}
                </span>
              )}
              <label htmlFor="email" className="nhsuk-label">
                Start time
              </label>
              <input
                type="time"
                className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${errors.time ? 'nhsuk-input--error' : ''}`}
                value={startTime}
                onChange={e => setStartTime(e.target.value)}
                aria-label="enter start time"
              />
              <label
                htmlFor="email"
                className="nhsuk-label"
                style={{ marginTop: '24px' }}
              >
                End time
              </label>
              <input
                type="time"
                className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${errors.time ? 'nhsuk-input--error' : ''}`}
                value={endTime}
                onChange={e => setEndTime(e.target.value)}
                aria-label="enter start time"
              />
            </div>
            <div className="nhsuk-form-group">
              <label htmlFor="email" className="nhsuk-label">
                Maximum simultaneous appointments
              </label>
              <input
                type="number"
                className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5`}
                value={sessionHolders}
                onChange={e => setSessionHolders(parseInt(e.target.value))}
                aria-label="enter maximum number of simultaneous appointments"
              />
            </div>
            <div className="nhsuk-form-group">
              <label htmlFor="email" className="nhsuk-label">
                Services
              </label>
              <div className="nhsuk-checkboxes">
                <div className="nhsuk-checkboxes__item">
                  <input
                    id="break"
                    type="checkbox"
                    className="nhsuk-checkboxes__input"
                    checked={selectedServices.length === 0}
                    onChange={() => setSelectedServices([])}
                  />
                  <label
                    htmlFor="break"
                    className="nhsuk-label nhsuk-checkboxes__label"
                  >
                    No services - this is a break period
                  </label>
                </div>
                <div className="nhsuk-checkboxes__item">
                  <input
                    id="all"
                    type="checkbox"
                    className="nhsuk-checkboxes__input"
                    checked={selectedServices.length === services.length}
                    onChange={() =>
                      setSelectedServices(services.map(svc => svc.key))
                    }
                  />
                  <label
                    htmlFor="all"
                    className="nhsuk-label nhsuk-checkboxes__label"
                  >
                    All services
                  </label>
                </div>
                {services.map(svc => (
                  <div key={svc.key} className="nhsuk-checkboxes__item">
                    <input
                      id={svc.key}
                      type="checkbox"
                      className="nhsuk-checkboxes__input"
                      value={svc.key}
                      checked={selectedServices.includes(svc.key)}
                      onChange={() => toggleService(svc.key)}
                    />
                    <label
                      htmlFor={svc.key}
                      className="nhsuk-label nhsuk-checkboxes__label"
                    >
                      {svc.value}
                    </label>
                  </div>
                ))}
              </div>
              <div style={{ marginTop: '20px', width: '600px' }}>
                <div className="nhsuk-navigation">
                  <button
                    type="button"
                    aria-label="save user"
                    className="nhsuk-button nhsuk-u-margin-bottom-0"
                    onClick={save}
                  >
                    Save
                  </button>
                  <button
                    type="button"
                    aria-label="cancel"
                    className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
                    onClick={cancelChanges}
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </When>
          <When condition={targetBlock === null}>
            <div style={{ width: '600px' }}>
              <span className="nhsuk-caption-l nhsuk-caption--bottom">
                <span className="nhsuk-u-visually-hidden">-</span>
                No session is selected. Select on from the day preview or add a
                new session.
              </span>
            </div>
          </When>
        </div>
        <div>
          <div className="nhsuk-card nhsuk-card">
            <div className="nhsuk-card__content nhsuk-card__content--primary">
              <h2 className="nhsuk-card__heading nhsuk-heading-m">
                Day Preview
              </h2>
              <DaySummary
                blocks={previewBlocks}
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
                onClick={() => backToWeek()}
                style={{ marginLeft: '20px' }}
              >
                Back to week view
              </a>
            </div>
          </div>
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

export default SessionPage;
