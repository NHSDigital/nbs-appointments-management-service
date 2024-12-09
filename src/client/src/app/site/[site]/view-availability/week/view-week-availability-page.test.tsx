import { render, screen } from '@testing-library/react';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import {
  mockDetailedDays,
  mockEmptyDays,
  mockWeekAvailabilityEnd,
  mockWeekAvailabilityStart,
} from '@testing/data';

describe('View Week Availability Page', () => {
  it('renders', () => {
    render(
      <ViewWeekAvailabilityPage
        days={mockDetailedDays}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Monday 2 December' }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole('table')).toHaveLength(6);
  });

  it('renders no availability', () => {
    render(
      <ViewWeekAvailabilityPage
        days={mockEmptyDays}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Monday 2 December' }),
    ).toBeInTheDocument();
    expect(screen.queryAllByRole('table')).toHaveLength(0);
  });

  it('renders correct information for a day', () => {
    render(
      <ViewWeekAvailabilityPage
        days={mockDetailedDays}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: '09:00 - 17:00 RSV (Adult) 5 booked 118 unbooked',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', {
        name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
      }),
    ).toBeInTheDocument();
  });

  it('renders pagination options with the correct values', () => {
    render(
      <ViewWeekAvailabilityPage
        days={mockDetailedDays}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Next : 9-15 December 2024' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Previous : 25 November-1 December 2024',
      }),
    ).toBeInTheDocument();
  });
});
