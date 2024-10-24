'use server';
import { Breadcrumbs, Breadcrumb } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import { NhsHeader } from '@components/nhs-header';
import NotificationBanner from '@components/notification-banner';
import { cookies } from 'next/headers';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import NhsHeaderLogOut from '@components/nhs-header-log-out';

type Props = {
  title: string;
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
};

const NhsPage = ({
  title,
  children = null,
  headerAuthComponent = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
}: Props) => {
  const notification = cookies().get('ams-notification')?.value;

  return (
    <>
      <NhsHeader>{headerAuthComponent ?? NhsHeaderLogOut()}</NhsHeader>
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
