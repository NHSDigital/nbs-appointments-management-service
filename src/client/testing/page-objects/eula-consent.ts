import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class EulaConsentPage extends RootPage {
  readonly title: Locator;
  readonly acceptAndContinueButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Agree to the terms of use',
    });
    this.acceptAndContinueButton = page.getByRole('button', {
      name: 'Accept and continue',
    });
  }
}
