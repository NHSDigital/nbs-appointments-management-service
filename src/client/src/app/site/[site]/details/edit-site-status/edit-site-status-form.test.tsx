import { useRouter } from 'next/navigation';
import EditSiteStatusForm from './edit-site-status-form';
import { mockOfflineSite, mockSite } from '@testing/data';
import { screen } from '@testing-library/dom';
import * as appointmentsService from '@services/appointmentsService';
import render from '@testing/render';
import { UserEvent } from '@testing-library/user-event';

jest.mock('@services/appointmentsService');

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

const mockUpdateSiteStatus = jest.spyOn(
  appointmentsService,
  'updateSiteStatus',
);

let user: UserEvent;

describe('Edit Site Status Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('renders', async () => {
    render(<EditSiteStatusForm site={mockSite} />);

    expect(
      screen.getByRole('group', { name: 'What do you want to do?' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', { name: 'Take site offline' }),
    ).toBeVisible();
    expect(
      screen.getByRole('radio', { name: 'Keep site online' }),
    ).toBeVisible();
  });

  it('submits the form with the correct value', async () => {
    const renderResult = render(<EditSiteStatusForm site={mockOfflineSite} />);
    user = renderResult.user;

    const onlineRadio = screen.getByRole('radio', { name: 'Make site online' });
    await user.click(onlineRadio);

    const saveButton = screen.getByRole('button', {
      name: 'Save and continue',
    });
    await user.click(saveButton);

    expect(mockUpdateSiteStatus).toHaveBeenCalledWith(
      mockOfflineSite.id,
      'Online',
    );
  });
});
