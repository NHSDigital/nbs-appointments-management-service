import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { ManageUserPage, RemoveUserPage } from '@testing-page-objects';

export default class UsersPage extends RootPage {
  readonly title: Locator;
  readonly site: Site;
  readonly emailColumn: Locator;
  readonly addUserButton: Locator;
  readonly manageColumn: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
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

  async clickAddUser(): Promise<ManageUserPage> {
    await this.addUserButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users/manage`);

    return new ManageUserPage(this.page, this.site);
  }

  async clickEditUserLink(userEmail: string): Promise<ManageUserPage> {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(userEmail) })
      .getByRole('link', { name: 'Edit' })
      .click();

    await this.page.waitForURL(`**/site/${this.site.id}/users/manage`);

    return new ManageUserPage(this.page, this.site);
  }

  async clickRemoveUserLink(userEmail: string): Promise<RemoveUserPage> {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(userEmail) })
      .getByRole('link', { name: 'Remove from this site' })
      .click();

    await this.page.waitForURL(`**/site/${this.site.id}/users/manage`);

    return new RemoveUserPage(this.page, this.site, userEmail);
  }

  // async verifyUserRoles(roleName: string, newUserName: string) {
  //   await expect(
  //     this.page
  //       .getByRole('row')
  //       .filter({ has: this.page.getByText(newUserName) })
  //       .getByText(roleName),
  //   ).toBeVisible();
  // }

  // async userExists(email: string) {
  //   await expect(this.page.getByRole('cell', { name: email })).toBeVisible();
  // }

  // async userDoesNotExist(email: string) {
  //   await expect(
  //     this.page.getByRole('cell', { name: email }),
  //   ).not.toBeVisible();
  // }

  // async removeFromThisSiteLink(newUserName: string) {
  //   await this.page
  //     .getByRole('row')
  //     .filter({ has: this.page.getByText(newUserName) })
  //     .getByRole('link', { name: 'Remove from this site' })
  //     .click();
  // }

  // async verifyRemoveUserSuccessBannerDisplayed(userName: string) {
  //   await expect(
  //     this.page.getByRole('main').filter({
  //       has: this.page.getByText(
  //         `You have successfully removed ${userName} from the current site.`,
  //       ),
  //     }),
  //   ).toBeVisible();
  // }

  // async closeBanner() {
  //   this.page.getByRole('button', { name: 'Close' }).click();
  // }

  // async verifyRemoveUserSuccessBannerNotDisplayed(userName: string) {
  //   await expect(
  //     this.page.getByRole('main').filter({
  //       has: this.page.getByText(
  //         `You have successfully removed ${userName} from the current site.`,
  //       ),
  //     }),
  //   ).not.toBeVisible();
  // }

  // async clickEditLink(newUserName: string): Promise<ManageUserPage> {
  //   await this.page
  //     .getByRole('row')
  //     .filter({ has: this.page.getByText(newUserName) })
  //     .getByRole('link', { name: 'Edit' })
  //     .click();

  //   return new ManageUserPage(this.page, this.site);
  // }

  // async verifyUserRoleRemoved(roleName: string, newUserName: string) {
  //   await expect(
  //     this.page
  //       .getByRole('row')
  //       .filter({ has: this.page.getByText(newUserName) })
  //       .getByText(roleName),
  //   ).not.toBeVisible();
  // }

  // async verifyLinkNotVisible(newUserName: string, linkName: string) {
  //   await expect(
  //     this.page
  //       .getByRole('row')
  //       .filter({ has: this.page.getByText(newUserName) })
  //       .getByRole('link', { name: `${linkName}` }),
  //   ).not.toBeVisible();
  // }
}
