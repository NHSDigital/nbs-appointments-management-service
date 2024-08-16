import { HomePage } from './home-page';
import { render, screen } from '@testing-library/react';
import { mockSites } from './testing/data';

describe('Home Page', () => {
  it('should render the home page', () => {
    render(<HomePage sites={mockSites} />);

    expect(
      screen.getByRole('heading', { name: 'Choose a site' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Site Alpha' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Site Alpha' })).toHaveAttribute(
      'href',
      `site/1001`,
    );

    expect(screen.getByRole('link', { name: 'Site Beta' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Site Beta' })).toHaveAttribute(
      'href',
      `site/1002`,
    );

    expect(
      screen.getByRole('link', { name: 'Site Gamma' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Site Gamma' })).toHaveAttribute(
      'href',
      `site/1003`,
    );
  });
});
