import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class WeekViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;
  readonly previousButton: Locator;
  readonly backToMonthButton: Locator;
  readonly addAvailabilityButton: Locator;

  constructor(page: Page) {
    super(page);
    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });
    this.previousButton = page.getByRole('link', {
      name: 'Previous',
    });
    this.backToMonthButton = page.getByRole('link', {
      name: 'Back to month view',
    });
    this.addAvailabilityButton = page.getByRole('link', {
      name: 'Add availability to this day',
    });
  }

  async verifyWeekViewDisplayed() {
    await expect(this.backToMonthButton).toBeVisible();
  }

  async addAvailability(
    requiredDate: string,
    startTime: string,
    endTime: string,
    capacity: string,
    duration: string,
  ) {
    // await this.page
    //   .getByRole('main')
    //   .filter({ has: this.page.getByText(requiredDate) })
    //   .getByRole('link', { name: 'Add availability to this day' }).click();
    await this.page
      .getByRole('link', { name: 'Add availability to this day' })
      .filter({ has: this.page.getByText(requiredDate) })
      .click();
  }
}
