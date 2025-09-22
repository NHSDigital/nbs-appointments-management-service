'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { Site, Role, UserProfile, User } from '@types';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import SummaryStep from './wizard-steps/summary-step';
import { useRouter } from 'next/navigation';
import NamesStep from './wizard-steps/names-step';
import SetRolesStep from './wizard-steps/set-roles-step';
import EmailStep from './wizard-steps/email-step';
import {
  setUserRolesFormSchema,
  SetUserRolesFormValues,
} from './set-user-roles-form';
import { useTransition } from 'react';
import fromServer from '@server/fromServer';

type Props = {
  site: Site;
  roleOptions: Role[];
  sessionUser: UserProfile;
  userToEdit?: User;
  oktaEnabled: boolean;
};

const SetUserRolesWizard = ({
  site,
  roleOptions,
  sessionUser,
  userToEdit,
  oktaEnabled,
}: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const methods = useForm<SetUserRolesFormValues>({
    defaultValues: {
      email: userToEdit?.id ?? '',
      roleIds: userToEdit?.roleAssignments.map(role => role.role) ?? [],
      userIdentityStatus: userToEdit
        ? {
            extantInSite: true,
            extantInIdentityProvider: true,
            identityProvider: 'NhsMail',
            meetsWhitelistRequirements: true,
          }
        : undefined,
    },
    resolver: yupResolver(setUserRolesFormSchema),
  });
  const router = useRouter();

  const userIdentityStatus = methods.watch('userIdentityStatus');
  const isCreatingNewOktaUser =
    userIdentityStatus?.identityProvider === 'Okta' &&
    userIdentityStatus?.extantInIdentityProvider === false;

  const submitForm: SubmitHandler<SetUserRolesFormValues> = async form => {
    startTransition(async () => {
      await fromServer(
        saveUserRoleAssignments(
          site.id,
          form.email,
          form.firstName ?? '',
          form.lastName ?? '',
          form.roleIds,
        ),
      );

      router.push(`/site/${site.id}/users`);
    });
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
          pendingSubmit={pendingSubmit}
        >
          {userToEdit === undefined && (
            <WizardStep>
              {stepProps => (
                <EmailStep
                  {...stepProps}
                  site={site}
                  sessionUser={sessionUser}
                  oktaEnabled={oktaEnabled}
                />
              )}
            </WizardStep>
          )}

          {userToEdit === undefined && isCreatingNewOktaUser && (
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
