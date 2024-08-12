'use client';
/* eslint-disable import/no-extraneous-dependencies */
import { WeekInfo } from '@types';
import dayjs from 'dayjs';
import MonthBlock from './month-block';

const AvailabilityOverviewPage = () => {
  let cursor = dayjs('2024-01-01');
  const weeks = [] as WeekInfo[];

  let weekNumber = 1;
  while (cursor.year() === 2024) {
    weeks.push({
      weekNumber,
      month: cursor.format('MMMM'),
      commencing: cursor,
    });
    weekNumber++;
    cursor = cursor.add(1, 'week');
  }

  const months = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December',
  ];

  return (
    <div className="nhsuk-width-container-fluid">
      <div>Availability Overview</div>
      <ul
        className="nhsuk-grid-row nhsuk-card-group"
        style={{ padding: '20px' }}
      >
        {months.map(m => (
          <li
            key={m}
            className="nhsuk-grid-column-one-third nhsuk-card-group__item"
          >
            <MonthBlock month={m} weeks={weeks.filter(w => w.month === m)} />
          </li>
        ))}
      </ul>
    </div>
  );
};

export default AvailabilityOverviewPage;
