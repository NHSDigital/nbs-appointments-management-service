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

  async verifyViewMonthDisplayed(requiredWeek: string) {
    await expect(this.nextButton).toBeEnabled();
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredWeek}`) }),
    ).toBeVisible();
  }

  async openWeekViewHavingDate(requiredWeek: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${requiredWeek}`) })
      .getByRole('link', { name: 'View week' })
      .click();
  }

  async navigateToRequiredMonth(month: string) {
    await this.page.goto(
      `/manage-your-appointments/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/view-availability?date=${month}`,
    );
  }
}
