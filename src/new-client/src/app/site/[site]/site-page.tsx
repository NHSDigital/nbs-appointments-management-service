import { Card } from '@nhsuk-frontend-components';
import { Site } from '@types';

interface SitePageProps {
  site: Site | undefined;
}

export const SitePage = ({ site }: SitePageProps) => {
  if (site === undefined) throw new Error('You cannot access this site.');

  return (
    <>
      <div className="nhsuk-card">
        <div className="nhsuk-card__content">
          <h3 className="nhsuk-card__heading">{site.name}</h3>
          <p className="nhsuk-card__description">{site.address}</p>
        </div>
      </div>
      <ul className="nhsuk-grid-row nhsuk-card-group">
        <li className="nhsuk-grid-column-two-thirds nhsuk-card-group__item">
          <Card
            href={`${site.id}/users`}
            title="User Management"
            description="Assign roles to users to give them access to features at this site"
          />
        </li>
      </ul>
    </>
  );
};
