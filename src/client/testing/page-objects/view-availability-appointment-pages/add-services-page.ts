import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class AddServicesPage extends RootPage {
  readonly goBackButton: Locator;
  readonly continueButton: Locator;

  constructor(page: Page) {
    super(page);
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
  }

  async verifyAddServicesPageDisplayed() {
    await expect(
      this.page.getByText('Add services to your session'),
    ).toBeVisible();
  }

  async addService(serviceName: string) {
    await this.page.getByLabel(serviceName).check();
    await this.continueButton.click();
  }
}
