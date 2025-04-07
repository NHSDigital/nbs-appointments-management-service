import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class CancelSessionDetailsPage extends RootPage {
  readonly goBackButton: Locator;
  readonly continueButton: Locator;
  readonly cancelSessionHeader: Locator;

  constructor(page: Page) {
    super(page);
    this.cancelSessionHeader = page.getByRole('heading', { level: 1 }).first();
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
  }

  async verifyCancelSessionDetailsPageDisplayed() {
    await expect(
      this.page.getByText('Are you sure you want to cancel this session?', {
        exact: true,
      }),
    ).toBeVisible();
  }

  async confirmSessionCancelation(option: 'Yes' | 'No') {
    if (option == 'Yes') {
      await this.page.getByLabel('Yes, I want to cancel this session').click();
    }
    if (option == 'No') {
      await this.page
        .getByLabel(`No, I don't want to cancel this session`)
        .click();
    }
    await this.continueButton.click();
  }

  async verifySessionCancelled(requiredDate: string) {
    await expect(this.page.getByRole('main')).toContainText(
      `Cancelled session for ${requiredDate}`,
    );
  }

  async clickCancelAppointment() {
    await expect(
      this.page
        .getByRole('main')
        .getByText(`You'll need to manually cancel any affected appointments.`),
    ).toBeVisible();
    await this.page.getByRole('link', { name: 'Cancel appointments' }).click();
  }
}
