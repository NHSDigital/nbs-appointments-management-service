'use client';
import { usePathname } from 'next/navigation';
import { Header, HeaderAccountItem } from 'nhsuk-react-components';
import path from 'path';

export type NavigationLink = {
  label: string;
  href: string;
  pathToCheckIfCurrent?: string;
};

type Props = {
  userEmail?: string;
  navigationLinks?: NavigationLink[];
  showChangeSiteButton?: boolean;
};

const NhsPageHeader = ({
  userEmail,
  navigationLinks = [],
  showChangeSiteButton,
}: Props) => {
  const pathname = usePathname();

  return (
    <Header
      service={{
        href: '/manage-your-appointments/sites',
        text: 'Manage Your Appointments',
      }}
    >
      <Header.Account>
        <HeaderAccountItem href="#" icon>
          {userEmail}
        </HeaderAccountItem>
        <Header.AccountItem
          formProps={{
            action: '/manage-your-appointments/log-out',
            method: 'post',
          }}
        >
          Log out
        </Header.AccountItem>
        {showChangeSiteButton && (
          <HeaderAccountItem href="/manage-your-appointments/sites">
            Change site
          </HeaderAccountItem>
        )}
      </Header.Account>
      {navigationLinks.length > 0 && (
        <Header.Navigation>
          {navigationLinks.map((link, index) => {
            return (
              <Header.NavigationItem
                href={link.href}
                key={`naviagtion-item-${index}`}
                {...(isCurrentPage(pathname, link.pathToCheckIfCurrent)
                  ? { 'aria-current': 'true' }
                  : {})}
              >
                {link.label}
              </Header.NavigationItem>
            );
          })}
        </Header.Navigation>
      )}
    </Header>
  );
};

const isCurrentPage = (
  pathname: string,
  pathToCheckIfCurrent: string | undefined,
) => {
  if (!pathToCheckIfCurrent) {
    return false;
  }
  return pathname.includes(pathToCheckIfCurrent);
};

export default NhsPageHeader;
