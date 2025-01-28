import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from '../root';

export default class CreateUserPage extends RootPage {
  readonly title: Locator;
  readonly emailInput: Locator;
  readonly searchUserButton: Locator;
  readonly cancelButton: Locator;
  readonly confirmAndSaveButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Staff Role Management',
    });
    this.emailInput = page.getByRole('textbox', {
      name: 'Enter an email address',
    });
    this.searchUserButton = page.getByRole('button', {
      name: 'Search user',
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
    this.confirmAndSaveButton = page.getByRole('button', {
      name: 'Confirm and save',
    });
  }

  async notSelectedAnyRolesErrorMsg() {
    await expect(
      this.page.getByText('You have not selected any roles'),
    ).toBeVisible();
  }

  async notEnteredValidEmailAddressErrorMsg() {
    await expect(
      this.page.getByText('You have not entered a valid NHS email address'),
    ).toBeVisible();
  }

  async selectStaffRole(roleName: string) {
    await this.page.getByLabel(roleName).check();
  }
}
