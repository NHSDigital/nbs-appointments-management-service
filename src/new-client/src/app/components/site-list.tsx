import { Site } from '@types';
import Link from 'next/link';

type Props = {
  sites: Site[];
};

const SiteList = ({ sites }: Props) => {
  return (
    <div className="nhsuk-card nhsuk-card--feature">
      <div className="nhsuk-card__content nhsuk-card__content--feature">
        <h2 className="nhsuk-card__heading nhsuk-card__heading--feature nhsuk-u-font-size-24">
          Choose a site
        </h2>
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
      </div>
    </div>
  );
};

export default SiteList;
