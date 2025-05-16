'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';
import { SetUserRolesFormValues } from '../set-user-roles-form';
import { useRouter } from 'next/navigation';
import { fetchUsers, proposeNewUser } from '@services/appointmentsService';
import { Site, UserProfile } from '@types';

export type EmailStepProps = {
  site: Site;
  sessionUser: UserProfile;
};

const NamesStep = ({
  goToNextStep,
  returnRouteUponCancellation,
  goToPreviousStep,
  site,
  sessionUser,
}: InjectedWizardProps & EmailStepProps) => {
  const router = useRouter();
  const { formState, trigger, register, watch, setError, setValue } =
    useFormContext<SetUserRolesFormValues>();
  const { errors } = formState;

  const emailWatch = watch('email');

  const onContinue = async () => {
    const stepIsValid = await trigger('email', { shouldFocus: true });
    if (!stepIsValid) {
      return;
    }

    const sanitisedEmail = emailWatch.trim().toLowerCase();
    if (sessionUser.emailAddress === sanitisedEmail) {
      setError('email', { message: 'You may not edit your own roles' });
      return;
    }

    const proposedUser = await proposeNewUser(site.id, sanitisedEmail);
    setValue('userIdentityStatus', proposedUser);

    if (proposedUser.extantInSite) {
      const currentRoles =
        (await fetchUsers(site.id))
          .find(user => user.id === sanitisedEmail)
          ?.roleAssignments.map(roleAssignment => roleAssignment.role) ?? [];
      setValue('roleIds', currentRoles);
    }

    if (!proposedUser.meetsWhitelistRequirements) {
      setError('email', {
        message: 'Email address must be nhs.net or an authorised email domain',
      });
      return;
    }

    goToNextStep();
  };

  const authorisedDomainsUrl =
    'https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/care-identity-email-domain-allow-list';
  const userGuidanceUrl =
    'https://digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance/log-in-and-select-site';
  const emailDomainRequestUrl =
    'https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/request-an-addition-to-the-email-domain-allow-list';

  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Go back"
      />
      <NhsHeading title="Add a user" />

      <FormGroup error={errors?.email?.message}>
        <div className="nhsuk-hint">
          Email address must be nhs.net or on the list of{' '}
          <a href={authorisedDomainsUrl} target="_blank" rel="noreferrer">
            authorised email domains
          </a>
          . Read the{' '}
          <a href={userGuidanceUrl} target="_blank" rel="noreferrer">
            user guidance on logging in without an NHS.net account
          </a>{' '}
          or you can apply for their{' '}
          <a href={emailDomainRequestUrl} target="_blank" rel="noreferrer">
            email domain to be approved
          </a>
          .
        </div>
        <TextInput
          id="email"
          label="Enter email address"
          {...register('email')}
        />
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
