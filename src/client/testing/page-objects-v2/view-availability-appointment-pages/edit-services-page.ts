import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class EditServicesPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Select services to remove/i,
  });

  readonly goBackLink: Locator = this.page.getByRole('link', {
    name: /Go back|Back/i,
  });

  readonly continueButton: Locator = this.page.getByRole('button', {
    name: 'Continue',
  });

  /**
   * Verifies page display, including the specific date in the heading
   * and that all expected services are present as checkboxes/labels.
   */
  async verifyEditServicesPageDisplayed(
    date: string,
    expectedServices: string[],
  ) {
    // Check the heading specifically for the removal instruction and date
    await expect(
      this.page.getByRole('heading', {
        name: new RegExp(`Select services to remove on ${date}`, 'i'),
      }),
    ).toBeVisible();

    // Check each service is visible
    for (const service of expectedServices) {
      await expect(this.page.getByLabel(service)).toBeVisible();
    }
  }

  /**
   * Checks the box for a single service and continues
   */
  async removeService(serviceName: string) {
    await this.page.getByLabel(serviceName).check();
    await this.continueButton.click();
  }

  /**
   * Checks multiple boxes before clicking continue
   */
  async removeServices(serviceNames: string[]) {
    for (const serviceName of serviceNames) {
      // .check() is safer than .click() as it won't uncheck an already checked box
      await this.page.getByLabel(serviceName).check();
    }

    await this.continueButton.click();
  }
}
