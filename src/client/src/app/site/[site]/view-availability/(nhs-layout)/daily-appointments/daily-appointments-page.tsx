'use client';
import { Booking, ClinicalService } from '@types';
import { toTimeFormat, jsDateFormat } from '@services/timeService';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { Pagination, Table } from 'nhsuk-react-components';

const ROWS_PER_PAGE = 50;

type Props = {
  bookings: Booking[];
  site: string;
  displayAction: boolean;
  message?: string;
  clinicalServices: ClinicalService[];
};

export const DailyAppointmentsPage = ({
  bookings,
  site,
  displayAction,
  message,
  clinicalServices,
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

  const hasPreviousPage = page > 1;

  return (
    <>
      {message && <p className="no-print">{message}</p>}

      <Table>
        <Table.Head>
          <Table.Row>
            <Table.Cell>Time</Table.Cell>
            <Table.Cell>Name and NHS number</Table.Cell>
            <Table.Cell>Date of birth</Table.Cell>
            <Table.Cell>Contact details</Table.Cell>
            <Table.Cell>Services</Table.Cell>
            {displayAction && (
              <Table.Cell className="no-print">Action</Table.Cell>
            )}
          </Table.Row>
        </Table.Head>
        <Table.Body>
          {getPagedBookings(bookings).map(booking => (
            <Table.Row key={booking.reference}>
              <Table.Cell>{toTimeFormat(booking.from)}</Table.Cell>
              <Table.Cell>
                {booking.attendeeDetails.firstName}{' '}
                {booking.attendeeDetails.lastName}
                <br />
                {booking.attendeeDetails.nhsNumber}
              </Table.Cell>
              <Table.Cell>
                {jsDateFormat(booking.attendeeDetails.dateOfBirth)}
              </Table.Cell>
              <Table.Cell>
                {booking.contactDetails && booking.contactDetails.length > 0
                  ? booking.contactDetails.map((details, key) => (
                      <span
                        key={key}
                        className={details.type === 'Email' ? 'no-print' : ''}
                      >
                        {details.value}
                        <br />
                      </span>
                    ))
                  : 'Not provided'}
              </Table.Cell>
              <Table.Cell>
                {clinicalServices.find(c => c.value === booking.service)
                  ?.label ?? booking.service}
              </Table.Cell>
              {displayAction && (
                <Table.Cell className="no-print">
                  <Link
                    href={`/site/${site}/appointment/${booking.reference}/cancel`}
                  >
                    Cancel
                  </Link>
                </Table.Cell>
              )}
            </Table.Row>
          ))}
        </Table.Body>
      </Table>

      <Pagination>
        {hasPreviousPage ? (
          <Pagination.Item
            previous
            onClick={() => {
              params.set('page', String(page - 1));
              window.history.pushState(null, '', `?${params.toString()}`);
            }}
            style={{ cursor: 'pointer' }}
            labelText={`Page ${page - 1}`}
          >
            Previous
          </Pagination.Item>
        ) : null}
        {hasNextPage() ? (
          <Pagination.Item
            next
            onClick={() => {
              params.set('page', String(page + 1));
              window.history.pushState(null, '', `?${params.toString()}`);
            }}
            style={{ cursor: 'pointer' }}
            labelText={`Page ${page + 1}`}
          >
            Next
          </Pagination.Item>
        ) : null}
      </Pagination>
    </>
  );
};
