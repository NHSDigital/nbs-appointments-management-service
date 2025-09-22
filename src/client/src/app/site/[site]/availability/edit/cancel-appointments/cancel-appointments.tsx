'use client';

import { useState } from 'react';
import { ClinicalService, DaySummaryV2 } from '@types';
type Props = {
  site: string;
  date: string;
  daySummary: DaySummaryV2;
  affectedCount: number;
  clinicalServices: ClinicalService[];
};
import {
  compareTimes,
  dateTimeFormat,
  parseToUkDatetime,
  parseToTimeComponents,
  toTimeFormat,
} from '@services/timeService';

export default function CancelAppointmentsPage({
  site,
  date,
  daySummary,
  affectedCount,
  clinicalServices,
}: Props) {
  const [confirmed, setConfirmed] = useState(false);

  const handleCancel = async () => {
    // Call your bulk cancel API here
    // await cancelBookings(site, date);
    setConfirmed(true);
  };

  return (
    <section className="max-w-xl mx-auto bg-white shadow-md rounded-lg p-6">
      <h1 className="text-xl font-semibold mb-4">Manage your appointments</h1>

      <div className="mb-4">
        <h2 className="text-lg font-medium">{site}</h2>
        <p className="text-sm text-gray-600">
          New time and capacity for {date}
        </p>
        <ul className="mt-2 text-sm text-gray-700">
          <li>
            <strong>Services:</strong>{' '}
            {clinicalServices.map(s => s.value).join(', ')}
          </li>
          <li>
            <strong>Booked:</strong> {daySummary.totalOrphanedAppointments}{' '}
            booked
          </li>
        </ul>
      </div>

      <div className="bg-red-50 border border-red-200 p-4 rounded mb-4">
        <p className="text-red-700 font-medium">
          Changing the time and capacity will affect{' '}
          {daySummary.totalOrphanedAppointments} bookings.
        </p>
        <p className="text-sm text-red-600 mt-1">
          People will be sent a text message or email confirming their
          appointment has been cancelled.
        </p>
      </div>

      {!confirmed ? (
        <div className="flex gap-4">
          <button
            className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
            onClick={handleCancel}
          >
            Cancel appointments
          </button>
          <button
            className="bg-gray-200 text-gray-800 px-4 py-2 rounded hover:bg-gray-300"
            onClick={() => window.history.back()}
          >
            No, go back
          </button>
        </div>
      ) : (
        <p className="text-green-700 font-medium mt-4">
          Appointments cancelled. Notifications will be sent.
        </p>
      )}
    </section>
  );
}
