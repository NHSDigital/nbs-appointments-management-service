'use server';
import { Breadcrumbs, Breadcrumb, Header } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import NotificationBanner from '@components/notification-banner';
import { cookies } from 'next/headers';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import NhsHeaderLogOut from '@components/nhs-header-log-out';
import NhsHeading, { NhsHeadingProps } from './nhs-heading';

type Props = {
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
} & NhsHeadingProps;

const NhsPage = ({
  title,
  caption,
  children = null,
  headerAuthComponent = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
}: Props) => {
  const notification = cookies().get('ams-notification')?.value;

  return (
    <>
      <Header>{headerAuthComponent ?? NhsHeaderLogOut()}</Header>
      <Breadcrumbs
        trail={[
          ...breadcrumbs,
          ...(!omitTitleFromBreadcrumbs ? [{ name: title }] : []),
        ]}
      />
      <NhsMainContainer>
        <NhsHeading title={title} caption={caption} />
        <NotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

export default NhsPage;
