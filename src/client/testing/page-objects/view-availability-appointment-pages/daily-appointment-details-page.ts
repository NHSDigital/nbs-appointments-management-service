import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class DailyAppointmentDetailsPage extends RootPage {
  readonly backToWeekViewButton: Locator;

  constructor(page: Page) {
    super(page);
    this.backToWeekViewButton = page.getByRole('link', {
      name: 'Back to week view',
    });
  }

  async verifyDailyAppointmentDetailsPageDisplayed() {
    await expect(this.page.getByText('Scheduled')).toBeVisible();
  }

  async navigateToWeekView() {
    await this.backToWeekViewButton.click();
  }

  async cancelAppointment(appointmentTime: string) {
    await this.page
      .getByRole('row')
      .filter({ hasText: `${appointmentTime}` })
      .getByRole('link', { name: 'Cancel' })
      .click();
  }

  async confirmAppointmentCancelation(option: 'Yes' | 'No') {
    if (option == 'Yes') {
      await this.page
        .getByLabel('Yes, I want to cancel this appointment')
        .click();
    }
    if (option == 'No') {
      await this.page
        .getByLabel(`No, I don't want to cancel this session`)
        .click();
    }
  }
}
