import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class UsersPage extends RootPage {
  readonly title: Locator;
  readonly emailColumn: Locator;
  readonly assignStaffRolesLink: Locator;
  readonly manageColumn: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Manage Staff Roles',
    });
    this.emailColumn = page.getByRole('columnheader', {
      name: 'Email',
    });
    this.manageColumn = page.getByRole('columnheader', {
      name: 'Manage',
    });
    this.assignStaffRolesLink = page.getByRole('link', {
      name: 'Assign Staff Roles',
    });
  }

  async verifyUserRoles(roleName: string, newUserName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({ has: this.page.getByText(newUserName) })
        .getByText(roleName),
    ).toBeVisible();
  }
}
