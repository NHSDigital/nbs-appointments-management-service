import render from '@testing/render';
import { screen } from '@testing-library/react';
import { ReportsSelect } from './reports-select';
import { ReportType } from '../[reportType]/reports-template-wizard';

describe('ReportsSelect', () => {
  it('renders all sites report card only with valid permissions', () => {
    render(<ReportsSelect userPermissions={['reports:master-site-list']} />);

    const allSitesCard = screen.getByRole('link', { name: 'All sites report' });
    expect(allSitesCard).toBeInTheDocument();
    expect(allSitesCard).toHaveAttribute(
      'href',
      `/reports/${ReportType.MasterSiteList}`,
    );
  });

  it('renders users report card only with valid permissions', () => {
    render(<ReportsSelect userPermissions={['reports:siteusers']} />);

    const usersCard = screen.getByRole('link', { name: 'Users report' });
    expect(usersCard).toBeInTheDocument();
    expect(usersCard).toHaveAttribute(
      'href',
      `/reports/${ReportType.SitesUsers}`,
    );
  });

  it('renders the back link pointing to the sites list', () => {
    render(<ReportsSelect userPermissions={[]} />);

    const backLink = screen.getByRole('link', { name: 'Back' });
    expect(backLink).toBeInTheDocument();
    expect(backLink).toHaveAttribute('href', '/sites');
  });

  it.each([
    {
      permission: 'reports:master-site-list',
      name: 'All sites report',
      expectedHref: `/reports/${ReportType.MasterSiteList}`,
    },
    {
      permission: 'reports:siteusers',
      name: 'Users report',
      expectedHref: `/reports/${ReportType.SitesUsers}`,
    },
    {
      permission: 'reports:sitesummary',
      name: 'Site booking and capacity report',
      expectedHref: `/reports/${ReportType.SiteSummary}`,
    },
  ])(
    'navigates to the correct URL for $name',
    ({ permission, name, expectedHref }) => {
      render(<ReportsSelect userPermissions={[permission]} />);

      const card = screen.getByRole('link', { name });
      expect(card).toHaveAttribute('href', expectedHref);
    },
  );
});
