import render from '@testing/render';
import { screen, within } from '@testing-library/react';
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
      mockClinicalServices = [
        {
          label: 'RSV Adult',
          value: 'RSV:Adult',
          serviceType: 'RSV Adult',
          url: 'RSV',
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
  });

  describe('Multiple Services Enabled', () => {
    beforeAll(() => {
      mockClinicalServices = [
        {
          value: 'RSV:Adult',
          label: 'RSV Adult',
          serviceType: 'RSV Adult',
          url: 'RSV',
        },
        {
          value: 'COVID:5_11',
          label: 'COVID 5-11',
          serviceType: 'COVID 5-11',
          url: 'COVID-19',
        },
        {
          value: 'COVID:12_17',
          label: 'COVID 12-17',
          serviceType: 'COVID 12-17',
          url: 'COVID-19',
        },
        {
          value: 'COVID:18+',
          label: 'COVID 18+',
          serviceType: 'COVID 18+',
          url: 'COVID-19',
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

    it('does not render a group heading if no services for that group are provided', () => {
      const filteredServices = mockClinicalServices.filter(
        s => s.url === 'COVID-19',
      );
      renderStep(filteredServices);

      expect(screen.getByText('COVID-19 services')).toBeInTheDocument();
      expect(screen.queryByText('Flu services')).not.toBeInTheDocument();
    });

    it('clears the validation error immediately once a service is checked', async () => {
      const { user } = renderStep(mockClinicalServices);

      await user.click(screen.getByRole('button', { name: 'Continue' }));
      expect(screen.getByText('Select a service')).toBeInTheDocument();

      const checkbox = screen.getByRole('checkbox', { name: 'RSV Adult' });
      await user.click(checkbox);

      expect(screen.queryByText('Select a service')).not.toBeInTheDocument();
    });

    it('renders services grouped by their category headings', () => {
      renderStep(mockClinicalServices);

      expect(screen.getByText('COVID-19 services')).toBeInTheDocument();
      expect(screen.getByText('RSV services')).toBeInTheDocument();
    });

    it('renders services in the correct sorted order within a group', () => {
      renderStep(mockClinicalServices);

      const covidGroup = screen.getByRole('group', {
        name: /Add COVID-19 services to your session/i,
      });

      const checkboxes = within(covidGroup).getAllByRole('checkbox');

      expect(checkboxes).toHaveLength(3);
      expect(checkboxes[0]).toHaveAttribute('value', 'COVID:5_11');
      expect(checkboxes[1]).toHaveAttribute('value', 'COVID:12_17');
      expect(checkboxes[2]).toHaveAttribute('value', 'COVID:18+');
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
