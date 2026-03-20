import render from '@testing/render';
import { screen } from '@testing-library/react';
import EditServicesConfirmed from './edit-services-confirmed';
import { mockSingleService, mockSite } from '@testing/data';

describe('Cancellation Confirmed Page', () => {
  it('renders the correct session in the table', () => {
    render(
      <EditServicesConfirmed
        removedServicesSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        date="2025-01-15"
        clinicalServices={mockSingleService}
        site={mockSite}
        chosenAction="cancel-appointments"
        hasBookings={true}
        servicesCount={1}
        newlyUnsupportedBookingsCount={2}
        cancelledWithDetailsCount={0}
        cancelledWithoutDetailsCount={0}
      />,
    );

    expect(
      screen.getByRole('row', { name: '09:00 - 12:00 RSV Adult' }),
    ).toBeInTheDocument();
  });
});
