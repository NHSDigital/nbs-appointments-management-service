import { render, screen } from '@testing-library/react';
import CancelledAppointments from './cancelled-appointments';
import { mockBookings } from '@testing/availability-and-bookings-mock-data';
import { mockClinicalServices } from '@testing/data';
import { SearchParamsContext } from 'next/dist/shared/lib/hooks-client-context.shared-runtime';
import { ReadonlyURLSearchParams } from 'next/navigation';

describe('Cancelled Appointments Without Contact Details Page', () => {
  it('renders appointments', () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('date=2024-12-24&page=1')}
      >
        <CancelledAppointments
          bookings={mockBookings}
          clinicalServices={mockClinicalServices}
          site="TEST01"
        />
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.getByRole('row', {
        name: 'Time Name and NHS number Date of birth Contact details Services',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '09:00 John Smith 9999999990 1 February 1979 RSV Adult',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '09:10 Brian Smith 9999999995 1 February 1984 FLU:18_64',
      }),
    ).toBeInTheDocument();

    expect(screen.getAllByRole('row').length).toBe(7);
  });

  it('does not render the action column', () => {
    render(
      <SearchParamsContext.Provider
        value={new ReadonlyURLSearchParams('date=2024-12-24&page=1')}
      >
        <CancelledAppointments
          bookings={mockBookings}
          clinicalServices={mockClinicalServices}
          site="TEST01"
        />
      </SearchParamsContext.Provider>,
    );

    expect(
      screen.queryByRole('columnheader', { name: 'Action' }),
    ).not.toBeInTheDocument();
  });
});
