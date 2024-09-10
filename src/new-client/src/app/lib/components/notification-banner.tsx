'use client';
import CloseNotificationForm from '@components/close-notification-form';

type Props = {
  notification?: string;
};

const exists = (notification: string | undefined | null): boolean => {
  return (
    notification !== undefined && notification !== null && notification !== ''
  );
};

const NotificationBanner = ({ notification }: Props) => {
  return exists(notification) ? (
    <div className="nhsuk-warning-callout-custom">
      <div className="nhsuk-warning-callout-custom__container">
        {notification}
        <CloseNotificationForm />
      </div>
    </div>
  ) : null;
};

export default NotificationBanner;
