import { DaySummary, WeekSummary } from '@types';

type AppointmentCountsSummaryProps = {
  period: DaySummary | WeekSummary;
};

export const AppointmentCountsSummary = ({
  period,
  period: {
    maximumCapacity,
    bookedAppointments,
    orphanedAppointments,
    remainingCapacity,
    cancelledAppointments,
  },
}: AppointmentCountsSummaryProps) => {
  const periodLength = 'daySummaries' in period ? 'week' : 'day';

  return (
    <>
      <div style={{ marginTop: 10, marginBottom: 10 }}>
        <OrphanedAppointmentsMessage
          orphanedAppointments={orphanedAppointments}
          periodLength={periodLength}
        />
        <CancelledAppointmentsMessage
          cancelledAppointments={cancelledAppointments}
          periodLength={periodLength}
        />
      </div>
      <div className="appointments-summary">
        <span>
          <strong>Total appointments: {maximumCapacity}</strong>
        </span>
        <span>Booked: {bookedAppointments + orphanedAppointments}</span>
        <span>Unbooked: {remainingCapacity}</span>
      </div>
    </>
  );
};

const OrphanedAppointmentsMessage = ({
  orphanedAppointments,
  periodLength,
}: {
  orphanedAppointments: number;
  periodLength: 'week' | 'day';
}) => {
  switch (true) {
    case periodLength === 'week' && orphanedAppointments === 0:
      return null;
    case periodLength === 'week' && orphanedAppointments === 1:
      return (
        <div>
          There is <strong>1</strong> manual cancellation in this week.
        </div>
      );
    case periodLength === 'week' && orphanedAppointments > 1:
      return (
        <div>
          There are <strong>{orphanedAppointments}</strong> manual cancellations
          in this week.
        </div>
      );
    case periodLength === 'day' && orphanedAppointments === 0:
      return null;
    case periodLength === 'day' && orphanedAppointments === 1:
      return (
        <div>
          There is <strong>1</strong> manual cancellation on this day.
        </div>
      );
    case periodLength === 'day' && orphanedAppointments > 1:
      return (
        <div>
          There are <strong>{orphanedAppointments}</strong> manual cancellations
          on this day.
        </div>
      );
    default:
      return null;
  }
};

const CancelledAppointmentsMessage = ({
  cancelledAppointments,
  periodLength,
}: {
  cancelledAppointments: number;
  periodLength: 'week' | 'day';
}) => {
  switch (true) {
    case periodLength === 'week' && cancelledAppointments === 0:
      return null;
    case periodLength === 'week' && cancelledAppointments === 1:
      return (
        <div>
          There is <strong>1</strong> cancelled appointment in this week.
        </div>
      );
    case periodLength === 'week' && cancelledAppointments > 1:
      return (
        <div>
          There are <strong>{cancelledAppointments}</strong> cancelled
          appointments in this week.
        </div>
      );
    case periodLength === 'day' && cancelledAppointments === 0:
      return null;
    case periodLength === 'day' && cancelledAppointments === 1:
      return (
        <div>
          There is <strong>1</strong> cancelled appointment on this day.
        </div>
      );
    case periodLength === 'day' && cancelledAppointments > 1:
      return (
        <div>
          There are <strong>{cancelledAppointments}</strong> cancelled
          appointments on this day.
        </div>
      );
    default:
      return null;
  }
};
