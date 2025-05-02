'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { yupResolver } from '@hookform/resolvers/yup';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import {
  Site,
  Role,
  UserIdentityStatus,
  UserProfile,
  IdentityProvider,
} from '@types';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import SummaryStep from './wizard-steps/summary-step';
import { useRouter } from 'next/navigation';
import NamesStep from './wizard-steps/names-step';
import SetRolesStep from './wizard-steps/set-roles-step';
import EmailStep from './wizard-steps/email-step';

export type SetUserRolesFormValues = {
  email: string;
  roleIds: string[];
  firstName?: string;
  lastName?: string;
  userIdentityStatus: UserIdentityStatus;
};

const userIdentityStatusSchema: yup.ObjectSchema<UserIdentityStatus> = yup
  .object({
    identityProvider: yup
      .mixed<IdentityProvider>()
      .oneOf(['Okta', 'NhsMail'])
      .required(),
    extantInIdentityProvider: yup.boolean().required(),
    extantInMya: yup.boolean().required(),
    meetsWhitelistRequirements: yup.boolean().required(),
  })
  .required();

export const setUserRolesFormSchema: yup.ObjectSchema<SetUserRolesFormValues> =
  yup
    .object({
      email: yup
        .string()
        .required('Enter a valid email address')
        .trim()
        .lowercase()
        .email('Enter a valid email address'),
      roleIds: yup.array().required(),
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
              identityProvider === 'Okta' && extantInIdentityProvider === true,
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
              identityProvider === 'Okta' && extantInIdentityProvider === true,
            then: schema =>
              schema
                .required('Enter a last name')
                .max(50, 'Last name cannot exceed 50 characters'),
            otherwise: schema => schema.notRequired(),
          },
        ),
      userIdentityStatus: userIdentityStatusSchema,
    })
    .required();

type Props = {
  site: Site;
  roleOptions: Role[];
  sessionUser: UserProfile;
};

const SetUserRolesWizard = ({ site, roleOptions, sessionUser }: Props) => {
  const methods = useForm<SetUserRolesFormValues>({
    resolver: yupResolver(setUserRolesFormSchema),
  });
  const router = useRouter();

  const userIdentityStatus = methods.watch('userIdentityStatus');
  const isCreatingNewOktaUser =
    userIdentityStatus?.identityProvider === 'Okta' &&
    userIdentityStatus?.extantInIdentityProvider === false;

  const submitForm: SubmitHandler<SetUserRolesFormValues> = async form => {
    await saveUserRoleAssignments(
      site.id,
      form.email,
      form.firstName ?? '',
      form.lastName ?? '',
      form.roleIds,
    );

    router.push(`/site/${site.id}/users`);
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="set-user-roles-wizard"
          initialStep={1}
          returnRouteUponCancellation={`/site/${site.id}/users`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
        >
          <WizardStep>
            {stepProps => (
              <EmailStep {...stepProps} site={site} sessionUser={sessionUser} />
            )}
          </WizardStep>
          {isCreatingNewOktaUser && (
            <WizardStep>{stepProps => <NamesStep {...stepProps} />}</WizardStep>
          )}
          <WizardStep>
            {stepProps => (
              <SetRolesStep {...stepProps} roleOptions={roleOptions} />
            )}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <SummaryStep {...stepProps} roleOptions={roleOptions} />
            )}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default SetUserRolesWizard;
