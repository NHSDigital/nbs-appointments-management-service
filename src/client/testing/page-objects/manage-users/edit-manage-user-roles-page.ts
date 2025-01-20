import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from '../root';
import { stat } from 'fs';

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

  async selectStaffRole(roleName: string) {
    await this.page.getByLabel(roleName).check();
  }

  async verifyUserRedirectedToEditRolePage(
    roleName: string,
    status: 'Checked' | 'UnChecked',
  ) {
    await expect(this.title).toBeVisible();
    if (status == 'Checked') {
      await expect(this.page.getByLabel(roleName)).toBeChecked();
    } else {
      await expect(this.page.getByLabel(roleName)).not.toBeChecked();
    }
  }
}
