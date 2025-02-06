import { expect } from '../../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { SiteWithAttributes } from '@types';

export default class RemoveUserPage extends RootPage {
  readonly title: Locator;
  readonly confirmRemoveButton: Locator;
  readonly cancelButton: Locator;
  readonly siteDetails: SiteWithAttributes;

  constructor(page: Page, siteDetails: SiteWithAttributes) {
    super(page);
    this.siteDetails = siteDetails;
    this.title = page.getByRole('heading', {
      name: 'Remove User',
    });
    this.confirmRemoveButton = page.getByRole('button', {
      name: 'Remove this account',
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
  }

  async verifyUserNavigatedToRemovePage(userName: string) {
    await this.page.waitForURL(
      `**/site/${this.siteDetails.id}/users/remove?user=${userName}`,
    );
    await expect(
      this.page.getByText(
        `Are you sure you wish to remove ${userName} from ${this.siteDetails.name}?`,
      ),
    ).toBeVisible();
  }

  async clickButton(buttonName: 'Remove this account' | 'Cancel') {
    await this.page.getByRole('button', { name: `${buttonName}` }).click();
  }
}
