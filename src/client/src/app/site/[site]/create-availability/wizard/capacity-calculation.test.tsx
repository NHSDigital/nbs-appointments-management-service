import render from '@testing/render';
import { screen } from '@testing-library/react';
import CapacityCalculation, { calculateCapacity } from './capacity-calculation';

describe('Capacity Calculation', () => {
  it('renders', () => {
    render(
      <CapacityCalculation
        slotLength={5}
        capacity={2}
        startTime={{
          hour: 9,
          minute: 0,
        }}
        endTime={{
          hour: 12,
          minute: 0,
        }}
      />,
    );

    expect(screen.getByText('72')).toBeInTheDocument();
    expect(
      screen.getByText(/total appointments in the session/),
    ).toBeInTheDocument();

    expect(screen.getByText(/Up to/)).toBeInTheDocument();
    expect(screen.getByText('24')).toBeInTheDocument();
    expect(screen.getByText(/appointments per hour/)).toBeInTheDocument();
  });

  it('hides the appointments per hour calculation if session length is under 1 hour', () => {
    render(
      <CapacityCalculation
        slotLength={5}
        capacity={1}
        startTime={{
          hour: 9,
          minute: 0,
        }}
        endTime={{
          hour: 9,
          minute: 30,
        }}
      />,
    );

    expect(screen.getByText('6')).toBeInTheDocument();
    expect(
      screen.getByText(/total appointments in the session/),
    ).toBeInTheDocument();

    expect(screen.queryByText(/Up to/)).not.toBeInTheDocument();
    expect(screen.queryByText('24')).not.toBeInTheDocument();
    expect(screen.queryByText(/appointments per hour/)).not.toBeInTheDocument();
  });

  it.each([
    [9, 0, 12, 0, 5, 1, 36, 12],
    [9, 0, 12, 0, 5, 2, 72, 24],
    [9, 0, 9, 0, 5, 1, 0, 0], // start time == end time
    [9, 0, 9, 5, 5, 1, 1, undefined],
    [9, 0, 9, 5, 6, 1, 0, 0], // slot length > end time
    [10, 0, 9, 0, 5, 1, 0, 0], // start time > end time
    [9, 30, 9, 40, 5, 1, 2, undefined], // time span under an hour

    // Test cases debated in https://nhsd-jira.digital.nhs.uk/browse/APPT-239 and changes requested in 02/12/2024 comment
    [9, 0, 11, 0, 7, 3, 51, 27],
    [9, 0, 10, 0, 9, 2, 12, 14],
    [9, 0, 12, 0, 9, 5, 100, 35],
    [10, 57, 11, 3, 6, 1, 1, undefined],
    [9, 57, 11, 3, 6, 1, 11, 10],

    // No decimals allowed
    [9.5, 0, 12, 0, 5, 1, 0, 0],
    [9, 0.5, 12, 0, 5, 1, 0, 0],
    [9, 0, 12.5, 0, 5, 1, 0, 0],
    [9, 0, 12, 0.5, 5, 1, 0, 0],
    [9, 0, 12, 0, 5.5, 1, 0, 0],
    [9, 0, 12, 0, 5, 1.5, 0, 0],

    // No NaNs or Infinity
    [NaN, 30, 17, 0, 5, 1, 0, 0],
    [9, NaN, 12, 0, 5, 1, 0, 0],
    [9, 0, NaN, 0, 5, 1, 0, 0],
    [9, 0, 12, NaN, 5, 1, 0, 0],
    [9, 0, 12, 0, NaN, 1, 0, 0],
    [9, 0, 12, 0, 5, NaN, 0, 0],
    [Infinity, 0, 9, 5, 6, 1, 0, 0],
    [9, Infinity, 12, 0, 5, 1, 0, 0],
    [9, 0, Infinity, 0, 5, 1, 0, 0],
    [9, 0, 12, Infinity, 5, 1, 0, 0],
    [9, 0, 12, 0, Infinity, 1, 0, 0],
    [9, 0, 12, 0, 5, Infinity, 0, 0],
  ])(
    'calculates correctly: %i:%i - %i:%i, %i, %i',
    (
      startTimeHour,
      startTimeMinute,
      endTimeHour,
      endTimeMinute,
      slotLength,
      capacity,
      expectedCapacityPerSession,
      expectedCapacityPerHour,
    ) => {
      const result = calculateCapacity({
        startTime: { hour: startTimeHour, minute: startTimeMinute },
        endTime: { hour: endTimeHour, minute: endTimeMinute },
        slotLength,
        capacity,
      });

      expect(result.appointmentsPerSession).toBe(expectedCapacityPerSession);
      expect(result.appointmentsPerHour).toBe(expectedCapacityPerHour);
    },
  );
});
