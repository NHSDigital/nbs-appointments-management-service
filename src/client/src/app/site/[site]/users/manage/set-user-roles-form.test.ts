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
    extantInMya: false,
    meetsWhitelistRequirements: true,
  },
};

describe('Set User Roles Form', () => {
  it('validates the schema', () => {
    expect(setUserRolesFormSchema.isValidSync(validFormValues)).toBe(true);
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
});
