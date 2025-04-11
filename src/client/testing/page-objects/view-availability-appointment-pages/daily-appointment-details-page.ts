import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class DailyAppointmentDetailsPage extends RootPage {
  readonly backToWeekViewButton: Locator;
  readonly continueButton: Locator;

  constructor(page: Page) {
    super(page);
    this.backToWeekViewButton = page.getByRole('link', {
      name: 'Back to week view',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
  }

  async verifyDailyAppointmentDetailsPageDisplayed() {
    await expect(this.page.getByText('Scheduled')).toBeVisible();
  }

  async navigateToWeekView() {
    await this.backToWeekViewButton.click();
  }

  async cancelAppointment(appointmentDetail: string) {
    await this.page
      .getByRole('row')
      .filter({ hasText: `${appointmentDetail}` })
      .getByRole('link', { name: 'Cancel' })
      .click();
  }

  async confirmAppointmentCancellation(option: 'Yes' | 'No') {
    if (option == 'Yes') {
      await this.page
        .getByLabel('Yes, I want to cancel this appointment')
        .click();
    }
    if (option == 'No') {
      await this.page
        .getByLabel(`No, I do not want to cancel this appointment`)
        .click();
    }
    await this.continueButton.click();
  }

  async verifyAppointmentNotCancelled(appointmentDetail: string) {
    await expect(
      this.page.getByRole('row').filter({ hasText: `${appointmentDetail}` }),
    ).not.toBeVisible();
  }

  async verifyAppointmentCancelled(appointmentDetail: string) {
    await expect(
      this.page.getByRole('row').filter({ hasText: `${appointmentDetail}` }),
    ).toBeVisible();
  }

  async verifyManualAppointment() {
    await expect(
      this.page
        .getByRole('main')
        .getByText(
          `There are no booked appointments affected by availability changes.`,
        ),
    ).toBeVisible();
  }
}
