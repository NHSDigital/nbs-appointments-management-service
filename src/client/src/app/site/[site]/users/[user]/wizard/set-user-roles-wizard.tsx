'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { Site, Role } from '@types';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import SummaryStep from './summary-step';
import { useRouter } from 'next/navigation';
import NamesStep from './names-step';
import SetRolesStep from './set-roles-step';

export type SetUserRolesFormValues = {
  email: string;
  roleIds: string[];
  firstName: string;
  lastName: string;
};

type Props = {
  site: Site;
  roleOptions: Role[];
  email: string;
  nameRequired: boolean;
};

const SetUserRolesWizard = ({
  site,
  email,
  nameRequired,
  roleOptions,
}: Props) => {
  const methods = useForm<SetUserRolesFormValues>({
    defaultValues: {
      email,
      roleIds: [],
      firstName: undefined,
      lastName: undefined,
    },
  });
  const router = useRouter();

  const submitForm: SubmitHandler<SetUserRolesFormValues> = async (
    form: SetUserRolesFormValues,
  ) => {
    await saveUserRoleAssignments(
      site.id,
      email,
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
          {nameRequired && (
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
                nameRequired={nameRequired}
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
