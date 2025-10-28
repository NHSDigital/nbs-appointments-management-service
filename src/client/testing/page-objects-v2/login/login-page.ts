import { PageObject } from '@e2etests/types';
import { type Locator } from '@playwright/test';
import { MockOidcLoginPage } from '@e2etests/page-objects';

export default class LoginPage extends PageObject {
  readonly nhsMailLogInButton: Locator = this.page.getByRole('button', {
    name: 'Sign in to service with NHS Mail',
  });

  readonly OKTALogInLink: Locator = this.page.getByRole('link', {
    name: 'Sign in to service with Other Email',
  });

  async goto(): Promise<LoginPage> {
    await this.page.goto('/');
    await this.page.waitForURL('**/manage-your-appointments/**');

    return this;
  }

  async logInWithNhsMail(): Promise<MockOidcLoginPage> {
    await this.goto();
    await this.page.waitForURL('**/manage-your-appointments/**');

    await this.nhsMailLogInButton.click();
    await this.page.waitForURL('**/Account/Login?ReturnUrl=**');

    return new MockOidcLoginPage(this.page);
  }
}
