'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { yupResolver } from '@hookform/resolvers/yup';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { Site, Role, UserIdentityStatus, UserProfile } from '@types';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import SummaryStep from './wizard-steps/summary-step';
import { useRouter } from 'next/navigation';
import NamesStep from './wizard-steps/names-step';
import SetRolesStep from './wizard-steps/set-roles-step';
import EmailStep from './wizard-steps/email-step';

export type SetUserRolesFormValues = {
  email: string;
  roleIds: string[];
  firstName: string;
  lastName: string;
  userIdentityStatus?: UserIdentityStatus;
};

const schema = yup
  .object({
    email: yup
      .string()
      .required()
      .trim()
      .lowercase()
      .email('Enter a valid email address'),
    roleIds: yup.array().required(),
    firstName: yup.string().required(),
    lastName: yup.string().required(),
  })
  .required();

type Props = {
  site: Site;
  roleOptions: Role[];
  sessionUser: UserProfile;
};

const SetUserRolesWizard = ({ site, roleOptions, sessionUser }: Props) => {
  const methods = useForm<SetUserRolesFormValues>({
    resolver: yupResolver(schema),
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
      form.firstName,
      form.lastName,
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
            {stepProps => <EmailStep {...stepProps} site={site} />}
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
              <SummaryStep
                {...stepProps}
                site={site}
                roleOptions={roleOptions}
              />
            )}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default SetUserRolesWizard;
