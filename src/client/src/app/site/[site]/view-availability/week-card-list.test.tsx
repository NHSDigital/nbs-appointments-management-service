import { render } from '@testing-library/react';
import { mockSite } from '@testing/data';
import { ClinicalService, clinicalServices, WeekSummary } from '@types';
import { WeekCardList } from './week-card-list';
import { summariseWeek } from '@services/availabilityCalculatorService';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';
import { fetchClinicalServices } from '@services/appointmentsService';
import { DayJsType, parseDateStringToUkDatetime } from '@services/timeService';

jest.mock('@services/availabilityCalculatorService', () => ({
  summariseWeek: jest.fn(),
}));

jest.mock('@services/appointmentsService', () => ({
  fetchClinicalServices: jest.fn(),
}));

const mockSummariseWeek = summariseWeek as jest.Mock<Promise<WeekSummary>>;
const mockClinicalServices = fetchClinicalServices as jest.Mock<
  Promise<ClinicalService[]>
>;

const mockServices = clinicalServices;

const mockWeeks: DayJsType[][] = [
  [
    parseDateStringToUkDatetime('2024-06-10'),
    parseDateStringToUkDatetime('2024-06-11'),
    parseDateStringToUkDatetime('2024-06-12'),
    parseDateStringToUkDatetime('2024-06-13'),
    parseDateStringToUkDatetime('2024-06-14'),
    parseDateStringToUkDatetime('2024-06-15'),
    parseDateStringToUkDatetime('2024-06-16'),
  ],
  [
    parseDateStringToUkDatetime('2024-06-17'),
    parseDateStringToUkDatetime('2024-06-18'),
    parseDateStringToUkDatetime('2024-06-19'),
    parseDateStringToUkDatetime('2024-06-20'),
    parseDateStringToUkDatetime('2024-06-21'),
    parseDateStringToUkDatetime('2024-06-22'),
    parseDateStringToUkDatetime('2024-06-23'),
  ],
  [
    parseDateStringToUkDatetime('2024-06-24'),
    parseDateStringToUkDatetime('2024-06-25'),
    parseDateStringToUkDatetime('2024-06-26'),
    parseDateStringToUkDatetime('2024-06-27'),
    parseDateStringToUkDatetime('2024-06-28'),
    parseDateStringToUkDatetime('2024-06-29'),
    parseDateStringToUkDatetime('2024-06-30'),
  ],
];

describe('Week Card List', () => {
  beforeEach(() => {
    mockSummariseWeek.mockReturnValue(Promise.resolve(mockWeekSummary));
    mockClinicalServices.mockReturnValue(Promise.resolve(mockServices));
  });

  it('renders', async () => {
    const jsx = await WeekCardList({
      site: mockSite,
      ukWeeks: mockWeeks,
    });
    render(jsx);
  });

  it('requests a summary for each week', async () => {
    const jsx = await WeekCardList({
      site: mockSite,
      ukWeeks: mockWeeks,
    });
    render(jsx);

    expect(mockSummariseWeek).toHaveBeenCalledTimes(3);
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      parseDateStringToUkDatetime('2024-06-10'),
      parseDateStringToUkDatetime('2024-06-16'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      parseDateStringToUkDatetime('2024-06-17'),
      parseDateStringToUkDatetime('2024-06-23'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      parseDateStringToUkDatetime('2024-06-24'),
      parseDateStringToUkDatetime('2024-06-30'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockClinicalServices).toHaveBeenCalledTimes(1);
  });
});
