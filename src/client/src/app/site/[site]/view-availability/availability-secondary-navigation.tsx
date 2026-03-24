'use client';

import { GetCurrentDateTime } from '@services/timeService';
import Link from 'next/link';
import { usePathname, useSearchParams } from 'next/navigation';

type Props = {
  site: string;
};

export const AvailabilitySecondaryNavigation = ({ site }: Props) => {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const date = searchParams.get('date') ?? GetCurrentDateTime('YYYY-MM-DD');
  const page = searchParams.get('page');

  const query = new URLSearchParams();
  query.set('date', date);

  if (page) {
    query.set('page', page);
  } else {
    query.set('page', '1');
  }

  const queryString = query.toString();

  return (
    <nav className="app-secondary-navigation">
      <ul className="app-secondary-navigation__list">
        <li className="app-secondary-navigation__list-item">
          <Link
            className="app-secondary-navigation__link"
            href={`/site/${site}/view-availability/daily-appointments?${queryString}`}
            {...(pathname.includes('daily-appointments')
              ? { 'aria-current': true }
              : {})}
          >
            Day view
          </Link>
        </li>
        <li className="app-secondary-navigation__list-item">
          <Link
            className="app-secondary-navigation__link"
            href={`/site/${site}/view-availability/week?${queryString}`}
            {...(pathname.includes('week') ? { 'aria-current': true } : {})}
          >
            Week view
          </Link>
        </li>
        <li className="app-secondary-navigation__list-item">
          <Link
            className="app-secondary-navigation__link"
            href={`/site/${site}/view-availability?${queryString}`}
            {...(!pathname.includes('week') &&
            !pathname.includes('daily-appointments')
              ? { 'aria-current': true }
              : {})}
          >
            Month view
          </Link>
        </li>
      </ul>
    </nav>
  );
};
