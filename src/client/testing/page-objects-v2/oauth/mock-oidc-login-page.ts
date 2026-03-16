import { type Locator } from '@playwright/test';
import { EulaConsentPage, SiteSelectionPage } from '@e2etests/page-objects';
import { MockOidcUser, PageObject } from '@e2etests/types';

export default class MockOidcLoginPage extends PageObject {
  readonly usernameField: Locator = this.page.getByLabel('Username');
  readonly passwordField: Locator = this.page.getByLabel('Password');

  async signIn(user: MockOidcUser): Promise<SiteSelectionPage> {
    await this.enterCredentials(user.username, user.password);

    await this.page.waitForURL(`**/sites`);
    return new SiteSelectionPage(this.page);
  }

  async signInExpectingEulaRedirect(
    user: MockOidcUser,
  ): Promise<EulaConsentPage> {
    await this.enterCredentials(user.username, user.password);

    await this.page.waitForURL('**/eula');
    return new EulaConsentPage(this.page);
  }

  private async enterCredentials(
    username: string,
    password: string,
  ): Promise<void> {
    await this.usernameField.fill(username);
    await this.passwordField.fill(password);
    await this.passwordField.press('Enter');
  }
}
