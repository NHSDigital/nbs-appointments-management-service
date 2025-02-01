import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class WeekViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;
  readonly previousButton: Locator;
  readonly backToMonthButton: Locator;
  readonly sessionSuccessMsg: Locator;

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
    this.sessionSuccessMsg = page.getByText(
      'You have successfully created availability for the current site.',
    );
  }

  async verifyWeekViewDisplayed() {
    await expect(this.backToMonthButton).toBeVisible();
  }

  async addAvailability(requiredDate: string) {
    const addAvailabilityButton = await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${requiredDate}`) })
      .getByRole('link', { name: 'Add availability to this day' });
    const totalCount: number = addAvailabilityButton.count();
    if (totalCount == 1) {
      await addAvailabilityButton.click();
    } else {
      await this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredDate}`) })
        .getByRole('link', { name: 'Add Session' })
        .click();
    }
  }

  async verifySessionAdded() {
    await expect(this.sessionSuccessMsg).toBeVisible();
  }
}
