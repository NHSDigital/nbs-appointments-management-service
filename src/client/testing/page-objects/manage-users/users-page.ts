import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class UsersPage extends RootPage {
  readonly title: Locator;
  readonly emailColumn: Locator;
  readonly addUserButton: Locator;
  readonly manageColumn: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Manage users',
    });
    this.emailColumn = page.getByRole('columnheader', {
      name: 'Email',
    });
    this.manageColumn = page.getByRole('columnheader', {
      name: 'Manage',
    });
    this.addUserButton = page.getByRole('button', {
      name: 'Add user',
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

  async userExists(email: string) {
    await expect(this.page.getByRole('cell', { name: email })).toBeVisible();
  }

  async userDoesNotExist(email: string) {
    await expect(
      this.page.getByRole('cell', { name: email }),
    ).not.toBeVisible();
  }

  async removeFromThisSiteLink(newUserName: string) {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(newUserName) })
      .getByRole('link', { name: 'Remove from this site' })
      .click();
  }

  async verifyRemoveUserSuccessBannerDisplayed(userName: string) {
    await expect(
      this.page.getByRole('main').filter({
        has: this.page.getByText(
          `You have successfully removed ${userName} from the current site.`,
        ),
      }),
    ).toBeVisible();
  }

  async closeBanner() {
    this.page.getByRole('button', { name: 'Close' }).click();
  }

  async verifyRemoveUserSuccessBannerNotDisplayed(userName: string) {
    await expect(
      this.page.getByRole('main').filter({
        has: this.page.getByText(
          `You have successfully removed ${userName} from the current site.`,
        ),
      }),
    ).not.toBeVisible();
  }

  async clickEditLink(newUserName: string) {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(newUserName) })
      .getByRole('link', { name: 'Edit' })
      .click();
  }

  async verifyUserRoleRemoved(roleName: string, newUserName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({ has: this.page.getByText(newUserName) })
        .getByText(roleName),
    ).not.toBeVisible();
  }

  async verifyLinkNotVisible(newUserName: string, linkName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({ has: this.page.getByText(newUserName) })
        .getByRole('link', { name: `${linkName}` }),
    ).not.toBeVisible();
  }
}
