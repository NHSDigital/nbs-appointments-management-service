'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  CheckBox,
  CheckBoxes,
  FormGroup,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';
import { Role } from '@types';
import { sortRolesByName } from '@sorting';
import { useRouter } from 'next/navigation';
import { SetUserRolesFormValues } from '../set-user-roles-form';

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
      <NhsHeading title="Additional details" />

      <h2>Email</h2>
      <p>{getValues('email')}</p>

      <FormGroup error={errors.roleIds?.message} legend="Roles">
        <CheckBoxes>
          {roleOptions.toSorted(sortRolesByName).map(r => (
            <CheckBox
              id={r.id}
              label={r.displayName}
              hint={r.description}
              key={`checkbox-key-${r.id}`}
              value={r.id}
              {...register('roleIds')}
            />
          ))}
        </CheckBoxes>
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

export default SetRolesStep;
