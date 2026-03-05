'use client';
import { BackLink, Button, ButtonGroup } from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';
import { useFormContext } from 'react-hook-form';
import {
  ChangeAvailabilityFormValues,
  ProposedCancellationSummary,
} from './change-availability-form-schema';

interface Props {
  cancelADateRangeWithBookingsEnabled: boolean;
  site: string;
}

const CancellationImpactStep = ({
  cancelADateRangeWithBookingsEnabled,
  site,
  goToPreviousStep,
  goToNextStep,
}: InjectedWizardProps & Props) => {
  const router = useRouter();

  const { watch } = useFormContext<ChangeAvailabilityFormValues>();
  const proposedSummary = watch(
    'proposedCancellationSummary',
  ) as ProposedCancellationSummary;

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

  const renderNoSessions = () => (
    <>
      <NhsHeading title="There are no sessions in this date range" />
      <p>You should choose a new date range.</p>
      <Button onClick={goToPreviousStep}>Choose a new date range.</Button>
    </>
  );
  const renderNoBookings = () => (
    <>
      <NhsHeading
        title={`You are about to cancel ${proposedSummary.sessionCount} ${proposedSummary.sessionCount > 1 ? 'sessions' : 'session'}`}
      />
      <p>
        There are no bookings for{' '}
        {proposedSummary.sessionCount > 1 ? 'these sessions' : 'this session'}
      </p>
      <Button onClick={goToNextStep}>Continue</Button>
    </>
  );

  const renderContent = () => {
    if (!cancelADateRangeWithBookingsEnabled) {
      return renderCannotCancel();
    }

    if (proposedSummary.sessionCount === 0) {
      return renderNoSessions();
    }

    if (proposedSummary.bookingCount === 0) {
      return renderNoBookings();
    }
  };

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">{renderContent()}</div>
      </div>
    </>
  );
};

export default CancellationImpactStep;
