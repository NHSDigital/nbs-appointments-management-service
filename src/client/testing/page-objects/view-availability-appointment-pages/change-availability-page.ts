import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class ChangeAvailabilityPage extends RootPage {
  readonly goBackButton: Locator;
  readonly continueButton: Locator;

  constructor(page: Page) {
    super(page);
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
  }

  async selectChangeType(changeType: 'ShortenLenght' | 'CancelSession') {
    if (changeType == 'ShortenLenght') {
      await this.page
        .getByLabel('Change the length or capacity of this session')
        .click();
    }
    if (changeType == 'CancelSession') {
      await this.page.getByLabel('Cancel this session').click();
    }
  }

  async saveChanges() {
    await this.continueButton.click();
  }

  async verifySessionUpdated() {
    await expect(
      this.page.getByText('You have successfully edited the session.'),
    ).toBeVisible();
  }
}
