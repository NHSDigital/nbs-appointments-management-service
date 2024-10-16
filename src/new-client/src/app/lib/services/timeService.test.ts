import { isValidDate } from '@services/timeService';

describe('Time Service', () => {
  it('foo', () => {
    const foobar = isValidDate(31, 1, 2020);

    expect(foobar).toEqual(false);
  });

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
    'can parse date components: day %p, month %p, year %p should be: %p',
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
});
