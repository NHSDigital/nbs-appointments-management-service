import NhsHeading from '@components/nhs-heading';
import { BackLink, Button, FormGroup } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

type ConfirmCancelChoiceStepProps = {
  siteName: string;
  siteId: string;
  date: string;
};

export const ConfirmCancelChoiceStep = ({
  setCurrentStep,
  goToPreviousStep,
  siteName,
  siteId,
  date,
}: InjectedWizardProps & ConfirmCancelChoiceStepProps) => {
  const router = useRouter();

  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Back"
      />
      <NhsHeading title={`Cancel ${date}`} caption={siteName} />

      <FormGroup legend="Are you sure you want to cancel this day?" error="">
        <form>
          <Button type="submit" styleType="warning">
            Cancel day
          </Button>
          <Link
            href={`/site/${siteId}/view-availability/week?date=${date}`}
            className="nhsuk-link"
          >
            No, go back
          </Link>
        </form>
      </FormGroup>
    </>
  );
};
