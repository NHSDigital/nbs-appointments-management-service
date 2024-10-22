import { Breadcrumbs, Breadcrumb } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import NotificationBanner from './notification-banner';
import { cookies } from 'next/headers';
import NhsFooter from './nhs-footer';
import NhsMainContainer from './nhs-main-container';

type Props = {
  title: string;
  children: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
};

const NhsPage = ({
  title,
  children = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
}: Props) => {
  const notification = cookies().get('ams-notification')?.value;

  return (
    <>
      <NhsHeader />
      <Breadcrumbs
        trail={[
          ...breadcrumbs,
          ...(!omitTitleFromBreadcrumbs ? [{ name: title }] : []),
        ]}
      />
      <NhsMainContainer>
        <h1 className="app-page-heading">{title}</h1>
        <NotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

export default NhsPage;
