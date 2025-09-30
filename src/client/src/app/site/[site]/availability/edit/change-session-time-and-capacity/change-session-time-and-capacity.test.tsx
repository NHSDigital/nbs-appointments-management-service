import { screen } from '@testing-library/react';
import ChangeSessionTimeAndCapacityPage from './change-session-time-and-capacity';
import { mockSite, mockSession4, mockSession4A } from '@testing/data';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';
import render from '@testing/render';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

jest.mock('@services/appointmentsService');
const mockCancelBooking = jest.spyOn(appointmentsService, 'cancelAppointment');

describe('Change Session Time And Capacity Page', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('renders', () => {
    render(
      <ChangeSessionTimeAndCapacityPage
        site={mockSite}
        date="2024-11-10"
        originalSession={mockSession4}
        updatedSession={mockSession4A}
        bookingReferences={['TEST123']}
        orphanedCount={1}
        bookingsCount={1}
      />,
    );

    verifySummaryListItem('Date and time', ['10 November 2024', '14:05pm']);

    expect(
      screen.getByRole('radio', {
        name: 'Yes, cancel the appointments and change this session',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Cancel appointment' }),
    ).toBeInTheDocument();
  });

  it('does not call the change appointments and shows an error when no option is selected', async () => {
    const { user } = render(
      <ChangeSessionTimeAndCapacityPage
        site={mockSite}
        date="2024-11-10"
        originalSession={mockSession4}
        updatedSession={mockSession4A}
        bookingReferences={['TEST123']}
        orphanedCount={1}
        bookingsCount={1}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Select an optiion')).toBeInTheDocument();
    expect(mockCancelBooking).not.toHaveBeenCalled();
  });

  it('shows change session when no is selected', async () => {
    const { user } = render(
      <ChangeSessionTimeAndCapacityPage
        site={mockSite}
        date="2024-11-10"
        originalSession={mockSession4}
        updatedSession={mockSession4A}
        bookingReferences={['TEST123']}
        orphanedCount={1}
        bookingsCount={1}
      />,
    );
    await user.click(
      screen.getByRole('radio', {
        name: 'No, do not cancel the appointments but change this session',
      }),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('Are you sure you want to change the session?'),
    ).toBeInTheDocument();
    expect(mockCancelBooking).not.toHaveBeenCalled();
  });
});
