import { render } from '@testing-library/react';
import { mockSite } from '@testing/data';
import dayjs from 'dayjs';
import { ClinicalService, clinicalServices, WeekSummary } from '@types';
import { WeekCardList } from './week-card-list';
import { summariseWeek } from '@services/availabilityCalculatorService';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';
import { fetchClinicalServices } from '@services/appointmentsService';

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

const mockWeeks: dayjs.Dayjs[][] = [
  [
    dayjs('2024-06-10T00:00:00.000Z'),
    dayjs('2024-06-11T00:00:00.000Z'),
    dayjs('2024-06-12T00:00:00.000Z'),
    dayjs('2024-06-13T00:00:00.000Z'),
    dayjs('2024-06-14T00:00:00.000Z'),
    dayjs('2024-06-15T00:00:00.000Z'),
    dayjs('2024-06-16T00:00:00.000Z'),
  ],
  [
    dayjs('2024-06-17T00:00:00.000Z'),
    dayjs('2024-06-18T00:00:00.000Z'),
    dayjs('2024-06-19T00:00:00.000Z'),
    dayjs('2024-06-20T00:00:00.000Z'),
    dayjs('2024-06-21T00:00:00.000Z'),
    dayjs('2024-06-22T00:00:00.000Z'),
    dayjs('2024-06-23T00:00:00.000Z'),
  ],
  [
    dayjs('2024-06-24T00:00:00.000Z'),
    dayjs('2024-06-25T00:00:00.000Z'),
    dayjs('2024-06-26T00:00:00.000Z'),
    dayjs('2024-06-27T00:00:00.000Z'),
    dayjs('2024-06-28T00:00:00.000Z'),
    dayjs('2024-06-29T00:00:00.000Z'),
    dayjs('2024-06-30T00:00:00.000Z'),
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
      weeks: mockWeeks,
    });
    render(jsx);
  });

  it('requests a summary for each week', async () => {
    const jsx = await WeekCardList({
      site: mockSite,
      weeks: mockWeeks,
    });
    render(jsx);

    expect(mockSummariseWeek).toHaveBeenCalledTimes(3);
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      dayjs('2024-06-10T00:00:00.000Z'),
      dayjs('2024-06-16T00:00:00.000Z'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      dayjs('2024-06-17T00:00:00.000Z'),
      dayjs('2024-06-23T00:00:00.000Z'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockSummariseWeek).toHaveBeenCalledWith(
      dayjs('2024-06-24T00:00:00.000Z'),
      dayjs('2024-06-30T00:00:00.000Z'),
      '34e990af-5dc9-43a6-8895-b9123216d699',
    );
    expect(mockClinicalServices).toHaveBeenCalledTimes(1);
  });
});
