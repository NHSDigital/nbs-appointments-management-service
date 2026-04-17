'use client';
import { Heading, Button } from 'nhsuk-react-components';
import { BackLink, ButtonGroup } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';

interface Props {
  cancelADateRangeWithBookingsEnabled: boolean;
  previousUrl?: string;
}

const BeforeYouContinueStep = ({
  cancelADateRangeWithBookingsEnabled,
  goToNextStep,
  previousUrl,
}: InjectedWizardProps & Props) => {
  const router = useRouter();

  const handleBack = () => {
    if (previousUrl) {
      // Because currentViewPath above starts with /site/...,
      // router.push will treat it as relative to the application base path.
      router.push(previousUrl);
    } else {
      // Fallback for external traffic
      router.push('/sites');
    }
  };

  const onContinue = (e: React.FormEvent) => {
    e.preventDefault();
    goToNextStep();
  };

  return (
    <>
      <BackLink onClick={handleBack} renderingStrategy="client" text="Back" />

      <Heading headingLevel="h2">Before you continue</Heading>

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
