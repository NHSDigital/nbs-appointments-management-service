'use client';

import { SecondaryNavigation } from '@components/secondary-navigation';
import { GetCurrentDateTime } from '@services/timeService';
import { NavigationLink } from '@types';
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

  const links: NavigationLink[] = [
    {
      id: 'day-view',
      href: `/site/${site}/view-availability/daily-appointments?${queryString}`,
      isCurrent: pathname.includes('daily-appointments'),
      label: 'Day view',
    },
    {
      id: 'week-view',
      href: `/site/${site}/view-availability/week?${queryString}`,
      isCurrent: pathname.includes('week'),
      label: 'Week view',
    },
    {
      id: 'month-view',
      href: `/site/${site}/view-availability?${queryString}`,
      isCurrent:
        !pathname.includes('week') && !pathname.includes('daily-appointments'),
      label: 'Month view',
    },
  ];

  return <SecondaryNavigation links={links} />;
};
