import Link from 'next/link';
import NhsLogo from '@components/nhsuk-frontend/icons/nhs-logo';
import { ReactNode } from 'react';

export type NavigationLink = {
  label: string;
  href: string;
};

type Props = {
  children?: ReactNode;
  navigationLinks?: NavigationLink[];
  showChangeSiteButton?: boolean;
};

/**
 * A header component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * TODO: we're created a transactional header, but from the docs it sounds like we should be using Organisational?
 * @see https://service-manual.nhs.uk/design-system/components/header
 */
const Header = ({
  children,
  navigationLinks = [],
  showChangeSiteButton = true,
}: Props) => {
  return (
    <header className="nhsuk-header" role="banner">
      <div className="nhsuk-header__container">
        <div className="nhsuk-header__logo">
          <Link
            className="nhsuk-header__link nhsuk-header__link--service"
            href="/"
            aria-label="NHS Appointment Book"
          >
            <NhsLogo />

            <span className="nhsuk-header__service-name">
              NHS Appointment Book
            </span>
          </Link>
        </div>

        <div className="nhsuk-header__content" id="content-header">
          <div className="nhsuk-header-custom__user-controls">
            <div
              className="nhsuk-header-custom__user-control-wrap"
              id="wrap-user-controls"
            >
              {children}
              {showChangeSiteButton && (
                <span className="nhsuk-header-custom__user-control">
                  <Link
                    href="/"
                    locale={false}
                    className="nhsuk-header-custom__user-control-link"
                  >
                    Change site
                  </Link>
                </span>
              )}
            </div>
          </div>
        </div>
      </div>

      {navigationLinks.length > 0 && (
        <div className="nhsuk-navigation-container">
          <nav
            className="nhsuk-navigation"
            id="header-navigation"
            role="navigation"
            aria-label="Primary navigation"
          >
            <ul className="nhsuk-header__navigation-list-custom">
              {navigationLinks.map((link, linkIndex) => (
                <li
                  className="nhsuk-header__navigation-item"
                  key={`navigation-link-${linkIndex}`}
                >
                  <Link
                    className="nhsuk-header__navigation-link"
                    href={link.href}
                  >
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      )}
    </header>
  );
};

export default Header;
