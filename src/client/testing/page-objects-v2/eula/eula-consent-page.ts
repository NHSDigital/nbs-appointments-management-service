import { type Locator } from '@playwright/test';
import { SiteSelectionPage } from '@e2etests/page-objects';
import { MYALayout } from '@e2etests/types';

export default class EulaConsentPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: 'Agree to the terms of use',
  });

  readonly acceptAndContinueButton: Locator = this.page.getByRole('button', {
    name: 'Accept and continue',
  });

  async acceptEula(): Promise<SiteSelectionPage> {
    await this.acceptAndContinueButton.click();
    await this.page.waitForURL('**/sites');

    return new SiteSelectionPage(this.page);
  }
}
