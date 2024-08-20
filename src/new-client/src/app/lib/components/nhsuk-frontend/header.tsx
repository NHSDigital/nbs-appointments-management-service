import Link from 'next/link';
import NhsLogo from './icons/nhs-logo';
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
    <header className="nhsuk-header nhsuk-header__transactional" role="banner">
      <div className="nhsuk-header__container">
        <div className="nhsuk-header__logo nhsuk-header__transactional--logo">
          <NhsLogo />
        </div>
        <div className="nhsuk-header__transactional-service-name">
          <Link
            className="nhsuk-header__transactional-service-name--link"
            href="/"
          >
            NHS Appointment Book
          </Link>
        </div>
        {children}
      </div>
    </header>
  );
};

export default Header;
