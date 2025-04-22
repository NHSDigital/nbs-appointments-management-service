import { render, screen } from '@testing-library/react';
import { mockSite } from '@testing/data';
import { ClinicalService, clinicalServices, WeekSummary } from '@types';
import { summariseWeek } from '@services/availabilityCalculatorService';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';
import { DayCardList } from './day-card-list';
import {
  fetchClinicalServices,
  fetchPermissions,
} from '@services/appointmentsService';
import { parseDateStringToUkDatetime } from '@services/timeService';

jest.mock('@services/availabilityCalculatorService', () => ({
  summariseWeek: jest.fn(),
}));
jest.mock('@services/appointmentsService', () => ({
  fetchPermissions: jest.fn(),
  fetchClinicalServices: jest.fn(),
}));

const mockSummariseWeek = summariseWeek as jest.Mock<Promise<WeekSummary>>;
const mockFetchPermissions = fetchPermissions as jest.Mock<Promise<string[]>>;
const mockClinicalServices = fetchClinicalServices as jest.Mock<
  Promise<ClinicalService[]>
>;

const mockServices = clinicalServices;

describe('Day Card List', () => {
  beforeEach(() => {
    mockSummariseWeek.mockReturnValue(Promise.resolve(mockWeekSummary));
    mockFetchPermissions.mockReturnValue(
      Promise.resolve(['availability:setup']),
    );
    mockClinicalServices.mockReturnValue(Promise.resolve(mockServices));
  });

  it('renders', async () => {
    const jsx = await DayCardList({
      site: mockSite,
      ukWeekStart: parseDateStringToUkDatetime('2024-06-10'),
      ukWeekEnd: parseDateStringToUkDatetime('2024-06-16'),
    });
    render(jsx);
  });

  it('requests a summary for the week', async () => {
    const jsx = await DayCardList({
      site: mockSite,
      ukWeekStart: parseDateStringToUkDatetime('2024-06-10'),
      ukWeekEnd: parseDateStringToUkDatetime('2024-06-16'),
    });
    render(jsx);

    expect(mockSummariseWeek).toHaveBeenCalledWith(
      parseDateStringToUkDatetime('2024-06-10'),
      parseDateStringToUkDatetime('2024-06-16'),
      mockSite.id,
    );
    expect(mockFetchPermissions).toHaveBeenCalled();
    expect(mockClinicalServices).toHaveBeenCalledTimes(1);
  });

  it('renders a card for each day in the week', async () => {
    const jsx = await DayCardList({
      site: mockSite,
      ukWeekStart: parseDateStringToUkDatetime('2024-06-10'),
      ukWeekEnd: parseDateStringToUkDatetime('2024-06-16'),
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
