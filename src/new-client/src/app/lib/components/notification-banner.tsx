'use client';
import { removeNotification } from './remove-notification';

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
        <form action={removeNotification}>
          <button
            type="submit"
            className="nhsuk-warning-callout-custom__close-button"
          >
            Close
          </button>
        </form>
      </div>
    </div>
  ) : null;
};

export default NotificationBanner;
