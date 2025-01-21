import render from '@testing/render';
import { screen } from '@testing-library/react';
import { DaySummaryCard } from './day-summary-card';
import { mockDaySummaries, mockEmptyDays } from '@testing/data';
import { isInTheFuture } from '@services/timeService';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    isInTheFuture: jest.fn(),
  };
});

const mockIsInTheFuture = isInTheFuture as jest.Mock<boolean>;

describe('Day Summary Card', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockIsInTheFuture.mockReturnValue(true);
  });

  it('renders', () => {
    render(
      <DaySummaryCard daySummary={mockDaySummaries[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.getByRole('heading', { name: 'Monday 2 December' }),
    ).toBeInTheDocument();
  });

  it('renders availability table if there is availability to show', () => {
    render(
      <DaySummaryCard daySummary={mockDaySummaries[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.getByRole('row', { name: 'Time Services Booked Unbooked Action' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '09:00 - 17:00 RSV (Adult) 5 booked 118 unbooked',
      }),
    ).toBeInTheDocument();
  });

  it('renders no availability message if there is no availability to show', () => {
    render(
      <DaySummaryCard daySummary={mockEmptyDays[0]} siteId={'mock-site'} />,
    );

    expect(screen.getByText('No availability')).toBeInTheDocument();

    expect(
      screen.queryByRole('row', {
        name: 'Time Services Booked Unbooked Action',
      }),
    ).toBeNull();
  });

  it('renders add session link if the date is in the future', () => {
    mockIsInTheFuture.mockReturnValue(true);

    render(
      <DaySummaryCard daySummary={mockDaySummaries[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.getByRole('link', { name: 'Add Session' }),
    ).toBeInTheDocument();
  });

  it('does not render add session link if the date is in the past', () => {
    mockIsInTheFuture.mockReturnValue(false);

    render(
      <DaySummaryCard daySummary={mockDaySummaries[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.queryByRole('link', { name: 'Add Session' }),
    ).not.toBeInTheDocument();
  });

  it('renders total appointments table if there is availability to show', () => {
    render(
      <DaySummaryCard daySummary={mockDaySummaries[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.getByRole('row', {
        name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
      }),
    ).toBeInTheDocument();
  });

  it('does not render total appointments table if there is no availability to show', () => {
    render(
      <DaySummaryCard daySummary={mockEmptyDays[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.queryByRole('row', {
        name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
      }),
    ).toBeNull();
  });

  it('renders view appointments link', () => {
    render(
      <DaySummaryCard daySummary={mockDaySummaries[0]} siteId={'mock-site'} />,
    );

    expect(
      screen.getByRole('link', {
        name: 'View daily appointments',
      }),
    ).toBeInTheDocument();
  });
});
