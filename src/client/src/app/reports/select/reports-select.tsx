'use client';
import NhsHeading from '@components/nhs-heading';
import { Card, BackLink } from '@nhsuk-frontend-components';
import { ReportType } from '../[reportType]/reports-template-wizard';

interface Props {
  userPermissions: string[];
}

enum ReportsPermissions {
  UsersReport = 'reports:siteusers',
  AllSitesReport = 'reports:master-site-list',
  SiteSummaryReport = 'reports:sitesummary',
}

export const ReportsSelect = ({ userPermissions }: Props) => {
  const hasPermission = (permission: string): boolean => {
    return userPermissions.includes(permission);
  };

  return (
    <>
      <BackLink href={'/sites'} renderingStrategy="server" text="Back" />

      <NhsHeading title="Select a report" />

      <ul className="nhsuk-grid-row nhsuk-card-group">
        {hasPermission(ReportsPermissions.SiteSummaryReport) && (
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`/reports/${ReportType.SiteSummary}`}
              title="Site booking and capacity report"
              description="Total bookings and capacity for all of your sites"
            />
          </li>
        )}
        {hasPermission(ReportsPermissions.AllSitesReport) && (
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`/reports/${ReportType.MasterSiteList}`}
              title="All sites report"
              description="A list of all sites and their IDs"
            />
          </li>
        )}
        {hasPermission(ReportsPermissions.UsersReport) && (
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`/reports/${ReportType.SitesUsers}`}
              title="Users report"
              description="All users at your sites and their last login"
            />
          </li>
        )}
      </ul>
    </>
  );
};
