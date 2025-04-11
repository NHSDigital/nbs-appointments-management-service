import { render, screen } from '@testing-library/react';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import {
  mockSite,
  mockWeekAvailabilityEnd,
  mockWeekAvailabilityStart,
} from '@testing/data';

jest.mock('./day-card-list', () => {
  const MockDayCardList = () => {
    return <div>This is a list of day cards</div>;
  };
  return {
    DayCardList: MockDayCardList,
  };
});

describe('View Week Availability Page', () => {
  it('renders', async () => {
    const jsx = await ViewWeekAvailabilityPage({
      ukWeekStart: mockWeekAvailabilityStart,
      ukWeekEnd: mockWeekAvailabilityEnd,
      site: mockSite,
    });
    render(jsx);
  });

  it('renders pagination options with the correct values', async () => {
    const jsx = await ViewWeekAvailabilityPage({
      ukWeekStart: mockWeekAvailabilityStart,
      ukWeekEnd: mockWeekAvailabilityEnd,
      site: mockSite,
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Next : 9-15 December 2024' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Previous : 25 November-1 December 2024',
      }),
    ).toBeInTheDocument();
  });

  it('renders a list of day cards', async () => {
    const jsx = await ViewWeekAvailabilityPage({
      ukWeekStart: mockWeekAvailabilityStart,
      ukWeekEnd: mockWeekAvailabilityEnd,
      site: mockSite,
    });
    render(jsx);

    expect(screen.getByText('This is a list of day cards')).toBeInTheDocument();
  });
});
