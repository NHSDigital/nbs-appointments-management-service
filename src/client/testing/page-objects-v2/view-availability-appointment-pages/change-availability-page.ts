import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class ChangeAvailabilityPage extends MYALayout {
  // Use regex for the heading to handle different dates/sessions dynamically
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Change (availability|session)/i,
  });

  readonly goBackLink: Locator = this.page.getByRole('link', {
    name: /Back to (week view|all availability)|Go back/i,
  });

  readonly continueButton: Locator = this.page.getByRole('button', {
    name: 'Continue',
  });

  readonly changeHeader: Locator = this.page
    .getByRole('heading', { level: 1 })
    .first();

  // Radio Options - Using Regex for resilience
  readonly editLengthCapacityRadioOption: Locator = this.page.getByRole(
    'radio',
    {
      name: 'Change the length or capacity of this session',
    },
  );

  readonly editServicesRadioOption: Locator = this.page.getByRole('radio', {
    name: 'Remove a service or multiple services',
  });

  readonly cancelRadioOption: Locator = this.page.getByRole('radio', {
    name: 'Cancel the session',
  });

  // This usually appears on the second 'Confirmation' page
  readonly confirmCancelRadioOption: Locator = this.page.getByRole('radio', {
    name: 'Yes, I want to cancel this session',
  });

  readonly cancelSessionButton: Locator = this.page.getByRole('button', {
    name: 'Cancel session',
  });

  /**
   * Selects the high-level change category
   */
  async selectChangeType(
    changeType: 'ChangeLengthCapacity' | 'CancelSession' | 'ReduceServices',
  ) {
    if (changeType === 'ChangeLengthCapacity') {
      await this.editLengthCapacityRadioOption.check();
    } else if (changeType === 'CancelSession') {
      await this.cancelRadioOption.check();
    } else if (changeType === 'ReduceServices') {
      await this.editServicesRadioOption.check();
    }
  }

  /**
   * Submits the selection to move to the next step
   */
  async saveChanges() {
    await this.continueButton.click();
  }

  /**
   * Verifies the page is displayed, checking for the presence of the date
   */
  async verifyChangeAvailabilityPageDisplayed(expectedDate: string) {
    await expect(this.title).toBeVisible();
    await expect(this.page.getByText(expectedDate).first()).toBeVisible();
  }

  async backToWeekView() {
    await this.goBackLink.click();
  }
}
