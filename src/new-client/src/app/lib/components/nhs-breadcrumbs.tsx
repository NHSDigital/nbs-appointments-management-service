import Link from 'next/link';
import React from 'react';

export interface Breadcrumb {
  name: string;
  href?: string;
}

interface Props {
  breadcrumbs?: Breadcrumb[];
}

const Breadcrumbs = ({ breadcrumbs = [] }: Props) => {
  const breadCrumbsWithHome = [{ name: 'Home', href: '/' }, ...breadcrumbs];

  return (
    <nav className="nhsuk-breadcrumb" aria-label="Breadcrumb">
      <div className="nhsuk-width-container">
        <ol className="nhsuk-breadcrumbs__list">
          {breadCrumbsWithHome.map((breadcrumb, index) => {
            return (
              <li
                key={`$breadcrumb_${index}`}
                className="nhsuk-breadcrumb__item"
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
        {breadCrumbsWithHome && breadCrumbsWithHome.length > 1 && (
          <p className="nhsuk-breadcrumb__back">
            <a className="nhsuk-breadcrumb__backlink" href="#">
              <span className="nhsuk-u-visually-hidden">Back to &nbsp;</span>
              {breadCrumbsWithHome.slice(-1)[0].name}
            </a>
          </p>
        )}
      </div>
    </nav>
  );
};

export default Breadcrumbs;
