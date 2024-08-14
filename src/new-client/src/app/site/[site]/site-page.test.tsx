import { render, screen } from '@testing-library/react';
import { SitePage } from './page';
import { mockSites } from '../../testing/data';

describe('Site Page', () => {
  it('should render the appropriate site information', async () => {
    const mockSite = mockSites[0];

    render(<SitePage site={mockSite} />);

    expect(
      screen.getByRole('heading', { name: mockSite.name }),
    ).toBeInTheDocument();

    expect(screen.getByText(mockSite.address)).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'User Management' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'User Management' }),
    ).toHaveAttribute('href', `${mockSite.id}/users`);
  });
});
