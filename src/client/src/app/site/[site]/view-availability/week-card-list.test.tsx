import { render } from '@testing-library/react';
import { mockSite } from '@testing/data';
import { ClinicalService, WeekSummary } from '@types';
import { WeekCardList } from './week-card-list';
import { summariseWeek } from '@services/availabilityCalculatorService';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';
import { fetchClinicalServices } from '@services/appointmentsService';
import { DayJsType, parseToUkDatetime } from '@services/timeService';

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

const clinicalServices: ClinicalService[] = [
  { label: 'RSV Adult', value: 'RSV:Adult' },
];

const mockWeeks: DayJsType[][] = [
  [
    parseToUkDatetime('2024-06-10'),
    parseToUkDatetime('2024-06-11'),
    parseToUkDatetime('2024-06-12'),
    parseToUkDatetime('2024-06-13'),
    parseToUkDatetime('2024-06-14'),
    parseToUkDatetime('2024-06-15'),
    parseToUkDatetime('2024-06-16'),
  ],
  [
    parseToUkDatetime('2024-06-17'),
    parseToUkDatetime('2024-06-18'),
    parseToUkDatetime('2024-06-19'),
    parseToUkDatetime('2024-06-20'),
    parseToUkDatetime('2024-06-21'),
    parseToUkDatetime('2024-06-22'),
    parseToUkDatetime('2024-06-23'),
  ],
  [
    parseToUkDatetime('2024-06-24'),
    parseToUkDatetime('2024-06-25'),
    parseToUkDatetime('2024-06-26'),
    parseToUkDatetime('2024-06-27'),
    parseToUkDatetime('2024-06-28'),
    parseToUkDatetime('2024-06-29'),
    parseToUkDatetime('2024-06-30'),
  ],
];

describe('Week Card List', () => {
  beforeEach(() => {
    mockSummariseWeek.mockReturnValue(Promise.resolve(mockWeekSummary));
    mockClinicalServices.mockReturnValue(Promise.resolve(clinicalServices));
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
      parseToUkDatetime('2024-06-10'),
      parseToUkDatetime('2024-06-16'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      parseToUkDatetime('2024-06-17'),
      parseToUkDatetime('2024-06-23'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      parseToUkDatetime('2024-06-24'),
      parseToUkDatetime('2024-06-30'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockClinicalServices).toHaveBeenCalledTimes(1);
  });
});
