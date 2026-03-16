'use client';

import { GetCurrentDateTime } from '@services/timeService';
import Link from 'next/link';
import { usePathname, useSearchParams } from 'next/navigation';

export default function AvailabilitySecondaryNav({ site }: { site: string }) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const date = searchParams.get('date') ?? GetCurrentDateTime('YYYY-MM-DD');
  const page = searchParams.get('page');

  const query = new URLSearchParams();
  query.set('date', date);
  if (page) {
    query.set('page', page);
  }

  const queryString = query.toString();

  const linkClass = (path: string) =>
    `app-secondary-navigation__link ${
      pathname.includes(path) ? 'app-secondary-navigation__link:active' : ''
    }`;

  return (
    <nav className="app-secondary-navigation">
      <ul className="app-secondary-navigation__list">
        <li>
          <Link
            className={linkClass(`/site/${site}/view-availability`)}
            href={`/site/${site}/view-availability/daily-appointments?${queryString}`}
          >
            Day view
          </Link>
        </li>
        <li>
          <Link
            className={linkClass(`/site/${site}/view-availability/week`)}
            href={`/site/${site}/view-availability/week?${queryString}`}
          >
            Week view
          </Link>
        </li>
        <li>
          <Link
            className={linkClass(
              `/site/${site}/view-availability/daily-appointments`,
            )}
            href={`/site/${site}/view-availability?${queryString}`}
          >
            Month view
          </Link>
        </li>
      </ul>
    </nav>
  );
}
