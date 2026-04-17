'use client';
import { BackLink, ButtonGroup, FormGroup } from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import { Role } from '@types';
import { sortRolesByName } from '@sorting';
import { useRouter } from 'next/navigation';
import { SetUserRolesFormValues } from '../set-user-roles-form';
import { Heading, Button, Checkboxes } from 'nhsuk-react-components';

export type RolesStepProps = {
  roleOptions: Role[];
};

const SetRolesStep = ({
  goToNextStep,
  returnRouteUponCancellation,
  goToPreviousStep,
  roleOptions,
}: InjectedWizardProps & RolesStepProps) => {
  const router = useRouter();

  const { register, formState, trigger, getValues } =
    useFormContext<SetUserRolesFormValues>();
  const { errors } = formState;

  const onContinue = async () => {
    const formIsValid = await trigger(['roleIds']);
    if (!formIsValid) {
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
      <Heading>Additional details</Heading>

      <h2>Email</h2>
      <p>{getValues('email')}</p>

      <FormGroup error={errors.roleIds?.message} legend="Roles">
        <Checkboxes>
          {roleOptions.toSorted(sortRolesByName).map(r => (
            <Checkboxes.Item
              id={r.id}
              hint={r.description}
              key={`checkbox-key-${r.id}`}
              value={r.id}
              {...register('roleIds')}
            >
              {r.displayName}
            </Checkboxes.Item>
          ))}
        </Checkboxes>
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
          secondary
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

export default SetRolesStep;
