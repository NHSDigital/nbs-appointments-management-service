import { screen, waitFor } from '@testing-library/react';
import { EditSessionConfirmation } from './edit-session-confirmation';
import render from '@testing/render';
import { mockMultipleServices } from '@testing/data';
import { Session } from '@types';
import { useRouter } from 'next/navigation';
import { editSession } from '@services/appointmentsService';
import { ServerActionResult } from '@types';

const mockSessionSummary = {
  id: '1',
  startTime: '09:00',
  endTime: '12:00',
  capacity: 2,
  services: ['RSV:Adult'],
  totalSupportedAppointments: 2,
  totalUnsupportedAppointments: 0,
  totalAppointments: 2,
  totalSupportedAppointmentsByService: {
    'RSV:Adult': 2,
  },
};

const mockNewSessionDetails: Session = {
  startTime: { hour: 13, minute: 0 },
  endTime: { hour: 17, minute: 0 },
  break: 'no',
  capacity: 3,
  slotLength: 30,
  services: ['RSV:Adult', 'RSV:Child'],
};

const mockSessionToEdit: Session = {
  startTime: { hour: 13, minute: 0 },
  endTime: { hour: 17, minute: 0 },
  break: 'no',
  capacity: 3,
  slotLength: 30,
  services: ['RSV:Adult', 'RSV:Child'],
};

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();
const mockPush = jest.fn();
jest.mock('@services/appointmentsService');

describe('EditSessionConfirmation', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockUseRouter.mockReturnValue({
      replace: mockReplace,
      push: mockPush,
    });

    it('No unsupported bookings, renders question to change session', () => {
      render(
        <EditSessionConfirmation
          unsupportedBookingsCount={0}
          clinicalServices={mockMultipleServices}
          session={btoa(JSON.stringify(mockSessionSummary))}
          newSessionDetails={mockNewSessionDetails}
          sessionToEdit={mockSessionToEdit}
          site="site-123"
          date="2024-06-10"
        />,
      );

      expect(
        screen.getByRole('button', { name: 'Change session' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Continue' }),
      ).not.toBeInTheDocument();
      expect(
        screen.getByText('Are you sure you want to change this session?'),
      ).toBeInTheDocument();
    });

    it('Has unsupported bookings, renders Yes/No question to cancel the appointments', () => {
      render(
        <EditSessionConfirmation
          unsupportedBookingsCount={3}
          clinicalServices={mockMultipleServices}
          session={btoa(JSON.stringify(mockSessionSummary))}
          newSessionDetails={mockNewSessionDetails}
          sessionToEdit={mockSessionToEdit}
          site="site-123"
          date="2024-06-10"
        />,
      );

      expect(
        screen.getByRole('button', { name: 'Continue' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Change session' }),
      ).not.toBeInTheDocument();
      expect(
        screen.getByLabelText(/Yes, cancel the appointments/),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText(/No, do not cancel the appointments/),
      ).toBeInTheDocument();
    });

    it('Has unsupported bookings, user choose "Yes" to cancel the appointments', async () => {
      const { user } = render(
        <EditSessionConfirmation
          unsupportedBookingsCount={3}
          clinicalServices={mockMultipleServices}
          session={btoa(JSON.stringify(mockSessionSummary))}
          newSessionDetails={mockNewSessionDetails}
          sessionToEdit={mockSessionToEdit}
          site="site-123"
          date="2024-06-10"
        />,
      );

      await user.click(screen.getByLabelText(/Yes, cancel the appointments/));
      await user.click(screen.getByRole('button', { name: 'Continue' }));
      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Cancel appointments' }),
        ).toBeInTheDocument();
      });
    });

    it('Has unsupported bookings, user choose "No" to cancel the appointments', async () => {
      const { user } = render(
        <EditSessionConfirmation
          unsupportedBookingsCount={3}
          clinicalServices={mockMultipleServices}
          session={btoa(JSON.stringify(mockSessionSummary))}
          newSessionDetails={mockNewSessionDetails}
          sessionToEdit={mockSessionToEdit}
          site="site-123"
          date="2024-06-10"
        />,
      );

      await user.click(
        screen.getByLabelText(/No, do not cancel the appointments/),
      );
      await user.click(screen.getByRole('button', { name: 'Continue' }));
      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Change session' }),
        ).toBeInTheDocument();
      });
    });
  });
});
