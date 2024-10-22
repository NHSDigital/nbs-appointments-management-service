'use client';
import NhsHeading from '@components/nhs-heading';
import { BackLink, Button } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
// type Props = { userProfile: UserProfile } & InjectedWizardProps;

const SingleOrRepeatingSessionStep = ({
  goToPreviousStep,
  goToNextStep,
}: InjectedWizardProps) => {
  const onContinue = async () => {
    goToNextStep();
  };

  return (
    <>
      <BackLink onClick={goToPreviousStep} href="" />
      <NhsHeading
        title="What type of session do you want to create?"
        caption="Create availability period"
      />
      {/* TODO: This step is to be completed during ticket 240 https://nhsd-jira.digital.nhs.uk/browse/APPT-240
      It exists here for now because the acceptance criteria of 231 involves "progressing" to the next step of the wizard*/}
      <Button
        type="button"
        onClick={async () => {
          await onContinue();
        }}
      >
        Continue
      </Button>
    </>
  );
};

export default SingleOrRepeatingSessionStep;
