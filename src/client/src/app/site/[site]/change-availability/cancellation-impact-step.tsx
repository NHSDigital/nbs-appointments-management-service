'use client';
import { BackLink, Button, ButtonGroup } from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';

interface Props {
  cancelADateRangeWithBookingsEnabled: boolean;
  site: string;
}

const CancellationImpactStep = ({
  cancelADateRangeWithBookingsEnabled,
  site,
  goToPreviousStep,
}: InjectedWizardProps & Props) => {
  const router = useRouter();

  const renderCannotCancel = () => (
    <>
      <NhsHeading title="You cannot cancel these sessions" />
      <p>
        There are existing bookings for these sessions. You should first cancel
        the bookings, then return to change availability.
      </p>
      <ButtonGroup>
        <Button
          onClick={() => {
            router.push(`/site/${site}/view-availability`);
          }}
        >
          Return to view availability
        </Button>
        <Button styleType="secondary" onClick={goToPreviousStep}>
          Select different dates
        </Button>
      </ButtonGroup>
    </>
  );

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          {!cancelADateRangeWithBookingsEnabled && renderCannotCancel()}
        </div>
      </div>
    </>
  );
};

export default CancellationImpactStep;
