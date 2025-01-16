import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from '../root';

export default class EditManageUserRolesPage extends RootPage {
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
    await this.page.getByRole('checkbox', { name: role }).check();
  }

  async userExists(email: string) {
    await expect(this.page.getByRole('cell', { name: email })).toBeVisible();
  }

  async userDoesNotExist(email: string) {
    await expect(
      this.page.getByRole('cell', { name: email }),
    ).not.toBeVisible();
  }

  async selectStaffRole(roleName: string) {
    await this.page.getByLabel(roleName).check();
  }
}
