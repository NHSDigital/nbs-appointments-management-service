import { Locator, Page } from '@playwright/test';
import { parseToUkDatetime } from '@services/timeService';
import { RootPage } from '@testing-page-objects';
import { expect } from '../../fixtures';

export default class ConfirmedCancellationPage extends RootPage {
  readonly viewBookingsLink: Locator;
  readonly viewAllBookingsLink: Locator;

  constructor(page: Page) {
    super(page);

    this.viewBookingsLink = page.getByText(
      /^View bookings without contact details/,
    );
    this.viewAllBookingsLink = page.getByRole('link', {
      name: 'View all bookings for this week',
    });
  }

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
