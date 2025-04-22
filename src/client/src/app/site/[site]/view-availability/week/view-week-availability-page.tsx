import { Pagination, Spinner } from '@components/nhsuk-frontend';
import { Site } from '@types';
import { Suspense } from 'react';
import { DayCardList } from './day-card-list';
import {
  addToUkDate,
  DayJsType,
  dateStringFormat,
} from '@services/timeService';

type Props = {
  ukWeekStart: DayJsType;
  ukWeekEnd: DayJsType;
  site: Site;
};

export const ViewWeekAvailabilityPage = async ({
  ukWeekStart,
  ukWeekEnd,
  site,
}: Props) => {
  const nextUkWeek = addToUkDate(ukWeekStart, 1, 'week');
  const previousUkWeek = addToUkDate(ukWeekStart, -1, 'week');

  // Example: 2-8 December
  const getPaginationTextSameMonth = (
    firstDate: DayJsType,
    secondDate: DayJsType,
  ): string => {
    return `${firstDate.format('D')}-${secondDate.format('D MMMM YYYY')}`;
  };

  // Example: 25 November-1 December
  const getPaginationTextDifferentMonth = (
    firstDate: DayJsType,
    secondDate: DayJsType,
  ): string => {
    return `${firstDate.format('D MMMM')}-${secondDate.format('D MMMM YYYY')}`;
  };

  const oneWeekAfterWeekEnd = addToUkDate(ukWeekEnd, 1, 'week');
  const oneWeekBeforeWeekEnd = addToUkDate(ukWeekEnd, -1, 'week');

  const next = {
    title:
      nextUkWeek.month() > oneWeekAfterWeekEnd.month()
        ? getPaginationTextDifferentMonth(nextUkWeek, oneWeekAfterWeekEnd)
        : getPaginationTextSameMonth(nextUkWeek, oneWeekAfterWeekEnd),
    href: `week?date=${nextUkWeek.format(dateStringFormat)}`,
  };

  const previous = {
    title:
      previousUkWeek.month() < ukWeekStart.month()
        ? getPaginationTextDifferentMonth(previousUkWeek, oneWeekBeforeWeekEnd)
        : getPaginationTextSameMonth(previousUkWeek, oneWeekBeforeWeekEnd),
    href: `week?date=${previousUkWeek.format(dateStringFormat)}`,
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
