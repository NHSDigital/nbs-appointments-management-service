import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { ManageUserPage, RemoveUserPage, TopNav } from '@testing-page-objects';

export default class UsersPage extends RootPage {
  readonly title: Locator;
  readonly topNav: TopNav;
  readonly site: Site;
  readonly emailColumn: Locator;
  readonly addUserButton: Locator;
  readonly manageColumn: Locator;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.topNav = new TopNav(page, site);

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

    await this.page.waitForURL(
      `**/site/${this.site.id}/users/manage?user=${userEmail}`,
    );

    return new ManageUserPage(this.page, this.site);
  }

  async clickRemoveUserLink(userEmail: string): Promise<RemoveUserPage> {
    await this.page
      .getByRole('row')
      .filter({ has: this.page.getByText(userEmail) })
      .getByRole('link', { name: 'Remove from this site' })
      .click();

    await this.page.waitForURL(
      `**/site/${this.site.id}/users/remove?user=${userEmail}`,
    );

    return new RemoveUserPage(this.page, this.site, userEmail);
  }
}
