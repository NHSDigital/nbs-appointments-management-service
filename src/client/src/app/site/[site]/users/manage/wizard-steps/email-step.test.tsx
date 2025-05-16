import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import render from '@testing/render';
import MockForm from '@testing/mockForm';
import {
  setUserRolesFormSchema,
  SetUserRolesFormValues,
} from '../set-user-roles-form';
import EmailStep, { EmailStepProps } from './email-step';
import {
  getMockUserAssignments,
  mockSite,
  mockUserProfile,
} from '@testing/data';
import { InjectedWizardProps } from '@components/wizard';
import { proposeNewUser, fetchUsers } from '@services/appointmentsService';
import { User, UserIdentityStatus } from '@types';

jest.mock('@services/appointmentsService');
const mockProposeNewUser = proposeNewUser as jest.Mock<
  Promise<UserIdentityStatus>
>;

const mockFetchUsers = fetchUsers as jest.Mock<Promise<User[]>>;

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps: InjectedWizardProps & EmailStepProps = {
  stepNumber: 1,
  currentStep: 1,
  isActive: true,
  setCurrentStep: mockSetCurrentStep,
  goToNextStep: mockGoToNextStep,
  goToLastStep: mockGoToLastStep,
  goToPreviousStep: mockGoToPreviousStep,
  returnRouteUponCancellation: '/',
  site: mockSite,
  sessionUser: mockUserProfile,
  oktaEnabled: true,
};

describe('Email step', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
    mockFetchUsers.mockResolvedValue(getMockUserAssignments(mockSite.id));
    mockProposeNewUser.mockResolvedValue({
      identityProvider: 'NhsMail',
      extantInIdentityProvider: true,
      extantInSite: true,
      meetsWhitelistRequirements: true,
    });
  });

  it('renders', () => {
    const { container } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Add a user' }),
    ).toBeInTheDocument();

    expect(container.getElementsByClassName('nhsuk-hint').length).toBe(1);

    // Links
    const authorisedDomainsLink = screen.getByRole('link', {
      name: 'authorised email domains',
    });
    expect(authorisedDomainsLink).toBeInTheDocument();
    expect(authorisedDomainsLink).toHaveAttribute(
      'href',
      'https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/care-identity-email-domain-allow-list',
    );
    expect(authorisedDomainsLink).toHaveAttribute('target', '_blank');

    const userGuidanceUrl = screen.getByRole('link', {
      name: 'user guidance on logging in without an NHS.net account',
    });
    expect(userGuidanceUrl).toBeInTheDocument();
    expect(userGuidanceUrl).toHaveAttribute(
      'href',
      'https://digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance/log-in-and-select-site',
    );
    expect(userGuidanceUrl).toHaveAttribute('target', '_blank');

    const emailDomainRequestLink = screen.getByRole('link', {
      name: 'email domain to be approved',
    });
    expect(emailDomainRequestLink).toBeInTheDocument();
    expect(emailDomainRequestLink).toHaveAttribute(
      'href',
      'https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/request-an-addition-to-the-email-domain-allow-list',
    );
    expect(emailDomainRequestLink).toHaveAttribute('target', '_blank');
  });

  it('navigates to the cancellation route when cancel is clicked', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep
          {...defaultProps}
          returnRouteUponCancellation="/route-to-cancel-back-to"
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(mockPush).toHaveBeenCalledWith('/route-to-cancel-back-to');
  });

  it('shows a validation error when no data is submitted', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Enter a valid email address')).toBeVisible();
  });

  it('shows a validation error when an invalid email address is submitted', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'Enter email address',
      }),
      'invalid@@email@nhs.com',
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('Enter a valid email address'),
    ).toBeVisible();
  });

  it('trims and lowercases the input', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'Enter email address',
      }),
      '   TEST@nhs.net  ',
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockProposeNewUser).toHaveBeenCalledWith(
      mockSite.id,
      'test@nhs.net',
    );
  });

  it('shows a validation error when a user enters their own email', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'Enter email address',
      }),
      mockUserProfile.emailAddress,
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('You may not edit your own roles'),
    ).toBeVisible();
  });

  it('fetches roles data if the email entered already exists in MYA', async () => {
    mockProposeNewUser.mockResolvedValue({
      identityProvider: 'NhsMail',
      extantInIdentityProvider: true,
      extantInSite: true,
      meetsWhitelistRequirements: true,
    });

    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'Enter email address',
      }),
      'some.user@nhs.net',
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockFetchUsers).toHaveBeenCalledWith(mockSite.id);
  });

  it('shows a validation error when the email does not meet whitelist criteria', async () => {
    mockProposeNewUser.mockResolvedValue({
      identityProvider: 'NhsMail',
      extantInIdentityProvider: true,
      extantInSite: true,
      meetsWhitelistRequirements: false,
    });

    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        schema={setUserRolesFormSchema}
      >
        <EmailStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'Enter email address',
      }),
      'some.user@nhs.net',
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockFetchUsers).toHaveBeenCalledWith(mockSite.id);
    expect(mockProposeNewUser).toHaveBeenCalledWith(
      mockSite.id,
      'some.user@nhs.net',
    );

    expect(
      await screen.findByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeVisible();
  });
});
