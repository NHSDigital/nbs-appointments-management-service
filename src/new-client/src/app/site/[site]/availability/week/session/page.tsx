'use client';
import dayjs from 'dayjs';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import { services } from '../../services';
import { DaySummary } from '../../day-summary';
import { useAvailability } from '../../blocks';

type Errors = {
  time?: string;
};

const SessionPage = () => {
  const { dayBlocks } = useAvailability();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();

  const date = searchParams.get('date');
  const block = searchParams.get('block');

  const day = dayjs(date);

  const [errors, setErrors] = React.useState<Errors>({});
  const [startTime, setStartTime] = React.useState('09:00');
  const [endTime, setEndTime] = React.useState('12:00');
  const [sessionHolders, setSessionHolders] = React.useState(1);
  const [selectedServices, setSelectedServices] = React.useState<string[]>([]);

  const toggleService = (svc: string) => {
    if (selectedServices.includes(svc))
      setSelectedServices(selectedServices.filter(s => s !== svc));
    else setSelectedServices([...selectedServices, svc]);
  };

  const backToWeek = () => {
    const weekNumber = Math.floor(day.diff(dayjs('2024-01-01'), 'day') / 7) + 1;
    const params = new URLSearchParams(searchParams);
    params.delete('date');
    params.delete('block');
    params.set('wn', weekNumber.toString());

    replace(`${pathname.replace('/session', '')}?${params.toString()}`);
  };

  React.useEffect(() => {});

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
      <div style={{ display: 'flex' }}>
        <div
          className={`nhsuk-form-group ${errors.time ? 'nhsuk-form-group--error' : ''}`}
        >
          <div className="nhsuk-form-group">
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
          </div>
          <div style={{ marginTop: '20px' }}>
            <div className="nhsuk-navigation">
              <button
                type="button"
                aria-label="save user"
                className="nhsuk-button nhsuk-u-margin-bottom-0"
              >
                Save
              </button>
              <button
                type="button"
                aria-label="save user"
                className="nhsuk-button nhsuk-u-margin-bottom-0 nhsuk-u-margin-left-3"
              >
                Save and add another
              </button>
              <button
                type="button"
                aria-label="cancel"
                className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
                onClick={backToWeek}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
        <div>
          <ul
            className="nhsuk-grid-row nhsuk-card-group"
            style={{ padding: '20px' }}
          >
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <div className="nhsuk-card nhsuk-card">
                <div className="nhsuk-card__content nhsuk-card__content--primary">
                  <h2 className="nhsuk-card__heading nhsuk-heading-m">
                    Day Preview
                  </h2>
                  <DaySummary
                    blocks={dayBlocks(day)}
                    onAddBlock={() => {}}
                    day={day}
                  />
                </div>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </>
  );
};

export default SessionPage;
