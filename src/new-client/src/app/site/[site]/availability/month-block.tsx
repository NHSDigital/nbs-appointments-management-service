'use client';
import { WeekInfo } from '@types';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';
import Link from 'next/link';
import { useAvailability } from './blocks';

type MonthData = {
  month: string;
  weeks: WeekInfo[];
};

const MonthBlock = ({ month, weeks }: MonthData) => {
  const { blocks } = useAvailability();
  const getWeekRange = (commencing: dayjs.Dayjs) => {
    const start = commencing.format('DD');
    const end = commencing.add(6, 'day').format('DD');
    return `${start} - ${end}`;
  };

  const canChange = (week: WeekInfo) => week.commencing > dayjs();

  const precis = (week: WeekInfo) => {
    if (week.commencing < dayjs()) return 'N/A';
    else return hasAvailability(week) ? 'Open' : 'Closed';
  };

  const hasAvailability = (week: WeekInfo) =>
    blocks.filter(
      av =>
        av.day.isAfter(week.commencing.add(-1, 'day')) &&
        av.day.isBefore(week.commencing.add(6, 'day')),
    ).length > 0;

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
