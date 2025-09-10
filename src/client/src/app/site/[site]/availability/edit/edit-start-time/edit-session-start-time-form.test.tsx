import { editSession } from '@services/appointmentsService';
import render from '@testing/render';
import { useRouter } from 'next/navigation';
import EditSessionStartTimeForm from './edit-session-start-time-form';
import { mockSession1, mockSite } from '@testing/data';
import { screen } from '@testing-library/react';
import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();
const mockPush = jest.fn();

jest.mock('@services/appointmentsService');
const editSessionMock = editSession as jest.Mock;

describe('Edit Session Start Time Form', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockUseRouter.mockReturnValue({
      replace: mockReplace,
      push: mockPush,
    });
  });

  it('renders', () => {
    render(
      <EditSessionStartTimeForm
        date="2025-09-10"
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
        updatedSession={mockSession1}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'What time do you want the session to start?',
      }),
    ).toBeInTheDocument();
  });

  it('renders the correct nearest aligned start times', () => {
    render(
      <EditSessionStartTimeForm
        date="2025-09-10"
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
        updatedSession={mockSession1}
      />,
    );

    expect(screen.getByRole('radio', { name: '09:00am' })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: '09:05am' })).toBeInTheDocument();
  });

  it('displays a validation message when no option selected', async () => {
    const { user } = render(
      <EditSessionStartTimeForm
        date="2025-09-10"
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
        updatedSession={mockSession1}
      />,
    );

    const changeSessionButton = screen.getByRole('button', {
      name: 'Change session',
    });
    await user.click(changeSessionButton);

    expect(screen.getByText('Select an option')).toBeInTheDocument();
    expect(editSessionMock).not.toHaveBeenCalled();
  });

  it('calls edit session when the form is valid', async () => {
    const date = '2025-09-10';
    const { user } = render(
      <EditSessionStartTimeForm
        date={date}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
        updatedSession={mockSession1}
      />,
    );

    const startTimeOptionRadio = screen.getByRole('radio', {
      name: '09:05am',
    });
    await user.click(startTimeOptionRadio);

    const changeSessionButton = screen.getByRole('button', {
      name: 'Change session',
    });
    await user.click(changeSessionButton);

    expect(editSessionMock).toHaveBeenCalledTimes(1);
    expect(editSessionMock).toHaveBeenCalledWith({
      date: date,
      site: mockSite.id,
      mode: 'Edit',
      sessions: [
        {
          from: '09:05',
          until: '12:00',
          slotLength: 5,
          capacity: 2,
          services: ['RSV:Adult'],
        },
      ],
      sessionToEdit: {
        from: '09:00',
        until: '12:00',
        slotLength: 5,
        capacity: 2,
        services: ['RSV:Adult'],
      },
    });
  });
});
