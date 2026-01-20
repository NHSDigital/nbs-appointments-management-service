import render from '@testing/render';
import { screen } from '@testing-library/react';
import { ClinicalService, SessionSummary } from '@types';
import EditServicesForm from './edit-services-form';
import { useRouter } from 'next/navigation';
import { mockSite } from '@testing/data';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

const mockPush = jest.fn();

const mockClinicalServices: ClinicalService[] = [
  {
    value: 'COVID:18+',
    label: 'COVID 18+',
    serviceType: 'COVID-19',
    url: 'covid',
  },
  {
    value: 'COVID:5_11',
    label: 'COVID 5-11',
    serviceType: 'COVID-19',
    url: 'covid',
  },
  { value: 'RSV:Adult', label: 'RSV Adult', serviceType: 'RSV', url: 'rsv' },
  { value: 'FLU', label: 'Flu', serviceType: 'FLU', url: 'flu' },
];

const mockExistingSession: SessionSummary = {
  ukStartDatetime: '2023-10-10T09:00:00Z',
  ukEndDatetime: '2023-10-10T17:00:00Z',
  slotLength: 10,
  capacity: 1,
  maximumCapacity: 1,
  totalSupportedAppointments: 20,
  totalSupportedAppointmentsByService: {
    'COVID:18+': 10,
    'COVID:5_11': 5,
    'RSV:Adult': 5,
  },
};

describe('EditServicesForm', () => {
  beforeEach(() => {
    (useRouter as jest.Mock).mockReturnValue({ push: mockPush });
    jest.clearAllMocks();
  });

  const renderForm = (props = {}) => {
    return render(
      <EditServicesForm
        site={mockSite}
        existingSession={mockExistingSession}
        date="2023-10-10"
        clinicalServices={mockClinicalServices}
        changeSessionUpliftedJourneyEnabled={false}
        {...props}
      />,
    );
  };

  describe('Checkbox Grouping', () => {
    it('renders services grouped by their category headings', () => {
      renderForm();

      expect(screen.getByText('COVID-19 services')).toBeInTheDocument();
      expect(screen.getByText('RSV services')).toBeInTheDocument();
      expect(screen.queryByText('Flu services')).not.toBeInTheDocument();
    });

    it('renders a default title if the serviceType is not in the mapping', () => {
      const customServices: ClinicalService[] = [
        {
          value: 'new:1-100',
          label: 'label',
          serviceType: 'newServiceType',
          url: 'url',
        },
      ];
      const customSession = {
        ...mockExistingSession,
        totalSupportedAppointmentsByService: { 'new:1-100': 1 },
      };

      renderForm({
        clinicalServices: customServices,
        existingSession: customSession,
      });

      expect(screen.getByText('newServiceType')).toBeInTheDocument();
    });
  });
});
