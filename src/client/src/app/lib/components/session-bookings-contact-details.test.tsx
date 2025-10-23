import React from 'react';
import { render, screen } from '@testing-library/react';
import { SessionBookingsContactDetailsPage } from './session-bookings-contact-details';
import { useSearchParams } from 'next/navigation';
import { mockBookings, mockMultipleServices } from '@testing/data';

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useSearchParams: jest.fn(),
}));

describe('SessionBookingsContactDetailsPage', () => {
  beforeEach(() => {
    (useSearchParams as jest.Mock).mockReturnValue({
      toString: () => 'page=1',
      get: (key: string) => (key === 'page' ? '1' : null),
    });
  });

  it('renders table headers and booking rows', () => {
    render(
      <SessionBookingsContactDetailsPage
        bookings={mockBookings}
        site="TEST01"
        displayAction={true}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getByText('Time')).toBeInTheDocument();
    expect(screen.getByText('Name and NHS number')).toBeInTheDocument();
    expect(screen.getByText('Date of birth')).toBeInTheDocument();
    expect(screen.getByText('Contact details')).toBeInTheDocument();
    expect(screen.getByText('Services')).toBeInTheDocument();
    expect(screen.getByText('Action')).toBeInTheDocument();

    expect(screen.getByText('John Smith')).toBeInTheDocument();
    expect(screen.getByText('RSV Adult')).toBeInTheDocument();
    expect(screen.getByText('FLU 18-64')).toBeInTheDocument();
    expect(screen.getAllByText('Cancel')).toHaveLength(mockBookings.length);
  });

  it('renders "Not provided" when contactDetails are missing', () => {
    const bookingsWithoutContact = mockBookings.map(b => ({
      ...b,
      contactDetails: [],
    }));

    render(
      <SessionBookingsContactDetailsPage
        bookings={bookingsWithoutContact}
        site="TEST01"
        displayAction={false}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getAllByText('Not provided')).toHaveLength(
      bookingsWithoutContact.length,
    );
  });

  it('renders message when provided', () => {
    render(
      <SessionBookingsContactDetailsPage
        bookings={mockBookings}
        site="TEST01"
        displayAction={false}
        message="Test message"
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getByText('Test message')).toBeInTheDocument();
  });

  it('does not render table when bookings are empty', () => {
    render(
      <SessionBookingsContactDetailsPage
        bookings={[]}
        site="TEST01"
        displayAction={false}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.queryByText('Time')).not.toBeInTheDocument();
  });

  it('renders pagination controls when bookings exceed page size', () => {
    const largeBookings = Array.from({ length: 55 }, (_, i) => ({
      ...mockBookings[0],
      reference: `ref-${i}`,
    }));

    render(
      <SessionBookingsContactDetailsPage
        bookings={largeBookings}
        site="TEST01"
        displayAction={true}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getByTitle('Page 2')).toBeInTheDocument();
  });
});
