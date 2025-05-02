import { screen } from '@testing-library/react';
import render from '@testing/render';
import MockForm from '@testing/mockForm';
import {
  setUserRolesFormSchema,
  SetUserRolesFormValues,
} from '../set-user-roles-wizard';
import { mockRoles } from '@testing/data';
import { InjectedWizardProps } from '@components/wizard';
import SummaryStep, { SummaryStepProps } from './summary-step';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps: InjectedWizardProps & SummaryStepProps = {
  stepNumber: 1,
  currentStep: 1,
  isActive: true,
  setCurrentStep: mockSetCurrentStep,
  goToNextStep: mockGoToNextStep,
  goToLastStep: mockGoToLastStep,
  goToPreviousStep: mockGoToPreviousStep,
  returnRouteUponCancellation: '/',
  roleOptions: mockRoles,
};

const formState: SetUserRolesFormValues = {
  email: 'new.user@nhs.net',
  firstName: '',
  lastName: '',
  roleIds: ['role-1', 'role-2'],
  userIdentityStatus: {
    identityProvider: 'NhsMail',
    extantInIdentityProvider: true,
    extantInMya: false,
    meetsWhitelistRequirements: true,
  },
};

describe('Summary Step', () => {
  it('renders', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={formState}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Check user details' }),
    ).toBeInTheDocument();
  });

  it('summarises the user to be created', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={formState}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    verifySummaryListItem('Email address', 'new.user@nhs.net');
    verifySummaryListItem('Roles', 'Beta Role, Charlie Role');

    expect(
      screen.queryByRole('term', { name: 'Name' }),
    ).not.toBeInTheDocument();
  });

  it('displays name if a new okta used is being created', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: 'Elizabeth',
          lastName: 'Kensington-Jones',
          userIdentityStatus: {
            identityProvider: 'Okta',
            extantInIdentityProvider: false,
            extantInMya: false,
            meetsWhitelistRequirements: true,
          },
        }}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    verifySummaryListItem('Name', 'Elizabeth Kensington-Jones');
  });

  it('hides the name summary for Nhs Mail users', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: 'Elizabeth',
          lastName: 'Kensington-Jones',
          userIdentityStatus: {
            identityProvider: 'NhsMail',
            extantInIdentityProvider: false,
            extantInMya: false,
            meetsWhitelistRequirements: true,
          },
        }}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.queryByRole('term', { name: 'Name' }),
    ).not.toBeInTheDocument();
  });

  it('hides the name summary for Oktas users who already exist', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: 'Elizabeth',
          lastName: 'Kensington-Jones',
          userIdentityStatus: {
            identityProvider: 'Okta',
            extantInIdentityProvider: true,
            extantInMya: false,
            meetsWhitelistRequirements: true,
          },
        }}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.queryByRole('term', { name: 'Name' }),
    ).not.toBeInTheDocument();
  });

  it('displays an email advice message', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={formState}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.getByText(
        'new.user@nhs.net will be sent information about how to log in.',
      ),
    ).toBeInTheDocument();
  });

  it('displays a different advice message for Okta users', async () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: 'Elizabeth',
          lastName: 'Kensington-Jones',
          userIdentityStatus: {
            identityProvider: 'Okta',
            extantInIdentityProvider: false,
            extantInMya: false,
            meetsWhitelistRequirements: true,
          },
        }}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.getByText(
        'Elizabeth Kensington-Jones will be sent information about how to log in.',
      ),
    ).toBeInTheDocument();
  });

  it('displays a Confirm button which submits the form', async () => {
    const mockOnSubmit = jest.fn();

    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={mockOnSubmit}
        defaultValues={formState}
        schema={setUserRolesFormSchema}
      >
        <SummaryStep {...defaultProps} />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(mockOnSubmit).toHaveBeenCalledWith(formState);
  });
});
