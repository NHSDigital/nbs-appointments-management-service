import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';

export default class AddServicesPage extends MYALayout {
  readonly title = this.page.getByRole('heading', {
    name: 'Add services to your session',
  });

  readonly goBackButton = this.page.getByRole('link', {
    name: 'Go back',
  });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue',
  });  

  async verifyAddServicesPageDisplayed() {
    await expect(
      this.page.getByText('Add services to your session'),
    ).toBeVisible();
  }

  async addService(serviceName: string) {
    await this.page
      .getByRole('checkbox', { name: serviceName, exact: true })
      .click();
    await this.continueButton.click();
  }

  async addServices(serviceNames: string[]) {
    for (let index = 0; index < serviceNames.length; index++) {
      await this.page
        .getByRole('checkbox', { name: serviceNames[index], exact: true })
        .click();
    }

    await this.continueButton.click();
  }
}
