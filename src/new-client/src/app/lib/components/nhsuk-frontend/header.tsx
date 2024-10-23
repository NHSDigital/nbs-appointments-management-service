import Link from 'next/link';
import NhsLogo from '@components/nhsuk-frontend/icons/nhs-logo';
import { ReactNode } from 'react';

type Props = {
  children?: ReactNode;
};

/**
 * A header component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * TODO: we're created a transactional header, but from the docs it sounds like we should be using Organisational?
 * @see https://service-manual.nhs.uk/design-system/components/header
 */
const Header = ({ children }: Props) => {
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
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;
