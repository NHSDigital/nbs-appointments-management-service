import { Card } from '@nhsuk-frontend-components';
import { Site } from '@types';

interface SitePageProps {
  site: Site;
  permissions: string[];
}

export const SitePage = ({ site, permissions }: SitePageProps) => {
  const permissionsRelevantToCards = permissions.filter(
    p =>
      p === 'users:view' ||
      p === 'site:manage' ||
      p === 'site:view' ||
      p === 'availability:set-setup' ||
      p === 'availability:query',
  );

  return (
    <>
      <div className="nhsuk-card">
        <div className="nhsuk-card__content">
          <h3 className="nhsuk-card__heading">{site.name}</h3>
          <p className="nhsuk-card__description">{site.address}</p>
        </div>
      </div>
      {permissionsRelevantToCards.length > 0 && (
        <ul className="nhsuk-grid-row nhsuk-card-group">
          {permissionsRelevantToCards.includes('availability:query') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/view-availability`}
                title="View availability and manage appointments for your site"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('availability:set-setup') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/create-availability`}
                title="Create availability"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card href={`${site.id}/users`} title="Manage users" />
            </li>
          )}
          {(permissionsRelevantToCards.includes('site:manage') ||
            permissionsRelevantToCards.includes('site:view')) && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/details`}
                title="Change site details and accessibility information"
              />
            </li>
          )}
        </ul>
      )}
    </>
  );
};
