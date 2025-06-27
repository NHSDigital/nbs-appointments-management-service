import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class EditServicesPage extends RootPage {
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

  async verifyEditServicesPageDisplayed(
    date: string,
    expectedServices: string[],
  ) {
    await expect(this.page.getByRole('main')).toContainText(
      `Remove services for ${date}`,
    );

    for (let index = 0; index < expectedServices.length; index++) {
      await expect(this.page.getByLabel(expectedServices[index])).toBeVisible();
    }
  }

  async removeService(serviceName: string) {
    await this.page.getByLabel(serviceName).check();
    await this.continueButton.click();
  }

  async removeServices(serviceNames: string[]) {
    for (let index = 0; index < serviceNames.length; index++) {
      await this.page.getByLabel(serviceNames[index]).check();
    }

    await this.continueButton.click();
  }
}
