'use client';

import { When } from '@components/when';
import React from 'react';

const timeSort = function (a: AvailabilityBlock, b: AvailabilityBlock) {
  return (
    Date.parse('1970/01/01 ' + a.start) - Date.parse('1970/01/01 ' + b.start)
  );
};

type AvailabilityBlock = {
  day: string;
  start: string;
  end: string;
  sessionHolders: number;
  services: number;
  isPreview?: boolean;
};

const AvailabilityPage = () => {
  const [mode, setMode] = React.useState('week');
  const [blocks, setBlocks] = React.useState([] as AvailabilityBlock[]);
  const [selectedDay, setSelectedDay] = React.useState('Monday');

  const onAddBlock = (day: string) => {
    setSelectedDay(day);
    setMode('session');
  };

  const saveTimeBlock = (block: AvailabilityBlock) => {
    setBlocks([...blocks, block]);
    setMode('week');
  };

  const dayBlocks = (d: string) =>
    blocks.filter(b => b.day === d).sort(timeSort);

  return (
    <div className="nhsuk-width-container-fluid">
      <When condition={mode === 'week'}>
        <WeekView onAddBlock={onAddBlock} blocks={blocks} />
      </When>
      <When condition={mode === 'session'}>
        <SessionView
          day={selectedDay}
          blocks={dayBlocks(selectedDay)}
          onSave={saveTimeBlock}
          onCancel={() => setMode('week')}
        />
      </When>
    </div>
  );
};

type SessionViewProps = {
  day: string;
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
          <h3>{day}</h3>
          <div className="nhsuk-hint">Add a new time block to this day</div>
        </div>

        <div
          className={`nhsuk-form-group ${false ? 'nhsuk-form-group--error' : ''}`}
        >
          <label className="nhsuk-label">Start Time</label>
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
          <label className="nhsuk-label">End Time</label>
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
          <label className="nhsuk-label">Session holders</label>
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

type WeekViewProps = {
  onAddBlock: (day: string) => void;
  blocks: AvailabilityBlock[];
};

const WeekView = ({ onAddBlock, blocks }: WeekViewProps) => {
  const days = [
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday',
    'Sunday',
  ];

  const dayBlocks = (d: string) =>
    blocks.filter(b => b.day === d).sort(timeSort);

  return (
    <>
      <div>Week Template</div>
      <div className="nhsuk-width-container-fluid">
        <ul
          className="nhsuk-grid-row nhsuk-card-group"
          style={{ padding: '20px' }}
        >
          {days.map(d => (
            <li
              key={d}
              className="nhsuk-grid-column-one-third nhsuk-card-group__item"
            >
              <DayCard
                day={d}
                blocks={dayBlocks(d)}
                onAddBlock={() => onAddBlock(d)}
              />
            </li>
          ))}
        </ul>
      </div>
    </>
  );
};

type DayCardProps = {
  onAddBlock: () => void;
  day: string;
  blocks: AvailabilityBlock[];
};

const DayCard = ({ day, onAddBlock, blocks }: DayCardProps) => {
  return (
    <div className="nhsuk-card nhsuk-card">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">{day}</h2>
        <DaySummary blocks={blocks} />
        <a href="#" onClick={onAddBlock}>
          Add a time block
        </a>
      </div>
    </div>
  );
};

const DaySummary = ({ blocks }: { blocks: AvailabilityBlock[] }) => {
  return (
    <dl className="nhsuk-summary-list">
      {blocks.map((b, i) => (
        <div key={i} className="nhsuk-summary-list__row">
          <dt className="nhsuk-summary-list__key">
            {b.start} - {b.end}
          </dt>
          <dd className="nhsuk-summary-list__value">
            {b.sessionHolders} session holders , {b.services} services
          </dd>
          <dd className="nhsuk-summary-list__actions">
            <a href="#">
              Change<span className="nhsuk-u-visually-hidden"> name</span>
            </a>
          </dd>
        </div>
      ))}
    </dl>
  );
};

export default AvailabilityPage;
