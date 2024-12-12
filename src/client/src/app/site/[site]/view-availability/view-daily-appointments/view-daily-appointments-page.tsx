import { Table, Pagination } from '@nhsuk-frontend-components';
import { PaginationLink } from '../../../../lib/components/nhsuk-frontend/pagination';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import {
  FetchBookingsRequest,
  AttendeeDetails,
  ContactItem,
  Booking,
} from '@types';
import { formatDateTimeToTime, dateToString } from '@services/timeService';
import { ReactNode } from 'react';

type Props = {
  page: number;
  site: string;
  date: string;
};

export const ViewDailyAppointmentsPage = async ({
  page,
  site,
  date,
}: Props) => {
  const rowsPerPage = 8;
  const fetchBookingsRequest: FetchBookingsRequest = {
    from: new Date('12/12/2024 00:00').toISOString(),
    to: new Date('12/12/2024 23:59').toISOString(),
    site: site,
  };
  const bookings = await fetchBookings(fetchBookingsRequest);

  const hasNextPage = (): boolean => {
    return page * rowsPerPage < bookings.length;
  };
  const buildNextPage = (): PaginationLink | null => {
    if (!hasNextPage()) return null;

    return {
      title: `Page ${Number(page) + 1}`,
      href: `view-daily-appointments?date=Thursday%2012%20December&page=${Number(page) + 1}`,
    };
  };
  const buildPreviousPage = (): PaginationLink | null => {
    if (Number(page) === 1) return null;

    return {
      title: `Page ${Number(page) - 1}`,
      href: `view-daily-appointments?date=Thursday%2012%20December&page=${Number(page) - 1}`,
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

  const mapTableData = async () => {
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
        'Cancel', //TODO: Convert to action/link
      ];
    });

    return { headers, rows };
  };

  const appointmentsTableData = await mapTableData();

  return (
    <>
      {appointmentsTableData && <Table {...appointmentsTableData}></Table>}
      <Pagination previous={buildPreviousPage()} next={buildNextPage()} />
    </>
  );
};
