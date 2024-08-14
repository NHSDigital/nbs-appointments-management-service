import Link from 'next/link';
import React from 'react';

export interface Breadcrumb {
  name: string;
  href?: string;
}

interface Props {
  trail?: Breadcrumb[];
}

const Breadcrumbs = ({ trail = [] }: Props) => {
  const trailWithHome = [{ name: 'Home', href: '/' }, ...trail];

  const previousCrumb =
    trailWithHome.length > 1 ? trailWithHome.slice(-2)[0] : undefined;

  return (
    <nav className="nhsuk-breadcrumb" aria-label="Breadcrumb">
      <div className="nhsuk-width-container">
        <ol className="nhsuk-breadcrumbs__list">
          {trailWithHome.map((breadcrumb, index) => {
            return (
              <li
                key={`$breadcrumb_${index}`}
                className="nhsuk-breadcrumb__item"
                aria-label={breadcrumb.name}
              >
                {breadcrumb.href ? (
                  <Link
                    className="nhsuk-breadcrumb__link"
                    href={breadcrumb.href}
                  >
                    {breadcrumb.name}
                  </Link>
                ) : (
                  breadcrumb.name
                )}
              </li>
            );
          })}
        </ol>
        {previousCrumb && (
          <p className="nhsuk-breadcrumb__back">
            <a className="nhsuk-breadcrumb__backlink" href={previousCrumb.href}>
              <span className="nhsuk-u-visually-hidden">Back to &nbsp;</span>
              {previousCrumb.name}
            </a>
          </p>
        )}
      </div>
    </nav>
  );
};

export default Breadcrumbs;
