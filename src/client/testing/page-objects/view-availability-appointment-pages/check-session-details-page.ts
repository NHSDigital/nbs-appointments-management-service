import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class CheckSessionDetailsPage extends RootPage {
  readonly goBackButton: Locator;
  readonly saveSessionButton: Locator;

  constructor(page: Page) {
    super(page);
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.saveSessionButton = page.getByRole('button', {
      name: 'Save and publish availability',
    });
  }

  async verifyCheckSessionDetailsPageDisplayed() {
    await expect(this.page.getByText('Check your answers')).toBeVisible();
  }

  async saveSession() {
    await this.saveSessionButton.click();
  }
}
