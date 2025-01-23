import Link from 'next/link';
import { Site } from '@types';
import { Card } from '@nhsuk-frontend-components';
import { sortSitesByName } from '@sorting';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  const sortedSites = sites.toSorted(sortSitesByName);

  return (
    <Card title="Choose a site">
      <ul className="nhsuk-list nhsuk-list--border">
        {sortedSites.map(s => (
          <li key={s.id}>
            <Link
              aria-label={s.name}
              className="nhsuk-back-link__link"
              href={`/site/${s.id}`}
            >
              {s.name}
            </Link>
          </li>
        ))}
      </ul>
    </Card>
  );
};

export default SiteList;
