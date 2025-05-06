import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default abstract class ManageUserStep extends RootPage {
  readonly title: Locator;
  readonly goBackButton: Locator;
  readonly continueButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page, positiveActionButtonText = 'Continue') {
    super(page);

    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.title = page.getByRole('heading', {
      name: 'Remove User',
    });
    this.continueButton = page.getByRole('button', {
      name: positiveActionButtonText,
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
  }
}
