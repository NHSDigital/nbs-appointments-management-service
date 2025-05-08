import { type Locator, type Page } from '@playwright/test';
import OAuthLoginPage from './oauth';

type CookieBanner = {
  preAcceptanceHeader: Locator;
  postAcceptanceMessage: Locator;
  acceptCookiesButton: Locator;
  rejectCookiesButton: Locator;
};

type FooterLinks = {
  userGuidance: Locator;
  termsOfUse: Locator;
  privacyPolicy: Locator;
  cookiesPolicy: Locator;
};

export default class RootPage {
  readonly page: Page;
  readonly headerLogInButton: Locator;
  readonly pageContentLogInButton: Locator;
  readonly OKTALogInButton: Locator;
  readonly logOutButton: Locator;
  readonly serviceName: Locator;
  readonly homeBreadcrumb: Locator;
  readonly acceptButton: Locator;
  readonly cookieBanner: CookieBanner;
  readonly footerLinks: FooterLinks;
  readonly buildNumber: Locator;

  readonly notificationBanner: Locator;
  readonly dismissNotificationBannerButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.serviceName = page.getByRole('link', { name: 'NHS Appointment Book' });
    this.headerLogInButton = page.getByRole('button', { name: 'Log In' });
    this.pageContentLogInButton = page.getByRole('button', {
      name: 'Sign in to service with NHS Mail',
    });
    this.OKTALogInButton = page.getByRole('link', {
      name: 'Sign in to service with Other Email',
    });
    this.logOutButton = page.getByRole('button', { name: 'Log Out' });
    this.homeBreadcrumb = page.getByRole('link', {
      name: 'Home',
    });
    this.acceptButton = page.getByLabel('Accept and continue');
    this.cookieBanner = {
      preAcceptanceHeader: page.getByRole('heading', {
        name: 'Cookies on the NHS website',
      }),
      postAcceptanceMessage: page.getByText(
        'You can change your cookie settings at any time',
      ),
      acceptCookiesButton: page.getByRole('button', {
        name: `I'm OK with analytics cookies`,
      }),
      rejectCookiesButton: page.getByRole('button', {
        name: 'Do not use analytics cookies',
      }),
    };
    this.footerLinks = {
      userGuidance: page.getByRole('link', { name: 'User guidance' }),
      termsOfUse: page.getByRole('link', { name: 'Terms of use' }),
      privacyPolicy: page.getByRole('link', { name: 'Privacy Policy' }),
      cookiesPolicy: page.getByRole('link', { name: 'Cookies Policy' }),
    };
    this.buildNumber = page.getByText(/^Build number: /);

    this.notificationBanner = page.getByRole('banner');
    this.dismissNotificationBannerButton = this.notificationBanner.getByRole(
      'button',
      { name: 'Close' },
    );
  }

  async logInWithNhsMail(): Promise<OAuthLoginPage> {
    await this.pageContentLogInButton.click();
    await this.page.waitForURL('**/Account/Login?ReturnUrl=**');

    return new OAuthLoginPage(this.page);
  }

  async goto() {
    await this.page.goto('/manage-your-appointments/');
  }

  async logOut() {
    await this.logOutButton.click();
  }
}
