import { ValidationError } from 'yup';
import {
  setUserRolesFormSchema,
  SetUserRolesFormValues,
} from './set-user-roles-form';

const validFormValues: SetUserRolesFormValues = {
  email: 'new.user@nhs.net',
  firstName: '',
  lastName: '',
  roleIds: ['role-1', 'role-2'],
  userIdentityStatus: {
    identityProvider: 'NhsMail',
    extantInIdentityProvider: true,
    extantInSite: false,
    meetsWhitelistRequirements: true,
  },
};

describe('Set User Roles Form', () => {
  it('validates the schema', () => {
    expect(setUserRolesFormSchema.isValidSync(validFormValues)).toBe(true);
  });

  it('validates the schema when creating a new Okta user', () => {
    expect(
      setUserRolesFormSchema.isValidSync({
        ...validFormValues,
        firstName: 'Elizabeth',
        lastName: 'Kensington-Jones',
        userIdentityStatus: {
          identityProvider: 'Okta',
          extantInIdentityProvider: false,
          extantInSite: false,
          meetsWhitelistRequirements: true,
        },
      }),
    ).toBe(true);
  });

  it.each([
    ['', 'Enter a valid email address'],
    ['two', 'Enter a valid email address'],
    [undefined, 'Enter a valid email address'],
    ['frogs@@nhs.net', 'Enter a valid email address'],
  ])(
    'validates the email field',
    async (email: string | undefined, expectedValidationMessage: string) => {
      try {
        await setUserRolesFormSchema.validate({
          ...validFormValues,
          email,
        });

        fail('Expected a validation error but none was thrown');
      } catch (error) {
        if (error instanceof ValidationError) {
          expect(error.message).toBe(expectedValidationMessage);
        } else {
          throw error;
        }
      }
    },
  );

  it('does not validate name fields if the user is not creating a new Okta user', async () => {
    const formValues = {
      ...validFormValues,
      firstName: '',
      lastName: '',
      userIdentityStatus: {
        identityProvider: 'NhsMail',
        extantInIdentityProvider: true,
        extantInSite: false,
        meetsWhitelistRequirements: true,
      },
    };

    await expect(
      setUserRolesFormSchema.validate(formValues),
    ).resolves.not.toThrow();
    expect(setUserRolesFormSchema.isValidSync(formValues)).toBe(true);
  });

  it.each([
    ['', 'Enter a first name'],
    [undefined, 'Enter a first name'],
    [
      'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
      'First name cannot exceed 50 characters',
    ],
  ])(
    'validates first name if the user is creating a new Okta user',
    async (
      firstName: string | undefined,
      expectedValidationMessage: string,
    ) => {
      try {
        await setUserRolesFormSchema.validate({
          ...validFormValues,
          firstName,
          lastName: 'Kensington-Jones',
          userIdentityStatus: {
            identityProvider: 'Okta',
            extantInIdentityProvider: false,
            extantInSite: false,
            meetsWhitelistRequirements: true,
          },
        });

        fail('Expected a validation error but none was thrown');
      } catch (error) {
        if (error instanceof ValidationError) {
          expect(error.message).toBe(expectedValidationMessage);
        } else {
          throw error;
        }
      }
    },
  );

  it.each([
    ['', 'Enter a last name'],
    [undefined, 'Enter a last name'],
    [
      'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
      'Last name cannot exceed 50 characters',
    ],
  ])(
    'validates last name if the user is creating a new Okta user',
    async (lastName: string | undefined, expectedValidationMessage: string) => {
      try {
        await setUserRolesFormSchema.validate({
          ...validFormValues,
          firstName: 'Elizabeth',
          lastName: lastName,
          userIdentityStatus: {
            identityProvider: 'Okta',
            extantInIdentityProvider: false,
            extantInSite: false,
            meetsWhitelistRequirements: true,
          },
        });

        fail('Expected a validation error but none was thrown');
      } catch (error) {
        if (error instanceof ValidationError) {
          expect(error.message).toBe(expectedValidationMessage);
        } else {
          throw error;
        }
      }
    },
  );

  it('validates the roleIds field', async () => {
    try {
      await setUserRolesFormSchema.validate({
        ...validFormValues,
        roleIds: [],
      });

      fail('Expected a validation error but none was thrown');
    } catch (error) {
      if (error instanceof ValidationError) {
        expect(error.message).toBe(
          'You have not selected any roles for this user',
        );
      } else {
        throw error;
      }
    }
  });
});
