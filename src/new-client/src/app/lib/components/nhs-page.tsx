import { Breadcrumbs, Breadcrumb, Footer } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import { fetchUserProfile } from '@services/appointmentsService';
import LogInButton from './log-in-button';
import NotificationBanner from './notification-banner';
import { cookies } from 'next/headers';

type Props = {
  title: string;
  children: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
};

const NhsPage = async ({
  title,
  children = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
}: Props) => {
  const userProfile = await fetchUserProfile();
  const notification = cookies().get('ams-notification')?.value;

  return (
    <>
      <NhsHeader userEmail={userProfile?.emailAddress} />
      <Breadcrumbs
        trail={[
          ...breadcrumbs,
          ...(!omitTitleFromBreadcrumbs ? [{ name: title }] : []),
        ]}
      />
      <div className="nhsuk-width-container">
        <main className="nhsuk-main-wrapper" id="main-content" role="main">
          <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-full">
              <h1 className="app-page-heading">{title}</h1>
              <NotificationBanner notification={notification} />
              {userProfile === undefined ? (
                <>
                  <p>
                    You are currently not signed in. You must sign in to access
                    this service.
                  </p>
                  <LogInButton />
                </>
              ) : (
                children
              )}
            </div>
          </div>
        </main>
      </div>
      <Footer
        supportLinks={
          [
            // { text: 'Accessibility statement', href: '/accessibility-statement' },
            // { text: 'Contact us', href: '/contact-us' },
            // { text: 'Cookies', href: '/cookies' },
            // { text: 'Privacy policy', href: '/privacy-policy' },
            // { text: 'Terms and conditions', href: '/terms-and-conditions' },
          ]
        }
      />
    </>
  );
};

export default NhsPage;
