import { render, screen } from '@testing-library/react';
import { DailyAppointmentsPage } from './daily-appointments-page';
import { mockBookings, mockMultipleServices } from '@testing/data';
import { SearchParamsContext } from 'next/dist/shared/lib/hooks-client-context.shared-runtime';
import { ReadonlyURLSearchParams } from 'next/navigation';

describe('View Daily Appointments', () => {
  it('renders appointments', () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('date=2024-12-24&page=1')}
      >
        <DailyAppointmentsPage
          bookings={mockBookings}
          site="TEST01"
          displayAction={true}
          clinicalServices={mockMultipleServices}
        />
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.getByRole('row', {
        name: 'Time Name and NHS number Date of birth Contact details Services Action',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '14:05 John Smith 9999999990 1 February 1979 RSV Adult Cancel',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '09:34 Ian Goldsmith 9999999995 3 March 1973 FLU 18-64 Cancel',
      }),
    ).toBeInTheDocument();

    expect(screen.getAllByRole('row').length).toBe(6);
  });

  it('displays a message above the table if one is supplied', async () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('date=2024-12-24&page=1')}
      >
        <DailyAppointmentsPage
          bookings={mockBookings}
          site="TEST01"
          displayAction={true}
          message="Test message"
          clinicalServices={mockMultipleServices}
        />
      </SearchParamsContext.Provider>,
    );

    expect(screen.getByText('Test message')).toBeInTheDocument();
  });

  it('renders the action column', () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('date=2024-12-24&page=1')}
      >
        <DailyAppointmentsPage
          bookings={mockBookings}
          site="TEST01"
          displayAction={true}
          message="Test message"
          clinicalServices={mockMultipleServices}
        />
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.getByRole('columnheader', { name: 'Action' }),
    ).toBeInTheDocument();
  });

  it('does not render the action column', () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('date=2024-12-24&page=1')}
      >
        <DailyAppointmentsPage
          bookings={mockBookings}
          site="TEST01"
          displayAction={false}
          message="Test message"
          clinicalServices={mockMultipleServices}
        />
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.queryByRole('columnheader', { name: 'Action' }),
    ).not.toBeInTheDocument();
  });
});
