import { Pagination, Spinner } from '@components/nhsuk-frontend';
import { Site } from '@types';
import { Suspense } from 'react';
import {
  dateTimeStringFormat,
  DayJsType,
  dayStringFormat,
  getUkWeeksOfTheMonth,
} from '@services/timeService';
import { WeekCardList } from './week-card-list';

type Props = {
  site: Site;
  searchMonth: DayJsType;
};

export const ViewAvailabilityPage = async ({ site, searchMonth }: Props) => {
  const nextMonth = searchMonth.startOf('month').add(1, 'month');
  const previousMonth = searchMonth.startOf('month').subtract(1, 'month');

  const next = {
    title: nextMonth.format('MMMM YYYY'),
    href: `view-availability?date=${nextMonth.format(dayStringFormat)}`,
  };
  const previous = {
    title: previousMonth.format('MMMM YYYY'),
    href: `view-availability?date=${previousMonth.format(dayStringFormat)}`,
  };

  const ukWeeks = getUkWeeksOfTheMonth(searchMonth);

  return (
    <>
      <Pagination previous={previous} next={next} />
      {/* TODO does the suspense key have to be UTC?? */}
      <Suspense
        key={searchMonth.format(dateTimeStringFormat)}
        fallback={<Spinner />}
      >
        <WeekCardList site={site} ukWeeks={ukWeeks} />
      </Suspense>
    </>
  );
};
