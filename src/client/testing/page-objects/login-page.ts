import { type Locator, type Page } from '@playwright/test';
import { OAuthLoginPage } from '@testing-page-objects';

export default class LoginPage {
  readonly page: Page;
  readonly nhsMailLogInButton: Locator;
  readonly OKTALogInLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nhsMailLogInButton = page.getByRole('button', {
      name: 'Sign in to service with NHS Mail',
    });
    this.OKTALogInLink = page.getByRole('link', {
      name: 'Sign in to service with Other Email',
    });
  }

  async goto(): Promise<LoginPage> {
    await this.page.goto('/');
    await this.page.waitForURL('**/manage-your-appointments/**');

    return this;
  }

  async logInWithNhsMail(): Promise<OAuthLoginPage> {
    await this.goto();
    await this.page.waitForURL('**/manage-your-appointments/**');

    await this.nhsMailLogInButton.click();
    await this.page.waitForURL('**/Account/Login?ReturnUrl=**');

    return new OAuthLoginPage(this.page);
  }
}
