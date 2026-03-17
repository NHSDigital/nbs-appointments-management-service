'use client';
import {
  NotificationBanner,
  NotificationBannerTitle,
} from 'nhsuk-react-components';

type Props = {
  notification?: string;
};

const exists = (notification: string | undefined | null): boolean => {
  return (
    notification !== undefined && notification !== null && notification !== ''
  );
};

const NhsNotificationBanner = ({ notification }: Props) => {
  return exists(notification) ? (
    <NotificationBanner success>
      <NotificationBannerTitle>Success</NotificationBannerTitle>
      <p>{notification}</p>
    </NotificationBanner>
  ) : null;
};

export default NhsNotificationBanner;
