import { render, screen } from '@testing-library/react';
import Page from './page';
import { fetchUserProfile } from '@services/appointmentsService';

jest.mock('@services/appointmentsService');
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const fetchUserProfileMock = fetchUserProfile as jest.Mock<any>;

describe('Site Page', () => {
  it('should render the appropriate site information', async () => {
    fetchUserProfileMock.mockResolvedValue({
      emailAddress: 'test@test.com',
      availableSites: [
        { id: '1000', name: 'Site Alpha', address: 'Alpha Street' },
        { id: '1001', name: 'Site Beta', address: 'Beta Street' },
      ],
    });

    const jsx = await Page({ params: { site: '1001' } });
    await render(jsx);
    expect(screen.getByText('Site Beta')).toBeVisible();
    expect(screen.getByText('Beta Street')).toBeVisible();
    expect(await screen.queryByText('Alpha Beta')).toBeNull();
    expect(await screen.queryByText('Alpha Street')).toBeNull();
  });
});
