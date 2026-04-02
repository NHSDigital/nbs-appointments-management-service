import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class CancelSessionDetailsPage extends MYALayout {
  // Using a regex to cover both the "Confirmation" and the "Success" headers.
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Are you sure you want to cancel|Session cancelled/i,
  });

  readonly goBackLink: Locator = this.page.getByRole('link', {
    name: 'No, go back',
    exact: true,
  });

  readonly cancelSessionButton: Locator = this.page.getByRole('button', {
    name: 'Cancel session',
  });

  readonly cancelAppointmentsLink: Locator = this.page.getByRole('link', {
    name: 'Cancel appointments',
  });

  readonly viewBookingsLink: Locator = this.page.getByRole('link', {
    name: /View (all )?bookings for this week/i,
  });

  /**
   * Verifies the confirmation page is displayed before clicking cancel.
   */
  async verifyCancelSessionDetailsPageDisplayed() {
    await expect(this.title).toBeVisible();
    await expect(
      this.page.getByText(/Are you sure you want to cancel/i),
    ).toBeVisible();
  }

  /**
   * Confirms or reverts the cancellation.
   */
  async confirmSessionCancellation(option: 'Yes' | 'No') {
    if (option === 'Yes') {
      await this.cancelSessionButton.click();
    } else {
      await this.goBackLink.click();
    }
  }

  /**
   * Verifies the success message after cancellation.
   */
  async verifySessionCancelled(requiredDate: string) {
    await expect(this.title).toContainText(/Session cancelled/i);
    await expect(
      this.page.getByRole('heading', { name: requiredDate }),
    ).toBeVisible();
  }

  /**
   * Handles the link to manual appointment cancellation.
   */
  async clickCancelAppointment() {
    await expect(
      this.page.getByText(
        /You'll need to manually cancel any affected appointments/i,
      ),
    ).toBeVisible();
    await this.cancelAppointmentsLink.click();
  }

  /**
   * Navigates back to the week view.
   */
  async clickViewBookings() {
    await this.viewBookingsLink.click();
  }
}
