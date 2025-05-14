import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';
import SiteSelectionPage from './site-selection';

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

  async acceptEula(): Promise<SiteSelectionPage> {
    await this.acceptAndContinueButton.click();
    await this.page.waitForURL('**/sites');

    return new SiteSelectionPage(this.page);
  }
}
