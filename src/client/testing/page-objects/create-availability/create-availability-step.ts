import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default abstract class CreateAvailabilityStep extends RootPage {
  readonly goBackButton: Locator;
  readonly continueButton: Locator;

  constructor(page: Page, positiveActionButtonText = 'Continue') {
    super(page);

    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: positiveActionButtonText,
    });
  }
}
