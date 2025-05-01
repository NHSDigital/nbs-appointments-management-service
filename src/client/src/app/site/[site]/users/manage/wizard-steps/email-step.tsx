'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';
import { SetUserRolesFormValues } from '../set-user-roles-wizard';
import { useRouter } from 'next/navigation';
import { fetchUsers, proposeNewUser } from '@services/appointmentsService';
import { Site } from '@types';

type EmailStepProps = {
  site: Site;
};

const NamesStep = ({
  goToNextStep,
  setCurrentStep,
  returnRouteUponCancellation,
  goToPreviousStep,
  site,
}: InjectedWizardProps & EmailStepProps) => {
  const router = useRouter();
  const { formState, trigger, register, watch, setError, setValue } =
    useFormContext<SetUserRolesFormValues>();
  const { errors } = formState;

  const emailWatch = watch('email');

  const onContinue = async () => {
    const stepIsValid = await trigger('email', { shouldFocus: true });
    if (!stepIsValid) {
      return;
    }

    const proposedUser = await proposeNewUser(site.id, emailWatch);
    setValue('userIdentityStatus', proposedUser);

    if (proposedUser.extantInMya) {
      const currentRoles =
        (await fetchUsers(site.id))
          .find(user => user.id === emailWatch)
          ?.roleAssignments.map(roleAssignment => roleAssignment.role) ?? [];
      setValue('roleIds', currentRoles);
      setCurrentStep(3);

      return;
    }

    if (!proposedUser.meetsWhitelistRequirements) {
      setError('email', { message: 'Enter a valid email address' });
      return;
    }

    goToNextStep();
  };

  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Go back"
      />
      <NhsHeading title="Enter name" />

      <FormGroup error={errors?.email?.message}>
        <TextInput
          id="email"
          label="Enter email address"
          {...register('email')}
        />
      </FormGroup>

      <ButtonGroup>
        <Button
          type="button"
          onClick={async () => {
            await onContinue();
          }}
        >
          Continue
        </Button>
        <Button
          type="button"
          styleType="secondary"
          onClick={async () => {
            router.push(returnRouteUponCancellation);
          }}
        >
          Cancel
        </Button>
      </ButtonGroup>
    </>
  );
};

export default NamesStep;
