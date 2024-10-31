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

    expect(
      screen.getByText('72 appointments per day. 24 per hour.'),
    ).toBeInTheDocument();
  });

  it.each([
    [9, 0, 12, 0, 5, 1, '36 appointments per day. 12 per hour.'],
    [9, 0, 12, 0, 5, 2, '72 appointments per day. 24 per hour.'],
    [9, 0, 9, 0, 5, 1, 'No capacity.'], // start time == end time
    [9, 0, 9, 5, 5, 1, '1 appointments per day. 12 per hour.'],
    [9, 0, 9, 5, 6, 1, 'No capacity.'], // slot length > end time
    [10, 0, 9, 0, 5, 1, 'No capacity.'], // start time > end time
    [9, 30, 9, 40, 5, 1, '2 appointments per day. 12 per hour.'], // time span under an hour

    // No decimals allowed
    [9.5, 0, 12, 0, 5, 1, 'No capacity.'],
    [9, 0.5, 12, 0, 5, 1, 'No capacity.'],
    [9, 0, 12.5, 0, 5, 1, 'No capacity.'],
    [9, 0, 12, 0.5, 5, 1, 'No capacity.'],
    [9, 0, 12, 0, 5.5, 1, 'No capacity.'],
    [9, 0, 12, 0, 5, 1.5, 'No capacity.'],

    // No NaNs or Infinity
    [NaN, 30, 17, 0, 5, 1, 'No capacity.'],
    [9, NaN, 12, 0, 5, 1, 'No capacity.'],
    [9, 0, NaN, 0, 5, 1, 'No capacity.'],
    [9, 0, 12, NaN, 5, 1, 'No capacity.'],
    [9, 0, 12, 0, NaN, 1, 'No capacity.'],
    [9, 0, 12, 0, 5, NaN, 'No capacity.'],
    [Infinity, 0, 9, 5, 6, 1, 'No capacity.'],
    [9, Infinity, 12, 0, 5, 1, 'No capacity.'],
    [9, 0, Infinity, 0, 5, 1, 'No capacity.'],
    [9, 0, 12, Infinity, 5, 1, 'No capacity.'],
    [9, 0, 12, 0, Infinity, 1, 'No capacity.'],
    [9, 0, 12, 0, 5, Infinity, 'No capacity.'],
  ])(
    'calculates correctly: %i:%i - %i:%i, %i, %i',
    (
      startTimeHour,
      startTimeMinute,
      endTimeHour,
      endTimeMinute,
      slotLength,
      capacity,
      expectedResult,
    ) => {
      const result = calculateCapacity({
        startTime: { hour: startTimeHour, minute: startTimeMinute },
        endTime: { hour: endTimeHour, minute: endTimeMinute },
        slotLength,
        capacity,
      });

      expect(result).toBe(expectedResult);
    },
  );
});
