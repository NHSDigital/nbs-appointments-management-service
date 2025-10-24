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

    const expectedHeaders = [
      'Time',
      'Name and NHS number',
      'Date of birth',
      'Contact details',
      'Services',
      'Action',
    ];

    expectedHeaders.forEach(header => {
      expect(
        screen.getByRole('columnheader', { name: header }),
      ).toBeInTheDocument();
    });
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
  it('renders clinical service label from clinicalServices', () => {
    render(
      <SessionBookingsContactDetailsPage
        bookings={mockBookings}
        site="TEST01"
        displayAction={false}
        clinicalServices={mockMultipleServices}
      />,
    );

    mockBookings.forEach(booking => {
      const label =
        mockMultipleServices.find(s => s.value === booking.service)?.label ??
        booking.service;
      const count = mockBookings.filter(
        b => b.service === booking.service,
      ).length;
      expect(screen.getAllByText(label)).toHaveLength(count);
    });
  });

  it('renders cancel link for each booking when displayAction is true', () => {
    render(
      <SessionBookingsContactDetailsPage
        bookings={mockBookings}
        site="TEST01"
        displayAction={true}
        clinicalServices={mockMultipleServices}
      />,
    );

    const cancelLinks = screen.getAllByRole('link', { name: 'Cancel' });

    expect(cancelLinks).toHaveLength(mockBookings.length);

    cancelLinks.forEach((link, index) => {
      expect(link).toHaveAttribute(
        'href',
        `/site/TEST01/appointment/${mockBookings[index].reference}/cancel`,
      );
    });
  });
});
