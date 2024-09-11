'use client';
import { Site } from '@types';
import { Pagination } from '@components/nhsuk-frontend';
import MonthBlock from './month-block';
import { getMonthsOfTheYear, parseDate } from '@services/timeService';

type YearViewProps = {
  referenceDate: string;
  site: Site;
};

const YearViewPage = ({ referenceDate, site }: YearViewProps) => {
  const parsedDate = parseDate(referenceDate);
  const lastYear = parsedDate.subtract(1, 'year');
  const nextYear = parsedDate.add(1, 'year');

  const monthsInThisYear = getMonthsOfTheYear(parsedDate);

  return (
    <div>
      <Pagination
        previous={{
          title: lastYear.format('YYYY'),
          href: `/site/${site.id}/availability/year?date=${lastYear.format('DD-MM-YYYY')}`,
        }}
        next={{
          title: nextYear.format('YYYY'),
          href: `/site/${site.id}/availability/year?date=${nextYear.format('DD-MM-YYYY')}`,
        }}
      />
      <ul className="nhsuk-grid-row nhsuk-card-group">
        {monthsInThisYear.map((month, index) => (
          <li
            key={`month-card-${month.month}-${index}`}
            className="nhsuk-grid-column-one-half nhsuk-card-group__item"
          >
            <MonthBlock month={month} site={site} />
          </li>
        ))}
      </ul>
    </div>
  );
};

export default YearViewPage;
