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
  site: Site;
  roleOptions: Role[];
};

const SummaryStep = ({
  setCurrentStep,
  goToPreviousStep,
  site,
  roleOptions,
}: InjectedWizardProps & SummaryStepProps) => {
  const router = useRouter();
  const {
    getValues,
    formState: { isSubmitting, isSubmitSuccessful },
    watch,
  } = useFormContext<SetUserRolesFormValues>();
  const userIdentityStatus = watch('userIdentityStatus');
  const { email, roleIds, firstName, lastName } = getValues();

  const rolesSummary = roleOptions
    .filter(roleOption => roleIds.some(roleId => roleId === roleOption.id))
    .toSorted(sortRolesByName)
    .map(x => x.displayName)
    .join(', ');

  const isCreatingNewOktaUser =
    userIdentityStatus?.identityProvider === 'Okta' &&
    userIdentityStatus?.extantInIdentityProvider === false;
  const nameSummary: SummaryListItem[] = isCreatingNewOktaUser
    ? [
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
      ]
    : [];

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

  const summaryItems: SummaryListItem[] = [...nameSummary, ...userDetails];

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
