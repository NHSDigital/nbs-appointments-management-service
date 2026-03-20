import { Locator, type Page } from '@playwright/test';
import { RootPage } from '@testing-page-objects';

export default class EditAvailabilityConfirmationPage extends RootPage {
  readonly yesRadio: Locator;
  readonly noRadio: Locator;

  constructor(page: Page) {
    super(page);

    this.yesRadio = page.getByRole('radio', {
      name: 'Yes, cancel the appointments and change this session',
    });

    this.noRadio = page.getByRole('radio', {
      name: 'No, do not cancel the appointments but change this session',
    });
  }

  async confirmSessionChange(option: 'Yes' | 'No') {
    if (option == 'Yes') {
      await this.page.getByRole('button', { name: 'Change session' }).click();
    }
    if (option == 'No') {
      await this.page.getByText(`No, go back`).click();
    }
  }
}
