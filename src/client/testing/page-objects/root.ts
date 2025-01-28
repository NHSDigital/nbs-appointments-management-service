import { type Locator, type Page } from '@playwright/test';

export default class RootPage {
  readonly page: Page;
  readonly headerLogInButton: Locator;
  readonly pageContentLogInButton: Locator;
  readonly logOutButton: Locator;
  readonly serviceName: Locator;
  readonly homeBreadcrumb: Locator;
  readonly acceptButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.serviceName = page.getByRole('link', { name: 'NHS Appointment Book' });
    this.headerLogInButton = page.getByRole('button', { name: 'Log In' });
    this.pageContentLogInButton = page.getByRole('button', {
      name: 'Sign in to service with NHS Mail',
    });
    this.logOutButton = page.getByRole('button', { name: 'Log Out' });
    this.homeBreadcrumb = page.getByRole('link', {
      name: 'Home',
    });
    this.acceptButton = page.getByLabel('Accept and continue');
  }

  async goto() {
    await this.page.goto('/manage-your-appointments/');
  }

  async logOut() {
    await this.logOutButton.click();
  }
}
