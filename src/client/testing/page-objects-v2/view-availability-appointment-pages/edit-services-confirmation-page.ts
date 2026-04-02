import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class EditServicesConfirmationPage extends MYALayout {
  // Matches the common NHS "Are you sure..." or "Confirm removal" heading patterns.
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Are you sure you want to remove|Confirm removal/i,
  });

  readonly removeServicesButton: Locator = this.page.getByRole('button', {
    name: 'Remove services',
  });

  readonly goBackLink: Locator = this.page.getByRole('link', {
    name: /No, go back|Go back/i,
  });

  /**
   * Confirms or cancels the service removal
   * @param option - 'Yes' to remove, 'No' to go back
   */
  async confirmServiceChange(option: 'Yes' | 'of No') {
    if (option === 'Yes') {
      await this.removeServicesButton.click();
    } else {
      await this.goBackLink.click();
    }
  }

  /**
   * Optional helper to ensure we are on the right confirmation screen
   */
  async verifyConfirmationPageDisplayed() {
    await expect(this.title).toBeVisible();
    await expect(this.removeServicesButton).toBeVisible();
  }
}
