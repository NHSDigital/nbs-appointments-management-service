'use client';
import { AvailabilityBlock, WeekInfo } from '@types';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';
import Link from 'next/link';

type MonthData = {
  month: string;
  weeks: WeekInfo[];
};

const MonthBlock = ({ month, weeks }: MonthData) => {
  const getWeekRange = (commencing: dayjs.Dayjs) => {
    const start = commencing.format('DD');
    const end = commencing.add(6, 'day').format('DD');
    return `${start} - ${end}`;
  };

  const loadAvailability = () => {
    const storedAvailability = localStorage.getItem('availability');
    if (storedAvailability) {
      const temp = JSON.parse(storedAvailability) as AvailabilityBlock[];
      return temp.map(t => ({
        ...t,
        day: dayjs(t.day),
      })) as AvailabilityBlock[];
    }
    return [] as AvailabilityBlock[];
  };

  const availability = loadAvailability();

  const canChange = (week: WeekInfo) => week.commencing > dayjs();

  const precis = (week: WeekInfo) => {
    if (week.commencing < dayjs()) return 'N/A';
    else return hasAvailability(week) ? 'Open' : 'Closed';
  };

  const hasAvailability = (week: WeekInfo) =>
    availability.find(
      av =>
        av.day.isAfter(week.commencing) &&
        av.day.isBefore(week.commencing.add(6, 'day')),
    ) !== undefined;

  return (
    <div className="nhsuk-card nhsuk-card">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">{month}</h2>
        <dl className="nhsuk-summary-list">
          {weeks.map((w, i) => (
            <div key={i} className="nhsuk-summary-list__row">
              <dt className="nhsuk-summary-list__key">
                {getWeekRange(w.commencing)}
              </dt>
              <dd className="nhsuk-summary-list__value">{precis(w)}</dd>
              <dd className="nhsuk-summary-list__actions">
                <Link href={`availability/week?wn=${w.weekNumber}`}>
                  {canChange(w) ? 'Change' : 'View'}
                  <span className="nhsuk-u-visually-hidden"> name</span>
                </Link>
              </dd>
            </div>
          ))}
        </dl>
      </div>
    </div>
  );
};

export default MonthBlock;
