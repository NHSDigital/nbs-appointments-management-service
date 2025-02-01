import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class MonthViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;

  constructor(page: Page) {
    super(page);
    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });
  }

  async verifyViewMonthDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }

  async openWeekViewHavingDate(requiredDate: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${requiredDate}`) })
      .getByRole('link', { name: 'View week' })
      .click();
  }
}
