import render from '@testing/render';
import { screen } from '@testing-library/react';
import { NoNotificationsPage } from './no-notifications-page';
import { mockBookings, mockMultipleServices } from '@testing/data';
import { useSearchParams } from 'next/navigation';

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useSearchParams: jest.fn(),
}));

beforeEach(() => {
  (useSearchParams as jest.Mock).mockReturnValue({
    get: (key: string) => (key === 'page' ? '1' : null),
    toString: () => 'page=1',
  });
});

describe('Create No Notifications Page', () => {
  it('renders', () => {
    render(
      <NoNotificationsPage
        bookings={mockBookings}
        site="TEST01"
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(
      screen.getByRole('columnheader', { name: 'Name and NHS number' }),
    ).toBeInTheDocument();
  });
});
