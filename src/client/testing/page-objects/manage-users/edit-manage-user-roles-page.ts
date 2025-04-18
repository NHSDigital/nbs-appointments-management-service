import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from '../root';

export default class EditManageUserRolesPage extends RootPage {
  readonly title: Locator;
  readonly emailInput: Locator;
  readonly continueButton: Locator;
  readonly cancelButton: Locator;
  readonly confirmAndSaveButton: Locator;
  readonly firstName: Locator;
  readonly lastName: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'User details',
    });
    this.emailInput = page.getByRole('textbox', {
      name: 'Enter email address',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
    this.confirmAndSaveButton = page.getByRole('button', {
      name: 'Confirm and send',
    });
    this.firstName = page.getByLabel('First name');
    this.lastName = page.getByLabel('Last name');
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

  async verifyFirstNameLastNameAvailable() {
    await expect(this.firstName).toBeVisible();
    await expect(this.lastName).toBeVisible();
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
}
