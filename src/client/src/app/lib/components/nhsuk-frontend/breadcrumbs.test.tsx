import { render, screen } from '@testing-library/react';
import { Breadcrumb, Breadcrumbs } from '@nhsuk-frontend-components';

const breadcrumbTrail: Breadcrumb[] = [
  { name: 'Home', href: '/sites' },
  { name: 'Page 1', href: '/page1' },
  { name: 'Page 2', href: '/page1/page2' },
  { name: 'Page 3' },
];

describe('NHSBreadcrumbs', () => {
  it('renders', () => {
    render(<Breadcrumbs />);

    expect(
      screen.getByRole('navigation', { name: 'Breadcrumb' }),
    ).toBeInTheDocument();
  });

  it('constructs a trail of breadcrumbs', () => {
    render(<Breadcrumbs trail={breadcrumbTrail} />);

    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute(
      'href',
      '/sites',
    );

    expect(screen.getByRole('link', { name: 'Page 1' })).toHaveAttribute(
      'href',
      '/page1',
    );

    expect(screen.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'href',
      '/page1/page2',
    );

    expect(
      screen.getByRole('listitem', { name: 'Page 3' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('listitem', { name: 'Page 3' }),
    ).not.toHaveAttribute('href');
  });

  it('renders an invisible back to button for the previous crumb in the trail', () => {
    render(<Breadcrumbs trail={breadcrumbTrail} />);
    expect(
      screen.getByRole('link', { name: /Back to Page 2/ }),
    ).toHaveAttribute('href', '/page1/page2');
  });

  it('renders an invisible back to home button if only one page away from home', () => {
    render(
      <Breadcrumbs
        trail={[{ name: 'Home', href: '/sites' }, { name: 'Page 1' }]}
      />,
    );
    expect(screen.getByRole('link', { name: /Back to Home/ })).toHaveAttribute(
      'href',
      '/sites',
    );
  });

  it('does not render an invisible back to button if on the home page', () => {
    render(<Breadcrumbs />);
    expect(
      screen.queryByRole('link', { name: /Back to Home/ }),
    ).not.toBeInTheDocument();
  });
});
