import { type Locator, type Page } from '@playwright/test';

export default class ConfirmDownloadStep {
  readonly goBackButton: Locator;

  readonly stepTitle: Locator;

  readonly continueButton: Locator;

  constructor(page: Page) {
    this.stepTitle = page.getByRole('heading', {
      name: 'Download the report',
    });
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });

    this.continueButton = page.getByRole('button', {
      name: 'Export data',
    });
  }
}
