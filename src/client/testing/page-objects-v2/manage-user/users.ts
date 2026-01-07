import { MYALayout } from '@e2etests/types';
import { type Locator, expect } from '@playwright/test';
import ManageUserPage from './manage-user-page';
import RemoveUser from './remove-user';

export default class Users extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage users',
  });

  readonly addUserButton: Locator = this.page.getByRole('button', {
    name: 'Add user',
  });

  readonly emailColumn: Locator = this.page.getByRole('columnheader', {
    name: 'Email',
  });

  readonly manageColumn: Locator = this.page.getByRole('columnheader', {
    name: 'Manage',
  });

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

  async clickRemoveFromThisSite(newUserName: string) {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(newUserName) })
      .getByRole('link', { name: 'Remove from this site' })
      .click();

    return new RemoveUser(this.page, this.site);
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
    await this.page.getByRole('button', { name: 'Close' }).click();
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

  async clickEditLink(newUserName: string): Promise<ManageUserPage> {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(newUserName) })
      .getByRole('link', { name: 'Edit' })
      .click();
    return new ManageUserPage(this.page, this.site);
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

  async clickAddUserButton(): Promise<ManageUserPage> {
    await this.addUserButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/users/manage`);
    return new ManageUserPage(this.page, this.site);
  }
}
