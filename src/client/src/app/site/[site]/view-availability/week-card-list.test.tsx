import { render, screen } from '@testing-library/react';
import { mockDetailedWeeks, mockSite } from '@testing/data';
import dayjs from 'dayjs';
import { getDetailedMonthView } from '@services/viewAvailabilityService';
import { Week } from '@types';
import { WeekCardList } from './week-card-list';

jest.mock('@services/viewAvailabilityService', () => ({
  getDetailedMonthView: jest.fn(),
}));

const mockGetDetailedMonthView = getDetailedMonthView as jest.Mock<
  Promise<Week[]>
>;

describe('Week Card List', () => {
  beforeEach(() => {
    mockGetDetailedMonthView.mockReturnValue(
      Promise.resolve(mockDetailedWeeks),
    );
  });

  it('renders', async () => {
    const jsx = await WeekCardList({
      site: mockSite,
      searchMonth: dayjs().year(2024).month(11),
    });
    render(jsx);

    expect(
      screen.getByRole('heading', { name: '1 December to 7 December' }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole('table')).toHaveLength(6);
  });

  it('renders the correct information for a week', async () => {
    const jsx = await WeekCardList({
      site: mockSite,
      searchMonth: dayjs().year(2024).month(11),
    });
    render(jsx);

    expect(
      screen.getByRole('row', {
        name: 'Total appointments: 30 Booked: 17 Unbooked: 13',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', { name: 'FLU 18-64 5' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', { name: 'COVID 75+ 10' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', { name: 'RSV (Adult) 2' }),
    ).toBeInTheDocument();
  });

  it('renders a link for each week', async () => {
    const jsx = await WeekCardList({
      site: mockSite,
      searchMonth: dayjs().year(2024).month(11),
    });
    render(jsx);

    expect(screen.getAllByText('View week')).toHaveLength(3);
  });
});
