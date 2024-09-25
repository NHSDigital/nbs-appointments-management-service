import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from './root';

export default class UserManagementPage extends RootPage {
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

  async selectRole(role: string) {
    await this.page.getByRole('checkbox', { name: role }).click();
  }

  async userExists(email: string) {
    expect(this.page.getByRole('cell', { name: email })).toBeVisible();
  }

  async userDoesNotExist(email: string) {
    expect(this.page.getByRole('cell', { name: email })).not.toBeVisible();
  }
}
