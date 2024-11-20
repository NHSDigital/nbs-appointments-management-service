import { Card } from '@nhsuk-frontend-components';
import { Site } from '@types';

interface SitePageProps {
  site: Site;
  permissions: string[];
}

export const SitePage = ({ site, permissions }: SitePageProps) => {
  // TODO: We want to avoid rendering the card-group if there are no cards to show.

  return (
    <>
      <div className="nhsuk-card">
        <div className="nhsuk-card__content">
          <h3 className="nhsuk-card__heading">{site.name}</h3>
          <p className="nhsuk-card__description">{site.address}</p>
        </div>
      </div>
      <ul className="nhsuk-grid-row nhsuk-card-group">
        {permissions.includes('availability:set-setup') && (
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`${site.id}/create-availability`}
              title="Create availability"
              description="Create and edit available dates and sessions for your site"
            />
          </li>
        )}
        {permissions.includes('users:view') && (
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`${site.id}/users`}
              title="User management"
              description="Assign roles to users to give them access to features at this site"
            />
          </li>
        )}
        {(permissions.includes('site:manage') ||
          permissions.includes('site:view')) && (
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`${site.id}/details`}
              title="Site management"
              description="Assign accessibility attributes to this site"
            />
          </li>
        )}
      </ul>
    </>
  );
};
