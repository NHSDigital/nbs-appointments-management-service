import { daysOfTheWeek, Site } from '@types';
import MonthViewCard from './month-view-card';
import dayjs from 'dayjs';
import { getWeeksOfTheMonth } from '@services/timeService';
import { Pagination } from '@components/nhsuk-frontend';

type MonthViewProps = {
  referenceDate: dayjs.Dayjs;
  site: Site;
};

const MonthOverviewPage = ({ referenceDate, site }: MonthViewProps) => {
  const datesInMonth = getWeeksOfTheMonth(referenceDate);

  const lastMonth = referenceDate.subtract(1, 'month');
  const nextMonth = referenceDate.add(1, 'month');

  return (
    <>
      <Pagination
        previous={{
          title: lastMonth.format('MMMM YYYY'),
          href: `/site/${site.id}/availability/month?date=${lastMonth.format('DD-MM-YYYY')}`,
        }}
        next={{
          title: nextMonth.format('MMMM YYYY'),
          href: `/site/${site.id}/availability/month?date=${nextMonth.format('DD-MM-YYYY')}`,
        }}
      />
      <div
        className="nhsuk-grid-row"
        style={{ marginLeft: 0, marginRight: 0, marginTop: 16 }}
      >
        <div className="nhsuk-grid-column-full">
          <div className="nhsuk-grid-row">
            {daysOfTheWeek.map((day, dayIndex) => (
              <div
                className="nhsuk-grid-column-custom__one-seventh"
                key={`day-of-the-week-${dayIndex}`}
              >
                <h6>{day}</h6>
              </div>
            ))}
          </div>

          {datesInMonth.map((week, weekIndex) => (
            <div
              className="nhsuk-grid-row"
              id={`week-${weekIndex}`}
              key={`week-${weekIndex}`}
            >
              {week.map((day, dayIndex) => (
                <div
                  className="nhsuk-grid-column-custom__one-seventh"
                  id={`week-${weekIndex}-day-${dayIndex}`}
                  key={`week-${weekIndex}-day-${dayIndex}`}
                >
                  <MonthViewCard date={day} site={site} />
                </div>
              ))}
            </div>
          ))}
        </div>
      </div>
    </>
  );
};

export default MonthOverviewPage;
