import render from '@testing/render';
import { screen, waitFor } from '@testing-library/react';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import { mockSite } from '@testing/data';
import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import { editSession } from '@services/appointmentsService';
import { useRouter } from 'next/navigation';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();
const mockPush = jest.fn();

jest.mock('@services/appointmentsService');
const editSessionMock = editSession as jest.Mock;

describe('Edit Session Time And Capacity Form', () => {
  beforeEach(() => {
    jest.resetAllMocks();

    mockUseRouter.mockReturnValue({
      replace: mockReplace,
      push: mockPush,
    });
  });

  it('renders', async () => {
    render(
      <EditSessionTimeAndCapacityForm
        date={'2024-06-10 07:00:00'}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Session times' }),
    ).toBeInTheDocument();
  });

  it('calls the service and navigates to the confirmation page when the form is submitted', async () => {
    const { user } = render(
      <EditSessionTimeAndCapacityForm
        date={'2024-06-10 07:00:00'}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
      />,
    );

    const startTimeHourInput = screen.getByRole('textbox', {
      name: 'Session start time - hour',
    });
    const startTimeMinuteInput = screen.getByRole('textbox', {
      name: 'Session start time - minute',
    });
    const endTimeHourInput = screen.getByRole('textbox', {
      name: 'Session end time - hour',
    });
    const endTimeMinuteInput = screen.getByRole('textbox', {
      name: 'Session end time - minute',
    });

    await user.clear(startTimeHourInput);
    await user.type(startTimeHourInput, '10');

    await user.clear(startTimeMinuteInput);
    await user.type(startTimeMinuteInput, '15');

    await user.clear(endTimeHourInput);
    await user.type(endTimeHourInput, '11');

    await user.clear(endTimeMinuteInput);
    await user.type(endTimeMinuteInput, '30');

    const capacityInput = screen.getByRole('spinbutton', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });
    await user.clear(capacityInput);
    await user.type(capacityInput, '1');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(editSessionMock).toHaveBeenCalledTimes(1);
    expect(editSessionMock).toHaveBeenCalledWith({
      date: '2024-06-10 07:00:00',
      site: '34e990af-5dc9-43a6-8895-b9123216d699',
      mode: 'Edit',
      sessions: [
        {
          from: '10:15',
          until: '11:30',
          slotLength: 5,
          capacity: 1,
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
    expect(mockPush).toHaveBeenCalledTimes(1);
  });

  it('permits start and end time data entry', async () => {
    const { user } = render(
      <EditSessionTimeAndCapacityForm
        date={'2024-06-10 07:00:00'}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
      />,
    );

    const startTimeHourInput = screen.getByRole('textbox', {
      name: 'Session start time - hour',
    });
    const startTimeMinuteInput = screen.getByRole('textbox', {
      name: 'Session start time - minute',
    });
    const endTimeHourInput = screen.getByRole('textbox', {
      name: 'Session end time - hour',
    });
    const endTimeMinuteInput = screen.getByRole('textbox', {
      name: 'Session end time - minute',
    });

    expect(startTimeHourInput).toHaveDisplayValue('09');
    await user.clear(startTimeHourInput);
    await user.type(startTimeHourInput, '12');
    expect(startTimeHourInput).toHaveDisplayValue('12');

    expect(startTimeMinuteInput).toHaveDisplayValue('00');
    await user.clear(startTimeMinuteInput);
    await user.type(startTimeMinuteInput, '17');
    expect(startTimeMinuteInput).toHaveDisplayValue('17');

    expect(endTimeHourInput).toHaveDisplayValue('12');
    await user.clear(endTimeHourInput);
    await user.type(endTimeHourInput, '6');
    expect(endTimeHourInput).toHaveDisplayValue('06');

    expect(endTimeMinuteInput).toHaveDisplayValue('00');
    await user.clear(endTimeMinuteInput);
    await user.type(endTimeMinuteInput, '54');
    expect(endTimeMinuteInput).toHaveDisplayValue('54');
  });

  it.each([
    ['08', '00', '12', '00'],
    ['09', '00', '13', '00'],
    ['09', '00', '12', '15'],
    ['15', '00', '19', '00'],
    ['09', '40', '12', '10'],
  ])(
    'forbids extending the session',
    async (
      startHour: string,
      startMinute: string,
      endHour: string,
      endMinute: string,
    ) => {
      const { user } = render(
        <EditSessionTimeAndCapacityForm
          date={'2024-06-10 07:00:00'}
          site={mockSite}
          existingSession={mockWeekAvailability__Summary[0].sessions[0]}
        />,
      );

      const startTimeHourInput = screen.getByRole('textbox', {
        name: 'Session start time - hour',
      });
      const startTimeMinuteInput = screen.getByRole('textbox', {
        name: 'Session start time - minute',
      });
      const endTimeHourInput = screen.getByRole('textbox', {
        name: 'Session end time - hour',
      });
      const endTimeMinuteInput = screen.getByRole('textbox', {
        name: 'Session end time - minute',
      });

      await user.clear(startTimeHourInput);
      await user.type(startTimeHourInput, startHour);

      await user.clear(startTimeMinuteInput);
      await user.type(startTimeMinuteInput, startMinute);

      await user.clear(endTimeHourInput);
      await user.type(endTimeHourInput, endHour);

      await user.clear(endTimeMinuteInput);
      await user.type(endTimeMinuteInput, endMinute);

      await user.click(screen.getByRole('button', { name: 'Continue' }));
      expect(
        screen.getByText(
          'Enter a start or end time that reduces the length of this session.',
        ),
      ).toBeInTheDocument();
    },
  );

  it('permits capacity data entry', async () => {
    const { user } = render(
      <EditSessionTimeAndCapacityForm
        date={'2024-06-10 07:00:00'}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
      />,
    );

    const capacityInput = screen.getByRole('spinbutton', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });

    expect(capacityInput).toHaveDisplayValue('2');
    await user.clear(capacityInput);
    await user.type(capacityInput, '1');
    expect(capacityInput).toHaveDisplayValue('1');
  });

  it('validates capacity entry', async () => {
    const { user } = render(
      <EditSessionTimeAndCapacityForm
        date={'2024-06-10 07:00:00'}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
      />,
    );

    const capacityInput = screen.getByRole('spinbutton', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });

    await user.clear(capacityInput);

    await user.click(screen.getByRole('button', { name: 'Continue' }));
    expect(screen.getByText('Capacity is required')).toBeInTheDocument();
  });

  it('forbids increasing capacity', async () => {
    const { user } = render(
      <EditSessionTimeAndCapacityForm
        date={'2024-06-10 07:00:00'}
        site={mockSite}
        existingSession={mockWeekAvailability__Summary[0].sessions[0]}
      />,
    );

    const capacityInput = screen.getByRole('spinbutton', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });

    expect(capacityInput).toHaveDisplayValue('2');
    await user.clear(capacityInput);
    await user.type(capacityInput, '3');
    expect(capacityInput).toHaveDisplayValue('3');

    await user.click(screen.getByRole('button', { name: 'Continue' }));
    waitFor(() => {
      expect(
        screen.getByText(
          'Enter a number that reduces the vaccinators or vaccinator spaces in this session.',
        ),
      ).toBeInTheDocument();
    });
  });
});
