import { render, screen } from '@testing-library/react';
import { mockSite } from '@testing/data';
import {
  ClinicalService,
  clinicalServices,
  FeatureFlag,
  WeekSummary,
  WeekSummaryV2,
} from '@types';
import { summariseWeek } from '@services/availabilityCalculatorService';
import {
  mockWeekSummary,
  mockWeekSummaryV2,
} from '@testing/availability-and-bookings-mock-data';
import { DayCardList } from './day-card-list';
import {
  fetchClinicalServices,
  fetchPermissions,
  fetchWeekSummaryV2,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { parseToUkDatetime } from '@services/timeService';

jest.mock('@services/availabilityCalculatorService', () => {
  const originalModule = jest.requireActual(
    '@services/availabilityCalculatorService',
  );
  return {
    ...originalModule,
    summariseWeek: jest.fn(),
  };
});

jest.mock('@services/appointmentsService', () => ({
  fetchPermissions: jest.fn(),
  fetchClinicalServices: jest.fn(),
  fetchWeekSummaryV2: jest.fn(),
  fetchFeatureFlag: jest.fn(),
}));

const mockSummariseWeek = summariseWeek as jest.Mock<Promise<WeekSummary>>;
const mockFetchPermissions = fetchPermissions as jest.Mock<Promise<string[]>>;
const mockFetchFeatureFlag = fetchFeatureFlag as jest.Mock<
  Promise<FeatureFlag>
>;
const mockFetchWeekSummaryV2 = fetchWeekSummaryV2 as jest.Mock<
  Promise<WeekSummaryV2>
>;
const mockClinicalServices = fetchClinicalServices as jest.Mock<
  Promise<ClinicalService[]>
>;

const mockServices = clinicalServices;

describe.each([false, true])(
  'Day Card List',
  (multipleServicesEnabled: boolean) => {
    beforeEach(() => {
      mockFetchFeatureFlag.mockReturnValue(
        Promise.resolve({
          enabled: multipleServicesEnabled,
        }),
      );

      mockFetchPermissions.mockReturnValue(
        Promise.resolve(['availability:setup']),
      );
      mockClinicalServices.mockReturnValue(Promise.resolve(mockServices));

      if (multipleServicesEnabled) {
        mockFetchWeekSummaryV2.mockReturnValue(
          Promise.resolve(mockWeekSummaryV2),
        );
      } else {
        mockSummariseWeek.mockReturnValue(Promise.resolve(mockWeekSummary));
      }
    });

    it('renders', async () => {
      const jsx = await DayCardList({
        site: mockSite,
        ukWeekStart: parseToUkDatetime('2024-06-10'),
        ukWeekEnd: parseToUkDatetime('2024-06-16'),
      });
      render(jsx);
    });

    it('requests a summary for the week', async () => {
      const jsx = await DayCardList({
        site: mockSite,
        ukWeekStart: parseToUkDatetime('2024-06-10'),
        ukWeekEnd: parseToUkDatetime('2024-06-16'),
      });
      render(jsx);

      if (multipleServicesEnabled) {
        expect(fetchWeekSummaryV2).toHaveBeenCalledWith(
          mockSite.id,
          '2024-06-10',
        );
      } else {
        expect(mockSummariseWeek).toHaveBeenCalledWith(
          parseToUkDatetime('2024-06-10'),
          parseToUkDatetime('2024-06-16'),
          mockSite.id,
        );
      }

      expect(mockFetchFeatureFlag).toHaveBeenCalled();
      expect(mockFetchPermissions).toHaveBeenCalled();
      expect(mockClinicalServices).toHaveBeenCalledTimes(1);
    });

    it('renders a card for each day in the week', async () => {
      const jsx = await DayCardList({
        site: mockSite,
        ukWeekStart: parseToUkDatetime('2024-06-10'),
        ukWeekEnd: parseToUkDatetime('2024-06-16'),
      });
      render(jsx);

      expect(
        screen.getByRole('heading', { name: 'Monday 10 June' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', { name: 'Tuesday 11 June' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', { name: 'Wednesday 12 June' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', { name: 'Thursday 13 June' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', { name: 'Friday 14 June' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', {
          name: 'Saturday 15 June',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', {
          name: 'Sunday 16 June',
        }),
      ).toBeInTheDocument();
    });
  },
);
