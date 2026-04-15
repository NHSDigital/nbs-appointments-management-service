import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class EditAvailabilityConfirmationPage extends MYALayout {
  // This usually matches the "Are you sure..." or "Confirm changes" question.
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Confirm (session change|changes)/i,
  });

  readonly yesRadio: Locator = this.page.getByRole('radio', {
    name: /Yes, cancel (the )?appointments and change/i,
  });

  readonly noRadio: Locator = this.page.getByRole('radio', {
    name: /No, do not cancel (the )?appointments/i,
  });

  readonly changeSessionButton: Locator = this.page.getByRole('button', {
    name: 'Change session',
  });

  readonly goBackLink: Locator = this.page.getByRole('link', {
    name: /No, go back|Go back/i,
  });

  /**
   * Handles the confirmation of a session change.
   * @param option - 'Yes' to confirm/cancel appointments, 'No' to go back.
   */
  async confirmSessionChange(option: 'Yes' | 'No') {
    if (option === 'Yes') {
      // If the UI has radios, we check it first; if not, we just click the button.
      if (await this.yesRadio.isVisible()) {
        await this.yesRadio.check();
      }
      await this.changeSessionButton.click();
    } else {
      await this.goBackLink.click();
    }
  }

  async verifyConfirmationPageDisplayed() {
    await expect(this.title).toBeVisible();
  }
}
