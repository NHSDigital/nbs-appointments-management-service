import { render, screen } from '@testing-library/react';
import { mockSite } from '@testing/data';
import {
  ClinicalService,
  FeatureFlag,
  ServerActionResult,
  WeekSummaryV2,
} from '@types';
import { mockWeekSummaryV2 } from '@testing/availability-and-bookings-mock-data';
import { DayCardList } from './day-card-list';
import {
  fetchClinicalServices,
  fetchPermissions,
  fetchWeekSummaryV2,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { parseToUkDatetime } from '@services/timeService';
import asServerActionResult from '@testing/asServerActionResult';

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

const mockFetchPermissions = fetchPermissions as jest.Mock<
  Promise<ServerActionResult<string[]>>
>;
const mockFetchFeatureFlag = fetchFeatureFlag as jest.Mock<
  Promise<ServerActionResult<FeatureFlag>>
>;
const mockFetchWeekSummaryV2 = fetchWeekSummaryV2 as jest.Mock<
  Promise<ServerActionResult<WeekSummaryV2>>
>;

const mockClinicalServices = fetchClinicalServices as jest.Mock<
  Promise<ServerActionResult<ClinicalService[]>>
>;

describe('Day Card List', () => {
  beforeEach(() => {
    mockFetchPermissions.mockResolvedValue(
      asServerActionResult(['availability:setup']),
    );
    mockClinicalServices.mockResolvedValue(
      asServerActionResult([
        {
          label: 'RSV Adult',
          value: 'RSV:Adult',
          serviceType: 'RSV',
          url: 'RSV',
        },
      ]),
    );

    mockFetchFeatureFlag.mockResolvedValue(
      asServerActionResult({
        enabled: false,
      }),
    );

    mockFetchWeekSummaryV2.mockResolvedValue(
      asServerActionResult(mockWeekSummaryV2),
    );
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

    expect(fetchWeekSummaryV2).toHaveBeenCalledWith(mockSite.id, '2024-06-10');

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
});
