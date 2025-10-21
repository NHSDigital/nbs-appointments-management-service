import { type Locator } from '@playwright/test';
import { CookieBanner, Footer, Header, PageObject } from '@e2etests/types';
import { LoginPage } from '@e2etests/page-objects';
import { CookiesPolicyPage } from '@testing-page-objects';

export default abstract class MYALayout extends PageObject {
  abstract title: Locator;

  readonly header: Header = {
    serviceName: this.page
      .getByRole('banner')
      .getByRole('link', { name: 'Manage Your Appointments' }),
    changeSiteButton: this.page
      .getByRole('banner')
      .getByRole('button', { name: 'Change Site' }),
    logOutButton: this.page
      .getByRole('banner')
      .getByRole('button', { name: 'Log Out' }),
    currentUser: (userName: string) =>
      this.page.getByRole('banner').getByText(userName),
    navBar: {
      viewAvailability: this.page
        .getByRole('navigation')
        .getByRole('link', { name: 'View availability' }),
      createAvailability: this.page
        .getByRole('navigation')
        .getByRole('link', { name: 'Create availability' }),
      changeSiteDetails: this.page
        .getByRole('navigation')
        .getByRole('link', { name: 'Change site details' }),
      manageUsers: this.page
        .getByRole('navigation')
        .getByRole('link', { name: 'Manage users' }),
      reports: this.page
        .getByRole('navigation')
        .getByRole('link', { name: 'Reports' }),
    },
  };

  readonly cookieBanner: CookieBanner = {
    preAcceptanceHeader: this.page.getByRole('heading', {
      name: 'Cookies on the NHS website',
    }),
    postAcceptanceMessage: this.page.getByText(
      'You can change your cookie settings at any time',
    ),
    acceptCookiesButton: this.page.getByRole('button', {
      name: `I'm OK with analytics cookies`,
    }),
    rejectCookiesButton: this.page.getByRole('button', {
      name: 'Do not use analytics cookies',
    }),
  };

  readonly notificationBanner: Locator = this.page
    .getByRole('main')
    .getByRole('banner');

  readonly footer: Footer = {
    buildNumber: async () => {
      const buildNumberSpan = await this.page
        .getByText(/^Build number: /)
        .textContent();
      return buildNumberSpan?.split('Build number: ')[1];
    },
    links: {
      userGuidance: this.page.getByRole('link', { name: 'User guidance' }),
      termsOfUse: this.page.getByRole('link', { name: 'Terms of use' }),
      privacyPolicy: this.page.getByRole('link', { name: 'Privacy Policy' }),
      cookiesPolicy: this.page.getByRole('link', { name: 'Cookies Policy' }),
      accessibilityStatement: this.page.getByRole('link', {
        name: 'Accessibility Statement',
      }),
    },
  };

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
