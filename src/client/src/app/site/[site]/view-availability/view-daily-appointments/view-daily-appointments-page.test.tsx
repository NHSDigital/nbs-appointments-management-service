import { render, screen } from '@testing-library/react';
import { ViewDailyAppointmentsPage } from './view-daily-appointments-page';
import { mockBookings } from '@testing/data';

describe('View Daily Appointments', () => {
  it('renders appointments', () => {
    render(
      <ViewDailyAppointmentsPage
        bookings={mockBookings}
        page={1}
        date={'2024-12-24'}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: 'Time Name and NHS number Date of birth Contact details Services Action',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '14:05 John Smith 9999999990 1 February 1979 RSV Cancel',
      }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole('row').length).toBe(5);
  });
});
