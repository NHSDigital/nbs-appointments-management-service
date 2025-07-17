import {
  editSiteDetailsFormSchema,
  EditSiteDetailsFormValues,
} from './edit-site-details-form-schema';
import { getValidationErrorMessageOrTrue } from '@testing/form-schema-test-utils';

const validFormValues: EditSiteDetailsFormValues = {
  name: 'Alderney Road Community Pharmacy',
  address: '67 Alderney Road, New Pudsey, LS28 7QH',
  phoneNumber: '01234 567890',
  latitude: 53.789,
  longitude: -1.234,
};

describe('Set User Roles Form', () => {
  it('validates the schema', () => {
    expect(editSiteDetailsFormSchema.isValidSync(validFormValues)).toBe(true);
  });

  it.each([
    ['', 'Enter a name'],
    [undefined, 'Enter a name'],
    ['Alderney Road Community Pharmacy', true],
    ['A', true],
    ['     ', 'Enter a name'],
  ])(
    'validates the site name',
    async (name: string | undefined, expectedErrorOrTrue: string | boolean) => {
      const result = await getValidationErrorMessageOrTrue(
        editSiteDetailsFormSchema,
        {
          ...validFormValues,
          name,
        },
      );

      expect(result).toBe(expectedErrorOrTrue);
    },
  );

  it.each([
    ['', 'Enter an address'],
    [undefined, 'Enter an address'],
    ['67 Alderney Road, New Pudsey, LS28 7QH', true],
    ['A', true],
    ['     ', 'Enter an address'],
  ])(
    'validates the site address',
    async (
      address: string | undefined,
      expectedErrorOrTrue: string | boolean,
    ) => {
      const result = await getValidationErrorMessageOrTrue(
        editSiteDetailsFormSchema,
        {
          ...validFormValues,
          address,
        },
      );

      expect(result).toBe(expectedErrorOrTrue);
    },
  );

  it.each([
    ['', true],
    [undefined, true],
    ['01234 567890', true],
    ['123 456 789 0', true],
    ['     ', true],
    ['abc', 'Enter a valid phone number'],
    ['123v567', 'Enter a valid phone number'],
    ['e', 'Enter a valid phone number'],
    ['    1 2 3 4 5 6 7 8 9 0   ', true],
  ])(
    'validates the site phone number',
    async (
      phoneNumber: string | undefined,
      expectedErrorOrTrue: string | boolean,
    ) => {
      const result = await getValidationErrorMessageOrTrue(
        editSiteDetailsFormSchema,
        {
          ...validFormValues,
          phoneNumber,
        },
      );

      expect(result).toBe(expectedErrorOrTrue);
    },
  );

  it.each([
    [undefined, 'Enter a latitude'],
    [-5, 'Enter a valid latitude'],
    [10.1, 'Enter a valid latitude'],
    [49.7, 'Enter a valid latitude'],
    [49.8, true],
    [54.1, true],
    [60, true],
    [60.9, true],
    [61.0, 'Enter a valid latitude'],
    [65.3, 'Enter a valid latitude'],
  ])(
    'validates the site latitude',
    async (
      latitude: number | undefined,
      expectedErrorOrTrue: string | boolean,
    ) => {
      const result = await getValidationErrorMessageOrTrue(
        editSiteDetailsFormSchema,
        {
          ...validFormValues,
          latitude,
        },
      );

      expect(result).toBe(expectedErrorOrTrue);
    },
  );

  it.each([
    [undefined, 'Enter a longitude'],
    [-1, true],
    [1, true],
    [-8.1, true],
    [-8.2, 'Enter a valid longitude'],
    [1.8, true],
    [1.9, 'Enter a valid longitude'],
  ])(
    'validates the site longitude',
    async (
      longitude: number | undefined,
      expectedErrorOrTrue: string | boolean,
    ) => {
      const result = await getValidationErrorMessageOrTrue(
        editSiteDetailsFormSchema,
        {
          ...validFormValues,
          longitude,
        },
      );

      expect(result).toBe(expectedErrorOrTrue);
    },
  );
});
