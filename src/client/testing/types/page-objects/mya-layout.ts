import { Page, type Locator } from '@playwright/test';
import { LoginPage, CookiePoliciesPage } from '@e2etests/page-objects';
import PageObject from './page-object';
import Footer from './footer';
import CookieBanner from './cookie-banner';
import Header from './header';
import { SiteDocument } from '../cosmos';

export default abstract class MYALayout extends PageObject {
  readonly site?: SiteDocument;

  constructor(page: Page, site?: SiteDocument) {
    super(page);
    this.site = site;
  }

  abstract title: Locator;
  readonly header: Header = new Header(this.page, this.site);
  readonly cookieBanner: CookieBanner = new CookieBanner(this.page);

  readonly notificationBanner: Locator = this.page
    .getByRole('main')
    .getByRole('alert');

  readonly footer: Footer = new Footer(this.page);

  async logOut(): Promise<LoginPage> {
    await this.header.logOutButton.click();
    await this.page.waitForURL('**/login');

    return new LoginPage(this.page);
  }

  async clickCookiesFooterLink(): Promise<CookiePoliciesPage> {
    await this.footer.links.cookiesPolicy.click();
    await this.page.waitForURL('**/cookies-policy');

    return new CookiePoliciesPage(this.page);
  }
}
