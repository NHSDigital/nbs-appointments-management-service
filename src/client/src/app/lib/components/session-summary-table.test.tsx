import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import { screen, within } from '@testing-library/react';
import { SessionSummaryTable } from './session-summary-table';
import render from '@testing/render';

jest.mock('@types', () => ({
  ...jest.requireActual('@types'),
  clinicalServices: [
    { label: 'RSV (Adult)', value: 'RSV:Adult' },
    { label: 'FLU 18-64', value: 'FLU:18_64' },
    { label: 'COVID 75+', value: 'COVID:75+' },
  ],
}));

describe('Session summary table', () => {
  it('renders', () => {
    render(
      <SessionSummaryTable
        sessionSummaries={mockWeekAvailability__Summary[0].sessions}
      />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();
  });

  it('renders expected headers and rows', () => {
    render(
      <SessionSummaryTable
        sessionSummaries={mockWeekAvailability__Summary[0].sessions}
      />,
    );

    expect(
      screen.getByRole('row', { name: 'Time Services Booked Unbooked' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV (Adult) 2 booked 70 unbooked',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '13:00 - 17:30 RSV (Adult) 0 booked 54 unbooked',
      }),
    ).toBeInTheDocument();
  });

  it('renders action column when showChangeSessionLink is provided', () => {
    render(
      <SessionSummaryTable
        sessionSummaries={mockWeekAvailability__Summary[0].sessions}
        showChangeSessionLink={{
          siteId: 'TEST01',
          date: mockWeekAvailability__Summary[0].date,
        }}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: 'Time Services Booked Unbooked Action',
      }),
    ).toBeInTheDocument();

    expect(
      within(
        screen.getByRole('row', {
          name: '09:00 - 12:00 RSV (Adult) 2 booked 70 unbooked Change',
        }),
      ).getByRole('link', { name: 'Change' }),
    ).toBeInTheDocument();

    const changeLink = within(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV (Adult) 2 booked 70 unbooked Change',
      }),
    ).getByRole('link', { name: 'Change' });

    const href = changeLink.getAttribute('href');
    expect(
      href?.startsWith(
        '/site/TEST01/view-availability/week/edit-session?date=2024-06-10&session=',
      ),
    ).toBe(true);
  });
});
