import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';

export default abstract class CreateAvailabilityStep extends RootPage {
  readonly site: Site;
  readonly goBackButton: Locator;
  readonly continueButton: Locator;

  constructor(page: Page, site: Site, positiveActionButtonText = 'Continue') {
    super(page);
    this.site = site;

    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: positiveActionButtonText,
    });
  }
}
