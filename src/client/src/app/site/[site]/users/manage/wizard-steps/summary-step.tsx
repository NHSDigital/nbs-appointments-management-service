'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  SmallSpinnerWithText,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useFormContext } from 'react-hook-form';
import { SetUserRolesFormValues } from '../set-user-roles-form';
import { sortRolesByName } from '@sorting';
import { Role } from '@types';
import { useRouter } from 'next/navigation';
import { Heading, SummaryList } from 'nhsuk-react-components';

export type SummaryStepProps = {
  roleOptions: Role[];
};

const SummaryStep = ({
  setCurrentStep,
  goToPreviousStep,
  roleOptions,
  returnRouteUponCancellation,
  pendingSubmit,
}: InjectedWizardProps & SummaryStepProps) => {
  const router = useRouter();
  const { getValues, watch } = useFormContext<SetUserRolesFormValues>();
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

  const isEditingExistingUser = userIdentityStatus.extantInSite;

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
      action: isEditingExistingUser
        ? undefined
        : {
            renderingStrategy: 'client',
            text: 'Change',
            onClick: () => {
              setCurrentStep(1);
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
          goToPreviousStep();
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
      <Heading>Check user details</Heading>
      <SummaryList>
        {summaryItems.map((item, index) => (
          <SummaryList.Row key={index}>
            <SummaryList.Key>{item.title}</SummaryList.Key>
            <SummaryList.Value>{item.value}</SummaryList.Value>
            {item.action &&
              (item.action.renderingStrategy === 'server' ? (
                <SummaryList.Action href={item.action.href}>
                  {item.action.text}
                </SummaryList.Action>
              ) : (
                <SummaryList.Action
                  onClick={item.action.onClick}
                  style={{
                    cursor: 'pointer',
                  }}
                >
                  {item.action.text}
                </SummaryList.Action>
              ))}
          </SummaryList.Row>
        ))}
      </SummaryList>

      {!isEditingExistingUser && (
        <p>{`${isCreatingNewOktaUser ? `${firstName} ${lastName}` : email} will be sent information about how to log in.`}</p>
      )}

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Confirm</Button>
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
      )}
    </>
  );
};

export default SummaryStep;
