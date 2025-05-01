'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useFormContext } from 'react-hook-form';
import { SetUserRolesFormValues } from '../set-user-roles-wizard';
import { sortRolesByName } from '@sorting';
import { useRouter } from 'next/navigation';
import { Role, Site } from '@types';

type SummaryStepProps = {
  nameRequired: boolean;
  site: Site;
  roleOptions: Role[];
};

const SummaryStep = ({
  setCurrentStep,
  goToPreviousStep,
  site,
  nameRequired,
  roleOptions,
}: InjectedWizardProps & SummaryStepProps) => {
  const router = useRouter();
  const {
    getValues,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useFormContext<SetUserRolesFormValues>();

  const { email, roleIds, firstName, lastName } = getValues();

  const rolesSummary = roleOptions
    .filter(roleOption => roleIds.some(roleId => roleId === roleOption.id))
    .toSorted(sortRolesByName)
    .map(x => x.displayName)
    .join(', ');

  const nameSummary: SummaryListItem[] = [
    {
      title: 'Name',
      value: `${firstName} ${lastName}`,
      action: {
        renderingStrategy: 'client',
        text: 'Change',
        onClick: () => {
          setCurrentStep(2);
        },
      },
    },
  ];

  const userDetails: SummaryListItem[] = [
    {
      title: 'Email address',
      value: email,
      action: {
        renderingStrategy: 'client',
        text: 'Change',
        onClick: () => {
          router.push(`/site/${site.id}/users/propose`);
        },
      },
    },
    {
      title: 'Roles',
      value: rolesSummary,
      action: {
        renderingStrategy: 'client',
        text: 'Change',
        onClick: () => {
          setCurrentStep(1);
        },
      },
    },
  ];

  const summaryItems: SummaryListItem[] = [
    ...(nameRequired ? nameSummary : []),
    ...userDetails,
  ];

  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Go back"
      />
      <NhsHeading title={'Check user details'} />
      <SummaryList items={summaryItems}></SummaryList>

      <p>{`${email} will be sent information about how to login.`}</p>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <Button type="submit">Confirm</Button>
      )}
    </>
  );
};

export default SummaryStep;
