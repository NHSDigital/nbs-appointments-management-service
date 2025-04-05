import { Pagination, Spinner } from '@components/nhsuk-frontend';
import { Site } from '@types';
import { Suspense } from 'react';
import { getWeeksOfTheMonth } from '@services/timeService';
import { WeekCardList } from './week-card-list';
import dayjs from 'dayjs';

type Props = {
  site: Site;
  searchMonth: dayjs.Dayjs;
};

export const ViewAvailabilityPage = async ({ site, searchMonth }: Props) => {
  const nextMonth = searchMonth.startOf('month').add(1, 'month');
  const previousMonth = searchMonth.startOf('month').subtract(1, 'month');

  const next = {
    title: nextMonth.format('MMMM YYYY'),
    href: `view-availability?date=${nextMonth.format('YYYY-MM-DD')}`,
  };
  const previous = {
    title: previousMonth.format('MMMM YYYY'),
    href: `view-availability?date=${previousMonth.format('YYYY-MM-DD')}`,
  };

  const weeks = getWeeksOfTheMonth(searchMonth);

  return (
    <>
      <Pagination previous={previous} next={next} />
      {/* TODO does the suspense key have to be UTC?? */}
      <Suspense
        key={searchMonth.format('YYYY-MM-DDTHH:mm:ss')}
        fallback={<Spinner />}
      >
        <WeekCardList site={site} ukWeeks={weeks} />
      </Suspense>
    </>
  );
};
