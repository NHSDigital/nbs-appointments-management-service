import { expect } from '@playwright/test';
import RootPage from '../root';

export default class EditAvailabilityConfirmedPage extends RootPage {
  async verifySessionUpdated() {
    await expect(
      this.page.locator(
        'h1.nhsuk-heading-l:has-text("Time and capacity changed for")',
      ),
    ).toBeVisible();
  }
}
