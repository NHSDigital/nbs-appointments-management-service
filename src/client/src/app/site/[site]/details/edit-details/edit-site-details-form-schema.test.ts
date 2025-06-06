import { ValidationError } from 'yup';
import {
  editSiteDetailsFormSchema,
  EditSiteDetailsFormValues,
} from './edit-site-details-form-schema';

const validFormValues: EditSiteDetailsFormValues = {
  name: 'Alderney Road Community Pharmacy',
  address: '67 Alderney Road, New Pudsey, LS28 7QH',
  phoneNumber: '01234 567890',
  latitude: 53.789,
  longitude: -1.234,
};

const getValidationErrorMessageOrTrue = async (
  formValues: EditSiteDetailsFormValues,
): Promise<string | true> => {
  try {
    await editSiteDetailsFormSchema.validate(formValues);
    return true;
  } catch (error) {
    if (error instanceof ValidationError) {
      return error.message;
    } else {
      throw error;
    }
  }
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
      const result = await getValidationErrorMessageOrTrue({
        ...validFormValues,
        name,
      } as EditSiteDetailsFormValues);

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
      const result = await getValidationErrorMessageOrTrue({
        ...validFormValues,
        address,
      } as EditSiteDetailsFormValues);

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
      const result = await getValidationErrorMessageOrTrue({
        ...validFormValues,
        phoneNumber,
      } as EditSiteDetailsFormValues);

      expect(result).toBe(expectedErrorOrTrue);
    },
  );

  it.each([
    [undefined, 'Enter a latitude'],
    [-10, true],
    [10, true],
    [-49.8, true],
    [-49.9, 'Enter a valid latitude'],
    [60.9, true],
    [61.0, 'Enter a valid latitude'],
  ])(
    'validates the site latitude',
    async (
      latitude: number | undefined,
      expectedErrorOrTrue: string | boolean,
    ) => {
      const result = await getValidationErrorMessageOrTrue({
        ...validFormValues,
        latitude,
      } as EditSiteDetailsFormValues);

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
      const result = await getValidationErrorMessageOrTrue({
        ...validFormValues,
        longitude,
      } as EditSiteDetailsFormValues);

      expect(result).toBe(expectedErrorOrTrue);
    },
  );
});
