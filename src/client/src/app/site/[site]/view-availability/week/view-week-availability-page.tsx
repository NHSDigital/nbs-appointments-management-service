import { Card, Pagination, Table } from '@components/nhsuk-frontend';
import { DaySummary } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';
import { isInTheFuture } from '@services/timeService';

type Props = {
  days: DaySummary[];
  weekStart: dayjs.Dayjs;
  weekEnd: dayjs.Dayjs;
  site: string;
};

export const ViewWeekAvailabilityPage = ({
  days,
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

  return (
    <>
      <Pagination previous={previous} next={next} />
      {days.map((d, i) => (
        <Card title={d.date.format('dddd D MMMM')} key={i}>
          {d.sessions.length > 0 ? (
            <>
              <Table
                headers={['Time', 'Services', 'Booked', 'Unbooked']}
                rows={d.sessions.map(session => {
                  return [
                    `${session.start.format('HH:mm')} - ${session.end.format('HH:mm')}`,
                    Object.keys(session.bookings).map((service, k) => {
                      return (
                        <span key={k}>
                          {service}
                          <br />
                        </span>
                      );
                    }),
                    Object.keys(session.bookings).map((service, j) => {
                      return (
                        <span key={j}>
                          {session.bookings[service].length} booked
                          <br />
                        </span>
                      );
                    }),
                    `${session.maximumCapacity - session.totalBookings} unbooked`,
                  ];
                })}
              ></Table>
              <br />
              {isInTheFuture(d.date.format('YYYY-MM-DD')) && (
                <Link
                  className="nhsuk-link"
                  href={`/site/${site}/create-availability/wizard?date=${d.date.format('YYYY-MM-DD')}`}
                >
                  Add Session
                </Link>
              )}
              <Table
                headers={[
                  `Total appointments: ${d.maximumCapacity}`,
                  `Booked: ${d.bookedAppointments}`,
                  `Unbooked: ${d.remainingCapacity}`,
                ]}
                rows={[]}
              />
              <br />
              <Link
                className="nhsuk-link"
                href={`daily-appointments?date=${d.date.format('YYYY-MM-DD')}&page=1`}
              >
                View daily appointments
              </Link>
            </>
          ) : (
            <>
              <div style={{ marginBottom: '20px' }}>No availability</div>
              {isInTheFuture(d.date.format('YYYY-MM-DD')) && (
                <Link
                  className="nhsuk-link"
                  href={`/site/${site}/create-availability/wizard?date=${d.date.format('YYYY-MM-DD')}`}
                >
                  Add Session
                </Link>
              )}
            </>
          )}
        </Card>
      ))}
    </>
  );
};
