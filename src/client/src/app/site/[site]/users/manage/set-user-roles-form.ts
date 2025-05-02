import * as yup from 'yup';
import { UserIdentityStatus, IdentityProvider } from '@types';

export type SetUserRolesFormValues = {
  email: string;
  roleIds: string[];
  firstName?: string;
  lastName?: string;
  userIdentityStatus: UserIdentityStatus;
};

export const setUserRolesFormSchema: yup.ObjectSchema<SetUserRolesFormValues> =
  yup
    .object({
      email: yup
        .string()
        .required('Enter a valid email address')
        .trim()
        .lowercase()
        .email('Enter a valid email address'),
      roleIds: yup
        .array()
        .required()
        .min(1, 'You have not selected any roles for this user'),
      firstName: yup
        .string()
        .when(
          [
            'userIdentityStatus.identityProvider',
            'userIdentityStatus.extantInIdentityProvider',
          ],
          {
            is: (
              identityProvider: IdentityProvider,
              extantInIdentityProvider: boolean,
            ) =>
              identityProvider === 'Okta' && extantInIdentityProvider === false,
            then: schema =>
              schema
                .required('Enter a first name')
                .max(50, 'First name cannot exceed 50 characters'),
            otherwise: schema => schema.notRequired(),
          },
        ),
      lastName: yup
        .string()
        .when(
          [
            'userIdentityStatus.identityProvider',
            'userIdentityStatus.extantInIdentityProvider',
          ],
          {
            is: (
              identityProvider: IdentityProvider,
              extantInIdentityProvider: boolean,
            ) =>
              identityProvider === 'Okta' && extantInIdentityProvider === false,
            then: schema =>
              schema
                .required('Enter a last name')
                .max(50, 'Last name cannot exceed 50 characters'),
            otherwise: schema => schema.notRequired(),
          },
        ),
      userIdentityStatus: yup
        .object({
          identityProvider: yup
            .mixed<IdentityProvider>()
            .oneOf(['Okta', 'NhsMail'])
            .required(),
          extantInIdentityProvider: yup.boolean().required(),
          extantInMya: yup.boolean().required(),
          meetsWhitelistRequirements: yup.boolean().required(),
        })
        .required(),
    })
    .required();
