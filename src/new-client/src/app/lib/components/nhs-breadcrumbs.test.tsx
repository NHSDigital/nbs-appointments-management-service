import { render, screen } from '@testing-library/react';
import Breadcrumbs, { Breadcrumb } from './nhs-breadcrumbs';

const breadcrumbTrail: Breadcrumb[] = [
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

  it('always renders the home crumb by default', () => {
    render(<Breadcrumbs />);

    expect(screen.getByRole('link', { name: 'Home' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute(
      'href',
      '/',
    );
  });

  it('constructs a trail of breadcrumbs', () => {
    render(<Breadcrumbs trail={breadcrumbTrail} />);

    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute(
      'href',
      '/',
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
    render(<Breadcrumbs trail={[{ name: 'Page 1' }]} />);
    expect(screen.getByRole('link', { name: /Back to Home/ })).toHaveAttribute(
      'href',
      '/',
    );
  });

  it('does not render an invisible back to button if on the home page', () => {
    render(<Breadcrumbs />);
    expect(
      screen.queryByRole('link', { name: /Back to Home/ }),
    ).not.toBeInTheDocument();
  });
});
