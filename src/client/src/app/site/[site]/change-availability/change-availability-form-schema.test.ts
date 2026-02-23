import { createChangeAvailabilityFormSchema } from './change-availability-form-schema';
import { parseToUkDatetime, ukNow, DayJsType } from '@services/timeService';

jest.mock('@services/timeService', () => {
  const original = jest.requireActual('@services/timeService');
  return {
    ...original,
    ukNow: jest.fn(),
  };
});

const mockUkNow = ukNow as jest.Mock<DayJsType>;

describe('changeAvailabilityFormSchema', () => {
  const schema = createChangeAvailabilityFormSchema(90);

  beforeEach(() => {
    mockUkNow.mockReturnValue(parseToUkDatetime('2026-02-23'));
    jest.clearAllMocks();
  });

  describe('Field-level Validation', () => {
    it('fails if dates are empty', async () => {
      const data = {
        startDate: { day: '', month: '', year: '' },
        endDate: { day: '', month: '', year: '' },
      };

      try {
        await schema.validate(data, { abortEarly: false });
        throw new Error('Schema should have thrown');
      } catch (err: any) {
        expect(err.errors).toContain('Enter a start date');
        expect(err.errors).toContain('Enter an end date');
      }
    });

    it('fails if dates are not in the future', async () => {
      const data = {
        startDate: { day: '23', month: '02', year: '2026' },
        endDate: { day: '24', month: '02', year: '2026' },
      };

      await expect(schema.validate(data)).rejects.toThrow(
        'Start date must be in the future',
      );
    });

    it('fails if the date is invalid (e.g., Feb 30th)', async () => {
      const data = {
        startDate: { day: '30', month: '02', year: '2027' },
        endDate: { day: '01', month: '03', year: '2027' },
      };

      await expect(schema.validate(data)).rejects.toThrow(
        'Enter a valid start date',
      );
    });
  });

  describe('Range Logic', () => {
    it('fails if end date is before start date', async () => {
      const data = {
        startDate: { day: '10', month: '03', year: '2026' },
        endDate: { day: '09', month: '03', year: '2026' },
      };

      await expect(schema.validate(data)).rejects.toThrow(
        'End date must be on or after the start date',
      );
    });

    it('passes if start and end date are the same', async () => {
      const data = {
        startDate: { day: '10', month: '03', year: '2026' },
        endDate: { day: '10', month: '03', year: '2026' },
      };

      const result = await schema.validate(data);
      expect(result).toEqual(data);
    });
  });

  describe('Max Days Logic', () => {
    it('fails if range is exactly 91 days (1 day over limit)', async () => {
      const data = {
        startDate: { day: '01', month: '03', year: '2026' },
        endDate: { day: '30', month: '05', year: '2026' },
      };

      try {
        await schema.validate(data, { abortEarly: false });
        throw new Error('Schema should have thrown');
      } catch (err: any) {
        expect(err.errors).toContain(
          'Start date must be within 90 days of the end date',
        );
        expect(err.errors).toContain(
          'End date must be within 90 days of the start date',
        );
      }
    });

    it('passes if range is exactly 90 days', async () => {
      const data = {
        startDate: { day: '01', month: '03', year: '2026' },
        endDate: { day: '29', month: '05', year: '2026' },
      };

      const result = await schema.validate(data);
      expect(result).toBeDefined();
    });

    it('respects a custom maxDays configuration', async () => {
      const strictSchema = createChangeAvailabilityFormSchema(10);
      const data = {
        startDate: { day: '01', month: '03', year: '2026' },
        endDate: { day: '12', month: '03', year: '2026' },
      };

      try {
        await strictSchema.validate(data, { abortEarly: false });
        throw new Error('Schema should have thrown');
      } catch (err: any) {
        expect(err.errors).toContain(
          'End date must be within 10 days of the start date',
        );
        expect(err.errors).toContain(
          'Start date must be within 10 days of the end date',
        );
      }
    });
  });
});
