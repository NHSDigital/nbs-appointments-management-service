import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import render from '@testing/render';
import MockForm from '@testing/mockForm';
import {
  setUserRolesFormSchema,
  SetUserRolesFormValues,
} from '../set-user-roles-form';
import { mockRoles } from '@testing/data';
import { InjectedWizardProps } from '@components/wizard';
import SetRolesStep, { RolesStepProps } from './set-roles-step';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps: InjectedWizardProps & RolesStepProps = {
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

describe('Set Roles Step', () => {
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
        <SetRolesStep {...defaultProps} />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Additional details' }),
    ).toBeInTheDocument();
  });

  // it('displays a check box for each available role', () => {
  //   render(
  //     <MockForm<SetUserRolesFormValues>
  //       submitHandler={jest.fn()}
  //       defaultValues={formState}
  //       schema={setUserRolesFormSchema}
  //     >
  //       <SetRolesStep {...defaultProps} />
  //     </MockForm>,
  //   );

  //   expect(screen.getByRole('checkbox', { name: 'Alpha Role' })).toBeVisible();
  //   expect(screen.getByRole('checkbox', { name: 'Beta Role' })).toBeVisible();
  //   expect(
  //     screen.getByRole('checkbox', { name: 'Charlie Role' }),
  //   ).toBeVisible();
  // });

  // it('displays checkboxes for all available roles in ascending alphabetical order', () => {
  //   render(
  //     <MockForm<SetUserRolesFormValues>
  //       submitHandler={jest.fn()}
  //       defaultValues={formState}
  //       schema={setUserRolesFormSchema}
  //     >
  //       <SetRolesStep {...defaultProps} />
  //     </MockForm>,
  //   );

  //   const checkboxes = screen.getAllByRole('checkbox');
  //   expect(checkboxes.length).toBe(3);
  //   expect(checkboxes[0].getAttribute('label')).toEqual('Alpha Role');
  //   expect(checkboxes[1].getAttribute('label')).toEqual('Beta Role');
  //   expect(checkboxes[2].getAttribute('label')).toEqual('Charlie Role');
  // });

  // it('checks the correct options', () => {
  //   render(
  //     <MockForm<SetUserRolesFormValues>
  //       submitHandler={jest.fn()}
  //       defaultValues={formState}
  //       schema={setUserRolesFormSchema}
  //     >
  //       <SetRolesStep {...defaultProps} />
  //     </MockForm>,
  //   );

  //   expect(screen.getByRole('checkbox', { name: 'Beta Role' })).toBeChecked();
  //   expect(
  //     screen.getByRole('checkbox', { name: 'Charlie Role' }),
  //   ).toBeChecked();
  //   expect(
  //     screen.getByRole('checkbox', { name: 'Alpha Role' }),
  //   ).not.toBeChecked();
  // });

  // it('display a validation error when attempting to submit the form with no roles selected', async () => {
  //   const { user } = render(
  //     <MockForm<SetUserRolesFormValues>
  //       submitHandler={jest.fn()}
  //       defaultValues={{ ...formState, roleIds: [] }}
  //       schema={setUserRolesFormSchema}
  //     >
  //       <SetRolesStep {...defaultProps} />
  //     </MockForm>,
  //   );

  //   await user.click(
  //     screen.getByRole('button', {
  //       name: 'Continue',
  //     }),
  //   );

  //   expect(
  //     screen.getByText('You have not selected any roles for this user'),
  //   ).toBeVisible();
  // });

  // it('navigates to the cancellation route when cancel is clicked', async () => {
  //   const { user } = render(
  //     <MockForm<SetUserRolesFormValues>
  //       submitHandler={jest.fn()}
  //       defaultValues={{ ...formState, roleIds: [] }}
  //       schema={setUserRolesFormSchema}
  //     >
  //       <SetRolesStep
  //         {...defaultProps}
  //         returnRouteUponCancellation="/route-to-cancel-back-to"
  //       />
  //     </MockForm>,
  //   );

  //   await user.click(screen.getByRole('button', { name: 'Cancel' }));
  //   expect(mockPush).toHaveBeenCalledWith('/route-to-cancel-back-to');
  // });

  // it('displays the email address of the user', async () => {
  //   render(
  //     <MockForm<SetUserRolesFormValues>
  //       submitHandler={jest.fn()}
  //       defaultValues={{ ...formState, roleIds: [] }}
  //       schema={setUserRolesFormSchema}
  //     >
  //       <SetRolesStep {...defaultProps} />
  //     </MockForm>,
  //   );

  //   const email = screen.getByRole('heading', { name: 'Email' });

  //   expect(email).toBeVisible();
  //   expect(screen.getByText('new.user@nhs.net')).toBeVisible();
  // });
});
