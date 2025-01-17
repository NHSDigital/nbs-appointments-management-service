import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from '../root';

export default class EditAccessNeedsPage extends RootPage {
  readonly title: Locator;
  readonly confirmSiteDetailsButton: Locator;
  readonly updateNotificationBanner: Locator;
  readonly closeNotificationBannerButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Site management',
    });
    this.updateNotificationBanner = page.getByText(
      'You have successfully updated the access needs for the current site.',
    );
    this.confirmSiteDetailsButton = page.getByRole('button', {
      name: 'Confirm site details',
    });
    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });
  }

  async selectAttribute(attribute: string) {
    await this.page.getByRole('checkbox', { name: attribute }).click();
  }

  async attributeChecked(attribute: string) {
    await expect(
      this.page.getByRole('checkbox', { name: attribute, exact: true }),
    ).toBeChecked();
  }

  async attributeNotChecked(attribute: string) {
    await expect(
      this.page.getByRole('checkbox', { name: attribute, exact: true }),
    ).not.toBeChecked();
  }

  async verifyAccessNeedsCheckedOrUnchecked(
    optionName: string,
    checkboxState: 'Checked' | 'UnChecked',
  ) {
    if (checkboxState == 'Checked') {
      await expect(this.page.getByLabel(optionName)).toBeChecked();
    } else {
      await expect(this.page.getByLabel(optionName)).not.toBeChecked();
    }
  }
}
