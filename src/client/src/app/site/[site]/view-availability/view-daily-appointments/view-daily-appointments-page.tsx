import { Table, Pagination } from '@nhsuk-frontend-components';
import { PaginationLink } from '../../../../lib/components/nhsuk-frontend/pagination';
import { AttendeeDetails, ContactItem, Booking } from '@types';
import { formatDateTimeToTime, dateToString } from '@services/timeService';
import { ReactNode } from 'react';
import dayjs from 'dayjs';
import Link from 'next/link';

type Props = {
  bookings: Booking[];
  page: number;
  date: string;
  site: string;
};

export const ViewDailyAppointmentsPage = ({
  bookings,
  page,
  date,
  site,
}: Props) => {
  const rowsPerPage = 8;

  const hasNextPage = (): boolean => {
    return page * rowsPerPage < bookings.length;
  };
  const buildNextPage = (): PaginationLink | null => {
    if (!hasNextPage()) return null;

    return {
      title: `Page ${page + 1}`,
      href: `view-daily-appointments?date=${dayjs(date).format('YYYY-MM-DD')}&page=${page + 1}`,
    };
  };
  const buildPreviousPage = (): PaginationLink | null => {
    if (page === 1) return null;

    return {
      title: `Page ${page - 1}`,
      href: `view-daily-appointments?date=${dayjs(date).format('YYYY-MM-DD')}&page=${page - 1}`,
    };
  };
  const getPagedBookings = (b: Booking[]): Booking[] => {
    const startIndex = page == 1 ? 0 : (page - 1) * rowsPerPage;
    const endIndex = startIndex + rowsPerPage;

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
      'Action',
    ];

    const rows = getPagedBookings(bookings).map(booking => {
      return [
        formatDateTimeToTime(booking.from),
        mapNameAndNHSNumber(booking.attendeeDetails),
        dateToString(booking.attendeeDetails.dateOfBirth),
        booking.contactDetails
          ? mapContactDetails(booking.contactDetails)
          : null,
        booking.service.split(':')[0],
        <Link
          key={`cancel-${booking.reference}`}
          href={`/site/${site}/appointment/${booking.reference}/cancel`}
        >
          Cancel
        </Link>,
      ];
    });

    return { headers, rows };
  };
  const appointmentsTableData = mapTableData();

  return (
    <>
      {appointmentsTableData && <Table {...appointmentsTableData}></Table>}
      <Pagination previous={buildPreviousPage()} next={buildNextPage()} />
    </>
  );
};
