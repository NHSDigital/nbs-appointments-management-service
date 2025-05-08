import render from '@testing/render';
import { screen, within } from '@testing-library/react';
import NotificationBanner from './notification-banner';

jest.mock('@components/close-notification-form', () => {
  const MockCloseNotificationForm = () => {
    return <div>Close</div>;
  };
  return MockCloseNotificationForm;
});

describe('Notification Banner', () => {
  it('renders', () => {
    render(<NotificationBanner notification="Test Notification" />);
    expect(screen.getByRole('banner')).toBeInTheDocument();
  });

  it('displays the notification text', () => {
    render(<NotificationBanner notification="Test Notification" />);

    expect(
      within(screen.getByRole('banner')).getByText('Test Notification'),
    ).toBeInTheDocument();
  });
});
