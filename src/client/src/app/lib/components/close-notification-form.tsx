import { removeNotification } from './remove-notification';

const CloseNotificationForm = () => {
  return (
    <form action={removeNotification}>
      <button
        type="submit"
        className="nhsuk-warning-callout-custom__close-button"
      >
        Close
      </button>
    </form>
  );
};

export default CloseNotificationForm;
