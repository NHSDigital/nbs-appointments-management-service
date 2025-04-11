import { expect } from '../../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class UserSummaryPage extends RootPage {
  readonly title: Locator;
  readonly name: Locator;
  readonly confirmButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Check user details',
    });
    this.confirmButton = page.getByRole('button', {
      name: 'Confirm and send',
    });
    this.name = page.locator('[aria-label="Name-description"]');
  }

  async verifyUserName(userName: string) {
    await expect(await this.name.textContent()).toBe(userName);
  }

  async clickButton() {
    await this.confirmButton.click();
  }
}
