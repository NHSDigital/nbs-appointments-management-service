import render from '@testing/render';
import { screen, waitFor } from '@testing-library/react';
import NoNotificationStep from './no-notification-step';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import * as appointmentsService from '@services/appointmentsService';
import fromServer from '@server/fromServer';
import { CancelDateRangeResponse } from '@types';

jest.mock('@components/session-bookings-contact-details', () => ({
  SessionBookingsContactDetailsPage: () => (
    <div data-testid="mock-contact-details" />
  ),
}));

jest.mock('@services/appointmentsService');
jest.mock('../../../lib/server/fromServer');

const siteID = 'site-123';
const mockBookings = [{ id: 'booking-1', name: 'John Doe' }];
const mockServices = [{ id: 'service-1', name: 'Clinical Service' }];

const defaultProps = {
  site: siteID,
  currentStep: 5,
  stepNumber: 5,
  isActive: true,
  goToNextStep: jest.fn(),
  goToPreviousStep: jest.fn(),
  setCurrentStep: jest.fn(),
  goToLastStep: jest.fn(),
  returnRouteUponCancellation: '',
};

const renderComponent = (overrides?: Partial<ChangeAvailabilityFormValues>) => {
  const defaultValues = {
    startDate: { day: '01', month: '03', year: '2026' },
    endDate: { day: '10', month: '03', year: '2026' },
    cancellationSummary: {
      bookingsWithoutContactDetailsCount: 3,
    },
    ...overrides,
  } as ChangeAvailabilityFormValues;

  return render(
    <MockForm<ChangeAvailabilityFormValues>
      defaultValues={defaultValues}
      submitHandler={jest.fn()}
    >
      <NoNotificationStep {...defaultProps} site={siteID} />
    </MockForm>,
  );
};

describe('NoNotificationStep', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (fromServer as jest.Mock).mockImplementation(promise => promise);
    (appointmentsService.fetchBookings as jest.Mock).mockResolvedValue(
      mockBookings,
    );
    (appointmentsService.fetchClinicalServices as jest.Mock).mockResolvedValue(
      mockServices,
    );
  });

  it('renders loading state initially', async () => {
    renderComponent();
    expect(screen.getByText(/Loading booking details.../i)).toBeInTheDocument();

    await waitFor(() => {
      expect(
        screen.queryByText(/Loading booking details.../i),
      ).not.toBeInTheDocument();
    });
  });

  it('renders correctly after data fetching', async () => {
    renderComponent();

    const heading = await screen.findByRole('heading', {
      name: /People who did not receive a notification/i,
    });

    expect(heading).toBeInTheDocument();
    expect(
      screen.getByText(
        /3 people with appointments from 1 March to 10 March 2026/i,
      ),
    ).toBeInTheDocument();

    expect(screen.getByTestId('mock-contact-details')).toBeInTheDocument();
  });

  it('handles pluralization correctly for a single person', async () => {
    renderComponent({
      cancellationSummary: {
        bookingsWithoutContactDetailsCount: 1,
      } as CancelDateRangeResponse,
    });

    const pluralText = await screen.findByText(/1 person with appointments/i);
    expect(pluralText).toBeInTheDocument();
  });

  it('calls fetchBookings with the correct formatted dates', async () => {
    renderComponent({
      startDate: { day: '01', month: '01', year: '2026' },
      endDate: { day: '02', month: '01', year: '2026' },
    });

    await screen.findByRole('heading', {
      name: /People who did not receive a notification/i,
    });

    expect(appointmentsService.fetchBookings).toHaveBeenCalledWith(
      expect.objectContaining({
        from: '2026-01-01T00:00:00',
        to: '2026-01-02T23:59:59',
        site: siteID,
      }),
      ['Cancelled'],
    );
  });
});
