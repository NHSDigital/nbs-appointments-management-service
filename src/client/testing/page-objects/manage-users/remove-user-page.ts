import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { UsersPage } from '@testing-page-objects';

export default class RemoveUserPage extends RootPage {
  readonly site: Site;
  readonly user: string;
  readonly title: Locator;
  readonly confirmRemoveButton: Locator;
  readonly cancelButton: Locator;

  readonly confirmationMessage: Locator;

  constructor(page: Page, site: Site, user: string) {
    super(page);
    this.site = site;
    this.user = user;

    this.title = page.getByRole('heading', {
      name: 'Remove User',
    });
    this.confirmRemoveButton = page.getByRole('button', {
      name: 'Remove this account',
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });

    this.confirmationMessage = this.page.getByText(
      `Are you sure you wish to remove ${this.user} from ${this.site.name}?`,
    );
  }

  async clickConfirmButton(): Promise<UsersPage> {
    await this.confirmRemoveButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users`);

    return new UsersPage(this.page, this.site);
  }

  async clickCancelButton(): Promise<UsersPage> {
    await this.cancelButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users`);

    return new UsersPage(this.page, this.site);
  }
}
