import Link from 'next/link';
import { Site } from '@types';
import NhsCard from '@components/nhs-card';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  return (
    <NhsCard title="Choose a site">
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
    </NhsCard>
  );
};

export default SiteList;
