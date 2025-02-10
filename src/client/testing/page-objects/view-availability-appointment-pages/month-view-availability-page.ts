import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class ViewAvailabilityPage extends RootPage {
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
}
