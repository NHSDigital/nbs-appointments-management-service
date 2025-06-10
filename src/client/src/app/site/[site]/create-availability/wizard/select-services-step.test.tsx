import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SelectServicesStep from './select-services-step';
import { ClinicalService } from '@types';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

let mockClinicalServices: ClinicalService[] = [];

describe('Select Services Step', () => {
  describe('Multiple Services Disabled', () => {
    beforeAll(() => {
      mockClinicalServices = [{ label: 'RSV Adult', value: 'RSV:Adult' }];
    });

    it('renders', async () => {
      renderStep(mockClinicalServices);

      expect(
        screen.getByRole('heading', {
          name: 'Create weekly session Add services to your session',
        }),
      ).toBeInTheDocument();
    });

    it('permits user input', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      expect(screen.getByRole('checkbox', { name: 'RSV Adult' })).toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
    });

    it('shows a validation error if no services are selected', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();

      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(screen.getByText('Select a service')).toBeInTheDocument();

      expect(mockGoToNextStep).not.toHaveBeenCalled();
    });

    it('continues to the next step if a service is selected', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(mockGoToNextStep).toHaveBeenCalled();
    });

    it('continues to the next step if a service is selected', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(mockGoToNextStep).toHaveBeenCalled();
    });
  });

  describe('Multiple Services Enabled', () => {
    beforeAll(() => {
      mockClinicalServices = [
        {
          value: 'RSV:Adult',
          label: 'RSV Adult',
        },
        {
          value: 'COVID:5_11',
          label: 'COVID 5-11',
        },
        {
          value: 'COVID:12_17',
          label: 'COVID 12-17',
        },
        {
          value: 'COVID:18+',
          label: 'COVID 18+',
        },
      ];
    });

    it('renders', async () => {
      renderStep(mockClinicalServices);

      expect(
        screen.getByRole('heading', {
          name: 'Create weekly session Add services to your session',
        }),
      ).toBeInTheDocument();
    });

    it('permits user input', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      expect(screen.getByRole('checkbox', { name: 'RSV Adult' })).toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
    });

    it('shows a validation error if no services are selected', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();

      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(screen.getByText('Select a service')).toBeInTheDocument();

      expect(mockGoToNextStep).not.toHaveBeenCalled();
    });

    it('continues to the next step if a service is selected', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(mockGoToNextStep).toHaveBeenCalled();
    });

    it('continues to the next step if a service is selected', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(mockGoToNextStep).toHaveBeenCalled();
    });

    it('selects all services when Select All is checked', async () => {
      const { user } = renderStep(mockClinicalServices);

      await user.click(screen.getByRole('checkbox', { name: 'Select all' }));
      expect(screen.getByRole('checkbox', { name: 'RSV Adult' })).toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 5-11' }),
      ).toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 12-17' }),
      ).toBeChecked();
      expect(screen.getByRole('checkbox', { name: 'COVID 18+' })).toBeChecked();
    });

    it('deselects all services when Select All is checked', async () => {
      const { user } = renderStep(mockClinicalServices, [
        'RSV:Adult',
        'COVID:5_11',
        'COVID:12_17',
        'COVID:18+',
      ]);
      expect(screen.getByRole('checkbox', { name: 'RSV Adult' })).toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 5-11' }),
      ).toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 12-17' }),
      ).toBeChecked();
      expect(screen.getByRole('checkbox', { name: 'COVID 18+' })).toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'Select all' }),
      ).toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'Select all' }));

      expect(
        screen.getByRole('checkbox', { name: 'RSV Adult' }),
      ).not.toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 5-11' }),
      ).not.toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 12-17' }),
      ).not.toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'COVID 18+' }),
      ).not.toBeChecked();
      expect(
        screen.getByRole('checkbox', { name: 'Select all' }),
      ).not.toBeChecked();
    });

    it('automatically checks Select All when each service is checked individually', async () => {
      const { user } = renderStep(mockClinicalServices);

      expect(
        screen.getByRole('checkbox', { name: 'Select all' }),
      ).not.toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'RSV Adult' }));
      await user.click(screen.getByRole('checkbox', { name: 'COVID 5-11' }));
      await user.click(screen.getByRole('checkbox', { name: 'COVID 12-17' }));
      await user.click(screen.getByRole('checkbox', { name: 'COVID 18+' }));

      expect(
        screen.getByRole('checkbox', { name: 'Select all' }),
      ).toBeChecked();
    });

    it('automatically unchecks Select All when a service is unchecked individually', async () => {
      const { user } = renderStep(mockClinicalServices, [
        'RSV:Adult',
        'COVID:5_11',
        'COVID:12_17',
        'COVID:18+',
      ]);

      expect(
        screen.getByRole('checkbox', { name: 'Select all' }),
      ).toBeChecked();

      await user.click(screen.getByRole('checkbox', { name: 'COVID 18+' }));

      expect(
        screen.getByRole('checkbox', { name: 'Select all' }),
      ).not.toBeChecked();
    });
  });
});

const renderStep = (
  services: ClinicalService[],
  currentFormValues: string[] = [],
) => {
  return render(
    <MockForm<CreateAvailabilityFormValues>
      submitHandler={jest.fn()}
      defaultValues={{ session: { services: currentFormValues ?? [] } }}
    >
      <SelectServicesStep
        stepNumber={1}
        currentStep={1}
        isActive
        setCurrentStep={mockSetCurrentStep}
        goToNextStep={mockGoToNextStep}
        goToLastStep={mockGoToLastStep}
        goToPreviousStep={mockGoToPreviousStep}
        clinicalServices={services}
        returnRouteUponCancellation="/"
      />
    </MockForm>,
  );
};
