'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  FormGroup,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import { SetUserRolesFormValues } from '../set-user-roles-form';
import { useRouter } from 'next/navigation';
import { Heading } from 'nhsuk-react-components';
import { TextInput } from 'nhsuk-react-components';

const NamesStep = ({
  goToNextStep,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const router = useRouter();

  const { formState, trigger, register } =
    useFormContext<SetUserRolesFormValues>();
  const { errors } = formState;

  const onContinue = async () => {
    const formIsValid = await trigger(['firstName', 'lastName']);
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
      <Heading>Enter name</Heading>

      <FormGroup error={errors.firstName?.message}>
        <TextInput
          {...register('firstName')}
          label="First name"
          id="first-name"
        />
      </FormGroup>

      <FormGroup error={errors.lastName?.message}>
        <TextInput {...register('lastName')} label="Last name" id="last-name" />
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
