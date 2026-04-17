import { render, screen } from '@testing-library/react';
import { DayView } from './day-view';
import { mockDate, mockMultipleServices, mockSite } from '@testing/data';
import { mockBookings } from '@testing/availability-and-bookings-mock-data';
import { useSearchParams } from 'next/navigation';

jest.mock('next/navigation', () => ({
  useSearchParams: jest.fn(),
}));

describe('Day View Component', () => {
  beforeEach(() => {
    (useSearchParams as jest.Mock).mockReturnValue({
      toString: () => 'page=1',
      get: (key: string) => (key === 'page' ? '1' : null),
    });
  });

  it('renders the pagination options with the correct values', () => {
    (useSearchParams as jest.Mock).mockReturnValue({
      toString: () => 'page=1',
      get: (key: string) => (key === 'page' ? '1' : null),
    });

    render(
      <DayView
        fromDate={mockDate}
        bookings={mockBookings}
        canCancelBookings={true}
        clinicalServices={mockMultipleServices}
        site={mockSite}
      />,
    );

    const nextLink = screen.getByRole('link', {
      name: 'Next : Wednesday 1 April',
    });
    const previousLink = screen.getByRole('link', {
      name: 'Previous : Monday 30 March',
    });

    expect(nextLink).toBeInTheDocument();
    expect(nextLink).toHaveAttribute(
      'href',
      'daily-appointments?date=2026-04-01&page=1',
    );

    expect(previousLink).toBeInTheDocument();
    expect(previousLink).toHaveAttribute(
      'href',
      'daily-appointments?date=2026-03-30&page=1',
    );
  });

  it('renders the scheduled and cancelled tabs', () => {
    render(
      <DayView
        fromDate={mockDate}
        bookings={mockBookings}
        canCancelBookings={true}
        clinicalServices={mockMultipleServices}
        site={mockSite}
      />,
    );

    expect(screen.getByRole('link', { name: 'Scheduled' })).toBeInTheDocument();

    expect(screen.getByRole('link', { name: 'Cancelled' })).toBeInTheDocument();
  });

  it('does not render cancelled appointments in the scheduled tab', () => {
    (useSearchParams as jest.Mock).mockReturnValue({
      toString: () => 'page=1',
      get: (key: string) => (key === 'page' ? '1' : null),
    });

    render(
      <DayView
        fromDate={mockDate}
        bookings={mockBookings}
        canCancelBookings={true}
        clinicalServices={mockMultipleServices}
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('cell', { name: 'John Smith 9999999990' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('Zack Jeremiah 9999999994'),
    ).not.toBeInTheDocument();
  });

  it('does not render scheduled appointments in the cancelled tab', () => {
    (useSearchParams as jest.Mock).mockReturnValue({
      toString: () => 'page=1&tab=1',
      get: (key: string) => (key === 'page' ? '1' : null),
    });

    render(
      <DayView
        fromDate={mockDate}
        bookings={mockBookings}
        canCancelBookings={true}
        clinicalServices={mockMultipleServices}
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('cell', { name: 'Zack Jeremiah 9999999994' }),
    ).toBeInTheDocument();
    expect(screen.queryByText('John Smith 9999999990')).not.toBeInTheDocument();
  });
});
