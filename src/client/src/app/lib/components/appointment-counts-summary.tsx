import { DaySummary, WeekSummary } from '@types';

type AppointmentCountsSummaryProps = {
  period: DaySummary | WeekSummary;
};

export const AppointmentCountsSummary = ({
  period: {
    maximumCapacity,
    bookedAppointments,
    orphanedAppointments,
    remainingCapacity,
  },
}: AppointmentCountsSummaryProps) => {
  return (
    <div className="appointments-summary">
      <span>
        <strong>Total appointments: {maximumCapacity}</strong>
      </span>
      <span>Booked: {bookedAppointments + orphanedAppointments}</span>
      <span>Unbooked: {remainingCapacity}</span>
    </div>
  );
};
