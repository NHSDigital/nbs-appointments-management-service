import {
  formatDateTimeToTime,
  formatTimeString,
  isValidDate,
  parseDateComponents,
  toTimeComponents,
  toTwoDigitFormat,
} from '@services/timeService';
import { TimeComponents } from '@types';

describe('Time Service', () => {
  it.each([
    [1, 12, 2020, true],
    ['1', '12', '2020', true],
    ['01', '09', '2020', true], // leading zeros
    [28, 3, 1937, true],
    [0, 1, 2000, false], // day under range
    [32, 1, 2000, false], // day over range
    [1, 0, 2000, false], // month under range
    [1, 13, 2000, false], // month over range
    [1, 1, 937, false], // year under range
    [1, 1, 10000, false], // day over range ?
    [29, 2, 2024, true], // 29th Feb, leap year
    [29, 2, 2023, false], // 29th Feb, not leap year
    [31, 11, 2023, false], // 31st Nov
  ])(
    'can validate date components: day %p, month %p, year %p should be: %p',
    (
      day: number | string,
      month: number | string,
      year: number | string,
      expectedResult: boolean,
    ) => {
      const result = isValidDate(day, month, year);

      expect(result).toEqual(expectedResult);
    },
  );

  it.each([
    [1, '01'],
    ['1', '01'],
    [0, '00'],
    [10, '10'],
    [42, '42'],
    [100, undefined],
    [-1, undefined],
    ['fp', undefined],
    ['e', undefined],
    ['01', '01'],
  ])(
    'can format numbers as double digits: %p should be %p',
    (input: string | number, expectedResult: string | undefined) => {
      const result = toTwoDigitFormat(input);

      expect(result).toBe(expectedResult);
    },
  );

  it.each([
    [1, 1, 2020, '2020-01-01T00:00:00.000Z'],
    [31, 12, 2018, '2018-12-31T00:00:00.000Z'],
    [29, 2, 2020, '2020-02-29T00:00:00.000Z'],
    [28, 2, 2021, '2021-02-28T00:00:00.000Z'],
    [16, 9, 2056, '2056-09-16T00:00:00.000Z'],
  ])(
    'can parse dates components: day %p, month %p, year %p should be: %p',
    (day: number, month: number, year: number, expectedResult: string) => {
      const parsedDate = parseDateComponents({
        day,
        month,
        year,
      });

      const result = parsedDate?.toISOString();
      expect(result).toBe(expectedResult);
    },
  );

  it.each([
    [1, 1, '01:01'],
    [13, 7, '13:07'],
    [0, 0, '00:00'],
    [23, 46, '23:46'],
    [-1, 7, undefined],
    [24, 7, undefined],
    [NaN, 7, undefined],
    [6, -1, undefined],
    [6, 65, undefined],
    [6, NaN, undefined],
  ])(
    'can format time components: hour %p, minute %p should be: %p',
    (hour: number, minute: number, expectedResult: string | undefined) => {
      const formattedTime = formatTimeString({
        hour,
        minute,
      });

      expect(formattedTime).toBe(expectedResult);
    },
  );

  it.each([
    ['09:00', { hour: 9, minute: 0 }],
    ['12:45', { hour: 12, minute: 45 }],
    ['2i:0o', undefined],
  ])(
    'parses a time string to time components',
    (time: string, expectedResult: TimeComponents | undefined) => {
      const result = toTimeComponents(time);

      expect(result).toEqual(expectedResult);
    },
  );

  it('formats dateTime to time', async () => {
    const dateTime = '2024-12-12T12:05:00';
    const result = formatDateTimeToTime(dateTime);

    expect(result).toEqual('12:05');
  });
});
