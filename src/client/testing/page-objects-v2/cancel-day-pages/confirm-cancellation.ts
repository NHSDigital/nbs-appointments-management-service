import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';

export default class ConfirmedCancellationPage extends MYALayout {

  title = this.page.getByRole('heading');

  readonly viewBookingsLink = this.page.getByText(
    /^View bookings without contact details/,
  );

  readonly viewAllBookingsLink = this.page.getByRole('link', {
    name: 'View all bookings for this week',
  });

  async verifyHeadingDisplayed(requiredDate: string) {
    const heading = this.title.filter({ hasText: `${requiredDate} cancelled` });
    await expect(heading).toBeVisible();
  }

  async verifyViewCancelledApptWithoutContactDetailsVisibility(
    visible: boolean,
  ) {
    visible
      ? await expect(this.viewBookingsLink).toBeVisible()
      : await expect(this.viewBookingsLink).not.toBeVisible();
  }
}
