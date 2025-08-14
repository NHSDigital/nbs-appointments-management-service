import render from '@testing/render';
import { screen } from '@testing-library/react';
import { DaySummaryCard } from './day-summary-card';
import { mockDaySummaries, mockEmptyDays } from '@testing/data';
import {
  DayJsType,
  isFutureCalendarDateUk,
  parseToUkDatetime,
  ukNow,
} from '@services/timeService';
import { clinicalServices } from '@types';
import { cookies } from 'next/headers';
import { fetchFeatureFlag } from '@services/appointmentsService';
import { FeatureFlag } from '@types';
jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    isFutureCalendarDateUk: jest.fn(),
    ukNow: jest.fn(),
  };
});

jest.mock('next/headers', () => ({
  cookies: jest.fn(),
}));
jest.mock('@services/appointmentsService', () => ({
  fetchFeatureFlag: jest.fn(),
}));

const mockIsFutureCalendarDateUk = isFutureCalendarDateUk as jest.Mock<boolean>;
const mockUkNow = ukNow as jest.Mock<DayJsType>;
const fetchFeatureFlagMock = fetchFeatureFlag as jest.Mock<
  Promise<FeatureFlag>
>;

describe('Day Summary Card', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockIsFutureCalendarDateUk.mockReturnValue(true);
    mockUkNow.mockReturnValue(parseToUkDatetime('2024-11-01'));
    (cookies as jest.Mock).mockReturnValue({
      get: jest.fn().mockReturnValue({ value: 'mock-token' }),
    });
    fetchFeatureFlagMock.mockResolvedValue({
      enabled: true,
    });
  });

  it('renders', async () => {
    const jsx = await DaySummaryCard({
      daySummary: mockDaySummaries[0],
      siteId: 'mock-site',
      canManageAvailability: true,
      clinicalServices: clinicalServices,
      canViewDailyAppointments: true,
    });
    render(jsx);

    expect(
      screen.getByRole('heading', { name: 'Monday 2 December' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'View daily appointments' }),
    ).toBeInTheDocument();
  });

  describe('when there is availability', () => {
    it('renders availability table', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('row', {
          name: 'Time Services Booked Unbooked Action',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('row', {
          name: '09:00 - 17:00 RSV Adult 5 booked 118 unbooked Change',
        }),
      ).toBeInTheDocument();
    });

    it('includes the action column if the user has the permission', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

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

    it('hides the action column if the user lacks the permission', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: false,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

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

    it('renders add session link if the date is in the future', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(true);

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', { name: 'Add Session' }),
      ).toBeInTheDocument();
    });

    it('does not render add session link if the user lacks the permission', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(true);

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: false,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.queryByRole('link', { name: 'Add Session' })).toBeNull();
    });

    it('does not render add session link if the date is in the past', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(false);

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'Add Session' }),
      ).not.toBeInTheDocument();
    });

    it('renders total appointments table', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.getByText('Total appointments: 123')).toBeInTheDocument();
      expect(screen.getByText('Booked: 5')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 118')).toBeInTheDocument();
    });

    it('renders view appointments link', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', {
          name: 'View daily appointments',
        }),
      ).toBeInTheDocument();
    });

    it('renders a link to view cancelled appointments if there are cancelled appointments', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockDaySummaries[0], cancelledAppointments: 3 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

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

    it('renders a warning if there are orphaned appointments', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockDaySummaries[0], orphanedAppointments: 20 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellations on this day./),
      ).toBeInTheDocument();
    });

    it('renders a link to view orphaned appointments if there are cancelled appointments', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockDaySummaries[0], orphanedAppointments: 20 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

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

    it('hides the View Daily Appointments link when the user does not have permission', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: false,
      });
      render(jsx);

      expect(
        screen.getByRole('heading', { name: 'Monday 2 December' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('link', { name: 'View daily appointments' }),
      ).not.toBeInTheDocument();
    });

    it('renders "Cancel day" link if the date is in the future', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(true);

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', { name: 'Cancel day' }),
      ).toBeInTheDocument();
      expect(screen.getByRole('link', { name: 'Cancel day' })).toHaveAttribute(
        'href',
        '/site/mock-site/cancel-day',
      );
    });

    it('does not render "Cancel day" link if the date is in the past', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(false);

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.queryByRole('link', { name: 'Cancel day' })).toBeNull();
    });

    it('does not render "Cancel day" link if feature toggle is disabled', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(false);
      fetchFeatureFlagMock.mockResolvedValue({
        enabled: false,
      });

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.queryByRole('link', { name: 'Cancel day' })).toBeNull();
    });

    it('renders "Cancel day" link if feature toggle is enabled', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(true);

      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', { name: 'Cancel day' }),
      ).toBeInTheDocument();
      expect(screen.getByRole('link', { name: 'Cancel day' })).toHaveAttribute(
        'href',
        '/site/mock-site/cancel-day',
      );
    });
  });

  describe('when there is no availability', () => {
    it('renders no availability message', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.getByText('No availability')).toBeInTheDocument();

      expect(
        screen.queryByRole('row', {
          name: 'Time Services Booked Unbooked Action',
        }),
      ).toBeNull();
    });

    it('does not render total appointments table', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('row', {
          name: 'Total appointments: 123 Booked: 5 Unbooked: 118',
        }),
      ).toBeNull();
    });

    it('renders add session link if the date is in the future', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(true);

      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', { name: 'Add availability to this day' }),
      ).toBeInTheDocument();
    });

    it('does not render add session link if the user lacks the permission', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(true);

      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: false,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'Add availability to this day' }),
      ).toBeNull();
    });

    it('does not render add session link if the date is in the past', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(false);

      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'Add availability to this day' }),
      ).not.toBeInTheDocument();
    });

    it('renders view cancelled appointments if there are any to show', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], cancelledAppointments: 1 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', { name: 'View cancelled appointments' }),
      ).toBeInTheDocument();
    });

    it('does not render view cancelled appointments if there none any to show', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], cancelledAppointments: 0 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'View cancelled appointments' }),
      ).toBeNull();
    });

    it('renders view orphaned appointments if there are any to show', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], orphanedAppointments: 1 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.getByRole('link', { name: 'View manual cancellations' }),
      ).toBeInTheDocument();
    });

    it('does not render view orphaned appointments if there none any to show', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], orphanedAppointments: 0 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'View manual cancellations' }),
      ).toBeNull();
    });

    it('does not render manage availability links on a day summary', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockDaySummaries[0],
        siteId: 'mock-site',
        canManageAvailability: false,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'Add Session' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('link', { name: 'Change' }),
      ).not.toBeInTheDocument();
    });

    it('does not render the add availability link on an empty day summary', async () => {
      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: false,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(
        screen.queryByRole('link', { name: 'Add availability to this day' }),
      ).not.toBeInTheDocument();
    });

    it('renders a link to view cancelled appointments if there are cancelled appointments', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], cancelledAppointments: 3 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

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

    it('renders a link to view orphaned appointments if there are cancelled appointments', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], orphanedAppointments: 20 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

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

    it('renders a warning if there are orphaned appointments', async () => {
      const jsx = await DaySummaryCard({
        daySummary: { ...mockEmptyDays[0], orphanedAppointments: 20 },
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellations on this day./),
      ).toBeInTheDocument();
    });

    it('does not render "Cancel day" link when there is no availability', async () => {
      mockIsFutureCalendarDateUk.mockReturnValue(false);

      const jsx = await DaySummaryCard({
        daySummary: mockEmptyDays[0],
        siteId: 'mock-site',
        canManageAvailability: true,
        clinicalServices: clinicalServices,
        canViewDailyAppointments: true,
      });
      render(jsx);

      expect(screen.queryByRole('link', { name: 'Cancel day' })).toBeNull();
    });
  });
});
