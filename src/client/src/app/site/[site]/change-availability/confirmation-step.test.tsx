import render from '@testing/render';
import { screen } from '@testing-library/react';
import ConfirmationStep from './confirmation-step';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';

const mockGoToNextStep = jest.fn();
const siteID = 'site-123';
const defaultProps = {
  site: siteID,
  currentStep: 4,
  stepNumber: 4,
  isActive: true,
  goToNextStep: mockGoToNextStep,
  goToPreviousStep: jest.fn(),
  setCurrentStep: jest.fn(),
  goToLastStep: jest.fn(),
  returnRouteUponCancellation: '',
};

const renderComponent = (
  cancellationSummary?: ChangeAvailabilityFormValues['cancellationSummary'],
  cancellationDecision?: ChangeAvailabilityFormValues['cancellationDecision'],
) => {
  const defaultValues = {
    cancellationSummary,
    cancellationDecision,
  } as ChangeAvailabilityFormValues;

  return render(
    <MockForm<ChangeAvailabilityFormValues>
      defaultValues={defaultValues}
      submitHandler={jest.fn()}
    >
      <ConfirmationStep {...defaultProps} />
    </MockForm>,
  );
};

const expectNavigationLinksToHaveCorrectSiteId = (siteId: string) => {
  const createLink = screen.getByRole('link', {
    name: /create availability/i,
  });
  const backToViewLink = screen.getByRole('link', {
    name: /go back to view availability/i,
  });

  expect(createLink).toHaveAttribute(
    'href',
    `/site/${siteId}/create-availability/wizard`,
  );
  expect(backToViewLink).toHaveAttribute(
    'href',
    `/site/${siteId}/view-availability`,
  );
};

describe('ConfirmationStep', () => {
  describe('sessionWithoutBookingsCancellation', () => {
    it('renders the correct heading for multiple cancelled sessions', () => {
      renderComponent({
        cancelledSessionsCount: 5,
        cancelledBookingsCount: 0,
        bookingsWithoutContactDetailsCount: 0,
      });

      expect(
        screen.getByRole('heading', { name: /5 sessions cancelled/i }),
      ).toBeInTheDocument();

      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });

    it('renders the correct heading for a single cancelled session', () => {
      renderComponent({
        cancelledSessionsCount: 1,
        cancelledBookingsCount: 0,
        bookingsWithoutContactDetailsCount: 0,
      });

      expect(
        screen.getByRole('heading', { name: /1 session cancelled/i }),
      ).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });
  });

  describe('keepBookingsCancellation', () => {
    it('renders correct page', () => {
      renderComponent(
        {
          cancelledSessionsCount: 5,
          cancelledBookingsCount: 0,
          bookingsWithoutContactDetailsCount: 0,
        },
        'keep-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('5 sessions cancelled');
      expect(
        screen.getByText(/The bookings will remain in your appointments list/i),
      ).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });

    it('renders the page title as a singleton', () => {
      renderComponent(
        {
          cancelledSessionsCount: 1,
          cancelledBookingsCount: 0,
          bookingsWithoutContactDetailsCount: 0,
        },
        'keep-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('1 session cancelled');
      expect(
        screen.getByText(/The bookings will remain in your appointments list/i),
      ).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });
  });

  describe('cancelBookingsAllNotifiedCancellation', () => {
    it('renders correct page', () => {
      renderComponent(
        {
          cancelledSessionsCount: 5,
          cancelledBookingsCount: 5,
          bookingsWithoutContactDetailsCount: 0,
        },
        'cancel-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('5 sessions and 5 bookings cancelled');
      expect(
        screen.getByText(/We have sent a text message or email to everyone/i),
      ).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });

    it('renders the page title as a singleton', () => {
      renderComponent(
        {
          cancelledSessionsCount: 1,
          cancelledBookingsCount: 1,
          bookingsWithoutContactDetailsCount: 0,
        },
        'cancel-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('1 session and 1 booking cancelled');
      expect(
        screen.getByText(/We have sent a text message or email to everyone/i),
      ).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });
  });

  describe('cancelBookingsSomeNotNotifiedCancellation', () => {
    it('renders correct page', () => {
      renderComponent(
        {
          cancelledSessionsCount: 5,
          cancelledBookingsCount: 5,
          bookingsWithoutContactDetailsCount: 3,
        },
        'cancel-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('5 sessions and 5 bookings cancelled');
      expect(
        screen.getByText(
          /We have sent a text message or email to everyone we have contact details for/i,
        ),
      ).toBeInTheDocument();

      const subHeading = screen.getByRole('heading', {
        level: 3,
        name: /3 people have not been notified/i,
      });
      expect(subHeading).toBeInTheDocument();

      expect(
        screen.getByText(
          /These people did not provide an email address or mobile number/i,
        ),
      ).toBeInTheDocument();

      const link = screen.getByText(
        /View the list of people who have not been notified/i,
      );
      expect(link).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });

    it('calls goToNextStep when the not notified link is clicked', async () => {
      const { user } = renderComponent(
        {
          cancelledSessionsCount: 5,
          cancelledBookingsCount: 5,
          bookingsWithoutContactDetailsCount: 3,
        },
        'cancel-bookings',
      );

      const link = screen.getByText(
        /View the list of people who have not been notified/i,
      );
      await user.click(link);
      expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
    });
  });

  describe('cancelBookingsNoOneNotifiedCancellation', () => {
    it('renders correct page', () => {
      renderComponent(
        {
          cancelledSessionsCount: 5,
          cancelledBookingsCount: 5,
          bookingsWithoutContactDetailsCount: 5,
        },
        'cancel-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('5 sessions and 5 bookings cancelled');
      expect(
        screen.getByText(
          /No one with a booking provided an email address or mobile number/i,
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByText(
          /If you can, you should contact people to tell them their booking is cancelled/i,
        ),
      ).toBeInTheDocument();
      const viewListLink = screen.getByText(
        /View the list of people who have not been notified/i,
      );
      expect(viewListLink).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });

    it('renders the page title as a singleton', () => {
      renderComponent(
        {
          cancelledSessionsCount: 1,
          cancelledBookingsCount: 1,
          bookingsWithoutContactDetailsCount: 1,
        },
        'cancel-bookings',
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('1 session and 1 booking cancelled');
      expect(
        screen.getByText(
          /No one with a booking provided an email address or mobile number/i,
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByText(
          /If you can, you should contact people to tell them their booking is cancelled/i,
        ),
      ).toBeInTheDocument();
      const viewListLink = screen.getByText(
        /View the list of people who have not been notified/i,
      );
      expect(viewListLink).toBeInTheDocument();
      expectNavigationLinksToHaveCorrectSiteId(siteID);
    });

    it('calls goToNextStep when the not notified link is clicked', async () => {
      const { user } = renderComponent(
        {
          cancelledSessionsCount: 1,
          cancelledBookingsCount: 1,
          bookingsWithoutContactDetailsCount: 1,
        },
        'cancel-bookings',
      );

      const link = screen.getByText(
        /View the list of people who have not been notified/i,
      );
      await user.click(link);
      expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
    });
  });
});
