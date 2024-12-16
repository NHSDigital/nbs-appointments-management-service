import { screen } from '@testing-library/react';
import CancelAppointmentPage from './cancel-appointment-page';
import { mockBookings } from '@testing/data';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';
import render from '@testing/render';

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
    render(<CancelAppointmentPage site="TEST01" booking={mockBookings[0]} />);

    expect(
      screen.getByRole('term', { name: 'Date and time' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', {
        name: 'Yes, I want to cancel this appointment',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Continue' }),
    ).toBeInTheDocument();
  });

  it('renders not provided when a user has not provided contact details', () => {
    render(<CancelAppointmentPage site="TEST01" booking={mockBookings[0]} />);

    expect(
      screen.getByRole('definition', { name: 'Not provided' }),
    ).toBeInTheDocument();
  });

  it('calls the cancel appointment endpoint when a user selects yes', async () => {
    const { user } = render(
      <CancelAppointmentPage site="TEST01" booking={mockBookings[0]} />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockCancelBooking).toHaveBeenCalled();
  });
});
