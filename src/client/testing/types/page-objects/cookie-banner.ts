import { type Locator } from '@playwright/test';
import PageObject from './page-object';

export default class CookieBanner extends PageObject {
  public readonly preAcceptanceHeader: Locator = this.page.getByRole(
    'heading',
    {
      name: 'Cookies on the NHS website',
    },
  );

  public readonly postAcceptanceMessage: Locator = this.page.getByText(
    'You can change your cookie settings at any time',
  );

  public readonly acceptCookiesButton: Locator = this.page.getByRole('button', {
    name: `I'm OK with analytics cookies`,
  });

  public readonly rejectCookiesButton: Locator = this.page.getByRole('button', {
    name: 'Do not use analytics cookies',
  });
}
