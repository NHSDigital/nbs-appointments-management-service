import Link from 'next/link';
import { When } from '@components/when';
import NhsHeaderLogIn from '@components/nhs-header-log-in';
import NhsHeaderLogOut from '@components/nhs-header-log-out';
import NhsLogo from '@components/nhs-logo';

type NhsHeaderProps = {
  userEmail?: string;
  // We still show the header in the error handling page, which has no access to the userProfile call
  // So to avoid it showing "Log in" when you're already logged in, we need to pass this prop for now
  // This is a temporary solution until we have a better way to handle this
  showAuthControls?: boolean;
};

export const NhsHeader = ({
  userEmail,
  showAuthControls = true,
}: NhsHeaderProps) => {
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
        <When condition={showAuthControls}>
          <When condition={userEmail !== undefined}>
            <div className="header__content" id="content-header">
              <div
                className="header__user-control"
                role="navigation"
                aria-label="Primary navigation"
              >
                <ul className="header__user-control-list">
                  <li className="header__user-control-item">{userEmail}</li>
                  <li className="header__user-control-item">
                    <NhsHeaderLogOut />
                  </li>
                </ul>
              </div>
            </div>
          </When>
          <When condition={userEmail === undefined}>
            <div className="header__content" id="content-header">
              <div
                className="header__user-control"
                role="navigation"
                aria-label="Primary navigation"
              >
                <ul className="header__user-control-list">
                  <li className="header__user-control-item">
                    <NhsHeaderLogIn />
                  </li>
                </ul>
              </div>
            </div>
          </When>
        </When>
      </div>
    </header>
  );
};
