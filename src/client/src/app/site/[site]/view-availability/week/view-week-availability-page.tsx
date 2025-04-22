import { Pagination, Spinner } from '@components/nhsuk-frontend';
import dayjs from 'dayjs';
import { Site } from '@types';
import { Suspense } from 'react';
import { DayCardList } from './day-card-list';

type Props = {
  ukWeekStart: dayjs.Dayjs;
  ukWeekEnd: dayjs.Dayjs;
  site: Site;
};

export const ViewWeekAvailabilityPage = async ({
  ukWeekStart,
  ukWeekEnd,
  site,
}: Props) => {
  const nextUkWeek = ukWeekStart.add(1, 'week');
  const previousUkWeek = ukWeekStart.add(-1, 'week');

  // Example: 2-8 December
  const getPaginationTextSameMonth = (
    firstDate: dayjs.Dayjs,
    secondDate: dayjs.Dayjs,
  ): string => {
    return `${firstDate.format('D')}-${secondDate.format('D MMMM YYYY')}`;
  };

  // Example: 25 November-1 December
  const getPaginationTextDifferentMonth = (
    firstDate: dayjs.Dayjs,
    secondDate: dayjs.Dayjs,
  ): string => {
    return `${firstDate.format('D MMMM')}-${secondDate.format('D MMMM YYYY')}`;
  };

  const next = {
    title:
      nextUkWeek.month() > ukWeekEnd.add(1, 'week').month()
        ? getPaginationTextDifferentMonth(nextUkWeek, ukWeekEnd.add(1, 'week'))
        : getPaginationTextSameMonth(nextUkWeek, ukWeekEnd.add(1, 'week')),
    href: `week?date=${nextUkWeek.format('YYYY-MM-DD')}`,
  };

  const previous = {
    title:
      previousUkWeek.month() < ukWeekStart.month()
        ? getPaginationTextDifferentMonth(
            previousUkWeek,
            ukWeekEnd.add(-1, 'week'),
          )
        : getPaginationTextSameMonth(previousUkWeek, ukWeekEnd.add(-1, 'week')),
    href: `week?date=${previousUkWeek.format('YYYY-MM-DD')}`,
  };

  return (
    <>
      <Pagination previous={previous} next={next} />
      <Suspense
        key={ukWeekStart.format('YYYY-MM-DDTHH:mm:ssZZ')}
        fallback={<Spinner />}
      >
        <DayCardList
          site={site}
          ukWeekStart={ukWeekStart}
          ukWeekEnd={ukWeekEnd}
        />
      </Suspense>
    </>
  );
};
