import { Booking, ClinicalService } from '@types';
import { DailyAppointmentsPage } from '../../view-availability/daily-appointments/daily-appointments-page';

type Props = {
  bookings: Booking[];
  clinicalServices: ClinicalService[];
  site: string;
};

const CancelledAppointments = ({ bookings, clinicalServices, site }: Props) => {
  return (
    <DailyAppointmentsPage
      bookings={bookings}
      clinicalServices={clinicalServices}
      displayAction={false}
      site={site}
    />
  );
};

export default CancelledAppointments;
