import { Locator, type Page } from '@playwright/test';
import { RootPage } from '@testing-page-objects';

export default class EditServicesConfirmationPage extends RootPage {
  readonly removeServicesButton: Locator;
  readonly goBackLink: Locator;

  constructor(page: Page) {
    super(page);

    this.removeServicesButton = page.getByRole('button', {
      name: 'Remove services',
    });
    this.goBackLink = page.getByRole('link', {
      name: 'No, go back',
    });
  }

  async confirmServiceChange(option: 'Yes' | 'No') {
    if (option == 'Yes') {
      await this.page.getByRole('button', { name: 'Remove services' }).click();
    }
    if (option == 'No') {
      await this.page.getByText(`No, go back`).click();
    }
  }
}
