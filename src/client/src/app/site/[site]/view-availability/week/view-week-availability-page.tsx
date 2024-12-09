import { Card, Pagination, Table } from '@components/nhsuk-frontend';
import { DayAvailabilityDetails } from '@types';
import dayjs from 'dayjs';

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

  let previousWeekDisplay = '';
  let nextWeekDisplay = '';

  if (previousWeek.month() < weekStart.month()) {
    previousWeekDisplay = `${previousWeek.format('D MMMM')}-${weekEnd.add(-1, 'week').format('D MMMM YYYY')}`;
  } else {
    previousWeekDisplay = `${weekStart.add(-1, 'week').format('D')}-${weekEnd.add(-1, 'week').format('D MMMM YYYY')}`;
  }

  if (nextWeek.month() > weekStart.month()) {
    nextWeekDisplay = `${nextWeek.format('D MMMM')}-${weekEnd.add(-1, 'week').format('D MMMM YYYY')}`;
  } else {
    nextWeekDisplay = `${weekStart.add(1, 'week').format('D')}-${weekEnd.add(1, 'week').format('D MMMM YYYY')}`;
  }

  const next = {
    title: nextWeekDisplay,
    href: `week?date=${nextWeek.format('YYYY-MM-DD')}`,
  };

  const previous = {
    title: previousWeekDisplay,
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
              {/* TODO: Add link for add session */}
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
            </>
          ) : (
            <span>No availability</span>
          )}
        </Card>
      ))}
    </>
  );
};
