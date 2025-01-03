import { Card, Pagination, Table } from '@components/nhsuk-frontend';
import { DayAvailabilityDetails } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';
import { isInTheFuture } from '@services/timeService';

type Props = {
  days: DayAvailabilityDetails[];
  weekStart: dayjs.Dayjs;
  weekEnd: dayjs.Dayjs;
};

export const ViewWeekAvailabilityPage = ({
  days,
  weekStart,
  weekEnd,
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

  return (
    <>
      <Pagination previous={previous} next={next} />
      {days.map((d, i) => (
        <Card title={d.date} key={i}>
          {d.serviceInformation ? (
            <>
              <Table
                headers={['Time', 'Services', 'Booked', 'Unbooked']}
                rows={
                  d.serviceInformation?.map(session => {
                    return [
                      `${session.time}`,
                      session.serviceDetails.map((sd, k) => {
                        return (
                          <span key={k}>
                            {sd.service}
                            <br />
                          </span>
                        );
                      }),
                      session.serviceDetails.map((sd, j) => {
                        return (
                          <span key={j}>
                            {sd.booked} booked
                            <br />
                          </span>
                        );
                      }),
                      `${session.unbooked ?? 0} unbooked`,
                    ];
                  }) ?? []
                }
              ></Table>
              <br />
              {isInTheFuture(d.fullDate) && (
                <Link
                  className="nhsuk-link"
                  href={`/site/ABC02/create-availability/wizard?date=${d.fullDate}`}
                >
                  Add Session
                </Link>
              )}
              <Table
                headers={[
                  `Total appointments: ${d.totalAppointments}`,
                  `Booked: ${d.booked}`,
                  `Unbooked: ${d.unbooked}`,
                ]}
                rows={[]}
              ></Table>
              <br />
              {/* TODO: Add link to view daily appointments */}
              <Link
                className="nhsuk-link"
                href={`daily-appointments?date=${d.fullDate}&page=1`}
              >
                View daily appointments
              </Link>
            </>
          ) : (
            <>
              <span>No availability</span>
              {/* TODO: Add link to add session */}
            </>
          )}
        </Card>
      ))}
    </>
  );
};
