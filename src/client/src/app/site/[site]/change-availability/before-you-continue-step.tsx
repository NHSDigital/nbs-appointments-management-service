'use client';
import NhsHeading from '@components/nhs-heading';
import { BackLink, Button, ButtonGroup } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';

interface Props {
  cancelADateRangeWithBookingsEnabled: boolean;
}

const BeforeYouContinueStep = ({
  cancelADateRangeWithBookingsEnabled,
  goToNextStep,
}: InjectedWizardProps & Props) => {
  const router = useRouter();
  const onContinue = (e: React.FormEvent) => {
    e.preventDefault();
    goToNextStep();
  };

  return (
    <>
      <BackLink
        onClick={() => router.back()}
        renderingStrategy="client"
        text="Back"
      />

      <NhsHeading title="Before you continue" />

      <p>
        You cannot edit sessions after you create them. To change a session, you
        must:
      </p>

      <ol className="nhsuk-list nhsuk-list--number">
        <li>Cancel the sessions you want to change</li>

        {cancelADateRangeWithBookingsEnabled && (
          <li>Choose to keep existing bookings</li>
        )}

        <li>Create new sessions with the updated details</li>
      </ol>

      <ButtonGroup>
        <Button type="button" onClick={onContinue}>
          Continue to cancel
        </Button>
      </ButtonGroup>
    </>
  );
};

export default BeforeYouContinueStep;
