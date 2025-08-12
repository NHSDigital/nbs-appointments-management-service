import { fetchSite } from '@services/appointmentsService';
import { mockOfflineSite, mockSite } from '@testing/data';
import { Site } from '@types';
import { EditSiteStatusPage } from './edit-site-status-page';
import { render, screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<Site>>;

describe('Edit Site Status Page', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
    fetchSiteMock.mockResolvedValue(mockSite);
  });

  it('renders', async () => {
    const jsx = await EditSiteStatusPage({
      siteId: mockSite.id,
    });
    render(jsx);

    expect(
      screen.getByRole('heading', {
        name: 'Site Alpha Manage site visibility',
      }),
    ).toBeVisible();
    expect(
      screen.getByRole('button', { name: 'Save and continue' }),
    ).toBeInTheDocument();
  });

  it('displays the correct status and radio button labels for online site', async () => {
    const jsx = await EditSiteStatusPage({
      siteId: mockSite.id,
    });
    render(jsx);

    verifySummaryListItem('Current site status', 'Online');

    expect(
      screen.getByRole('radio', { name: 'Take site offline' }),
    ).toBeVisible();
    expect(
      screen.getByRole('radio', { name: 'Keep site online' }),
    ).toBeVisible();
  });

  it('displays the correct status and radio button labels for offline site', async () => {
    fetchSiteMock.mockResolvedValue(mockOfflineSite);
    const jsx = await EditSiteStatusPage({
      siteId: mockOfflineSite.id,
    });
    render(jsx);

    verifySummaryListItem('Current site status', 'Offline');

    expect(
      screen.getByRole('radio', { name: 'Make site online' }),
    ).toBeVisible();
    expect(
      screen.getByRole('radio', { name: 'Keep site offline' }),
    ).toBeVisible();
  });
});
