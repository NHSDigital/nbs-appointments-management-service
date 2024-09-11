'use client';
import { useAvailability } from '@hooks/useAvailability';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';
import Link from 'next/link';
import { formatDateForUrl, Month } from '@services/timeService';
import { Site } from '@types';

type MonthData = {
  month: Month;
  site: Site;
};

const MonthBlock = ({ month, site }: MonthData) => {
  const { blocks } = useAvailability();

  const canChange = (week: dayjs.Dayjs[]) => week.some(day => day > dayjs());

  const precis = (week: dayjs.Dayjs[]) => {
    if (week[0] < dayjs()) return 'N/A';
    else return hasAvailability(week) ? 'Open' : 'Closed';
  };

  const hasAvailability = (week: dayjs.Dayjs[]) =>
    blocks.filter(
      av =>
        av.day.isAfter(week[0].add(-1, 'day')) &&
        av.day.isBefore(week[0].add(6, 'day')),
    ).length > 0;

  return (
    <div className="nhsuk-card nhsuk-card">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">{month.month}</h2>
        <dl className="nhsuk-summary-list" style={{ marginBottom: '16px' }}>
          {month.weeks.map((week, index) => (
            <div key={index} className="nhsuk-summary-list__row">
              <dt className="nhsuk-summary-list__key">{getWeekRange(week)}</dt>
              <dd className="nhsuk-summary-list__value">{precis(week)}</dd>
              <dd className="nhsuk-summary-list__actions">
                <Link
                  href={`/site/${site.id}/availability/week?date=${formatDateForUrl(week[0])}`}
                >
                  {canChange(week) ? 'Change' : 'View'}
                  <span className="nhsuk-u-visually-hidden"> name</span>
                </Link>
              </dd>
            </div>
          ))}
          <br />
          <Link
            href={`/site/${site.id}/availability/month?date=${formatDateForUrl(month.weeks[0][0])}`}
          >
            Month view
          </Link>
        </dl>
      </div>
    </div>
  );
};

const getWeekRange = (week: dayjs.Dayjs[]) => {
  const start = week[0].format('DD');
  const end = week[week.length - 1].format('DD');

  return `${start} - ${end}`;
};

export default MonthBlock;
