'use client';

import { NavigationLink } from '@types';
import Link from 'next/link';

type Props = {
  links: NavigationLink[];
};

export const SecondaryNavigation = ({ links }: Props) => {
  return (
    <nav className="app-secondary-navigation">
      <ul className="app-secondary-navigation__list">
        {links.map(link => (
          <li
            key={`secondary-nav-${link.id}`}
            className="app-secondary-navigation__list-item"
          >
            <Link
              className="app-secondary-navigation__link"
              href={link.href}
              {...(link.isCurrent ? { 'aria-current': 'page' } : {})}
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
};
