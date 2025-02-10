import render from '@testing/render';
import { screen } from '@testing-library/react';
import { DaySummaryCard } from './day-summary-card';
import { mockDaySummaries, mockEmptyDays } from '@testing/data';
import { isInTheFuture, now } from '@services/timeService';
import dayjs from 'dayjs';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    isInTheFuture: jest.fn(),
    now: jest.fn(),
  };
});

const mockIsInTheFuture = isInTheFuture as jest.Mock<boolean>;
const mockNow = now as jest.Mock<dayjs.Dayjs>;

describe('Day Summary Card', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockIsInTheFuture.mockReturnValue(true);
    mockNow.mockReturnValue(dayjs().year(2024).month(10).date(1));
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
          name: '09:00 - 17:00 RSV (Adult) 5 booked 118 unbooked Change',
        }),
      ).toBeInTheDocument();
    });

    it('includes the action column if the user has the permission', () => {
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
        screen.getByRole('link', {
          name: 'Change',
        }),
      ).toBeInTheDocument();
    });

    it('hides the action column if the user lacks the permission', () => {
      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={false}
        />,
      );

      expect(
        screen.getByRole('row', {
          name: 'Time Services Booked Unbooked',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('link', {
          name: 'Change',
        }),
      ).toBeNull();
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

    it('does not render add session link if the user lacks the permission', () => {
      mockIsInTheFuture.mockReturnValue(true);

      render(
        <DaySummaryCard
          daySummary={mockDaySummaries[0]}
          siteId={'mock-site'}
          canManageAvailability={false}
        />,
      );

      expect(screen.queryByRole('link', { name: 'Add Session' })).toBeNull();
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

      expect(screen.getByText('Total appointments: 123')).toBeInTheDocument();
      expect(screen.getByText('Booked: 5')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 118')).toBeInTheDocument();
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

    it('renders a warning if there are cancelled appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockDaySummaries[0], cancelledAppointments: 3 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('3')).toBeInTheDocument();
      expect(
        screen.getByText(/cancelled appointments on this day./),
      ).toBeInTheDocument();
    });

    it('renders a link to view cancelled appointments if there are cancelled appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockDaySummaries[0], cancelledAppointments: 3 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', {
          name: 'View cancelled appointments',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'View cancelled appointments',
        }),
      ).toHaveAttribute(
        'href',
        'daily-appointments?date=2024-12-02&page=1&tab=1',
      );
    });

    it('renders a warning if there are orphaned appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockDaySummaries[0], orphanedAppointments: 20 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellations on this day./),
      ).toBeInTheDocument();
    });

    it('renders a link to view orphaned appointments if there are cancelled appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockDaySummaries[0], orphanedAppointments: 20 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', {
          name: 'View manual cancellations',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'View manual cancellations',
        }),
      ).toHaveAttribute(
        'href',
        'daily-appointments?date=2024-12-02&page=1&tab=2',
      );
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

    it('does not render add session link if the user lacks the permission', () => {
      mockIsInTheFuture.mockReturnValue(true);

      render(
        <DaySummaryCard
          daySummary={mockEmptyDays[0]}
          siteId={'mock-site'}
          canManageAvailability={false}
        />,
      );

      expect(
        screen.queryByRole('link', { name: 'Add availability to this day' }),
      ).toBeNull();
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

    it('renders a link to view cancelled appointments if there are cancelled appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], cancelledAppointments: 3 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', {
          name: 'View cancelled appointments',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'View cancelled appointments',
        }),
      ).toHaveAttribute(
        'href',
        'daily-appointments?date=2024-12-02&page=1&tab=1',
      );
    });

    it('renders a link to view orphaned appointments if there are cancelled appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], orphanedAppointments: 20 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(
        screen.getByRole('link', {
          name: 'View manual cancellations',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'View manual cancellations',
        }),
      ).toHaveAttribute(
        'href',
        'daily-appointments?date=2024-12-02&page=1&tab=2',
      );
    });

    it('renders a warning if there are orphaned appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], orphanedAppointments: 20 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellations on this day./),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are cancelled appointments', () => {
      render(
        <DaySummaryCard
          daySummary={{ ...mockEmptyDays[0], cancelledAppointments: 3 }}
          siteId={'mock-site'}
          canManageAvailability={true}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('3')).toBeInTheDocument();
      expect(
        screen.getByText(/cancelled appointments on this day./),
      ).toBeInTheDocument();
    });
  });
});
