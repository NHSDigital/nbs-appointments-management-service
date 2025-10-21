import { SiteSelectionPage } from '@e2etests/page-objects';
import { MYALayout } from '@e2etests/types';
import { type Locator } from '@playwright/test';

type ManageCookieAcceptanceForm = {
  legend: Locator;
  consentRadioButton: Locator;
  rejectRadioButton: Locator;
  submitButton: Locator;
};

export default class CookiesPolicyPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: 'What are cookies?',
  });

  readonly manageCookieAcceptanceForm: ManageCookieAcceptanceForm = {
    legend: this.page.getByRole('heading', {
      name: 'Tell us if we can use analytics cookies',
    }),
    consentRadioButton: this.page.getByRole('radio', {
      name: 'Use cookies to measure my website use',
      exact: true,
    }),
    rejectRadioButton: this.page.getByRole('radio', {
      name: 'Do not use cookies to measure my website use',
      exact: true,
    }),
    submitButton: this.page.getByRole('button', {
      name: 'Save my cookie settings',
    }),
  };

  async saveCookiePreferences(): Promise<SiteSelectionPage> {
    await this.manageCookieAcceptanceForm.submitButton.click();
    await this.page.waitForURL('**/sites');

    return new SiteSelectionPage(this.page);
  }
}
