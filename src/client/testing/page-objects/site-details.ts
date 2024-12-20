import { expect } from '../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SiteDetailsPage extends RootPage {
  readonly title: Locator;
  readonly editSiteAttributesButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Site details',
    });
    this.editSiteAttributesButton = page.getByRole('link', {
      name: 'Edit access needs',
    });
  }

  async attributeIsTrue(attributeName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({
          has: this.page.getByText(attributeName),
        })
        .getByText('Yes'),
    ).toBeVisible();
  }
}
