import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class RemoveUserPage extends RootPage {
  readonly title: Locator;
  readonly confirmRemoveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
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
    await this.page.waitForURL(`**/site/ABC01/users/remove?user=${userName}`);
    await expect(
      this.page.getByText(
        `Are you sure you wish to remove ${userName} from Robin Lane Medical Centre?`,
      ),
    ).toBeVisible();
  }

  async clickButton(buttonName: 'Remove this account' | 'Cancel') {
    await this.page.getByRole('button', { name: `${buttonName}` }).click();
  }
}
