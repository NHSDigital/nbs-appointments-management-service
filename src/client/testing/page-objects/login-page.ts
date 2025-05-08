import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';
import { OAuthLoginPage } from '@testing-page-objects';

export default class LoginPage extends RootPage {
  readonly pageContentLogInButton: Locator;
  readonly OKTALogInButton: Locator;

  constructor(page: Page) {
    super(page);
    this.pageContentLogInButton = page.getByRole('button', {
      name: 'Sign in to service with NHS Mail',
    });
    this.OKTALogInButton = page.getByRole('button', {
      name: 'Sign in to service with Other Email',
    });
  }

  async logInWithNhsMail(): Promise<OAuthLoginPage> {
    await this.goto();
    await this.page.waitForURL('**/manage-your-appointments/**');

    await this.pageContentLogInButton.click();
    await this.page.waitForURL('**/Account/Login?ReturnUrl=**');

    return new OAuthLoginPage(this.page);
  }
}
