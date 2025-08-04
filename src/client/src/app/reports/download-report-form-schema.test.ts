import { downloadReportFormSchema } from './download-report-form-schema';
import { getValidationErrorMessageOrTrue } from '@testing/form-schema-test-utils';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});
const mockUkNow = ukNow as jest.Mock<DayJsType>;

describe('Download Report Form Schema', () => {
  beforeEach(() => {
    mockUkNow.mockReturnValue(parseToUkDatetime('2025-10-01'));
  });
  it.each([
    ['2025-03-01', '2025-03-31', true],
    ['2025-03-01', '2025-03-01', true],
    [
      '2025-01-01',
      '2025-03-31',
      'Select a date on or after 1 March 2025 and within 3 months from today',
    ],
    [
      '2025-03-01',
      '2030-03-31',
      'Select a date on or after 1 March 2025 and within 3 months from today',
    ],
    [
      '2025-03-10',
      '2025-03-07',
      'End date must be equal to or after start date',
    ],
    ['2025-03-01', undefined, 'Enter an end date'],
    [undefined, '2025-03-31', 'Enter a start date'],
    [
      '2025-05-28',
      '2025-04-01',
      'End date must be equal to or after start date',
    ],
    ['2025-03-01', '2025-06-01', true],
    [
      '2025-03-01',
      '2026-02-01',
      'Select a date on or after 1 March 2025 and within 3 months from today',
    ],
  ])(
    'validates the schema',
    async (
      startDate: string | undefined,
      endDate: string | undefined,
      expectedErrorOrTrue: string | boolean,
    ) => {
      const result = await getValidationErrorMessageOrTrue(
        downloadReportFormSchema,
        {
          startDate,
          endDate,
        },
      );

      expect(result).toBe(expectedErrorOrTrue);
    },
  );
});
