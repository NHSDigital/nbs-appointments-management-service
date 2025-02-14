import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

type ManageCookieAcceptanceForm = {
  legend: Locator;
  consentedRadio: Locator;
  rejectedRadio: Locator;
  submitButton: Locator;
};

export default class CookiesPolicyPage extends RootPage {
  readonly title: Locator;
  readonly manageCookieAcceptanceForm: ManageCookieAcceptanceForm;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'What are cookies?',
    });

    this.manageCookieAcceptanceForm = {
      legend: page.getByRole('heading', {
        name: 'Tell us if we can use analytics cookies',
      }),
      consentedRadio: page.getByRole('radio', {
        name: 'Use cookies to measure my website use',
        exact: true,
      }),
      rejectedRadio: page.getByRole('radio', {
        name: 'Do not use cookies to measure my website use',
        exact: true,
      }),
      submitButton: page.getByRole('button', {
        name: 'Save my cookie settings',
      }),
    };
  }
}
