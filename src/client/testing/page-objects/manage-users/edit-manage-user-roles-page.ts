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

  async selectRole(role: string) {
    await this.page.getByRole('checkbox', { name: role }).check();
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

  async unselectStaffRole(roleName: string) {
    await this.page.getByLabel(roleName).uncheck();
  }

  async verifyValidationMsgForNoRoles() {
    await expect(
      this.page.getByRole('main').filter({
        has: this.page.getByText(
          `You have not selected any roles for this user`,
        ),
      }),
    ).toBeVisible();
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
