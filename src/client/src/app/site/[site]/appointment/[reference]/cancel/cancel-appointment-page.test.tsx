import { screen } from '@testing-library/react';
import CancelAppointmentPage from './cancel-appointment-page';
import { mockBookings } from '@testing/data';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';
import render from '@testing/render';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';
import { clinicalServices } from '@types';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

jest.mock('@services/appointmentsService');
const mockCancelBooking = jest.spyOn(appointmentsService, 'cancelAppointment');

describe('Cancel Appointment Page', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('renders', () => {
    render(
      <CancelAppointmentPage
        site="TEST01"
        booking={mockBookings[0]}
        clinicalServices={clinicalServices}
      />,
    );

    verifySummaryListItem('Date and time', ['10 November 2024', '14:05pm']);

    expect(
      screen.getByRole('radio', {
        name: 'Cancelled by the citizen',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Cancel appointment' }),
    ).toBeInTheDocument();
  });

  it('renders not provided when a user has not provided contact details', () => {
    render(
      <CancelAppointmentPage
        site="TEST01"
        booking={mockBookings[0]}
        clinicalServices={clinicalServices}
      />,
    );

    verifySummaryListItem('Contact information', 'Not provided');
  });

  it('does not call the cancel endpoint and shows an error when no option is selected', async () => {
    const { user } = render(
      <CancelAppointmentPage
        site="TEST01"
        booking={mockBookings[0]}
        clinicalServices={clinicalServices}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Cancel appointment' }),
    );

    expect(
      screen.getByText('Select a reason for cancelling the appointment'),
    ).toBeInTheDocument();
    expect(mockCancelBooking).not.toHaveBeenCalled();
  });

  it.each(['Cancelled by the citizen', 'Cancelled by the site'])(
    'calls the cancel endpoint when an option is selected',
    async option => {
      const { user } = render(
        <CancelAppointmentPage
          site="TEST01"
          booking={mockBookings[0]}
          clinicalServices={clinicalServices}
        />,
      );

      await user.click(screen.getByLabelText(option));
      await user.click(
        screen.getByRole('button', { name: 'Cancel appointment' }),
      );

      expect(mockCancelBooking).toHaveBeenCalled();
    },
  );
});
