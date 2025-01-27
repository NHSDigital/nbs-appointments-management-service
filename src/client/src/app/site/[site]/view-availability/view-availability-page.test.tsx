import { render, screen } from '@testing-library/react';
import { ViewAvailabilityPage } from './view-availability-page';
import { mockSite } from '@testing/data';
import dayjs from 'dayjs';

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
    render(
      <ViewAvailabilityPage
        site={mockSite}
        searchMonth={dayjs().year(2024).month(11)}
      />,
    );
  });

  it('renders pagination options with the correct values', async () => {
    render(
      <ViewAvailabilityPage
        site={mockSite}
        searchMonth={dayjs().year(2024).month(11)}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Previous : November 2024' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Next : January 2025' }),
    ).toBeInTheDocument();
  });
});
