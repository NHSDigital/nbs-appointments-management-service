import { Page, type Locator } from '@playwright/test';
import { CookieBanner, Footer, Header, PageObject } from '@e2etests/types';
import { LoginPage } from '@e2etests/page-objects';
import { CookiesPolicyPage } from '@testing-page-objects';
import { Site } from '@types';

export default abstract class MYALayout extends PageObject {
  protected readonly site?: Site;

  constructor(page: Page, site?: Site) {
    super(page);
    this.site = site;
  }

  abstract title: Locator;
  readonly header: Header = new Header(this.page, this.site);
  readonly cookieBanner: CookieBanner = new CookieBanner(this.page);

  readonly notificationBanner: Locator = this.page
    .getByRole('main')
    .getByRole('banner');

  readonly footer: Footer = new Footer(this.page);

  async logOut(): Promise<LoginPage> {
    await this.header.logOutButton.click();
    await this.page.waitForURL('**/login');

    return new LoginPage(this.page);
  }

  async dismissNotification(): Promise<this> {
    await this.notificationBanner
      .getByRole('button', { name: 'Close' })
      .click();

    await this.page.waitForLoadState('networkidle');

    return this;
  }

  async clickCookiesFooterLink(): Promise<CookiesPolicyPage> {
    await this.footer.links.cookiesPolicy.click();
    await this.page.waitForURL('**/cookies-policy');

    return new CookiesPolicyPage(this.page);
  }
}
