'use client';
import { usePathname } from 'next/navigation';
import { Header, HeaderAccountItem } from 'nhsuk-react-components';

export type NavigationLink = {
  label: string;
  href: string;
  pathToCheckIfCurrent?: LinkCurrentCheck;
};

export type LinkCurrentCheck = {
  pathToCheckIfCurrent?: string;
  checkType: 'includes' | 'endsWith';
};

type Props = {
  userEmail?: string;
  navigationLinks?: NavigationLink[];
  showChangeSiteButton?: boolean;
  siteName?: string;
};

const NhsPageHeader = ({
  userEmail,
  navigationLinks = [],
  showChangeSiteButton,
  siteName,
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
        {showChangeSiteButton && (
          <HeaderAccountItem href="/manage-your-appointments/sites">
            {siteName}
          </HeaderAccountItem>
        )}
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
      </Header.Account>
      {navigationLinks.length > 0 && (
        <Header.Navigation>
          {navigationLinks.map((link, index) => {
            return (
              <Header.NavigationItem
                href={link.href}
                key={`naviagtion-item-${index}`}
                {...(isCurrentPage(pathname, link)
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

// Adding checkType so the new 'Home' link does not have the aria-current attribute
// when other pages were also 'current' as they included 'site/:id' in their path as well
const isCurrentPage = (pathname: string, link: NavigationLink) => {
  const { pathToCheckIfCurrent, checkType } = link.pathToCheckIfCurrent || {};
  if (!pathToCheckIfCurrent) {
    return false;
  }
  if (checkType === 'includes') {
    return pathname.includes(pathToCheckIfCurrent);
  }
  if (checkType === 'endsWith') {
    return pathname.endsWith(pathToCheckIfCurrent);
  }

  return false;
};

export default NhsPageHeader;
