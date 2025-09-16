import {
  toTimeFormat,
  isFutureCalendarDateUk,
  isValidDate,
  parseDateComponentsToUkDatetime,
  parseToTimeComponents,
  toTwoDigitFormat,
  getUkWeeksOfTheMonth,
  parseToUkDatetime,
  addToUkDatetime,
  startOfUkWeek,
  endOfUkWeek,
  isEqual,
  dateTimeFormat,
  isOnTheSameUkDay,
  ukNow,
  DayJsType,
  RFC3339Format,
  occurInOrder,
  stringIsValidDate,
  isValidStartTime,
  getNearestAlignedTimes,
  parseDateAndTimeComponentsToUkDateTime,
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
    [16, 9, 2056, '2056-09-15T23:00:00.000Z'],
    [25, 10, 2025, '2025-10-24T23:00:00.000Z'],
  ])(
    'can parse dates components: day %p, month %p, year %p should be: %p',
    (day: number, month: number, year: number, expectedResult: string) => {
      const parsedDate = parseDateComponentsToUkDatetime({
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
      const formattedTime = toTimeFormat({
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
      const result = parseToTimeComponents(time);

      expect(result).toEqual(expectedResult);
    },
  );

  it('GetUKWeeksOfTheMonth changes timezone when crosses DST barrier', async () => {
    const dstChangeDateTime = '2025-10-01';
    const dateTime = parseToUkDatetime(dstChangeDateTime, RFC3339Format);
    const weeks = getUkWeeksOfTheMonth(dateTime);

    expect(weeks).toHaveLength(5);

    //grab time change week
    const dstChange1 = weeks[3];

    //bst date should be 1 hour behind UTC
    expect(dstChange1[6].toISOString()).toBe('2025-10-25T23:00:00.000Z');

    //grab time change after week
    const dstChange2 = weeks[4];

    //utc should be level after change
    expect(dstChange2[0].toISOString()).toBe('2025-10-27T00:00:00.000Z');
  });

  it('formats winter dateTime to time', async () => {
    const dateTime = '2024-12-12T12:05:00';
    const result = toTimeFormat(dateTime);

    expect(result).toEqual('12:05');
  });

  describe('startOfUkWeek', () => {
    it('calculates summertime date correctly', async () => {
      const dateTime = '2025-10-20';
      const result = startOfUkWeek(dateTime);

      const iso = result.toISOString();
      //check it is 1hour behind due to UTC (BST)
      expect(iso).toBe('2025-10-19T23:00:00.000Z');

      const offset = result.format('Z');
      expect(offset).toBe('+01:00');

      //check it returns BST hour and mins
      const time = result.format('HH:mm');
      expect(time).toBe('00:00');
    });

    it('calculates wintertime date correctly', async () => {
      const dateTime = '2025-02-04';
      const result = startOfUkWeek(dateTime);

      const iso = result.toISOString();
      //check it is same as UTC (no-BST)
      expect(iso).toBe('2025-02-03T00:00:00.000Z');

      const offset = result.format('Z');
      expect(offset).toBe('+00:00');

      //check it returns UTC hour and mins
      const time = result.format('HH:mm');
      expect(time).toBe('00:00');
    });

    it('calculates 20th October 2025 date correctly', async () => {
      const dateTime = parseToUkDatetime('2025-10-20');
      const startOfWeek = startOfUkWeek('2025-10-20');

      const isoString1 = dateTime.toISOString();
      const isoString2 = startOfWeek.toISOString();

      expect(isoString1).toBe(isoString2);
    });

    it('calculates 27th October 2025 date correctly', async () => {
      const dateTime = parseToUkDatetime('2025-10-27');
      const startOfWeek = startOfUkWeek('2025-10-27');

      const isoString1 = dateTime.toISOString();
      const isoString2 = startOfWeek.toISOString();

      expect(isoString1).toBe(isoString2);
    });
  });

  describe('endOfUkWeek', () => {
    it('calculates summertime date correctly', async () => {
      const dateTime = '2025-07-18';
      const result = endOfUkWeek(dateTime);

      const iso = result.toISOString();
      //check it is 1hour behind due to UTC (BST)
      expect(iso).toBe('2025-07-20T22:59:59.999Z');

      const offset = result.format('Z');
      expect(offset).toBe('+01:00');

      //check it returns BST hour and mins
      const time = result.format('HH:mm');
      expect(time).toBe('23:59');
    });

    it('calculates wintertime date correctly', async () => {
      const dateTime = '2025-02-04';
      const result = endOfUkWeek(dateTime);

      const iso = result.toISOString();
      //check it is same as UTC (no-BST)
      expect(iso).toBe('2025-02-09T23:59:59.999Z');

      const offset = result.format('Z');
      expect(offset).toBe('+00:00');

      //check it returns UTC hour and mins
      const time = result.format('HH:mm');
      expect(time).toBe('23:59');
    });

    it('calculates 20th October 2025 date correctly', async () => {
      const dateTime = parseToUkDatetime('2025-10-26').endOf('day');
      const endOfWeek = endOfUkWeek('2025-10-20');

      const isoString1 = dateTime.toISOString();
      const isoString2 = endOfWeek.toISOString();

      expect(isoString1).toBe(isoString2);
    });

    it('calculates 27th October 2025 date correctly', async () => {
      const dateTime = parseToUkDatetime('2025-11-02').endOf('day');
      const endOfWeek = endOfUkWeek('2025-10-27');

      const isoString1 = dateTime.toISOString();
      const isoString2 = endOfWeek.toISOString();

      expect(isoString1).toBe(isoString2);
    });
  });

  describe('addToUkDatetime', () => {
    it('preserves UK timezone - 26th add 0 days', async () => {
      const dateTime = parseToUkDatetime('2025-10-26');
      const result1 = addToUkDatetime(dateTime, 0, 'day');
      const isoString1 = result1.toISOString();
      expect(isoString1).toEqual('2025-10-25T23:00:00.000Z');
    });

    it('preserves UK timezone - 26th add 1 day', async () => {
      const dateTime = parseToUkDatetime('2025-10-26');
      const result1 = addToUkDatetime(dateTime, 1, 'day');
      const isoString1 = result1.toISOString();
      expect(isoString1).toEqual('2025-10-27T00:00:00.000Z');
    });

    it('preserves UK timezone - 27th add 0 days', async () => {
      const dateTime = parseToUkDatetime('2025-10-27');
      const startOfWeek = startOfUkWeek('2025-10-27');

      const result1 = addToUkDatetime(dateTime, 0, 'day');
      const result2 = addToUkDatetime(startOfWeek, 0, 'day');

      const isoString1 = result1.toISOString();
      const isoString2 = result2.toISOString();

      expect(isoString1).toEqual('2025-10-27T00:00:00.000Z');
      expect(isoString2).toEqual('2025-10-27T00:00:00.000Z');
    });

    it('preserves UK timezone - 27th add 1 day', async () => {
      const dateTime = parseToUkDatetime('2025-10-27');
      const startOfWeek = startOfUkWeek('2025-10-27');

      const result1 = addToUkDatetime(dateTime, 1, 'day');
      const result2 = addToUkDatetime(startOfWeek, 1, 'day');

      const isoString1 = result1.toISOString();
      const isoString2 = result2.toISOString();

      expect(isoString1).toEqual('2025-10-28T00:00:00.000Z');
      expect(isoString2).toEqual('2025-10-28T00:00:00.000Z');
    });

    it('crossing DST - Clocks Forward 1', async () => {
      const dateTime = parseToUkDatetime('2026-03-29');
      expect(dateTime.toISOString()).toEqual('2026-03-29T00:00:00.000Z');

      const result1 = addToUkDatetime(dateTime, 1, 'day');
      expect(result1.toISOString()).toEqual('2026-03-29T23:00:00.000Z');
    });

    it('crossing DST - Clocks Forward 2', async () => {
      const dateTime = parseToUkDatetime('2026-03-30');
      expect(dateTime.toISOString()).toEqual('2026-03-29T23:00:00.000Z');

      const result1 = addToUkDatetime(dateTime, -1, 'day');
      expect(result1.toISOString()).toEqual('2026-03-29T00:00:00.000Z');
    });

    it('crossing DST - Clocks Back 1', async () => {
      const dateTime = parseToUkDatetime('2025-10-26');
      expect(dateTime.toISOString()).toEqual('2025-10-25T23:00:00.000Z');

      const result1 = addToUkDatetime(dateTime, 1, 'day');
      expect(result1.toISOString()).toEqual('2025-10-27T00:00:00.000Z');
    });

    it('crossing DST - Clocks Back 2', async () => {
      const dateTime = parseToUkDatetime('2025-10-27');
      expect(dateTime.toISOString()).toEqual('2025-10-27T00:00:00.000Z');

      const result1 = addToUkDatetime(dateTime, -1, 'day');
      expect(result1.toISOString()).toEqual('2025-10-25T23:00:00.000Z');
    });

    it('preserves UK timezone 1', async () => {
      const dateTime = parseToUkDatetime('2025-10-20');
      const startOfWeek = startOfUkWeek('2025-10-20');

      const result1 = addToUkDatetime(dateTime, 0, 'day');
      const result2 = addToUkDatetime(startOfWeek, 0, 'day');

      expect(isEqual(result1, result2)).toBe(true);

      expect(result1.toISOString()).toEqual('2025-10-19T23:00:00.000Z');
      expect(result2.toISOString()).toEqual('2025-10-19T23:00:00.000Z');
    });

    it('preserves UK timezone 2', async () => {
      const dateTime = parseToUkDatetime('2025-10-20');
      const startOfWeek = startOfUkWeek('2025-10-20');

      const result1 = addToUkDatetime(dateTime, 1, 'day');
      const result2 = addToUkDatetime(startOfWeek, 1, 'day');

      expect(isEqual(result1, result2)).toBe(true);

      expect(result1.toISOString()).toEqual('2025-10-20T23:00:00.000Z');
      expect(result2.toISOString()).toEqual('2025-10-20T23:00:00.000Z');
    });
  });

  describe('isOnTheSameUkDay', () => {
    it('calculates 25th correctly', async () => {
      const bookingDate = parseToUkDatetime(
        '2025-10-25T10:00:00',
        dateTimeFormat,
      );
      const ukDate = parseToUkDatetime('2025-10-25');
      const sameDayCorrect1 = isOnTheSameUkDay(ukDate, bookingDate);
      const sameDayCorrect2 = isOnTheSameUkDay(bookingDate, ukDate);
      expect(sameDayCorrect1).toBe(true);
      expect(sameDayCorrect2).toBe(true);

      //these pass when not crossing a DST, showing that the isSame check works in SOME cases
      const sameDayWrong1 = ukDate.isSame(bookingDate, 'day');
      const sameDayWrong2 = bookingDate.isSame(ukDate, 'day');
      expect(sameDayWrong1).toBe(true);
      expect(sameDayWrong2).toBe(true);
    });

    it('calculates 26th correctly', async () => {
      const bookingDate = parseToUkDatetime(
        '2025-10-26T10:00:00',
        dateTimeFormat,
      );
      const ukDate = parseToUkDatetime('2025-10-26');
      const sameDayCorrect1 = isOnTheSameUkDay(ukDate, bookingDate);
      const sameDayCorrect2 = isOnTheSameUkDay(bookingDate, ukDate);
      expect(sameDayCorrect1).toBe(true);
      expect(sameDayCorrect2).toBe(true);

      //prove that when running these tests in a different TZ,the isSame check fails and therefore can't be used
      //i.e to see, run: TZ="Etc/GMT+12" npm test
      const sameDayWrong1 = ukDate.isSame(bookingDate, 'day');
      const sameDayWrong2 = bookingDate.isSame(ukDate, 'day');
      expect(sameDayWrong1).toBe(true);
      expect(sameDayWrong2).toBe(true);
    });

    it('calculates 27th correctly', async () => {
      const bookingDate = parseToUkDatetime(
        '2025-10-27T10:00:00',
        dateTimeFormat,
      );
      const ukDate = parseToUkDatetime('2025-10-27');
      const sameDayCorrect1 = isOnTheSameUkDay(ukDate, bookingDate);
      const sameDayCorrect2 = isOnTheSameUkDay(bookingDate, ukDate);
      expect(sameDayCorrect1).toBe(true);
      expect(sameDayCorrect2).toBe(true);

      //these pass when not crossing a DST, showing that the isSame check works in SOME cases
      const sameDayWrong1 = ukDate.isSame(bookingDate, 'day');
      const sameDayWrong2 = bookingDate.isSame(ukDate, 'day');
      expect(sameDayWrong1).toBe(true);
      expect(sameDayWrong2).toBe(true);
    });
  });

  it('formats summer dateTime to time', async () => {
    const dateTime = '2025-07-07T12:05:00';
    const result = toTimeFormat(dateTime);

    expect(result).toEqual('12:05');
  });

  it.each([
    [addToUkDatetime(ukNow(), 1, 'day'), true],
    [addToUkDatetime(ukNow(), -1, 'day'), false],
    [addToUkDatetime(ukNow(), 0, 'day'), false],
  ])(
    'check if date is in the future',
    (dateToCheck: DayJsType, expectedOutcome: boolean) => {
      const result = isFutureCalendarDateUk(dateToCheck);

      expect(result).toBe(expectedOutcome);
    },
  );

  describe('occurInOrder', () => {
    it.each([
      [
        [
          parseToUkDatetime('2024-01-01T12:00:00Z'),
          parseToUkDatetime('2024-01-02T12:00:00Z'),
          parseToUkDatetime('2024-01-03T12:00:00Z'),
        ],
        true,
      ],
      [
        [
          parseToUkDatetime('2024-01-01T03:00:00Z'),
          parseToUkDatetime('2024-01-01T07:00:00Z'),
          parseToUkDatetime('2024-01-01T23:59:00Z'),
        ],
        true,
      ],
      [
        [
          parseToUkDatetime('2024-01-03T12:00:00Z'),
          parseToUkDatetime('2024-01-02T12:00:00Z'),
          parseToUkDatetime('2024-01-01T12:00:00Z'),
        ],
        false,
      ],
      [
        [
          parseToUkDatetime('2024-01-01T12:00:00Z'),
          parseToUkDatetime('2024-01-03T12:00:00Z'),
          parseToUkDatetime('2024-01-02T12:00:00Z'),
        ],
        false,
      ],
      [
        [
          parseToUkDatetime('2024-01-01T12:00:00Z'),
          parseToUkDatetime('2024-01-01T12:00:00Z'),
          parseToUkDatetime('2024-01-02T12:00:00Z'),
        ],
        true,
      ],
      [
        [
          parseToUkDatetime('2024-01-02T12:00:00Z'),
          parseToUkDatetime('2024-01-02T12:00:00Z'),
          parseToUkDatetime('2024-01-01T12:00:00Z'),
        ],
        false,
      ],
      [
        [
          parseToUkDatetime('2024-01-01T12:00:00Z'),
          parseToUkDatetime('2024-01-01T12:00:00Z'),
          parseToUkDatetime('2024-01-01T12:00:00Z'),
        ],
        true,
      ],
      [[parseToUkDatetime('2024-01-01T12:00:00Z')], true],
      [[], true],
    ])(
      'check if date is in the future',
      (datesToCheck: DayJsType[], expectedOutcome: boolean) => {
        const result = occurInOrder(datesToCheck);

        expect(result).toBe(expectedOutcome);
      },
    );
  });

  it.each([
    ['2025-03-10', true],
    ['2025-05-25', true],
    ['invalid', false],
  ])(
    'checks if a string is a valid date',
    (dateStringToCheck: string, expectedOutcome: boolean) => {
      const result = stringIsValidDate(dateStringToCheck, RFC3339Format);

      expect(result).toBe(expectedOutcome);
    },
  );

  it.each([
    [
      parseToUkDatetime('2025-09-10T09:00:00Z', dateTimeFormat),
      parseToUkDatetime('2025-09-10T12:00:00Z', dateTimeFormat),
      10,
      true,
    ],
    [
      parseToUkDatetime('2025-09-10T09:00:00Z', dateTimeFormat),
      parseToUkDatetime('2025-09-10T10:00:00Z', dateTimeFormat),
      7,
      false,
    ],
  ])(
    'correctly determines if a start time is valid for the session using slot length',
    (
      sessionStart: DayJsType,
      sessionEnd: DayJsType,
      slotLength: number,
      expectedOutcome: boolean,
    ) => {
      const result = isValidStartTime(sessionStart, sessionEnd, slotLength);
      expect(result).toBe(expectedOutcome);
    },
  );

  it.each([
    [
      parseToUkDatetime('2025-09-10T09:06:00Z', dateTimeFormat),
      10,
      parseToUkDatetime('2025-09-10T09:00:00Z', dateTimeFormat),
      parseToUkDatetime('2025-09-10T09:10:00Z', dateTimeFormat),
    ],
    [
      parseToUkDatetime('2025-09-10T09:06:00Z', dateTimeFormat),
      5,
      parseToUkDatetime('2025-09-10T09:05:00Z', dateTimeFormat),
      parseToUkDatetime('2025-09-10T09:10:00Z', dateTimeFormat),
    ],
  ])(
    'correctly gets nearest aligned start times',
    (
      requested: DayJsType,
      slotLength: number,
      expectedFloor: DayJsType,
      expectedCeil: DayJsType,
    ) => {
      const [floor, ceil] = getNearestAlignedTimes(requested, slotLength);
      expect(floor.format('HH:mma')).toBe(expectedFloor.format('HH:mma'));
      expect(ceil.format('HH:mma')).toBe(expectedCeil.format('HH:mma'));
    },
  );

  it.each([
    ['2025-09-10', { hour: 9, minute: 0 }, '2025-09-10T08:00:00.000Z'],
    ['2025-10-08', { hour: 11, minute: 15 }, '2025-10-08T10:15:00.000Z'],
    ['2025-12-13', { hour: 11, minute: 15 }, '2025-12-13T11:15:00.000Z'],
  ])(
    'correctly parses date string and time components into UK date time object',
    (date: string, time: TimeComponents, expectedOutcome: string) => {
      const parsedDateTime = parseDateAndTimeComponentsToUkDateTime(date, time);
      const result = parsedDateTime.toISOString();

      expect(result).toBe(expectedOutcome);
    },
  );
});
