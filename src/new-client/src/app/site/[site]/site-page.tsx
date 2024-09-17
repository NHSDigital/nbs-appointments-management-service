import { Card } from '@nhsuk-frontend-components';
import { Site } from '@types';

interface SitePageProps {
  site: Site | undefined;
  permissions: string[];
}

export const SitePage = ({ site, permissions }: SitePageProps) => {
  if (site === undefined) throw new Error('You cannot access this site.');

  // TODO: Improve this as we add more cards gated by permissions.
  // We want to avoid rendering the card-group if there are no cards to show.
  const permissionsRelevantToCards = permissions.filter(
    p => p === 'users:view',
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
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
              <Card
                href={`${site.id}/users`}
                title="User Management"
                description="Assign roles to users to give them access to features at this site"
              />
            </li>
          )}
          <li className="nhsuk-grid-column-one-half nhsuk-card-group__item">
            <Card
              href={`${site.id}/attributes`}
              title="Site Management"
              description="Assign accessibility attributes to this site"
            />
          </li>
        </ul>
      )}
    </>
  );
};
