import { render, screen } from '@testing-library/react';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import {
  mockDaySummaries,
  mockEmptyDays,
  mockWeekAvailabilityEnd,
  mockWeekAvailabilityStart,
} from '@testing/data';
import dayjs from 'dayjs';
import { now } from '@services/timeService';

jest.mock('@services/timeService', () => ({
  ...jest.requireActual('@services/timeService'),
  now: jest.fn(),
}));

const mockNow = now as jest.Mock<dayjs.Dayjs>;

jest.mock('@types', () => ({
  ...jest.requireActual('@types'),
  clinicalServices: [
    { label: 'RSV (Adult)', value: 'RSV:Adult' },
    { label: 'FLU 18-64', value: 'FLU:18_64' },
    { label: 'COVID 75+', value: 'COVID:75+' },
  ],
}));

describe('View Week Availability Page', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockNow.mockReturnValue(dayjs('2023-06-10 08:34:00'));
  });

  it('renders', () => {
    render(
      <ViewWeekAvailabilityPage
        days={mockDaySummaries}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
        site={'mock-site'}
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
        site={'mock-site'}
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
        days={mockDaySummaries}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
        site={'mock-site'}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: '09:00 - 17:00 RSV (Adult) 5 booked 118 unbooked Change',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', {
        name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
      }),
    ).toBeInTheDocument();

    const links = screen.getAllByRole('link', {
      name: /View daily appointments/i,
    });
    expect(links.length).toBe(3);
    expect(links[0].getAttribute('href')).toBe(
      'daily-appointments?date=2024-12-02&page=1',
    );
    expect(links[1].getAttribute('href')).toBe(
      'daily-appointments?date=2024-12-04&page=1',
    );
    expect(links[2].getAttribute('href')).toBe(
      'daily-appointments?date=2024-12-05&page=1',
    );
  });

  it('renders pagination options with the correct values', () => {
    render(
      <ViewWeekAvailabilityPage
        days={mockDaySummaries}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
        site={'mock-site'}
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

  it('Add Session only available for future date', () => {
    const daySummaries = mockDaySummaries;
    daySummaries[0].date = dayjs();
    daySummaries[1].date = dayjs().add(1, 'day');
    daySummaries[2].date = dayjs().add(2, 'day');

    render(
      <ViewWeekAvailabilityPage
        days={daySummaries}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
        site={'mock-site'}
      />,
    );

    const links = screen.getAllByRole('link', {
      name: /Add Session/i,
    });
    expect(links.length).toBe(2);
  });

  it('Allows future sessions to be changed', () => {
    const daySummaries = mockDaySummaries;
    daySummaries[0].date = dayjs();
    daySummaries[1].date = dayjs().add(1, 'day');
    daySummaries[2].date = dayjs().add(2, 'day');

    render(
      <ViewWeekAvailabilityPage
        days={mockDaySummaries}
        weekStart={mockWeekAvailabilityStart}
        weekEnd={mockWeekAvailabilityEnd}
        site={'mock-site'}
      />,
    );
  });

  it('Does not allow past sessions to be changed', () => {});
});
