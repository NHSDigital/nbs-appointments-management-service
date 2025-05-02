import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import render from '@testing/render';
import MockForm from '@testing/mockForm';
import {
  setUserRolesFormSchema,
  SetUserRolesFormValues,
} from '../set-user-roles-form';
import { InjectedWizardProps } from '@components/wizard';
import NamesStep from './names-step';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps: InjectedWizardProps = {
  stepNumber: 1,
  currentStep: 1,
  isActive: true,
  setCurrentStep: mockSetCurrentStep,
  goToNextStep: mockGoToNextStep,
  goToLastStep: mockGoToLastStep,
  goToPreviousStep: mockGoToPreviousStep,
  returnRouteUponCancellation: '/',
};

const formState: SetUserRolesFormValues = {
  email: 'new.user@nhs.net',
  firstName: '',
  lastName: '',
  roleIds: ['role-1', 'role-2'],
  userIdentityStatus: {
    identityProvider: 'Okta',
    extantInIdentityProvider: false,
    extantInMya: false,
    meetsWhitelistRequirements: true,
  },
};

describe('Names step', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
  });

  it('renders', () => {
    render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={formState}
        schema={setUserRolesFormSchema}
      >
        <NamesStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Enter name' }),
    ).toBeInTheDocument();
  });

  it('navigates to the cancellation route when cancel is clicked', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={formState}
        schema={setUserRolesFormSchema}
      >
        <NamesStep
          {...defaultProps}
          returnRouteUponCancellation="/route-to-cancel-back-to"
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(mockPush).toHaveBeenCalledWith('/route-to-cancel-back-to');
  });

  it('shows a validation error when first name is not entered', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: '',
          lastName: 'Kensington-Jones',
        }}
        schema={setUserRolesFormSchema}
      >
        <NamesStep {...defaultProps} />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Enter a first name')).toBeVisible();
  });

  it('shows a validation error when first name is too long', async () => {
    const over50Chars = 'a'.repeat(51);

    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: '',
          lastName: 'Kensington-Jones',
        }}
        schema={setUserRolesFormSchema}
      >
        <NamesStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'First name',
      }),
      over50Chars,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('First name cannot exceed 50 characters'),
    ).toBeVisible();
  });

  it('shows a validation error when last name is not entered', async () => {
    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: 'Elizabeth',
          lastName: '',
        }}
        schema={setUserRolesFormSchema}
      >
        <NamesStep {...defaultProps} />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Enter a last name')).toBeVisible();
  });

  it('shows a validation error when last name is too long', async () => {
    const over50Chars = 'a'.repeat(51);

    const { user } = render(
      <MockForm<SetUserRolesFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...formState,
          firstName: '',
          lastName: 'Kensington-Jones',
        }}
        schema={setUserRolesFormSchema}
      >
        <NamesStep {...defaultProps} />
      </MockForm>,
    );

    await user.type(
      screen.getByRole('textbox', {
        name: 'Last name',
      }),
      over50Chars,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('Last name cannot exceed 50 characters'),
    ).toBeVisible();
  });
});
