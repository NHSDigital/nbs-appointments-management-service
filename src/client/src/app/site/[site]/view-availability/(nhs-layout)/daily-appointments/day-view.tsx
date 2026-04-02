import { DayJsType, RFC3339Format } from '@services/timeService';
import { Booking, ClinicalService, Site } from '@types';
import { Pagination } from 'nhsuk-react-components';
import { DailyAppointmentsPage } from './daily-appointments-page';
import { Tab, Tabs } from '@nhsuk-frontend-components';

type DayViewProps = {
  fromDate: DayJsType;
  bookings: Booking[];
  canCancelBookings: boolean;
  clinicalServices: ClinicalService[];
  site: Site;
};

export const DayView = ({
  fromDate,
  bookings,
  canCancelBookings,
  clinicalServices,
  site,
}: DayViewProps) => {
  const previousDay = fromDate.add(-1, 'day');
  const nextDay = fromDate.add(1, 'day');

  const previous = {
    title: previousDay.format('dddd D MMMM'),
    href: `daily-appointments?date=${previousDay.format(RFC3339Format)}&page=1`,
  };
  const next = {
    title: nextDay.format('dddd D MMMM'),
    href: `daily-appointments?date=${nextDay.format(RFC3339Format)}&page=1`,
  };

  const bookedAppointments = bookings.filter(b => b.status === 'Booked');
  const cancelledAppointments = bookings.filter(b => b.status === 'Cancelled');

  return (
    <>
      <Pagination className="no-print">
        <Pagination.Item
          previous
          labelText={previous.title}
          href={previous.href}
        >
          Previous
        </Pagination.Item>
        <Pagination.Item next labelText={next.title} href={next.href}>
          Next
        </Pagination.Item>
      </Pagination>

      <Tabs paramsToSetOnTabChange={[{ key: 'page', value: '1' }]}>
        <Tab title="Scheduled">
          <div className="print-out-data" aria-hidden="true">
            <h3>Scheduled Appointments</h3>
          </div>
          <DailyAppointmentsPage
            bookings={bookedAppointments}
            site={site.id}
            displayAction={canCancelBookings}
            clinicalServices={clinicalServices}
          />
        </Tab>
        <Tab title="Cancelled">
          <div className="print-out-data" aria-hidden="true">
            <h3>Cancelled Appointments</h3>
          </div>
          <DailyAppointmentsPage
            bookings={cancelledAppointments}
            site={site.id}
            displayAction={false}
            clinicalServices={clinicalServices}
          />
        </Tab>
      </Tabs>
    </>
  );
};
