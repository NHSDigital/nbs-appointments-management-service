import { mockAvailabilitySummary } from '@testing/availability-and-bookings-mock-data';
import { screen, within } from '@testing-library/react';
import { ChangeSessionSummaryTable } from './change-session-summary-table';
import render from '@testing/render';
import {
  dateTimeFormat,
  DayJsType,
  parseToUkDatetime,
  ukNow,
} from '@services/timeService';
import { mockMultipleServices } from '@testing/data';

jest.mock('@types', () => ({
  ...jest.requireActual('@types'),
}));

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});
const mockUkNow = ukNow as jest.Mock<DayJsType>;

describe('Session summary table', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('renders', () => {
    mockUkNow.mockReturnValue(
      parseToUkDatetime('2024-06-10T08:34:00', dateTimeFormat),
    );

    render(
      <ChangeSessionSummaryTable
        sessionSummary={mockAvailabilitySummary}
        tableCaption="test caption"
      />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();
  });

  it('renders expected headers and rows', () => {
    mockUkNow.mockReturnValue(
      parseToUkDatetime('2024-06-10T08:34:00', dateTimeFormat),
    );

    render(
      <ChangeSessionSummaryTable
        sessionSummary={mockAvailabilitySummary}
        tableCaption="test caption"
      />,
    );

    expect(
      screen.getByRole('row', { name: 'Time Services' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV Adult',
      }),
    ).toBeInTheDocument();
  });

  it('renders a caption when tableCaption is provided', () => {
    render(
      <ChangeSessionSummaryTable
        sessionSummary={mockAvailabilitySummary}
        tableCaption="My custom caption"
      />,
    );

    expect(screen.getByText('My custom caption')).toBeInTheDocument();
  });
});
