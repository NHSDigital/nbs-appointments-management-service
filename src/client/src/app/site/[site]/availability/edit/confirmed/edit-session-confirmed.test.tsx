import render from '@testing/render';
import { screen } from '@testing-library/react';
import EditSessionConfirmed from './edit-session-confirmed';
import { mockSingleService, mockSite } from '@testing/data';

describe('Cancellation Confirmed Page', () => {
  it('renders the correct session in the table', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        date="2025-01-15"
        clinicalServices={mockSingleService}
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('row', { name: '09:00 - 12:00 RSV Adult' }),
    ).toBeInTheDocument();
  });

  it('renders a link back to the cancel appointments page', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toHaveAttribute(
      'href',
      '/site/34e990af-5dc9-43a6-8895-b9123216d699/view-availability/daily-appointments?date=2025-01-15&page=1&tab=2',
    );
  });
});
