import { render, screen } from '@testing-library/react';
import NhsErrorPage from './nhs-error-page';
import { fetchUserProfile } from '@services/appointmentsService';
import { UserProfile } from '@types';

jest.mock('@services/appointmentsService');
const fetchUserProfileMock = fetchUserProfile as jest.Mock<
  Promise<UserProfile | undefined>
>;

describe('Nhs Page', () => {
  beforeEach(() => {
    fetchUserProfileMock.mockResolvedValue({
      emailAddress: 'test@nhs.net',
      availableSites: [
        {
          id: 'TEST',
          name: 'Test site',
          address: '',
        },
      ],
    });
  });
  it('shows the correct title', async () => {
    const jsx = await NhsErrorPage({
      title: 'Test title',
      message: 'This is an error',
      breadcrumbs: [],
    });
    render(jsx);
    expect(screen.getByRole('heading', { name: /Test title/i })).toBeVisible();
  });
  it('shows the correct breadcrumbs including title', async () => {
    const jsx = await NhsErrorPage({
      title: 'Test title',
      message: 'This is an error',
      breadcrumbs: [
        { name: 'Level One', href: '/' },
        { name: 'Level Two', href: '/' },
      ],
    });
    render(jsx);
    expect(screen.getByRole('link', { name: 'Level One' })).toBeVisible();
    expect(screen.getByRole('link', { name: 'Level Two' })).toBeVisible();
    expect(screen.getByRole('listitem', { name: /Test title/i })).toBeVisible();
  });
  it('shows the correct breadcrumbs without title', async () => {
    const jsx = await NhsErrorPage({
      title: 'Test title',
      message: 'This is an error',
      breadcrumbs: [
        { name: 'Level One', href: '/' },
        { name: 'Level Two', href: '/' },
      ],
      omitTitleFromBreadcrumbs: true,
    });
    render(jsx);
    expect(screen.getByRole('link', { name: 'Level One' })).toBeVisible();
    expect(screen.getByRole('link', { name: 'Level Two' })).toBeVisible();
    expect(screen.queryByRole('listitem', { name: /Test title/i })).toBeNull();
  });
  it('displays the correct error message', async () => {
    const jsx = await NhsErrorPage({
      title: 'Test title',
      message: 'This is an error',
      breadcrumbs: [],
    });
    render(jsx);
    expect(screen.getByText('This is an error')).toBeVisible();
  });
});
