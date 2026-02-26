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
  const totalAppointments = bookedAppointments + orphanedAppointments;

  const className =
    orphanedAppointments > 0
      ? 'appointments-summary'
      : 'appointments-summary card-item-margin';

  return (
    <>
      <div className={className}>
        <span>
          <strong>Total appointments: {maximumCapacity}</strong>
        </span>
        <span>Booked: {totalAppointments}</span>
        <span>Unbooked: {remainingCapacity}</span>
      </div>
      {orphanedAppointments > 0 && (
        <div className="card-item-margin" style={{ marginTop: 8 }}>
          <OrphanedAppointmentsMessage
            orphanedAppointments={orphanedAppointments}
          />
        </div>
      )}
    </>
  );
};

const OrphanedAppointmentsMessage = ({
  orphanedAppointments,
}: {
  orphanedAppointments: number;
}) => {
  if (orphanedAppointments === 0) {
    return null;
  }

  return (
    <div className="nhsuk-hint">
      {orphanedAppointments === 1 &&
        '1 booking was kept when availability was changed or cancelled.'}
      {orphanedAppointments > 1 &&
        orphanedAppointments +
          ' bookings were kept when availability was changed or cancelled.'}
    </div>
  );
};
