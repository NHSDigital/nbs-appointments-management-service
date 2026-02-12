'use client';
import { Header, HeaderAccountItem } from 'nhsuk-react-components';

export type NavigationLink = {
  label: string;
  href: string;
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
  return (
    <Header
      service={{
        href: '/manage-your-appointments/sites',
        text: 'Manage Your Appointments',
      }}
    >
      <Header.Account>
        {userEmail && (
          <HeaderAccountItem href="#" icon>
            {userEmail}
          </HeaderAccountItem>
        )}
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

export default NhsPageHeader;
