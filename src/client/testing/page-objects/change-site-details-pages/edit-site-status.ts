import { Locator, Page } from '@playwright/test';
import { RootPage } from '@testing-page-objects';
import { expect } from '../../fixtures';

export default class EditSiteStatusPage extends RootPage {
  readonly title: Locator;
  readonly saveAndContinueButton: Locator;
  readonly backLink: Locator;
  // Site online radio labels
  readonly takeSiteOffline: Locator;
  readonly keepSiteOnline: Locator;
  // Site offline radio labels
  readonly makeSiteOnline: Locator;
  readonly keepSiteOffline: Locator;

  readonly siteStatusLabel = 'Current site status';

  constructor(page: Page) {
    super(page);

    this.title = page.getByRole('heading', {
      name: 'Manage Site Visibility',
    });
    this.saveAndContinueButton = page.getByRole('button', {
      name: 'Save and continue',
    });
    this.backLink = page.getByRole('link').filter({ hasText: 'Back' });
    this.takeSiteOffline = page.getByRole('radio', {
      name: 'Take site offline',
    });
    this.keepSiteOnline = page.getByRole('link', {
      name: 'Keep site online',
    });
    this.makeSiteOnline = page.getByRole('link', {
      name: 'Make site online',
    });
    this.keepSiteOffline = page.getByRole('link', {
      name: 'Keep site offline',
    });
  }

  async verifySummaryListItemContentValue(title: string, value: string) {
    const listitem = this.page.getByRole('listitem', {
      name: `${title} summary`,
    });
    await expect(listitem).toBeVisible();

    await expect(listitem.getByRole('term')).toBeVisible();
    await expect(listitem.getByRole('term')).toHaveText(title);
    await expect(listitem.getByRole('definition')).toBeVisible();
    await expect(listitem.getByRole('definition')).toHaveText(value);
  }
}
