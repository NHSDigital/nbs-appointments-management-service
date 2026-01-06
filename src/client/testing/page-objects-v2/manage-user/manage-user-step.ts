import { MYALayout } from '@e2etests/types';
import { type Locator, Page } from '@playwright/test';

export default abstract class ManageUserStep extends MYALayout {
  readonly goBackButton: Locator;
  readonly continueButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page, positiveActionButtonText = 'Continue') {
    super(page);

    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: positiveActionButtonText,
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
  }
}
