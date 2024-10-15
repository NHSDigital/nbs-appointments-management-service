'use client';
import { BackLinkClient } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
// type Props = { userProfile: UserProfile } & InjectedWizardProps;

const SingleOrRepeatingSessionStep = ({
  goToPreviousStep,
}: InjectedWizardProps) => {
  return (
    <>
      <BackLinkClient onClick={goToPreviousStep} />
      <h1 className="app-page-heading">
        <span className="nhsuk-caption-l">Create availability period</span>
        What type of session do you want to create?
      </h1>
      {/* TODO: This step is to be completed during ticket 240 https://nhsd-jira.digital.nhs.uk/browse/APPT-240
      It exists here for now because the acceptance criteria of 231 involves "progressing" to the next step of the wizard*/}
    </>
  );
};

export default SingleOrRepeatingSessionStep;
