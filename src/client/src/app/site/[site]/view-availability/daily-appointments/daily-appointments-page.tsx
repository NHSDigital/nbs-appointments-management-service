'use client';
import { Table, Pagination } from '@nhsuk-frontend-components';
import { AttendeeDetails, ContactItem, Booking } from '@types';
import { formatDateTimeToTime, dateToString } from '@services/timeService';
import { ReactNode } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';

const ROWS_PER_PAGE = 50;

type Props = {
  bookings: Booking[];
  site: string;
  displayAction: boolean;
};

export const DailyAppointmentsPage = ({
  bookings,
  site,
  displayAction,
}: Props) => {
  const searchParams = useSearchParams();
  const params = new URLSearchParams(searchParams.toString());

  const page = Number(params.get('page')) ?? 1;

  const hasNextPage = (): boolean => {
    return page * ROWS_PER_PAGE < bookings.length;
  };

  const getPagedBookings = (b: Booking[]): Booking[] => {
    const startIndex = page == 1 ? 0 : (page - 1) * ROWS_PER_PAGE;
    const endIndex = startIndex + ROWS_PER_PAGE;

    return b.slice(startIndex, endIndex);
  };

  const mapContactDetails = (contactDetails: ContactItem[]): ReactNode => {
    return contactDetails.map((details, key) => {
      return (
        <span key={key}>
          {details.value}
          <br />
        </span>
      );
    });
  };

  const mapNameAndNHSNumber = (attendeeDetails: AttendeeDetails): ReactNode => {
    return (
      <span>
        {attendeeDetails.firstName} {attendeeDetails.lastName}
        <br />
        {attendeeDetails.nhsNumber}
      </span>
    );
  };

  const mapTableData = () => {
    if (!bookings.length) {
      return undefined;
    }

    const headers = [
      'Time',
      'Name and NHS number',
      'Date of birth',
      'Contact details',
      'Services',
    ];

    if (displayAction) {
      headers.push('Action');
    }

    const rows = getPagedBookings(bookings).map(booking => {
      const row = [
        formatDateTimeToTime(booking.from),
        mapNameAndNHSNumber(booking.attendeeDetails),
        dateToString(booking.attendeeDetails.dateOfBirth),
        booking.contactDetails
          ? mapContactDetails(booking.contactDetails)
          : null,
        booking.service.split(':')[0],
      ];

      if (displayAction) {
        row.push(
          <Link
            key={`cancel-${booking.reference}`}
            href={`/site/${site}/appointment/${booking.reference}/cancel`}
          >
            Cancel
          </Link>,
        );
      }

      return row;
    });

    return { headers, rows };
  };

  const appointmentsTableData = mapTableData();

  const hasPreviousPage = page > 1;

  return (
    <>
      {appointmentsTableData && <Table {...appointmentsTableData}></Table>}

      <Pagination
        previous={
          hasPreviousPage
            ? {
                title: `Page ${page - 1}`,
                href: '',
                onClick: () => {
                  params.set('page', String(page - 1));
                  window.history.pushState(null, '', `?${params.toString()}`);
                },
              }
            : null
        }
        next={
          hasNextPage()
            ? {
                title: `Page ${page + 1}`,
                href: '',
                onClick: () => {
                  params.set('page', String(page + 1));
                  window.history.pushState(null, '', `?${params.toString()}`);
                },
              }
            : null
        }
      />
    </>
  );
};
