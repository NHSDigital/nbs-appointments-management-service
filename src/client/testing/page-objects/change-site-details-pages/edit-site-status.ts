import { Locator, Page } from '@playwright/test';
import { RootPage } from '@testing-page-objects';

export default class EditSiteStatusPage extends RootPage {
  readonly title: Locator;
  readonly saveAndContinueButton: Locator;
  readonly backLink: Locator;
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
  }
}
