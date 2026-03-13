'use client';
import { Pagination } from '@nhsuk-frontend-components';
import { Booking, ClinicalService } from '@types';
import { useSearchParams } from 'next/navigation';
import { SessionBookingsContactDetailsTableData } from './session-bookings-contact-details-table-data';

const ROWS_PER_PAGE = 50;

type Props = {
  bookings: Booking[];
  site: string;
  displayAction: boolean;
  message?: string;
  clinicalServices: ClinicalService[];
};

export const SessionBookingsContactDetailsPage = ({
  bookings,
  site,
  displayAction,
  message,
  clinicalServices,
}: Props) => {
  const searchParams = useSearchParams();
  const params = new URLSearchParams(searchParams.toString());
  const page = Number(params.get('page')) || 1;

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
      <SessionBookingsContactDetailsTableData
        bookings={getPagedBookings(bookings)}
        clinicalServices={clinicalServices}
        displayAction={displayAction}
        site={site}
      />

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

SessionBookingsContactDetailsPage.displayName =
  'SessionBookingsContactDetailsPage';
