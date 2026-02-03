import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';
import { parseToUkDatetime } from '@services/timeService';

export default class ConfirmedCancellationPage extends MYALayout {
  readonly title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly viewBookingsLink = this.page.getByText(
    /^View bookings without contact details/,
  );

  readonly viewAllBookingsLink = this.page.getByRole('link', {
    name: 'View all bookings for this week',
  });

  async verifyCorrectTitleDisplayed(date: string) {
    const parsedDate = parseToUkDatetime(date).format('dddd D MMMM');
    const heading = this.page.getByRole('heading', {
      name: `${parsedDate} cancelled`,
    });

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
