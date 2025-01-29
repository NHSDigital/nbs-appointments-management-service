import { Pagination } from '@components/nhsuk-frontend';
import dayjs from 'dayjs';
import { DaySummaryCard } from './day-summary-card';
import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import { fetchPermissions } from '@services/appointmentsService';

type Props = {
  weekStart: dayjs.Dayjs;
  weekEnd: dayjs.Dayjs;
  site: Site;
};

export const ViewWeekAvailabilityPage = async ({
  weekStart,
  weekEnd,
  site,
}: Props) => {
  const nextWeek = weekStart.add(1, 'week');
  const previousWeek = weekStart.add(-1, 'week');

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
      nextWeek.month() > weekEnd.add(1, 'week').month()
        ? getPaginationTextDifferentMonth(nextWeek, weekEnd.add(1, 'week'))
        : getPaginationTextSameMonth(nextWeek, weekEnd.add(1, 'week')),
    href: `week?date=${nextWeek.format('YYYY-MM-DD')}`,
  };

  const previous = {
    title:
      previousWeek.month() < weekStart.month()
        ? getPaginationTextDifferentMonth(previousWeek, weekEnd.add(-1, 'week'))
        : getPaginationTextSameMonth(previousWeek, weekEnd.add(-1, 'week')),
    href: `week?date=${previousWeek.format('YYYY-MM-DD')}`,
  };

  const daySummaries = await summariseWeek(weekStart, weekEnd, site.id);
  const canManageAvailability = (await fetchPermissions(site.id))?.includes(
    'availability:setup',
  );

  return (
    <>
      <Pagination previous={previous} next={next} />
      {daySummaries.map((day, dayIndex) => {
        return (
          <DaySummaryCard
            daySummary={day}
            siteId={site.id}
            key={`day-summary-${dayIndex}`}
            canManageAvailability={canManageAvailability}
          />
        );
      })}
    </>
  );
};
