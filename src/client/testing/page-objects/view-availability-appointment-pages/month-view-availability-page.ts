import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class ViewMonthAvailabilityPage extends RootPage {
  readonly nextButton: Locator;

  constructor(page: Page) {
    super(page);
    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });
  }

  async verifyViewNextMonthButtonDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }
}
