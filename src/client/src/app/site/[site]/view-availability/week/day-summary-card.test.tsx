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
      <DaySummaryCard
        daySummary={mockDaySummaries[0]}
        siteId={'mock-site'}
        canManageAvailability={true}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Monday 2 December' }),
    ).toBeInTheDocument();
  });

  describe('when there is availability', () => {
    it('renders availability table', () => {
      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('row', {
          name: 'Time Services Booked Unbooked Action',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('row', {
          name: '09:00 - 17:00 RSV (Adult) 5 booked 118 unbooked',
        }),
      ).toBeInTheDocument();
    });

    it('renders add session link if the date is in the future', () => {
      mockIsInTheFuture.mockReturnValue(true);

      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', { name: 'Add Session' }),
      ).toBeInTheDocument();
    });

    it('does not render add session link if the date is in the past', () => {
      mockIsInTheFuture.mockReturnValue(false);

      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'Add Session' }),
      ).not.toBeInTheDocument();
    });

    it('renders total appointments table', () => {
      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('row', {
          name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
        }),
      ).toBeInTheDocument();
    });

    it('renders view appointments link', () => {
      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', {
          name: 'View daily appointments',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('when there is no availability', () => {
    it('renders no availability message', () => {
      render(
        <DaySummaryCard
          daySummary={mockEmptyDays[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(screen.getByText('No availability')).toBeInTheDocument();

      expect(
        screen.queryByRole('row', {
          name: 'Time Services Booked Unbooked Action',
        }),
      ).toBeNull();
    });

    it('does not render total appointments table', () => {
      render(
        <DaySummaryCard
          daySummary={mockEmptyDays[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.queryByRole('row', {
          name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
        }),
      ).toBeNull();
    });

    it('renders add session link if the date is in the future', () => {
      mockIsInTheFuture.mockReturnValue(true);

      render(
        <DaySummaryCard
          daySummary={mockEmptyDays[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', { name: 'Add availability to this day' }),
      ).toBeInTheDocument();
    });

    it('does not render add session link if the date is in the past', () => {
      mockIsInTheFuture.mockReturnValue(false);

      render(
        <DaySummaryCard
          daySummary={mockEmptyDays[0]}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'Add availability to this day' }),
      ).not.toBeInTheDocument();
    });

    it('renders view cancelled appointments if there are any to show', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], cancelledAppointments: 1 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', { name: 'View cancelled appointments' }),
      ).toBeInTheDocument();
    });

    it('does not render view cancelled appointments if there none any to show', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], cancelledAppointments: 0 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'View cancelled appointments' }),
      ).toBeNull();
    });

    it('renders view orphaned appointments if there are any to show', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], orphanedAppointments: 1 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', { name: 'View manual cancellations' }),
      ).toBeInTheDocument();
    });

    it('does not render view orphaned appointments if there none any to show', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], orphanedAppointments: 0 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'View manual cancellations' }),
      ).toBeNull();
    });

    it('does not render manage availability links on a day summary', () => {
      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={false}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'Add Session' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('link', { name: 'Change' }),
      ).not.toBeInTheDocument();
    });

    it('does not render the add availability link on an empty day summary', () => {
      render(
        <DaySummaryCard
          daySummary={mockEmptyDays[0]}
          siteId={'mock-site'}
          canManageAvailability={false}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'Add availability to this day' }),
      ).not.toBeInTheDocument();
    });
  });
});
