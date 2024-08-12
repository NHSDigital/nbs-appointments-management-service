'use client';
import { AvailabilityBlock } from '@types';
import React from 'react';
import { timeSort } from './common';
import { When } from '@components/when';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';

type SessionViewProps = {
  day: dayjs.Dayjs;
  blocks: AvailabilityBlock[];
  onCancel: () => void;
  onSave: (block: AvailabilityBlock) => void;
};

const SessionView = ({ day, blocks, onSave, onCancel }: SessionViewProps) => {
  const [startTime, setStartTime] = React.useState('09:00');
  const [endTime, setEndTime] = React.useState('12:00');
  const [sessionHolders, setSessionHolders] = React.useState(1);
  const [previewBlocks, setPreviewBlocks] = React.useState(
    [] as AvailabilityBlock[],
  );

  React.useEffect(() => {
    const temp = [
      ...blocks,
      {
        day,
        start: startTime,
        end: endTime,
        sessionHolders,
        services: 1,
        isPreview: true,
      },
    ];
    temp.sort(timeSort);
    setPreviewBlocks(temp);
  }, [blocks, startTime, endTime, day, sessionHolders]);

  const save = () => {
    const block: AvailabilityBlock = {
      day,
      start: startTime,
      end: endTime,
      sessionHolders,
      services: 1,
    };
    onSave(block);
  };

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <h3>{day.format('MMMM DD ddd')}</h3>
          <div className="nhsuk-hint">Add a new time block to this day</div>
        </div>

        <div
          className={`nhsuk-form-group ${false ? 'nhsuk-form-group--error' : ''}`}
        >
          <span className="nhsuk-label">Start Time</span>
          <When condition={false}>
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error:</span> {'error'}
            </span>
          </When>
          <input
            type="time"
            className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${false ? 'nhsuk-input--error' : ''}`}
            aria-label="enter start time"
            defaultValue={startTime}
            onChange={e => setStartTime(e.target.value)}
          />
        </div>

        <div
          className={`nhsuk-form-group ${false ? 'nhsuk-form-group--error' : ''}`}
        >
          <span className="nhsuk-label">End Time</span>
          <When condition={false}>
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error:</span> {'error'}
            </span>
          </When>
          <input
            type="time"
            className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${false ? 'nhsuk-input--error' : ''}`}
            aria-label="enter end time"
            defaultValue={endTime}
            onChange={e => setEndTime(e.target.value)}
          />
        </div>

        <div
          className={`nhsuk-form-group ${false ? 'nhsuk-form-group--error' : ''}`}
        >
          <span className="nhsuk-label">Session holders</span>
          <When condition={false}>
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error:</span> {'error'}
            </span>
          </When>
          <input
            type="number"
            defaultValue={sessionHolders}
            onChange={e => setSessionHolders(parseInt(e.target.value))}
            className={`nhsuk-input nhsuk-date-input nhsuk-input--width-5 ${false ? 'nhsuk-input--error' : ''}`}
            aria-label="enter number of session holders"
          />
        </div>

        <div className="nhsuk-navigation">
          <button
            type="button"
            aria-label="save user"
            className="nhsuk-button nhsuk-u-margin-bottom-0"
            onClick={save}
          >
            Confirm and add time block
          </button>
          <button
            type="button"
            aria-label="cancel"
            className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
            onClick={onCancel}
          >
            Cancel
          </button>
        </div>
      </div>
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-card nhsuk-card">
          <div className="nhsuk-card__content nhsuk-card__content--primary">
            <h2 className="nhsuk-card__heading nhsuk-heading-m">Day Preview</h2>
            <dl className="nhsuk-summary-list">
              {previewBlocks.map((b, i) => (
                <div key={i} className="nhsuk-summary-list__row">
                  <dt className="nhsuk-summary-list__key">
                    {b.isPreview ? '*' : ''} {b.start} - {b.end}
                  </dt>
                  <dd className="nhsuk-summary-list__value">
                    {b.sessionHolders} session holders , {b.services} services
                  </dd>
                  <dd className="nhsuk-summary-list__actions">
                    <a href="#">
                      Apply services
                      <span className="nhsuk-u-visually-hidden"> name</span>
                    </a>
                  </dd>
                </div>
              ))}
            </dl>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SessionView;
