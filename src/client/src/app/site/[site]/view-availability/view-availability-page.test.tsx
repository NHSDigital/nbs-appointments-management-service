import { render, screen } from '@testing-library/react';
import { ViewAvailabilityPage } from './view-availability-page';
import { mockSite } from '@testing/data';
import { parseDateStringToUkDatetime } from '@services/timeService';

jest.mock('./week-card-list', () => {
  const MockWeekCardList = () => {
    return <div>This is a list of week cards</div>;
  };
  return {
    WeekCardList: MockWeekCardList,
  };
});

describe('View Availability Page', () => {
  it('renders', async () => {
    const jsx = await ViewAvailabilityPage({
      site: mockSite,
      searchMonth: parseDateStringToUkDatetime('2024-12-01'),
    });
    render(jsx);
  });

  it('renders pagination options with the correct values', async () => {
    const jsx = await ViewAvailabilityPage({
      site: mockSite,
      searchMonth: parseDateStringToUkDatetime('2024-12-01'),
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Previous : November 2024' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Next : January 2025' }),
    ).toBeInTheDocument();
  });

  it('renders a list of week cards', async () => {
    const jsx = await ViewAvailabilityPage({
      site: mockSite,
      searchMonth: parseDateStringToUkDatetime('2024-12-01'),
    });
    render(jsx);

    expect(
      screen.getByText('This is a list of week cards'),
    ).toBeInTheDocument();
  });
});
