import Link from 'next/link';
import { Site } from '@types';
import { Card } from '@nhsuk-frontend-components';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  return (
    <Card title="Choose a site">
      <ul className="nhsuk-list nhsuk-list--border">
        {sites.map(s => (
          <li key={s.id}>
            <Link
              aria-label={s.name}
              className="nhsuk-back-link__link"
              href={`site/${s.id}`}
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
